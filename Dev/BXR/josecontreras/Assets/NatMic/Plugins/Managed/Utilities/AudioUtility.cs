/* 
*   NatMic
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatMicU.Utilities {

    using UnityEngine;
    using System;
    using Core;

    /// <summary>
    /// Utility class for getting audio sample buffers from Unity AudioSource's or AudioListener's
    /// </summary>
    [AddComponentMenu("")]
    public sealed class AudioUtility : MonoBehaviour, IDisposable {

        #region --Op vars--
        private Format format;
        private SampleBufferCallback callback;
        #endregion


        #region --Client API--

        public static AudioUtility Create (GameObject source, SampleBufferCallback callback) {
            var component = source.AddComponent<AudioUtility>();
            component.callback = callback;
            component.format = Format.DefaultForMixing;
            return component;
        }

        public void Dispose () {
            Destroy(this);
        }
        #endregion


        #region --Operations--

        private AudioUtility () {}

        private void OnAudioFilterRead (float[] data, int channels) {
            callback(AudioEvent.OnSampleBuffer, data, (long)(AudioSettings.dspTime * 1e+9), format);
        }
        #endregion


        #region --Utility--

        public static void Repeat (float[] monoAudio, float[] dest, int channelCount) {
            for (int i = 0; i < monoAudio.Length * channelCount; i++)
                dest[i] = monoAudio[i / channelCount];
        }

        public static void Mix (float[] srcA, float[] srcB, float[] result) {
            for (int i = 0; i < srcA.Length; i++)
                result[i] = MixSample(srcA[i], srcB[i]);
        }

        private static float MixSample (float sampleA, float sampleB) {
            /**
             * If different signs, return sum
             * If both positive, return sum minus product
             * If both negative, return sum plus product
             */
            float sum = sampleA + sampleB;
            float product = sampleA * sampleB;
            // Product is negative iff different signs
            if (product < 0) return sum;
            // If control reaches here, then signs must be same
            if (sampleA > 0) return sum - product;
            else return sum + product;
        }
        #endregion
    }
}