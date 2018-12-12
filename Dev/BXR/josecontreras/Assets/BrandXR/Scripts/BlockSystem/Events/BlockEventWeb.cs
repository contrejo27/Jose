using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System;

namespace BrandXR
{
    public class BlockEventWeb: BlockEventBase
    {

        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            OpenURL,
            DownloadTexture,
            DownloadAudioClip,
            DownloadText,
            DownloadCSVFile
        }

        [TitleGroup( "Block Event - Web", "Used to call web events" )]
        public Actions action = Actions.None;
        private bool IsActionDownload() { return action == Actions.DownloadTexture || action == Actions.DownloadAudioClip || action == Actions.DownloadText || action == Actions.DownloadCSVFile; }

        //----------------- "OPEN URL" VARIABLES ------------------------------//
        [ShowIf( "action", Actions.OpenURL )]
        public string url = "http://www.brandxr.io";

        //----------------- "DOWNLOAD FILE" VARIABLES ---------------------//
        [Space( 15f ), ShowIf( "IsActionDownload" )]
        public string path = "";
        
        [Space( 15f ), ShowIf( "IsActionDownload" )]
        public WWWHelper.LocationType downloadLocation = WWWHelper.LocationType.TryAllLocations;
        private bool DownloadLocationIsTryAllOrWeb() { return IsActionDownload() && ( downloadLocation == WWWHelper.LocationType.TryAllLocations || downloadLocation == WWWHelper.LocationType.Web ); }

        [Space( 15f ), ShowIf( "DownloadLocationIsTryAllOrWeb" ), InfoBox( "If the file is downloaded from the web, enable this option to save it to the devices Local Storage (aka StreamingAssets path).\n\nWhen downloading the file in the future you can set your 'Download Location' to 'Try All Locations' or 'Web' and it will automatically find and load the cached file if your 'Path' matches the File Name set below" )]
        public bool cacheIfWeb = true;
        private bool ShowFileName() { return DownloadLocationIsTryAllOrWeb() && cacheIfWeb; }

        [Space( 15f ), ShowIf( "ShowFileName" ), InfoBox( "If the file is located on the web, then save the file to the Local Storage with the file name below.\n\nIf left empty, we will use 'Application.productName_FileType' as the name" )]
        public string fileName = "";

        //----------------- "DOWNLOAD TEXT FILE" EVENTS ------------------------------//
        private bool ShowOnTextDownloadCompletedEvent() { return action == Actions.DownloadText; }

        [Serializable]
        public class OnTextDownload: UnityEvent<string> { }

        [SerializeField, ShowIf( "ShowOnTextDownloadCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public OnTextDownload onDownloadTextCompleted = new OnTextDownload();

        //----------------- "DOWNLOAD CSV FILE" EVENTS ------------------------------//
        private bool ShowOnCSVDownloadCompletedEvent() { return action == Actions.DownloadCSVFile; }

        [Serializable]
        public class OnCSVDownload: UnityEvent<CSVData> { }

        [SerializeField, ShowIf( "ShowOnCSVDownloadCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public OnCSVDownload onDownloadCSVCompleted = new OnCSVDownload();

        //----------------- "DOWNLOAD TEXTURE FILE" EVENTS ------------------------------//
        private bool ShowOnTextureDownloadCompletedEvent() { return action == Actions.DownloadTexture; }

        [Serializable]
        public class OnTextureDownload: UnityEvent<Texture> { }

        [SerializeField, ShowIf( "ShowOnTextureDownloadCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public OnTextureDownload onDownloadTextureCompleted = new OnTextureDownload();

        //----------------- "DOWNLOAD AUDIOCLIP FILE" EVENTS ------------------------------//
        private bool ShowOnAudioClipDownloadCompletedEvent() { return action == Actions.DownloadAudioClip; }

        [Serializable]
        public class OnAudioClipDownload: UnityEvent<AudioClip> { }

        [SerializeField, ShowIf( "ShowOnAudioClipDownloadCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public OnAudioClipDownload onDownloadAudioClipCompleted = new OnAudioClipDownload();


        //----------------- "DOWNLOAD" EVENTS ------------------------------//
        private bool ShowOnDownloadFailed() { return action == Actions.DownloadCSVFile; }

        [Space( 15f ), ShowIf( "ShowOnDownloadFailed" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onDownloadFailed = new UnityEvent();

        //----------------- "OPEN URL" EVENTS ------------------------------//
        private bool ShowOnActionCompletedEvent() { return action == Actions.OpenURL; }

        [Space( 15f ), ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted = new UnityEvent();





        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.Web;

        } //END GetEventType

        //---------------------------------------------------------//
        public void SetAction( Actions action )
        //---------------------------------------------------------//
        {

            this.action = action;

        } //END SetAction

        //-------------------------------//
        public override void PrepareEvent()
        //-------------------------------//
        {

            if( action == Actions.OpenURL )
            {
                if( url != "" )
                {
                    eventReady = true;
                }
            }
            else if( action == Actions.DownloadText )
            {
                if( path != "" )
                {
                    eventReady = true;
                }
            }
            else if( action == Actions.DownloadCSVFile )
            {
                if( path != "" )
                {
                    eventReady = true;
                }
            }
            else if( action == Actions.DownloadTexture )
            {
                if( path != "" )
                {
                    eventReady = true;
                }
            }
            else if( action == Actions.DownloadAudioClip )
            {
                if( path != "" )
                {
                    eventReady = true;
                }
            }

        } //END PrepareEvent

        //-------------------------------//
        protected override void _CallEvent()
        //-------------------------------//
        {
            base._CallEvent();

            if( eventReady )
            {
                if( action == Actions.OpenURL )
                {
                    CallOpenURLEvent();
                }
                else if( action == Actions.DownloadText )
                {
                    CallDownloadTextFileEvent();
                }
                else if( action == Actions.DownloadCSVFile )
                {
                    CallDownloadCSVFileEvent();
                }
                else if( action == Actions.DownloadTexture )
                {
                    CallDownloadTextureFileEvent();
                }
                else if( action == Actions.DownloadAudioClip )
                {
                    CallDownloadAudioClipFileEvent();
                }
            }

        } //END CallEvent

        //------------------------------//
        private void CallOpenURLEvent()
        //------------------------------//
        {

            Application.OpenURL( url );

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END CallOpenURLEvent

        //------------------------------//
        private void CallDownloadTextFileEvent()
        //------------------------------//
        {
            bool cacheIfWeb = false;

            if( DownloadLocationIsTryAllOrWeb() )
            {
                cacheIfWeb = this.cacheIfWeb;
            }

            string fileName = Application.productName + "_TXT";

            if( DownloadLocationIsTryAllOrWeb() && this.fileName != "" )
            {
                fileName = this.fileName;
            }

            WWWHelper.instance.GetText( path, cacheIfWeb, TextDownloadSuccess, DownloadFailed, downloadLocation, fileName );

        } //END CallDownloadTextFileEvent

        //-------------------------------//
        private void TextDownloadSuccess( string text )
        //-------------------------------//
        {

            if( text != null && text != "" )
            {
                if( onDownloadTextCompleted != null ) { onDownloadTextCompleted.Invoke( text ); }
            }
            else
            {
                DownloadFailed();
            }

        } //END CSVDownloadSuccess

        //------------------------------//
        private void CallDownloadCSVFileEvent()
        //------------------------------//
        {
            bool cacheIfWeb = false;

            if( DownloadLocationIsTryAllOrWeb() )
            {
                cacheIfWeb = this.cacheIfWeb;
            }

            string fileName = Application.productName + "_CSV";

            if( DownloadLocationIsTryAllOrWeb() && this.fileName != "" )
            {
                fileName = this.fileName;
            }

            WWWHelper.instance.GetCSV( path, cacheIfWeb, CSVDownloadSuccess, DownloadFailed, downloadLocation, fileName );
            
        } //END CallDownloadCSVFileEvent

        //-------------------------------//
        private void CSVDownloadSuccess( string csvText )
        //-------------------------------//
        {

            if( csvText != null && csvText != "" )
            {
                //Create the scriptable object that will store the data of the CSV file
                CSVData csvData = ScriptableObject.CreateInstance<CSVData>();

                //Store the original data
                csvData.originalCSVText = csvText;
                csvData.rawCSV_original = CSVReader.SplitCsvGrid( csvText );

                //Grab the headers from the CSV data
                csvData.headers = CSVReader.GetHeaders( csvData.rawCSV_original );

                //Setup the columns from the headers
                csvData.AddColumnsFromHeaders();

                csvData.InitLength();

                if( onDownloadCSVCompleted != null ) { onDownloadCSVCompleted.Invoke( csvData ); }

            }
            else
            {
                DownloadFailed();
            }
            
        } //END CSVDownloadSuccess

        //------------------------------//
        private void CallDownloadTextureFileEvent()
        //------------------------------//
        {
            bool cacheIfWeb = false;

            if( DownloadLocationIsTryAllOrWeb() )
            {
                cacheIfWeb = this.cacheIfWeb;
            }

            string fileName = Application.productName + "_TEXTURE";

            if( DownloadLocationIsTryAllOrWeb() && this.fileName != "" )
            {
                fileName = this.fileName;
            }

            WWWHelper.instance.GetTexture( path, cacheIfWeb, TextureDownloadSuccess, DownloadFailed, downloadLocation, fileName );

        } //END CallDownloadTextureFileEvent

        //-------------------------------//
        private void TextureDownloadSuccess( Texture texture )
        //-------------------------------//
        {

            if( texture != null )
            {
                if( onDownloadTextureCompleted != null ) { onDownloadTextureCompleted.Invoke( texture ); }
            }
            else
            {
                DownloadFailed();
            }

        } //END TextureDownloadSuccess

        //------------------------------//
        private void CallDownloadAudioClipFileEvent()
        //------------------------------//
        {
            bool cacheIfWeb = false;

            if( DownloadLocationIsTryAllOrWeb() )
            {
                cacheIfWeb = this.cacheIfWeb;
            }

            string fileName = Application.productName + "_AUDIOCLIP";

            if( DownloadLocationIsTryAllOrWeb() && this.fileName != "" )
            {
                fileName = this.fileName;
            }

            WWWHelper.instance.GetAudioClip( path, cacheIfWeb, AudioClipDownloadSuccess, DownloadFailed, downloadLocation, fileName );

        } //END CallDownloadAudioClipFileEvent

        //-------------------------------//
        private void AudioClipDownloadSuccess( AudioClip audioClip )
        //-------------------------------//
        {

            if( audioClip != null )
            {
                if( onDownloadAudioClipCompleted != null ) { onDownloadAudioClipCompleted.Invoke( audioClip ); }
            }
            else
            {
                DownloadFailed();
            }

        } //END AudioClipDownloadSuccess

        //-------------------------------//
        private void DownloadFailed()
        //-------------------------------//
        {

            if( onDownloadFailed != null ) { onDownloadFailed.Invoke(); }

        } //END DownloadFailed


    } //END BlockEventUnity

} //END Namespace