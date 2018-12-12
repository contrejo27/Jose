/* 
*   NatMic
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatMicU.Core {

    using UnityEngine;
    using System;
    using Docs;

    #region --Delegates--
    /// <summary>
    /// Delegate invoked when a microphone event is raised
    /// </summary>
    [Doc(@"SampleBufferCallback")]
    public delegate void SampleBufferCallback (AudioEvent audioEvent, float[] sampleBuffer, long timestamp, Format format);
    #endregion


    #region --Enumerations--
    /// <summary>
    /// Audio event that triggers a sample buffer callback
    /// </summary>
    [Doc(@"AudioEvent")]
    public enum AudioEvent : int {
        /// <summary>
        /// The microphone has started.
        /// The sample buffer for this event will always be `null`.
        /// </summary>
        [Doc(@"OnInitialize", @"OnInitializeDescription")]
        OnInitialize = 1,
        /// <summary>
        /// The microphone has reported a new sample buffer
        /// </summary>
        [Doc(@"OnSampleBuffer")]
        OnSampleBuffer = 2,
        /// <summary>
        /// The microphone has stopped.
        /// The sample buffer for this event will always be `null`.
        /// </summary>
        [Doc(@"OnFinalize", @"OnFinalizeDescription")]
        OnFinalize = 3
    }
    #endregion


    #region --Value Types--

    /// <summary>
    /// Value type used to specify microphone configuration
    /// </summary>
    [Doc(@"Format")]
    public struct Format : IEquatable<Format> { // Practically same definition as in NatCorder
        /// <summary>
        /// Audio sample rate
        /// </summary>
        [Doc(@"SampleRate")]
        public int sampleRate;
        /// <summary>
        /// Audio channel count
        /// </summary>
        [Doc(@"ChannelCount")]
        public int channelCount;
        /// <summary>
        /// Default microphone format
        /// </summary>
        [Doc(@"FormatDefault")]
        public static Format Default { get { return new Format {
            sampleRate = AudioSettings.outputSampleRate,
            channelCount = 1
        };}}
        /// <summary>
        /// Default microphone format for audio mixing
        /// </summary>
        [Doc(@"FormatDefaultForMixing", @"FormatDefaultForMixingDescription")]
        public static Format DefaultForMixing { get { return new Format {
            sampleRate = AudioSettings.outputSampleRate,
            channelCount = (int)AudioSettings.speakerMode
        };}}

        public override string ToString () {
            return "{ " + string.Format("{0}@{1}Hz", channelCount, sampleRate) + " }";
        }
        public bool Equals (Format other) {
            return other.channelCount == channelCount && other.sampleRate == sampleRate;
        }
        public override int GetHashCode () {
            return sampleRate ^ channelCount;
        }
        public override bool Equals (object obj) {
            return (obj is Format) && this.Equals((Format)obj);
        }
        public static bool operator == (Format lhs, Format rhs) {
            return lhs.Equals(rhs);
        }
        public static bool operator != (Format lhs, Format rhs) {
            return !lhs.Equals(rhs);
        }
    }
    #endregion
}