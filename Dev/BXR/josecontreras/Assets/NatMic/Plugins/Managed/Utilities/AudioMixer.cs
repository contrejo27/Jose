/* 
*   NatMic
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatMicU.Utilities {

    using UnityEngine;
    using System;
    using System.Threading;
    using Core;

    public sealed class AudioMixer : IDisposable {

        #region --Op vars--
        private readonly SampleBufferCallback callback;
        private readonly Format format;
        private readonly CircularBuffer commitBuffer, unityBuffer;
        private readonly Thread mixerThread;
        private float[] commitStagingBuffer, unityStagingBuffer;
        private float[] microphoneStagingBuffer, mixedBuffer;
        private double timestamp;
        private bool running = true;
        private const int SampleCount = 512;
        #endregion

        
        #region --Client API--

        public AudioMixer (SampleBufferCallback callback) {
            // Set format
            this.callback = callback;
            this.format = Format.DefaultForMixing;
            // Create buffers
            commitBuffer = new CircularBuffer();
            unityBuffer = new CircularBuffer();
            mixedBuffer = new float[SampleCount * format.channelCount];
            // Start thread
            mixerThread = new Thread(MixerLoop);
            mixerThread.Start();
        }

        public void Dispose () {
            lock (this) running = false;
        }
        #endregion


        #region --Operations--

        public void OnMicrophoneSampleBuffer (AudioEvent audioEvent, float[] sampleBuffer, long timestamp, Format unused2) {
            if (audioEvent == AudioEvent.OnSampleBuffer) commitBuffer.Write(sampleBuffer);        
        }

        public void OnUnitySampleBuffer (AudioEvent audioEvent, float[] sampleBuffer, long timestamp, Format unused2) {
            if (audioEvent == AudioEvent.OnSampleBuffer) unityBuffer.Write(sampleBuffer);
        }

        private void MixerLoop () {
            // Send start event
            callback(AudioEvent.OnInitialize, null, (long)timestamp, format);
            double WaitTimeNs = 1e+9f * SampleCount / (double)format.sampleRate;
            // Sample buffers
            while (true) {
                // Sleep
                Thread.Sleep(3);
                // Check
                lock (this) if (!running) break;
                // Check that there are enough samples for this iteration
                if (unityBuffer.AvailableSamples < SampleCount * format.channelCount) continue;
                if (commitBuffer.AvailableSamples < SampleCount) continue;
                // Read
                unityBuffer.Read(ref unityStagingBuffer, SampleCount * format.channelCount);
                commitBuffer.Read(ref commitStagingBuffer, SampleCount);
                // Interleave
                if (format.channelCount != 1) {
                    microphoneStagingBuffer = microphoneStagingBuffer ?? new float[unityStagingBuffer.Length];
                    AudioUtility.Repeat(commitStagingBuffer, microphoneStagingBuffer, format.channelCount);
                }
                else microphoneStagingBuffer = commitStagingBuffer;
                // Mix
                AudioUtility.Mix(unityStagingBuffer, microphoneStagingBuffer, mixedBuffer);
                callback(
                    AudioEvent.OnSampleBuffer,
                    mixedBuffer,
                    //unityStagingBuffer,
                    //microphoneStagingBuffer,
                    (long)(timestamp += WaitTimeNs),
                    format
                );
            }
            // Send end event
            callback(AudioEvent.OnFinalize, null, 0L, new Format());
        }
        #endregion
    }
}