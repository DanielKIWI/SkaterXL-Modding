using UnityEngine;
using System.Collections;
using System.IO;
using System;

namespace XLShredReplayEditor {
    public static class SoundLoader {

        public static AudioClip LoadWav(string path) {
            //Read all bytes of file
            byte[] wav = File.ReadAllBytes(path);

            //Get channels (usually 1 or 2)
            int channels = wav[22];

            // Get past all the other sub chunks to get to the data subchunk:
            int pos = 12;   // First Subchunk ID from 12 to 16

            // Keep iterating until we find the data chunk
            while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97)) {
                pos += 4;
                int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
                pos += 4 + chunkSize;
            }
            pos += 8;

            // Pos is now positioned to start of actual sound data.
            int samples = (wav.Length - pos) / 2;     // 2 bytes per sample (16 bit sound mono)
            if (channels == 2) samples /= 2;        // 4 bytes per sample (16 bit stereo)

            //Create audio data array
            byte[] audioData = new byte[samples];
            //Copy over data chunk
            Array.Copy(wav, pos, audioData, 0, samples);
            //Get samples
            float[] audioSamples = BytesToFloats(audioData);

            //Create clip
            AudioClip clip = AudioClip.Create("clip", samples, channels, 44100, true, false);
            clip.SetData(audioSamples, 0);
            return clip;
        }
        /// <summary>
        /// Takes a byte array and returns a float[] of samples
        /// </summary>
        /// <param name="array">The array of bytes from file</param>
        private static float[] BytesToFloats(byte[] array) {
            //1 Short = 2 bytes, so divide by 2
            float[] floatArray = new float[array.Length / 2];
            //Iterate through and populate array
            for (int i = 0; i < floatArray.Length; i++) {
                //Convert each set of 2 bytes into a short and scale that to be in the range of -1, 1
                floatArray[i] = (BitConverter.ToInt16(array, i * 2) / (float)(short.MaxValue));
            }
            return floatArray;
        }
    }

}
