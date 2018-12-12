/* 
*   NatMic
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatMicU.Examples {

    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;
    using Core;
    using Core.Recorders;

    public class VoiceRecorder : MonoBehaviour {
        
        public AudioSource audioSource;
        private IRecorder recorder;

        public void ToggleRecording (Text buttonText) { // Invoked by UI
            if (!NatMic.IsRecording) {
                // Start recording
                NatMic.StartRecording(Format.Default, OnSampleBuffer);
                buttonText.text = @"Stop Recording";
            } else {
                // Stop recording
                NatMic.StopRecording();
                buttonText.text = @"Start Recording";
            }
        }

        private void OnSampleBuffer (AudioEvent audioEvent, float[] sampleBuffer, long timestamp, Format format) {
            switch (audioEvent) {
                case AudioEvent.OnInitialize:
                    // Create a WAV recorder to record the audio to a file
                    recorder = new WAVRecorder(format);
                    recorder.StartRecording(OnAudioRecording);
                    break;
                case AudioEvent.OnSampleBuffer:
                    // Commit the sample buffer to the WAV recorder
                    recorder.CommitSamples(sampleBuffer, timestamp);
                    break;
                case AudioEvent.OnFinalize:
                    // Stop recording the WAV file and dispose the recorder
                    recorder.Dispose();
                    break;
            }
        }

        private void OnAudioRecording (string path) {
            // Log the path
            Debug.Log("Saved recording to: "+path);
            // Play the recording in the scene
            StartCoroutine(PlayRecording(path));
        }

        private IEnumerator PlayRecording (string path) {
            // Load the recording as a clip
            var www = new WWW("file://" + path);
            yield return www;
            var audioClip = www.GetAudioClip();
            // Play it
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }
}