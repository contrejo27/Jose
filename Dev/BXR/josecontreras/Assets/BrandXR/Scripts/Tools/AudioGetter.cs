/* AudioGetter.cs
 * 
 * Attach to an AudioListener or AudioSource to recieve the OnAudioFilterRead() method, 
 * which we send to a linked BlockEventRecorder.cs script to commit the audio to a video recording
 */ 
using UnityEngine;
using System.Collections;

#if NATCORDER
using NatCorderU.Core;
#endif

namespace BrandXR
{
    public class AudioGetter: MonoBehaviour
    {
        //Reference to the BlockEventRecorder.cs script we want to send our audio samples to during recording
        private BlockEventRecorder blockEventRecorder = null;

        //------------------------------------------------------//
        /// <summary>
        /// Sends audio samples to the BlockEventRecorder every OnAudioFilterRead() method
        /// </summary>
        /// <param name="recorder">The BlockEventRecorder that should recieve the audio samples</param>
        public void StartRecordingAudio( BlockEventRecorder recorder )
        //------------------------------------------------------//
        {

            //Remove any previous links to BlockEventRecorder's
            blockEventRecorder = null;

            //Store the BlockEventRecorder, when OnAudioFilterRead() is called, we will pass in our audio data to this BlockEventRecorder
            if( recorder != null )
            {
                blockEventRecorder = recorder;
            }

            /*
            if( this.gameObject.GetComponent<AudioListener>() != null )
            {
                Debug.Log( "AudioGetter.cs StartRecordingAudio() found AudioListener component on this gameObject.name = " + name );
            }
            else
            {
                Debug.Log( "AudioGetter.cs StartRecordingAudio() could not find AudioListener component on this gameObject.name = " + name );
            }
            */

        } //END StartRecordingAudio

        //-----------------------------------------------//
        public void StopRecordingAudio()
        //-----------------------------------------------//
        {

            blockEventRecorder = null;

        } //END StopRecordingAudio

        //--------------------------------------------------------------//
        /// <summary>
        /// Called automatically by Unity Editor, passes in the audio data this AudioListener/AudioSource is hearing/sending
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="channels"></param>
        public void OnAudioFilterRead( float[] samples, int channels )
        //--------------------------------------------------------------//
        {

#if NATCORDER
            //If the BlockEventRecorder reference exists, send the samples
            if( blockEventRecorder != null )
            {
                blockEventRecorder.InternalAudioSampleRecieved( samples, Frame.CurrentTimestamp );
            }
#endif

        } //END OnAudioFilterRead

    } //END AudioGetter

} //END Namespace