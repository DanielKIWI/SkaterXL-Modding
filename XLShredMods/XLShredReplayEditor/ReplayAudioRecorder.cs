using System;
using System.Threading;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using XLShredReplayEditor.Extensions;

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

        // constants for the wave file header
        private const int HEADER_SIZE = 44;
        private const short BITS_PER_SAMPLE = 16;
        private string tempAudioDirectory;
        private string tempAudioPath;
        private string wavFilePath;
        // the number of audio channels in the output file
        private int sampleRate = 0;
        private int channels = 2;
        private int samplesWritten = 0;
        private long maxTmpStreamLength {
            get {
                return (long)(Main.settings.MaxRecordedTime * sampleRate) * (BITS_PER_SAMPLE / 8 * channels);
            }
        }

        private Thread fileStreamThread;
        EventWaitHandle fileThreadWait = new EventWaitHandle(false, EventResetMode.ManualReset);
        EventWaitHandle audioThreadWait = new EventWaitHandle(true, EventResetMode.ManualReset);

        private MemoryStream tmpMemoryStream;
        private FileStream fileOutputStream;
        private BinaryWriter tmpFileWriter;
        private AudioSourceDataForwarder[] audioSourceDataForwarders;
        public float desyncTolerance = 0.05f;
        /// <summary>
        /// timesSamples Count used for current buffer
        /// </summary>
        public int firstAudioSourceID = -1;

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

        private AudioBuffer audioBufferW;
        private AudioBuffer audioBufferFS;
        double dspEndTime = 0.0;
        float timeScale;
        object timeScaleLock = new object();

        public void Awake() {
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
            sampleRate = AudioSettings.outputSampleRate;
            sampleDeltaTime = 1.0 / (double)sampleRate;
            tempAudioDirectory = Application.dataPath + "\\Temp";
            tempAudioPath = tempAudioDirectory + "\\RecordedAudio.wav.temp";
            wavFilePath = tempAudioDirectory + "\\RecordedAudio.wav";
            if (!File.Exists(tempAudioPath)) {
                File.Create(tempAudioPath);
            }
            tmpMemoryStream = new MemoryStream();//new FileStream(tempAudioPath, FileMode.Truncate, FileAccess.ReadWrite, FileShare.ReadWrite, 2 << 22, true);
            tmpFileWriter = new BinaryWriter(tmpMemoryStream);
            dspEndTime = 0f;
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
                        SetPlaybackTime(ReplayManager.Instance.displayedPlaybackTime);
                    } catch (Exception e) { Debug.LogException(e); }
                }
            }
        }

        private void WriteLoop() {
            while (true) {
                long ticks = DateTime.Now.Ticks;
                fileThreadWait.WaitOne();
                fileThreadWait.Reset();
                if (audioBufferFS != null && audioBufferFS.Length > 0 && tmpMemoryStream != null) {
                    WriteAudioBufferToTmpFile();
                    //Console.WriteLine("Finished write " + audioBufferFS.Length + " Bytes to temp file, samplesWritten: " + samplesWritten + ", maxTmpStreamLength: " + maxTmpStreamLength);
                }
                audioThreadWait.Set();
            }
        }
        /// Write a chunk of data to the output stream.
        public void WriteAudioBufferToTmpFile() {
            float scale;
            lock (timeScaleLock) {
                scale = Time.timeScale;
            }
            if (scale <= 0.0001f) return;
            float sampleOffset = getSyncingOffset();
            int sampleCount = audioBufferFS.Length / channels;
            float sample = sampleOffset;
            int sampleIndex = (int)sampleOffset;
            if (sampleIndex < 0) sampleIndex = 0;
            long ticks = DateTime.Now.Ticks;
            while (sampleIndex < sampleCount) {
                for (int c = 0; c < channels; c++) {
                    short value = (short)(Mathf.Clamp(audioBufferFS.data[sampleIndex * channels + c] * volumeFactor, -1f, 1f) * (float)Int16.MaxValue);
                    if (tmpMemoryStream.Position > maxTmpStreamLength) {
                        tmpMemoryStream.Position = 0;
                    }
                    this.tmpFileWriter.Write(value);
                }
                samplesWritten++;
                sample += scale;
                sampleIndex = (int)sample;
            }
            Console.WriteLine("Writing Buffer to file took " + (DateTime.Now.Ticks - ticks) + " ticks");
            dspEndTime = samplesWritten * sampleDeltaTime;
        }

        public void StartRecording() {
            if (fileStreamThread != null) {
                if (fileStreamThread.IsAlive) throw new NotSupportedException();
                return;
            }
            fileStreamThread = new Thread(new ThreadStart(this.WriteLoop));
            fileStreamThread.Start();
        }
        public IEnumerator StartPlayback() {
            SaveToTemp();
            yield return LoadFromTemp();
            SetPlaybackTime(ReplayManager.Instance.displayedPlaybackTime);
        }
        public void StopPlayback() {
            playBackAudioSource.Stop();
            playBackAudioSource.clip = null;
        }
        public void SetPlaybackTime(float t) {
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
                //Debug.Log("timeSamples: " + bufferTimeSamples + " -> " + timeSamples);
                long ticks = DateTime.Now.Ticks;
                audioThreadWait.WaitOne();
                audioThreadWait.Reset();
                HandleBufferExchange();
                fileThreadWait.Set();
                audioBufferW = new AudioBuffer(data);
                //Debug.Log("Wait and HandleBufferExchange took " + (DateTime.Now.Ticks - ticks) + " ticks");
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
        public void SaveToTemp() {
            if (!Directory.Exists(tempAudioDirectory)) {
                Directory.CreateDirectory(tempAudioDirectory);
            }
            if (playBackAudioClip != null) {
                playBackAudioClip.UnloadAudioData();
                playBackAudioClip = null;
            }
            try {
                Save(wavFilePath);
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }
        public bool Save(string path) {
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

            long numberOfSamples = tmpMemoryStream.Length * 8 / (BITS_PER_SAMPLE * channels);
            Debug.LogWarning("numberOfSamples (" + numberOfSamples + ") = tmpFileStream.Length (" + tmpMemoryStream.Length + ") * 8 / (BITS_PER_SAMPLE * channels)");

            // create a new MemoryStream that will have both the audio data AND the header
            BinaryWriter writer = new BinaryWriter(fileOutputStream);

            // add a header to the file so we can send it to the SoundPlayer
            this.AddHeader(writer, numberOfSamples);
            // copy over the actual audio data
            this.tmpMemoryStream.CopyTo(fileOutputStream);
            tmpMemoryStream.Position = 0;
            CopyStream(tmpMemoryStream, fileOutputStream, prevPos);

            fileOutputStream.Close();

            tmpMemoryStream.Position = prevPos;

            Debug.Log("Finished saving to " + path + ", outputStream.Length: " + tmpMemoryStream.Length + ", prevPos: " + prevPos);
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

        private IEnumerator LoadFromTemp() {
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + wavFilePath, AudioType.WAV);
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
                    this.tmpFileWriter.Write((short)(0f * (float)Int16.MaxValue));
                }
                samplesWritten++;
                i++;
                dspEndTime = samplesWritten * sampleDeltaTime;
            }
            if (i != 0)
                Debug.Log("Added " + i + " empty samples for syncing");
            return 0f;
        }
        public static void CopyStream(Stream input, Stream output, int bytes) {
            byte[] buffer = new byte[32768];
            int read;
            while (bytes > 0 &&
                   (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0) {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }
    }

}
