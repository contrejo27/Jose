using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if NATSHARE
using NatShareU;
#endif

namespace BrandXR
{
    public class BlockEventNativeSharing : BlockEventBase
    {

        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            ShareTexture,
            ShareAssetAtPath,
            ShareAssetAtBlockEventRecorder,
            SaveToCameraRoll,
            RequestVideoThumbnail,
            ClearThumbnails
        }

        [Button("Add Necessary Scripting Define Symbols", ButtonSizes.Large), ShowIf("ShowScriptingDefineSymbolWarning"), InfoBox("WARNING: This script requires both the NATSHARE scripting define symbol be defined in your Unity Project Settings and that the NatShare plugin be in your Assets folder to work properly", InfoMessageType.Error)]
        public void AddScriptingDefineSymbols()
        {
#if UNITY_EDITOR && !NATSHARE
            string newDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup
                ( BuildPipeline.GetBuildTargetGroup( EditorUserBuildSettings.activeBuildTarget ) );

            if( !newDefineSymbols.Contains( "NATSHARE" ) )
            {
                //Debug.Log( "Define symbols = " + newDefineSymbols );
                newDefineSymbols += ";NATSHARE";
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildPipeline.GetBuildTargetGroup
                ( EditorUserBuildSettings.activeBuildTarget ), newDefineSymbols );
#endif
        }

        private bool ShowScriptingDefineSymbolWarning()
        {
#if NATSHARE && UNITY_EDITOR
            if( AssetDatabase.IsValidFolder("Assets/NatShare") )
            {
                return false;
            }
            else
            {
                return true;
            }
#else
            return true;
#endif
        }

        [TitleGroup("Block Event - Native Sharing", "Used to call native sharing events. Requires NATSHARE scripting define symbol in project settings")]
        public Actions action = Actions.ShareTexture;
        private bool IsActionShare() { return action == Actions.ShareTexture || action == Actions.ShareAssetAtPath || action == Actions.ShareAssetAtBlockEventRecorder; }

        //----------------- "SHARE TEXTURE" VARIABLES ------------------------------//
        [Space(15f), ShowIf("action", Actions.ShareTexture)]
        public Texture textureToShare = null;

        //----------------- "SHARE ASSET PATH" VARIABLES ------------------------------//
        [Space(15f), ShowIf("action", Actions.ShareAssetAtPath)]
        public string assetPathToShare = "";


        //----------------- "SHARE BLOCK EVENT RECORDER" VARIABLES ------------------------------//
        [Space(15f), ShowIf("action", Actions.ShareAssetAtBlockEventRecorder)]
        public BlockEventRecorder blockEventRecorder = null;

        public enum AssetType
        {
            Video,
            Texture
        }
        [Space(15f), ShowIf("action", Actions.ShareAssetAtBlockEventRecorder)]
        public AssetType assetType = AssetType.Video;

        public enum RecorderShareType
        {
            ShareLatest,
            ShareFirst
        }
        [Space(15f), ShowIf("action", Actions.ShareAssetAtBlockEventRecorder)]
        public RecorderShareType recorderShareType = RecorderShareType.ShareLatest;

        [Space(15f), Tooltip("You can enter a message to show when sharing"), ShowIf("IsActionShare")]
        public string messageToShow = "";

        //----------------- "SAVE TO CAMERA ROLL" VARIABLES ------------------------------//
        public enum SaveToCameraRollType
        {
            Texture,
            AssetPath,
            BlockEventExternalCamera,
            BlockEventRecorder
        }
        [Space(15f), ShowIf("action", Actions.SaveToCameraRoll)]
        public SaveToCameraRollType saveToCameraRollType = SaveToCameraRollType.Texture;
        private bool SaveToCameraRollTypeTexture() { return action == Actions.SaveToCameraRoll && saveToCameraRollType == SaveToCameraRollType.Texture; }
        private bool SaveToCameraRollTypePath() { return action == Actions.SaveToCameraRoll && saveToCameraRollType == SaveToCameraRollType.AssetPath; }
        private bool SaveToCameraRollTypeBlockEventExternalCamera() { return action == Actions.SaveToCameraRoll && saveToCameraRollType == SaveToCameraRollType.BlockEventExternalCamera; }
        private bool SaveToCameraRollTypeBlockEventRecorder() { return action == Actions.SaveToCameraRoll && saveToCameraRollType == SaveToCameraRollType.BlockEventRecorder; }

        [Space(15f), ShowIf("SaveToCameraRollTypeTexture")]
        public Texture textureToSave = null;

        [Space(15f), ShowIf("SaveToCameraRollTypePath")]
        public string assetPathToSave = "";


        [Space(15f), ShowIf("SaveToCameraRollTypeBlockEventExternalCamera")]
        public BlockEventExternalCamera blockEventExternalCamera = null;

        public enum SaveExternalCameraTypeToCameraRoll
        {
            CapturedPhoto,
            VideoPreview
        }
        [Space(15f), ShowIf("SaveToCameraRollTypeBlockEventExternalCamera")]
        public SaveExternalCameraTypeToCameraRoll cameraAssetToSaveToRoll = SaveExternalCameraTypeToCameraRoll.CapturedPhoto;
        private bool IsSaveCapturedPhotoToRoll() { return SaveToCameraRollTypeBlockEventExternalCamera() && cameraAssetToSaveToRoll == SaveExternalCameraTypeToCameraRoll.CapturedPhoto; }
        private bool IsSaveCapturedVideoPreviewToRoll() { return SaveToCameraRollTypeBlockEventExternalCamera() && cameraAssetToSaveToRoll == SaveExternalCameraTypeToCameraRoll.VideoPreview; }


        public enum RecorderSaveToCameraRollType
        {
            SaveLatest,
            SaveFirst
        }
        [Space(15f), ShowIf("IsSaveCapturedPhotoToRoll")]
        public RecorderSaveToCameraRollType recorderSaveToRollType = RecorderSaveToCameraRollType.SaveLatest;


        [Space(15f), ShowIf("SaveToCameraRollTypeBlockEventRecorder")]
        public BlockEventRecorder blockEventRecorderToSaveToRoll = null;

        [Space(15f), ShowIf("SaveToCameraRollTypeBlockEventRecorder")]
        public AssetType assetTypeToSaveToRoll = AssetType.Video;

        [Space(15f), ShowIf("SaveToCameraRollTypeBlockEventRecorder")]
        public RecorderSaveToCameraRollType recorderShareToRollType = RecorderSaveToCameraRollType.SaveLatest;


        //----------------- "REQUEST VIDEO THUMBNAIL" VARIABLES ------------------------------//
        public enum VideoWithThumbnailLocationType
        {
            Path,
            BlockEventRecorder
        }
        [Space(15f), ShowIf("action", Actions.RequestVideoThumbnail)]
        public VideoWithThumbnailLocationType thumbnailLocationType = VideoWithThumbnailLocationType.Path;
        private bool IsThumbnailAtPath() { return action == Actions.RequestVideoThumbnail && thumbnailLocationType == VideoWithThumbnailLocationType.Path; }
        private bool IsThumbnailAtBlockEventRecorder() { return action == Actions.RequestVideoThumbnail && thumbnailLocationType == VideoWithThumbnailLocationType.BlockEventRecorder; }

        [Space(15f), ShowIf("IsThumbnailAtPath")]
        public string assetPathOfVideo = "";

        [Space(15f), ShowIf("IsThumbnailAtBlockEventRecorder")]
        public BlockEventRecorder blockEventRecorderWithVideo = null;

        public enum RecorderThumbnailType
        {
            GetThumbnailFromLatestVideo,
            GetThumbnailFromFirstVideo
        }
        [Space(15f), ShowIf("IsThumbnailAtBlockEventRecorder")]
        public RecorderThumbnailType thumbnailType = RecorderThumbnailType.GetThumbnailFromLatestVideo;

        [Space(15f), ShowIf("action", Actions.RequestVideoThumbnail)]
        public float timeInVideoForThumbnail = 0f;

        [Space(15f), ShowIf("action", Actions.RequestVideoThumbnail)]
        public bool showOnColorTweener = true;
        private bool IsShowOnColorTweenerTrue() { return action == Actions.RequestVideoThumbnail && showOnColorTweener; }

        [Space(15f), ShowIf("IsShowOnColorTweenerTrue")]
        public UIColorTweener uiColorTweener = null;

        [Space(15f), ShowIf("action", Actions.RequestVideoThumbnail), InfoBox("WARNING: Thumbnails are not automatically garbage collected, to clear up memory use the ClearThumbnails action on another BlockEvent and link it to this script. Or Call ClearThumbnails() on this script.")]
        public List<Texture> thumbnails = new List<Texture>();


        //----------------- "CLEAR THUMBNAILS" VARIABLES ------------------------------//
        public enum ClearThumbnailsType
        {
            ClearAll,
            ClearOnSpecificBlock
        }
        [Space(15f), ShowIf("action", Actions.ClearThumbnails)]
        public ClearThumbnailsType clearThumbnailsType = ClearThumbnailsType.ClearAll;
        private bool ClearThumbnailsFromSpecificBlock() { return action == Actions.ClearThumbnails && clearThumbnailsType == ClearThumbnailsType.ClearOnSpecificBlock; }

        [Space(15f), Tooltip("The BlockEventNativeSharing script to clear any loaded thumbnails from"), ShowIf("ClearThumbnailsFromSpecificBlock")]
        public BlockEventNativeSharing blockEventNativeSharing = null;


        //----------------- "REQUEST VIDEO THUMBNAIL" EVENTS ------------------------------//

        [Serializable]
        public class OnThumbnailFound : UnityEvent<Texture2D> { }

        [SerializeField, ShowIf("action", Actions.RequestVideoThumbnail), FoldoutGroup("Event Messages")]
        public OnThumbnailFound onThumbnailReady = new OnThumbnailFound();

        //----------------- EVENTS ------------------------------//
        private bool ShowOnActionCompletedEvent() { return action != Actions.None && action != Actions.RequestVideoThumbnail; }

        [Space(15f), ShowIf("ShowOnActionCompletedEvent"), FoldoutGroup("Event Messages")]
        public UnityEvent onActionCompleted = new UnityEvent();



        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.NativeSharing;

        } //END GetEventType

        //---------------------------------------------------------//
        protected override void Start()
        //---------------------------------------------------------//
        {
            base.Start();

            showDebug = true;

        } //END Start

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

            if (action == Actions.ShareTexture)
            {
                if (textureToShare != null)
                {
                    eventReady = true;
                }
            }
            else if (action == Actions.ShareAssetAtPath)
            {
                if (assetPathToShare != "")
                {
                    eventReady = true;
                }
            }
            else if (action == Actions.ShareAssetAtBlockEventRecorder)
            {
                if (blockEventRecorder != null)
                {
                    eventReady = true;
                }
            }
            else if (action == Actions.SaveToCameraRoll)
            {
                if (SaveToCameraRollTypeTexture() && textureToSave != null)
                {
                    eventReady = true;
                }
                else if (SaveToCameraRollTypePath() && assetPathToSave != null)
                {
                    eventReady = true;
                }
                else if (SaveToCameraRollTypeBlockEventExternalCamera() && blockEventExternalCamera != null)
                {
                    eventReady = true;
                }
                else if (SaveToCameraRollTypeBlockEventRecorder() && blockEventRecorderToSaveToRoll != null)
                {
                    eventReady = true;
                }
            }
            else if (action == Actions.RequestVideoThumbnail)
            {
                if (IsThumbnailAtPath() && assetPathOfVideo != null && assetPathOfVideo != "")
                {
                    eventReady = true;
                }
                else if (IsThumbnailAtBlockEventRecorder() && blockEventRecorderWithVideo != null)
                {
                    eventReady = true;
                }
            }
            else if (action == Actions.ClearThumbnails)
            {
                if (clearThumbnailsType == ClearThumbnailsType.ClearAll)
                {
                    eventReady = true;
                }
                if (clearThumbnailsType == ClearThumbnailsType.ClearOnSpecificBlock)
                {
                    if (blockEventNativeSharing != null)
                    {
                        eventReady = true;
                    }
                }
            }

        } //END PrepareEvent

        //-------------------------------//
        protected override void _CallEvent()
        //-------------------------------//
        {
            base._CallEvent();

            if (eventReady)
            {
                if (action == Actions.ShareTexture)
                {
                    CallShareTextureEvent();
                }
                else if (action == Actions.ShareAssetAtPath)
                {
                    CallShareAssetAtPathEvent();
                }
                else if (action == Actions.ShareAssetAtBlockEventRecorder)
                {
                    CallShareBlockEventRecorder();
                }
                else if (action == Actions.SaveToCameraRoll)
                {
                    CallSaveToCameraRollEvent();
                }
                else if (action == Actions.RequestVideoThumbnail)
                {
                    CallRequestVideoThumbnailEvent();
                }
                else if (action == Actions.ClearThumbnails)
                {
                    CallClearThumbnailsEvent();
                }
            }

        } //END CallEvent

        //------------------------------//
        private void CallShareTextureEvent()
        //------------------------------//
        {
#if NATSHARE
            if( textureToShare != null )
            {
                NatShare.ShareImage( textureToShare as Texture2D, messageToShow );

                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }
#else
            Debug.LogError("BlockEventNativeSharing.cs CallShareTextureEvent() ERROR: Missing NATSHARE scripting define symbol under project settings");
#endif

        } //END CallShareTextureEvent

        //-----------------------------//
        private void CallShareAssetAtPathEvent()
        //-----------------------------//
        {

#if NATSHARE
            if( assetPathToShare != "" )
            {
                NatShare.ShareMedia( assetPathToShare, messageToShow );

                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }
#else
            Debug.LogError("BlockEventNativeSharing.cs CallShareAssetAtPathEvent() ERROR: Missing NATSHARE scripting define symbol under project settings");
#endif

        } //END CallShareAssetAtPathEvent

        //-----------------------------//
        private void CallShareBlockEventRecorder()
        //-----------------------------//
        {

#if NATSHARE
            if( blockEventRecorder != null )
            {
                if( assetType == AssetType.Video )
                {
                    if( blockEventRecorder.recordedVideos != null && blockEventRecorder.recordedVideos.Count > 0 )
                    {
                        if( recorderShareType == RecorderShareType.ShareLatest )
                        {
                            NatShare.ShareMedia( blockEventRecorder.recordedVideos[ blockEventRecorder.recordedVideos.Count - 1 ], messageToShow );
                            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
                        }
                        else if( recorderShareType == RecorderShareType.ShareFirst )
                        {
                            NatShare.ShareMedia( blockEventRecorder.recordedVideos[0], messageToShow );
                            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
                        }
                    }
                }
                else if( assetType == AssetType.Texture )
                {
                    if( blockEventRecorder.capturedPhotos != null && blockEventRecorder.capturedPhotos.Count > 0 )
                    {
                        if( recorderShareType == RecorderShareType.ShareLatest )
                        {
                            NatShare.ShareMedia( blockEventRecorder.capturedPhotos[ blockEventRecorder.capturedPhotos.Count - 1 ], messageToShow );
                            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
                        }
                        else if( recorderShareType == RecorderShareType.ShareFirst )
                        {
                            NatShare.ShareMedia( blockEventRecorder.capturedPhotos[ 0 ], messageToShow );
                            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
                        }
                    }
                }

            }
#else
            Debug.LogError("BlockEventNativeSharing.cs CallShareBlockEventRecorder() ERROR: Missing NATSHARE scripting define symbol under project settings");
#endif

        } //END CallShareBlockEventRecorder

        //-----------------------------//
        private void CallSaveToCameraRollEvent()
        //-----------------------------//
        {

#if NATSHARE
            if( SaveToCameraRollTypeTexture() && textureToSave != null )
            {
                if( NatShare.SaveToCameraRoll( textureToSave as Texture2D ) )
                {
                    if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
                }
            }
            else if( SaveToCameraRollTypePath() && assetPathToSave != null && assetPathToSave != "" )
            {
                if( NatShare.SaveToCameraRoll( assetPathToSave ) )
                {
                    if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
                }
            }
            else if( SaveToCameraRollTypeBlockEventExternalCamera() && blockEventExternalCamera != null )
            {
                //Tell the BlockEventExternalCamera that we want to send any photos it captures to the device's camera roll
                if( IsSaveCapturedPhotoToRoll() )
                {
                    blockEventExternalCamera.RequestCapturedPhoto( this );
                }

                //Tell the BlockEventExternalCamera that we want to send the video preview to the device's camera roll
                else if( IsSaveCapturedVideoPreviewToRoll() )
                {
                    if( cameraAssetToSaveToRoll == SaveExternalCameraTypeToCameraRoll.VideoPreview &&
                         blockEventExternalCamera.GetVideoPreview() != null )
                    {
                        if( NatShare.SaveToCameraRoll( blockEventExternalCamera.GetVideoPreview() as Texture2D ) )
                        {
                            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
                        }
                    }
                }
            }
            else if( SaveToCameraRollTypeBlockEventRecorder() && blockEventRecorderToSaveToRoll != null )
            {
                if( showDebug ) { Debug.Log( "BlockEventNativeSharing.cs CallSaveToCameraRoll() At start of trying to save to camera roll. AssetType = " + assetTypeToSaveToRoll + ", capturedPhotos.Length = " + blockEventRecorderToSaveToRoll.capturedPhotos.Count + ", capturedVideos.Length = " + blockEventRecorderToSaveToRoll.recordedVideos.Count ); }

                //If we're trying to save an asset from a Recorder to the camera roll
                if( assetTypeToSaveToRoll == AssetType.Texture &&
                    blockEventRecorderToSaveToRoll.capturedPhotos != null && blockEventRecorderToSaveToRoll.capturedPhotos.Count > 0 )
                {
                    if( recorderShareToRollType == RecorderSaveToCameraRollType.SaveFirst )
                    {
                        if( NatShare.SaveToCameraRoll( blockEventRecorderToSaveToRoll.capturedPhotos[ 0 ] ) )
                        {
                            if( showDebug ){ Debug.Log( "BlockEventNativeSharing.cs CallSaveToCameraRoll() Photo " + recorderShareToRollType + " Succesfully saved Photo! " + blockEventRecorderToSaveToRoll.capturedPhotos[ 0 ] ); }
                            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
                        }
                        else
                        {
                            if( showDebug ) { Debug.Log( "BlockEventNativeSharing.cs CallSaveToCameraRoll() Photo " + recorderShareToRollType + " Failed!" ); }
                        }
                    }
                    else if( recorderShareToRollType == RecorderSaveToCameraRollType.SaveLatest )
                    {
                        if( NatShare.SaveToCameraRoll( blockEventRecorderToSaveToRoll.capturedPhotos[ blockEventRecorderToSaveToRoll.capturedPhotos.Count - 1 ] ) )
                        {
                            if( showDebug ) { Debug.Log( "BlockEventNativeSharing.cs CallSaveToCameraRoll() Photo " + recorderShareToRollType + " Success! " + blockEventRecorderToSaveToRoll.capturedPhotos[ blockEventRecorderToSaveToRoll.capturedPhotos.Count - 1 ] ); }
                            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
                        }
                        else
                        {
                            if( showDebug ) { Debug.Log( "BlockEventNativeSharing.cs CallSaveToCameraRoll() Photo " + recorderShareToRollType + " Failed!" ); }
                        }
                    }
                }

                //Otherwise if we're trying to save a recorded video to the camera roll
                else if( assetTypeToSaveToRoll == AssetType.Video &&
                    blockEventRecorderToSaveToRoll.recordedVideos != null && blockEventRecorderToSaveToRoll.recordedVideos.Count > 0 )
                {
                    if( recorderShareToRollType == RecorderSaveToCameraRollType.SaveFirst )
                    {
                        if( NatShare.SaveToCameraRoll( blockEventRecorderToSaveToRoll.recordedVideos[ 0 ] ) )
                        {
                            if( showDebug ) { Debug.Log( "BlockEventNativeSharing.cs CallSaveToCameraRoll() Video " + recorderShareToRollType + " Success! " + blockEventRecorderToSaveToRoll.recordedVideos[ 0 ] ); }
                            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
                        }
                        else
                        {
                            if( showDebug ) { Debug.Log( "BlockEventNativeSharing.cs CallSaveToCameraRoll() Video " + recorderShareToRollType + " Failed!" ); }
                        }
                    }
                    else if( recorderShareToRollType == RecorderSaveToCameraRollType.SaveLatest )
                    {
                        if( NatShare.SaveToCameraRoll( blockEventRecorderToSaveToRoll.recordedVideos[ blockEventRecorderToSaveToRoll.recordedVideos.Count - 1 ] ) )
                        {
                            if( showDebug ) { Debug.Log( "BlockEventNativeSharing.cs CallSaveToCameraRoll() Video " + recorderShareToRollType + " Success!" + blockEventRecorderToSaveToRoll.recordedVideos[ blockEventRecorderToSaveToRoll.recordedVideos.Count - 1 ] ); }
                            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
                        }
                        else
                        {
                            if( showDebug ) { Debug.Log( "BlockEventNativeSharing.cs CallSaveToCameraRoll() Video " + recorderShareToRollType + " Failed!" ); }
                        }
                    }
                }
            }
#else
            Debug.LogError("BlockEventNativeSharing.cs CallSaveToCameraRollEvent() ERROR: Missing NATSHARE scripting define symbol under project settings");
#endif

        } //END CallSaveToCameraRollEvent

        //-----------------------------//
        public void SavePhotoToCameraRoll()
        //-----------------------------//
        {
#if NATSHARE
            if( blockEventExternalCamera != null )
            {
                //If we're trying to save the photo from the external camera straight to the camera roll 
                //( instead of to the local storage path in the app using the BlockEventRecorder)
                if( cameraAssetToSaveToRoll == SaveExternalCameraTypeToCameraRoll.CapturedPhoto &&
                    blockEventExternalCamera.capturedPhotos != null && blockEventExternalCamera.capturedPhotos.Count > 0 )
                {
                    if( recorderSaveToRollType == RecorderSaveToCameraRollType.SaveFirst )
                    {
                        if( NatShare.SaveToCameraRoll( blockEventExternalCamera.capturedPhotos[ 0 ] ) )
                        {
                            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
                        }
                    }
                    else if( recorderSaveToRollType == RecorderSaveToCameraRollType.SaveLatest )
                    {
                        if( NatShare.SaveToCameraRoll( blockEventExternalCamera.capturedPhotos[ blockEventExternalCamera.capturedPhotos.Count - 1 ] ) )
                        {
                            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
                        }
                    }
                }
            }
#else
            Debug.LogError("BlockEventNativeSharing.cs SavePhotoToCameraRoll() ERROR: Missing NATSHARE scripting define symbol in project settings");
            return;
#endif

        } //END SavePhotoToCameraRoll

        //-----------------------------//
        private void CallRequestVideoThumbnailEvent()
        //-----------------------------//
        {

#if NATSHARE
            Texture2D thumbnail = null;

            if( IsThumbnailAtPath() && assetPathOfVideo != null && assetPathOfVideo != "" )
            {
                thumbnail = NatShare.GetThumbnail( assetPathOfVideo, timeInVideoForThumbnail );
            }
            else if( IsThumbnailAtBlockEventRecorder() && blockEventRecorderWithVideo != null )
            {
                if( blockEventRecorderWithVideo.recordedVideos != null && blockEventRecorderWithVideo.recordedVideos.Count > 0 )
                {
                    if( thumbnailType == RecorderThumbnailType.GetThumbnailFromLatestVideo )
                    {
                        thumbnail = NatShare.GetThumbnail( blockEventRecorderWithVideo.recordedVideos[ blockEventRecorderWithVideo.recordedVideos.Count - 1 ], timeInVideoForThumbnail );
                    }
                    else if( thumbnailType == RecorderThumbnailType.GetThumbnailFromFirstVideo )
                    {
                        thumbnail = NatShare.GetThumbnail( blockEventRecorderWithVideo.recordedVideos[0], timeInVideoForThumbnail );
                    }
                }
                
            }
            
            if( thumbnail != null )
            {
                RequestThumbnailComplete( thumbnail );
            }

#else
            Debug.LogError("BlockEventNativeSharing.cs CallRequestVideoThumbnailEvent() ERROR: Missing NATSHARE scripting define symbol under project settings");
#endif

        } //END CallRequestVideoThumbnailEvent

        //------------------------------------//
        private void RequestThumbnailComplete(Texture thumbnail)
        //------------------------------------//
        {

            if (thumbnails == null) { thumbnails = new List<Texture>(); }

            if (showOnColorTweener && uiColorTweener != null)
            {
                uiColorTweener.SetTexture(thumbnail);
            }

            //if( onThumbnailReady != null ) { onThumbnailReady.Invoke( thumbnail ); }

        } //END RequestThumbnailComplete

        //-----------------------------//
        private void CallClearThumbnailsEvent()
        //-----------------------------//
        {

            if (clearThumbnailsType == ClearThumbnailsType.ClearAll)
            {
                //If we have more than one BlockEventNativeSharing...
                if (GameObject.FindObjectsOfType<BlockEventNativeSharing>() != null && GameObject.FindObjectsOfType<BlockEventNativeSharing>().Length > 1)
                {
                    List<BlockEventNativeSharing> blocks = GameObject.FindObjectsOfType<BlockEventNativeSharing>().ToList();

                    foreach (BlockEventNativeSharing block in blocks)
                    {
                        block.ClearThumbnails();
                    }

                    if (onActionCompleted != null) { onActionCompleted.Invoke(); }
                }
                //Otherwise just clear the thumbnails on this script
                else
                {
                    ClearThumbnails();
                    if (onActionCompleted != null) { onActionCompleted.Invoke(); }
                }
            }
            else if (clearThumbnailsType == ClearThumbnailsType.ClearOnSpecificBlock)
            {
                if (blockEventNativeSharing != null)
                {
                    blockEventNativeSharing.ClearThumbnails();
                    if (onActionCompleted != null) { onActionCompleted.Invoke(); }
                }
            }

        } //END CallClearThumbnailsEvent

        //-----------------------------------//
        private void ClearThumbnails()
        //-----------------------------------//
        {

            if (thumbnails != null && thumbnails.Count > 0)
            {
                foreach (Texture tex in thumbnails)
                {
                    if (tex != null)
                    {
                        Texture.Destroy(tex);
                    }
                }
            }

        } //END ClearThumbnails

    } //END BlockEventUnity

} //END Namespace