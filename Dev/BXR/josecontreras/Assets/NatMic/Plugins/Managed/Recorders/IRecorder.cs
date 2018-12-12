/* 
*   NatMic
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatMicU.Core.Recorders {

    using System;

    public interface IRecorder : IDisposable {
        /// <summary>
        /// Audio format of audio data that will be recorded
        /// </summary>
        Format format { get; }
        /// <summary>
        /// Start recording.
        /// The provided callback will be invoked on the main thread once recording is stopped
        /// </summary>
        /// <param name="callback">Callback invoked once recorded audio file is ready</param>
        void StartRecording (RecordingCallback callback);
        /// <summary>
        /// Commit audio data to be written to an audio file
        /// </summary>
        /// <param name="samples">Audio sample buffer</param>
        /// <param name="timestamp">Timestamp for the sample buffer in nanoseconds</param>
        void CommitSamples (float[] samples, long timestamp);
    }
}