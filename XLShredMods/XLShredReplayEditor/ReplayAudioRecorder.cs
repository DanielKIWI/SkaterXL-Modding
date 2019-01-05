using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using XLShredReplayEditor.Extensions;

namespace XLShredReplayEditor {
    public class ReplayAudioRecorder : MonoBehaviour {

        public bool isPlayback;
        private int sampleRate = 0;
        private double sampleDeltaTime;
        public float volumeFactor = 1f;
        public AudioSource playBackAudioSource;
        public AudioClip playBackAudioClip;

        // constants for the wave file header
        private const int HEADER_SIZE = 44;
        private const short BITS_PER_SAMPLE = 16;
        private string tempAudioDirectory;
        private string tempAudioPath;
        // the number of audio channels in the output file
        private int channels = 2;

        // the audio stream instance
        private int samplesWritten = 0;
        private FileStream fileOutputStream;
        private MemoryStream outputStream;
        private BinaryWriter outputWriter;
        private AudioSourceDataForwarder[] audioSourceDataForwarders;
        public float desyncTolerance = 0.05f;

        private int startingAudioSourceID;
        private float[] audioBuffer;
        double dspEndTime = 0.0;

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
            sampleRate = AudioSettings.outputSampleRate;
            sampleDeltaTime = 1.0 / (double)sampleRate;
            tempAudioDirectory = Application.dataPath + "\\Temp";
            tempAudioPath = tempAudioDirectory + "\\RecordedAudio.wav";
            dspEndTime = 0f;
            Clear();
        }

        public void Update() {
            if (ReplayManager.CurrentState == ReplayState.PLAYBACK) {
                playBackAudioSource.pitch = ReplayManager.Instance.playbackTimeScale;
                if (Mathf.Abs(playBackAudioSource.time - ReplayManager.Instance.playbackTime) > 0.1f) {
                    try {
                        SetPlaybackTime(ReplayManager.Instance.playbackTime);
                    } catch (Exception e) { Debug.LogException(e); }
                }
            }
            //Debug.Log("replayrecorder.endtime: " + ReplayManager.Instance.recorder.endTime + ", dspendtime: " + dspEndTime);
        }

        // reset the renderer
        public void Clear() {
            this.outputStream = new MemoryStream();
            this.outputWriter = new BinaryWriter(outputStream);
        }

        public IEnumerator StartPlayback() {
            SaveToTemp();   //TODO move to Background Thread
            yield return LoadFromTemp();
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
            Debug.Log("Changing PlaybackTime from " + playBackAudioSource.time + " to " + t + ", length: " + playBackAudioClip.length + ", " + (playBackAudioSource.clip == playBackAudioClip) + "," + (playBackAudioClip != null));
            playBackAudioSource.time = Mathf.Clamp(t, 0, playBackAudioClip.length);
        }

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
                    this.outputWriter.Write((short)(0f * (float)Int16.MaxValue));
                }
                samplesWritten++;
                i++;
                dspEndTime = samplesWritten * sampleDeltaTime;
            }
            if (i != 0)
                Debug.Log("Added " + i + " empty samples for syncing");
            return 0f;
        }
        public void ReceivedAudioData(float[] data, int channels, AudioSource source) {
            if (ReplayManager.CurrentState != ReplayState.RECORDING) {
                audioBuffer = null;
                return;
            }
            this.channels = channels;
            if ((startingAudioSourceID == source.GetInstanceID()) || (audioBuffer == null)) {
                if (audioBuffer != null) {
                    float offset = getSyncingOffset();
                    Write(audioBuffer, offset);
                }
                audioBuffer = new float[data.Length];
                startingAudioSourceID = source.GetInstanceID();
                Array.Copy(data, audioBuffer, data.Length);
            } else {
                for (int i = 0; i < data.Length && i < audioBuffer.Length; i++) {
                    audioBuffer[i] += data[i];
                }
            }
        }
        /// Write a chunk of data to the output stream.
        public void Write(float[] audioData, float sampleOffset) {
            if (Time.timeScale <= 0.0001f) return;
            int sampleCount = audioData.Length / channels;
            float sample = sampleOffset;
            int sampleIndex = (int)sampleOffset;
            if (sampleIndex < 0) sampleIndex = 0;
            float scale = Time.timeScale;
            if (scale != 1f)
                Debug.Log("scale: " + scale);
            while (sampleIndex < sampleCount) {
                for (int c = 0; c < channels; c++) {
                    short value = (short)(Mathf.Clamp(audioData[sampleIndex * channels + c] * volumeFactor, -1f, 1f) * (float)Int16.MaxValue);
                    this.outputWriter.Write(value);
                }
                samplesWritten++;
                sample += scale; //(float)sampleCount / (float)targetSampleCount;
                sampleIndex = (int)sample;
            }
            dspEndTime = samplesWritten * sampleDeltaTime;
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
                Save(tempAudioPath);
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }
        public bool Save(string path) {
            if (path.Length <= 0) {
                Debug.LogError("filename is empty");
                return false;
            }
            if (outputStream.Length <= 0) {
                Debug.LogWarning("There is no audio data to save!");
                return false;
            }
            if (File.Exists(path)) {
                File.Delete(path);
            }
            fileOutputStream = File.OpenWrite(path);
            long prevPos = outputStream.Position;

            //TODO change start Position if startTime > 0, //TODO adjust playbackTime handling
            outputStream.Position = 0;
            // add a header to the file so we can send it to the SoundPlayer
            this.AddHeader();
            // copy over the actual audio data
            this.outputStream.WriteTo(fileOutputStream);

            fileOutputStream.Close();

            outputStream.Position = prevPos;
            // for debugging only
            Debug.LogWarning("Finished saving to " + path + ", outputStream.Length: " + outputStream.Length + ", prevPos: " + prevPos);
            return true;

        }

        /// This generates a simple header for a canonical wave file, 
        /// which is the simplest practical audio file format. It
        /// writes the header and the audio file to a new stream, then
        /// moves the reference to that stream.
        /// 
        /// See this page for details on canonical wave files: 
        /// http://www.lightlink.com/tjweber/StripWav/Canon.html
        private void AddHeader() {
            // calculate the number of samples in the data chunk
            long numberOfSamples = (outputStream.Length - outputStream.Position) / (BITS_PER_SAMPLE / 8);
            Debug.LogWarning("(" + outputStream.Length + " - " + outputStream.Position + ") / (" + BITS_PER_SAMPLE + "/ 8) = numberOfSamples: " + numberOfSamples + ", samplesWritten * channels: " + (channels * samplesWritten) + ", channels: " + channels);
            // create a new MemoryStream that will have both the audio data AND the header
            BinaryWriter writer = new BinaryWriter(fileOutputStream);

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
        #endregion
        private IEnumerator LoadFromTemp() {
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + tempAudioPath, AudioType.WAV);
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
                    playBackAudioSource.time = ReplayManager.Instance.playbackTime;
                } catch (Exception e) {
                    Debug.LogException(e);
                }
            }
        }
    }

}
