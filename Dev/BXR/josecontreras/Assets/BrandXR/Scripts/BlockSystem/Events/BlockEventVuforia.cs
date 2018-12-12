using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if VUFORIA
using Vuforia;
#endif

namespace BrandXR
{
    public class BlockEventVuforia : BlockEventBase
    {

        #region WARNINGS
        [ShowIf("ShouldShowVuforiaDisabledWarning"), InfoBox("WARNING: 'VUFORIA' scripting define symbol missing in project settings. Please select the XRTechnologyManager and set your XR technology to use either 'VUFORIA' or 'VUFORIA_FUSION'", InfoMessageType.Error)]
        public int missingVuforiaScriptingDefineSymbol = 0;

        //-----------------------------------------------//
        private bool ShouldShowVuforiaDisabledWarning()
        //-----------------------------------------------//
        {
#if !VUFORIA
            if( GameObject.FindObjectOfType<XRTechnologyManager>() != null )
            {
                return true;
            }
#endif

            return false;
        }
        #endregion

        #region ACTIONS
        public enum Actions
        {
            None,
            SetFusionProvider,
            GetActiveFusionProvider
        }
        [TitleGroup("Block Event : Vuforia", "Sets or retrives settings for the Vuforia XR API")]
        public Actions action = Actions.None;
        private bool IsActionSetFusionProvider() { return action == Actions.SetFusionProvider; }
        private bool IsActionGetActiveFusionProvider() { return action == Actions.GetActiveFusionProvider; }

        #endregion

        #region SET FUSION PROVIDER VARIABLES

        //-------------------------- SET FUSION PROVIDER VARIABLES ------------------------//

#if VUFORIA
        [SerializeField, Space( 15f ), ShowIf("IsActionSetFusionProvider"), InfoBox("Use this action to set the Vuforia Fusion tracking technology that will be used in the project.\n\n" +
            "You can only call this action when Vuforia has yet to initialize.\n\n" +
            "It is recommended that you leave this option to 'All'")]
        FusionProviderType fusionProviderType = FusionProviderType.ALL;
        
#endif

#endregion

        #region GET ACTIVE FUSION PROVIDER VARIABLES
        //-------------------------- GET ACTIVE FUSION PROVIDER VARIABLES -----------------//
        [Space( 15f ), ShowIf("IsActionGetActiveFusionProvider"),
            InfoBox("Use this action once Vuforia has Initialized to determine what XR technology the API is using. You will recieve one of the following responses in the event message\n\n" +
            "INVALID_OPERATION : Returned when you try to ask for the Fusion Provider before Vuforia has initialized.\n\n" +
            "VUFORIA_VISION_ONLY : No access to ARCore/ARKit or other hardware based space tracking device API. Using older, less accurate vision logic for tracking with limited API.\n\n" +
            "VUFORIA_SENSOR_FUSION : Hardware enabled Vuforia space tracking API enabled. More accurate than VUFORIA_VISION_ONLY, but less accurate than PLATFORM_SENSOR_FUSION.\n\n" +
            "ALL : The default option. Vuforia has been directed to try to figure out the best combination of technology to use for the device.\n\n" +
            "OPTIMIZE_IMAGE_TARGETS_AND_VUMARKS : Optimized to look for Image Targets, you may experience less than optimal results with space tracking while using this.\n\n" +
            "OPTIMIZE_MODEL_TARGETS_AND_SMART_TERRAIN : Optimized for space tracking, you may experience less than optimal results with image tracking while using this.")]
        public int getActiveFusionMessage = 0;

        #endregion
        
        #region GET ACTIVE FUSION PROVIDER EVENTS
        //-------------------------- GET ACTIVE FUSION PROVIDER EVENTS --------------------//
#if VUFORIA
        private bool ShowGetActiveFusionProviderEvents() { return IsActionGetActiveFusionProvider(); }

        [Serializable]
        public class SendFusionProviderType : UnityEvent<FusionProviderType>{ };

        [SerializeField, ShowIf("ShowGetActiveFusionProviderEvents"), InfoBox("Send the 'FusionProviderType' variable"), FoldoutGroup("Event Messages")]
        public SendFusionProviderType onFusionProviderTypeFound = new SendFusionProviderType();

        [Serializable]
        public class SendFusionProviderTypeString : UnityEvent<string> { };

        [SerializeField, ShowIf("ShowGetActiveFusionProviderEvents"), InfoBox("Sends the 'FusionProviderType' variable as a string"), FoldoutGroup("Event Messages")]
        public SendFusionProviderTypeString onFusionProviderTypeFoundString = new SendFusionProviderTypeString();
#endif

        #endregion

        #region GENERIC EVENTS
        //-------------------------- GENERIC EVENTS ---------------------------------------//
        private bool ShowGenericEvent() { return IsActionSetFusionProvider(); }

        [ShowIf("ShowGenericEvent"), FoldoutGroup("Event Messages")]
        public UnityEvent onActionCompleted = new UnityEvent();

        #endregion

        #region SETUP
        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.Vuforia;

        } //END GetEventType

        //---------------------------------------------------------//
        public void SetAction(Actions action)
        //---------------------------------------------------------//
        {

            this.action = action;

        } //END SetAction


        //-------------------------------//
        public override void PrepareEvent()
        //-------------------------------//
        {
            if (action == Actions.SetFusionProvider)
            {
#if VUFORIA
                eventReady = true;
#endif
            }
            else if ( action == Actions.GetActiveFusionProvider )
            {
#if VUFORIA
                eventReady = true;
#endif
            }

        } //END PrepareEvent
        #endregion

        #region CALL EVENT
        //-------------------------------//
        protected override void _CallEvent()
        //-------------------------------//
        {
            base._CallEvent();

            if (eventReady)
            {
                if (action == Actions.SetFusionProvider)
                {
                    CallSetFusionProviderEvent();
                }
                else if (action == Actions.GetActiveFusionProvider)
                {
                    CallGetActiveFusionProviderEvent();
                }
            }

        } //END _CallEvent
        #endregion

        #region SET FUSION PROVIDER EVENT
        //---------------------------------//
        private void CallSetFusionProviderEvent()
        //---------------------------------//
        {

#if VUFORIA
            if( !VuforiaManager.Instance.Initialized )
            {
                VuforiaConfiguration.Instance.DeviceTracker.FusionMode = fusionProviderType;

                if (onActionCompleted != null) { onActionCompleted.Invoke(); }
            }
            else
            {
                Debug.LogError("BlockEventVuforia.cs CallSetFusionProviderEvent() ERROR: Unable to Set Fusion Provider, Vuforia has already been initialized. You can only call this action while Vuforia is de-initialized");
            }
#else
            Debug.LogError("BlockEventVuforia.cs CallSetFusionProviderEvent() ERROR: Unable to make calls to Vuforia API. Missing scripting define symbol 'VUFORIA' in Project Settings");
#endif

        } //END CallSetFusionProviderEvent

        #endregion

        #region GET ACTIVE FUSION PROVIDER EVENT
        //---------------------------------//
        private void CallGetActiveFusionProviderEvent()
        //---------------------------------//
        {

#if VUFORIA
            FusionProviderType fusionProviderType = VuforiaRuntimeUtilities.GetActiveFusionProvider();

            if( onFusionProviderTypeFound != null) { onFusionProviderTypeFound.Invoke( fusionProviderType ); }

            if (onFusionProviderTypeFoundString != null) { onFusionProviderTypeFoundString.Invoke( fusionProviderType.ToString() ); }
#else
            Debug.LogError("BlockEventVuforia.cs CallGetActiveFusionProviderEvent() ERROR: Unable to make calls to Vuforia API. Missing scripting define symbol 'VUFORIA' in Project Settings");
#endif
            
        } //END CallGetActiveFusionProviderEvent
        
        #endregion


    } //END Class

} //END Namespace