using System;
using System.Threading;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using XLShredReplayEditor.Utils;

namespace XLShredReplayEditor {
    public class ReplayAudioRecorder : MonoBehaviour {
        public static ReplayAudioRecorder Instance {
            get {
                return ReplayManager.Instance.audioRecorder;
            }
        }
        public bool isPlayback;
        private double sampleDeltaTime;
        public float volumeFactor = 1f;
        public AudioSource playBackAudioSource;
        public AudioClip playBackAudioClip;

        public float audioStartTime;

        // constants for the wave file header
        private const int HEADER_SIZE = 44;
        private const short BITS_PER_SAMPLE = 16;

        private int sampleRate = 0;
        private int channels = 2;
        private int samplesWritten = 0;
        private long maxTmpStreamLength;

        //Paths
        private string tempAudioDirectory;
        private string wavFilePath;
        //private string tempAudioPath;
        
        private MemoryStream tmpMemoryStream;
        private FileStream fileOutputStream;
        private BinaryWriter tmpStreamWriter;
        private AudioSourceDataForwarder[] audioSourceDataForwarders;
        public float desyncTolerance = 0.05f;
        
        /// <summary>
        /// timesSamples Count used for current buffer
        /// </summary>
        public int firstAudioSourceID = -1;

        private Thread fileStreamThread;
        EventWaitHandle fileThreadWait = new EventWaitHandle(false, EventResetMode.ManualReset);
        EventWaitHandle audioThreadWait = new EventWaitHandle(true, EventResetMode.ManualReset);
        object timeScaleLock = new object();

        float timeScale;
        double dspEndTime = 0.0;
        private AudioBuffer audioBufferW;
        private AudioBuffer audioBufferFS;
        

        public class AudioBuffer {
            public float[] data;
            public AudioBuffer(float[] d) {
                data = new float[d.Length];
                Array.Copy(d, data, d.Length);
            }
            public void AddData(float[] d) {
                int count = data.Length < d.Length ? data.Length : d.Length;
                for (int i = 0; i < count; i++) {
                    data[i] += d[i];
                }
            }
            public int Length {
                get {
                    return data.Length;
                }
            }
        }
        public void CalcMaxTmpStreamLength() {
            maxTmpStreamLength = (long)(Main.settings.MaxRecordedTime * sampleRate) * (BITS_PER_SAMPLE / 8 * channels);
        }

        public void Awake() {
            sampleRate = AudioSettings.outputSampleRate;
            sampleDeltaTime = 1.0 / (double)sampleRate;
            tempAudioDirectory = Application.dataPath + "\\Temp";
            wavFilePath = tempAudioDirectory + "\\RecordedAudio.wav";
            CalcMaxTmpStreamLength();
            playBackAudioSource = gameObject.AddComponent<AudioSource>();
            var audioSources = DeckSounds.Instance.getAudioSources();
            audioSourceDataForwarders = new AudioSourceDataForwarder[audioSources.Length];
            int i = 0;
            foreach (AudioSource audioSource in audioSources) {
                var dataForwarder = audioSource.gameObject.AddComponent<AudioSourceDataForwarder>();
                dataForwarder.receiver += ReceivedAudioData;
                dataForwarder.audioSource = audioSource;
                audioSourceDataForwarders[i] = dataForwarder;
                i++;
            }
            audioSourceDataForwarders = new AudioSourceDataForwarder[0];

            //tempAudioPath = tempAudioDirectory + "\\RecordedAudio.wav.temp";
            //if (!File.Exists(tempAudioPath)) {
            //    File.Create(tempAudioPath);
            //}
            tmpMemoryStream = new MemoryStream();//new FileStream(tempAudioPath, FileMode.Truncate, FileAccess.ReadWrite, FileShare.ReadWrite, 2 << 22, true);
            tmpStreamWriter = new BinaryWriter(tmpMemoryStream);
            dspEndTime = 0f;
        }

        internal IEnumerator LoadReplayAudio(string path) {
            yield return LoadWavFileToAudioSource(path);
            audioStartTime = 0;
            SetPlaybackTime(0f);
        }

        public void OnDisable() {
            if (tmpMemoryStream != null)
                tmpMemoryStream.Close();

            if (fileStreamThread != null) {
                fileStreamThread.Abort();
                fileStreamThread = null;
            }
        }
        public void Update() {
            lock (timeScaleLock) {
                timeScale = Time.timeScale;
            }
            if (ReplayManager.CurrentState == ReplayState.PLAYBACK) {
                playBackAudioSource.pitch = ReplayManager.Instance.playbackTimeScale;
                if (Mathf.Abs(playBackAudioSource.time - ReplayManager.Instance.displayedPlaybackTime) > 0.1f) {
                    try {
                        SetPlaybackTime(ReplayManager.Instance.playbackTime);
                    } catch (Exception e) { Debug.LogException(e); }
                }
            }
        }

        private void WriteLoop() {
            while (true) {
                fileThreadWait.WaitOne();
                fileThreadWait.Reset();
                if (audioBufferFS != null && audioBufferFS.Length > 0 && tmpMemoryStream != null) {
                    WriteAudioBufferToMemoryStream();
                }
                audioThreadWait.Set();
            }
        }
        /// Write a chunk of data to the output stream.
        public void WriteAudioBufferToMemoryStream() {
            float scale;
            lock (timeScaleLock) {
                scale = Time.timeScale;
            }
            if (scale <= 0.0001f) return;
            float sampleOffset = getSyncingOffset();
            int sampleCount = audioBufferFS.Length / channels;
            //Console.WriteLine("sampleCount: " + sampleCount + ", audioBufferFS.Length: " + audioBufferFS.Length + "sampleOffset: " + sampleOffset + ", samplesWritten: " + samplesWritten);
            float sample = sampleOffset;
            int sampleIndex = (int)sampleOffset;
            if (sampleIndex < 0) sampleIndex = 0;
            while (sampleIndex < sampleCount) {
                for (int c = 0; c < channels; c++) {
                    short value = (short)(Mathf.Clamp(audioBufferFS.data[sampleIndex * channels + c] * volumeFactor, -1f, 1f) * (float)Int16.MaxValue);
                    this.tmpStreamWriter.Write(value);
                    if (tmpMemoryStream.Position > maxTmpStreamLength) {
                        tmpMemoryStream.Position = 0;
                        Debug.Log("Reset tmpMemoryStream.Position to 0");
                    }
                }
                samplesWritten++;
                sample += scale;
                sampleIndex = (int)sample;
            }
            //Console.WriteLine("dspEndTime: " + dspEndTime + ", sampleDeltaTime: " + sampleDeltaTime + "Recorder.endTime: " + ReplayManager.Instance.recorder.endTime + ", samplesWritten: " + samplesWritten);
            dspEndTime = samplesWritten * sampleDeltaTime;
        }

        public void StartRecording() {
            fileStreamThread = new Thread(new ThreadStart(this.WriteLoop));
            fileStreamThread.Start();
        }
        public void StopRecording() {
            if (fileStreamThread != null) {
                fileStreamThread.Abort();
                fileStreamThread = null;
            }
            SaveToWavFile();
        }
        public IEnumerator StartPlayback() {
            yield return LoadWavFileToAudioSource(wavFilePath);
            SetPlaybackTime(ReplayManager.Instance.playbackTime);
        }
        public void StopPlayback() {
            playBackAudioSource.Stop();
            playBackAudioSource.clip = null;
        }
        public void SetPlaybackTime(float playbackTime) {
            float t = playbackTime - audioStartTime;
            if (playBackAudioClip == null) {
                Debug.LogWarning("playBackAudioClip is null");
                return;
            }
            if (playBackAudioSource.clip == null) {
                playBackAudioSource.clip = playBackAudioClip;
            }
            if (!playBackAudioSource.isPlaying) {
                playBackAudioSource.Play();
            }
            //Debug.Log("Changing PlaybackTime from " + playBackAudioSource.time + " to " + t + ", length: " + playBackAudioClip.length + ", " + (playBackAudioSource.clip == playBackAudioClip) + "," + (playBackAudioClip != null));
            playBackAudioSource.time = Mathf.Clamp(t, 0, playBackAudioClip.length);
        }

        public void ReceivedAudioData(float[] data, int channels, int id) {
            if (ReplayManager.CurrentState != ReplayState.RECORDING) {
                audioBufferW = null;
                return;
            }
            this.channels = channels;
            if ((firstAudioSourceID == id) || (audioBufferW == null)) {
                audioThreadWait.WaitOne();
                audioThreadWait.Reset();
                HandleBufferExchange();
                fileThreadWait.Set();
                audioBufferW = new AudioBuffer(data);
                firstAudioSourceID = id;
            } else {
                audioBufferW.AddData(data);
            }
        }
        private void HandleBufferExchange() {
            audioBufferFS = audioBufferW;
            audioBufferW = null;
        }

        #region File I/O
        public void SaveToWavFile() {
            if (!Directory.Exists(tempAudioDirectory)) {
                Directory.CreateDirectory(tempAudioDirectory);
            }
            if (playBackAudioClip != null) {
                playBackAudioClip.UnloadAudioData();
                playBackAudioClip = null;
            }
            WriteTmpStreamToPath(wavFilePath);
        }
        public bool WriteTmpStreamToPath(string path, float startTime = 0, float endTime = float.PositiveInfinity) {
            if (path.Length <= 0) {
                Debug.LogError("filename is empty");
                return false;
            }
            if (tmpMemoryStream.Length <= 0) {
                Debug.LogWarning("There is no audio data to save!");
                return false;
            }
            if (fileStreamThread != null) {
                fileStreamThread.Abort();
                fileStreamThread = null;
            }
            if (File.Exists(path)) {
                File.Delete(path);
            }
            fileOutputStream = File.OpenWrite(path);
            int prevPos = (int)tmpMemoryStream.Position;
            
            long samplesToBytes = (BITS_PER_SAMPLE * channels) / 8;
            
            long numberOfSamples = tmpMemoryStream.Length * 8 / (BITS_PER_SAMPLE * channels);
            audioStartTime = ReplayManager.Instance.recorder.endTime - ((float)numberOfSamples * sampleRate);

            BinaryWriter writer = new BinaryWriter(fileOutputStream);

            if (startTime > audioStartTime || endTime < ReplayManager.Instance.recorder.endTime) {
                startTime = Mathf.Max(startTime, audioStartTime);
                long targetNoS = (long)((endTime - startTime) * sampleRate);
                if (targetNoS < numberOfSamples) {
                    Debug.Log("numberOfSamples: " + numberOfSamples + " set to " + targetNoS + ", because startTime: " + startTime + ", endTime: " + endTime);
                    numberOfSamples = targetNoS;
                }
            }

            // add a header to the file so we can send it to the SoundPlayer
            this.AddHeader(writer, numberOfSamples);

            long bytesTillEndOfStream = tmpMemoryStream.Length - tmpMemoryStream.Position;
            long requestedBytes = numberOfSamples * samplesToBytes;

            //FIXME: add offset corresponding to startTime - audioStartTime

            if (bytesTillEndOfStream > requestedBytes) {
                CopyStream(tmpMemoryStream, fileOutputStream, (int)(requestedBytes));
            } else {
                this.tmpMemoryStream.CopyTo(fileOutputStream);      //FIXME: not sure if onle from position till end is copied (should be)
                tmpMemoryStream.Position = 0;
                long byteCountLeft = requestedBytes - bytesTillEndOfStream;
                if (byteCountLeft > 0) {
                    CopyStream(tmpMemoryStream, fileOutputStream, byteCountLeft);
                }
            }
            // copy over the actual audio data

            fileOutputStream.Close();

            tmpMemoryStream.Position = prevPos;

            Debug.Log("Finished saving to " + path + ", outputStream.Length: " + tmpMemoryStream.Length + ", prevPos: " + prevPos );
            return true;
        }

        /// This generates a simple header for a canonical wave file, 
        /// which is the simplest practical audio file format. It
        /// writes the header and the audio file to a new stream, then
        /// moves the reference to that stream.
        /// 
        /// See this page for details on canonical wave files: 
        /// http://www.lightlink.com/tjweber/StripWav/Canon.html
        private void AddHeader(BinaryWriter writer, long numberOfSamples) {

            writer.Write(0x46464952); // "RIFF" in ASCII

            // write the number of bytes in the entire file
            writer.Write((int)(HEADER_SIZE + (numberOfSamples * BITS_PER_SAMPLE * channels / 8)) - 8);

            writer.Write(0x45564157); // "WAVE" in ASCII
            writer.Write(0x20746d66); // "fmt " in ASCII
            writer.Write(16);

            // write the format tag. 1 = PCM
            writer.Write((short)1);

            // write the number of channels.
            writer.Write((short)channels);

            // write the sample rate. 44100 in this case. The number of audio samples per second
            writer.Write(sampleRate);

            writer.Write(sampleRate * channels * (BITS_PER_SAMPLE / 8));
            writer.Write((short)(channels * (BITS_PER_SAMPLE / 8)));

            // 16 bits per sample
            writer.Write(BITS_PER_SAMPLE);

            // "data" in ASCII. Start the data chunk.
            writer.Write(0x61746164);

            // write the number of bytes in the data portion
            writer.Write((int)(numberOfSamples * BITS_PER_SAMPLE * channels / 8));

        }

        public IEnumerator LoadWavFileToAudioSource(string path, AudioType type = AudioType.WAV) {
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + path, type);
            Debug.Log("Starting Request to " + www.url);
            yield return www.SendWebRequest();
            if (www.isHttpError || www.isNetworkError) {
                Debug.LogError(www.error);
            } else {
                Debug.Log("Request finished");
                try {
                    playBackAudioClip = DownloadHandlerAudioClip.GetContent(www);
                    Debug.Log("Audio LoadState: " + playBackAudioClip.loadState.ToString());
                    playBackAudioSource.clip = playBackAudioClip;
                    Debug.Log("Loaded Clip: " + playBackAudioSource.clip + ", length: " + playBackAudioSource.clip.length);
                    playBackAudioSource.Play();
                    playBackAudioSource.time = ReplayManager.Instance.displayedPlaybackTime;
                } catch (Exception e) {
                    Debug.LogException(e);
                }
            }
        }
        #endregion

        private float getSyncingOffset() {
            //Synchronising if more audio than visual recorded
            if ((float)dspEndTime - ReplayManager.Instance.recorder.endTime > desyncTolerance) {
                float offset = (float)(dspEndTime - ReplayManager.Instance.recorder.endTime) * sampleRate;
                //Debug.Log("Cut of " + offset.ToString("0.##") + " samples of Audio for syncing");
                return offset;
            }
            //Synchronising if more visual than audio recorded
            int i = 0;
            while (ReplayManager.Instance.recorder.endTime - (float)dspEndTime > desyncTolerance) {
                for (int c = 0; c < channels; c++) {
                    this.tmpStreamWriter.Write((short)(0f * (float)Int16.MaxValue));
                }
                samplesWritten++;
                i++;
                dspEndTime = samplesWritten * sampleDeltaTime;
            }
            if (i != 0)
                Debug.Log("Added " + i + " empty samples for syncing");
            return 0f;
        }
        public static void CopyStream(Stream input, Stream output, long byteCount) {
            try {
                byte[] buffer = new byte[32768];
                int read;
                while (byteCount > 0 && (read = input.Read(buffer, 0, (buffer.Length < byteCount ? buffer.Length : (int)byteCount))) > 0) { //????  //FIXME:
                    output.Write(buffer, 0, read);
                    byteCount -= read;
                }
            }catch (Exception e) {
                Main.modEntry.Logger.Error("CopyStream Error byteCount = " + byteCount + ": " + e);
            }
        }
        public void Destroy() {
            foreach (AudioSourceDataForwarder asdf in audioSourceDataForwarders) {
                Destroy(asdf);
            }
            if (playBackAudioSource != null)
                Destroy(playBackAudioSource);
            if (tmpMemoryStream != null)
                tmpMemoryStream.Close();
            if (fileStreamThread != null)
                fileStreamThread.Abort();
            if (fileOutputStream != null)
                fileOutputStream.Close();
            Destroy(this);
        }
    }

}
