# NatMic API
NatMic is a low-latency native microphone API for Unity Engine. 
NatMic provides a minimal API for streaming audio data directly 
from the microphone to Unity. 

NatMic's features include:
+ Low-latency microphone recording on dedicated audio threads.
+ Control microphone format like sample rate.
+ Mix microphone audio with game audio.
+ Record to audio files, currently supporting recording to WAV files.
+ Record on iOS, Android, macOS, and Windows.

## Fundamentals of Recording
NatMic works by forwarding microphone events and accompanying data to a provided callback. 
When a microphone event is raised, like the microphone starting or a new sample buffer being available, 
NatMic will invoke the callback with the `AudioEvent`, 
the sample buffer, the sample buffer's timestamp, and the current `Format` of the microphone:
```csharp
void OnSampleBuffer (AudioEvent audioEvent, float[] sampleBuffer, long timestamp, Format format) {
    // This callback is invoked on the NatMic microphone thread
    // Use the sample buffer here
}
```

The `AudioEvent` provides information regarding what event the callback is being invoked for:
```csharp
enum AudioEvent {
    // The microphone has started
    OnInitialize = 1,
    // The microphone has reported a new sample buffer
    OnSampleBuffer = 2,
    // The microphone has stopped
    OnFinalize = 3
}
```

The `Format` provides information about the microphone's current audio format:
```csharp
struct Format {
    // Audio sample rate
    public int sampleRate;
    // Audio channel count
    public int channelCount;
}
```

You can start recording by calling the `StartRecording` function, 
passing in a preferred microphone format and a `SampleBufferCallback`:
```csharp
NatMic.StartRecording(Format.Default, OnSampleBuffer);
```

NatMic will then invoke the callback repeatedly with microphone data until `StopRecording` is called:
```csharp
IEnumerator Start () {
    // Start the microphone
    NatMic.StartRecording(Format.Default, OnSampleBuffer);
    // Wait for ten seconds
    yield return new WaitForSeconds(10f);
    // Stop the microphone
    NatMic.StopRecording();
}

void OnSampleBuffer (AudioEvent audioEvent, float[] sampleBuffer, long timestamp, Format format) {
    switch (audioEvent) {
        case AudioEvent.OnInitialize: break;    // Do stuff...
        case AudioEvent.OnSampleBuffer: break;  // Do stuff...
        case AudioEvent.OnFinalize: break;      // Do stuff...
    }
}
```

## Recording to Audio Files
NatMic allows you to record microphone audio to audio files. 
To record audio to a file, you will use instances of the `IRecorder` interface:
```csharp
public interface IRecorder {
    // Start recording
    void StartRecording (RecordingCallback callback);
    // Commit audio data to be written to an audio file
    void CommitSamples (float[] samples, long timestamp);
    // Stop recording and invoke the RecordingCallback
    void Dispose ();
}
```

Currently, NatMic supports recording to `.wav` files. 
We expect to add support for `m4a` in a later release, and potentially `.mp3`. 
Here is an example showing how to record the mic to a WAVE file:
```csharp
// Callback invoked by NatMic
void OnSampleBuffer (AudioEvent audioEvent, float[] sampleBuffer, long timestamp, Format format) {
    switch (audioEvent) {
        case AudioEvent.OnInitialize:
            // Create a WAV recorder to record the audio to a file
            recorder = new WAVRecorder(format);
            // Start the recorder, passing a callback to be invoked once recording is complete
            recorder.StartRecording(OnWAVRecording);
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

void OnWAVRecording (string path) {
    // Use the recorded WAV file at `path`
}
```

## Recording with Game Audio
NatMic is able to provide the microphone audio data mixed with game audio. 
This is useful for doing things like voice overs or player commentary over gameplay. 
The workflow is largely the same: Simply call `NatMic.StartRecording`, 
passing in an `AudioSource` or an `AudioListener` to use for audio overlay on the microphone audio:
```csharp
public AudioListener audioListener; // This listener can 'hear' all game audio
...
NatMic.StartRecording(audioListener, Format.DefaultWithMixing, OnSampleBuffer);
...
void OnSampleBuffer (AudioEvent audioEvent, float[] sampleBuffer, long timestamp, Format format) {
    // `sampleBuffer` contains microphone audio overlaid with scene audio from the AudioListener
}
```

## Using NatMic with NatCorder
NatMic is built with NatCorder integration in mind. 
A common use case would be recording a video with NatCorder, and adding microphone audio with NatMic. 
To achieve this, simply send NatMic's microphone sample buffer to NatCorder's `CommitSamples` function:
```csharp
public void StartRecording () {
    // Start the microphone with NatMic
    var microphoneFormat = Format.Default;
    NatMic.StartRecording(microphoneFormat, OnSampleBuffer);
    // Start recording with NatCorder
    var audioFormat = new AudioFormat(microphoneFormat.sampleRate, microphoneFormat.channelCount);
    NatCorder.StartRecording(Container.MP4, VideoFormat.Screen, audioFormat, OnRecording);
    ...
}

// Invoked by NatMic on new microphone events
private void OnSampleBuffer (AudioEvent audioEvent, float[] sampleBuffer, long timestamp, Format format) {
    // Send sample buffers directly to NatCorder for recording
    if (audioEvent == AudioEvent.OnSampleBuffer)
        NatCorder.CommitSamples(sampleBuffer, clock.CurrentTimestamp);
}
```

Note that when `CommitSamples` is called, we use a NatCorder clock's 
timestamp instead of passing in the timestamp from NatMic (the `timestamp` argument in `OnSampleBuffer`). 
This is the case because NatCorder expects that all timestamps are on its own clock, 
whereas NatMic's timestamps are on a different clock.

Check out the [NatMicCorder Demo](https://github.com/olokobayusuf/NatMicCorder-Demo) 
for a full NatMic-NatCorder integration example.

## Tutorials
- [Unity Microphone That Works]
(https://medium.com/@olokobayusuf/natmic-api-unity-microphone-that-works-264d2b73cfa8)

## Requirements
- On Android, NatMic requires API Level 18 and up
- On iOS, NatMic requires iOS 7 and up
- On macOS, NatMic requires macOS 10.13 and up
- On Windows, NatMic requires Windows 8 and up

## Quick Tips
- Please peruse the included scripting reference [here]
	(https://olokobayusuf.github.io/NatMic-Docs/)
- To discuss or report an issue, visit Unity forums [here]
	(https://forum.unity.com/threads/natmic-native-microphone-api.431677/)
- Contact me at [olokobayusuf@gmail.com](mailto:olokobayusuf@gmail.com)

Thank you very much!