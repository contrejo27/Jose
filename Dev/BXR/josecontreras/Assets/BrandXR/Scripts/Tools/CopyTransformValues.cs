using UnityEngine;
using System.Collections;

namespace BrandXR
{
    public class CopyTransformValues: MonoBehaviour
    {

        public Transform transformToCopy;
        public Transform thisTransform;

        public bool copyGlobalPosition = false;
        public bool copyRotation = false;
        public bool copyScale = false;

        public bool beginCopyingOnStart = false;

        public float startCopyingIn = 0f;
        public float copyEveryNumberOfSeconds = 1f;

        //-------------------------------------------------------------//
        public void Start()
        //-------------------------------------------------------------//
        {

            if( thisTransform == null ) { thisTransform = this.transform; }

            if( beginCopyingOnStart && transformToCopy != null && thisTransform != null )
            {
                StartCopyingValues();
            }

        } //END Start

        //-------------------------------------------------------------//
        public void StartCopyingValues()
        //-------------------------------------------------------------//
        {
            //Debug.Log( "Start, about to call InvokeRepeating" );
            InvokeRepeating( "UpdateValues", startCopyingIn, copyEveryNumberOfSeconds );

        } //END StartCopyingValues

        //-------------------------------------------------------------//
        public void StopCopyingValues()
        //-------------------------------------------------------------//
        {
            if( IsInvoking( "UpdateValues" ) )
            {
                //Debug.Log( "StopCopyingValues()" );
                CancelInvoke( "UpdateValues" );
            }

        } //END StopCopyingValues

        //-------------------------------------------------------------//
        public void UpdateValues()
        //-------------------------------------------------------------//
        {
            if( transformToCopy != null && thisTransform != null )
            {
                if( copyGlobalPosition )
                {
                    //Debug.Log( "UpdateValues() position = " + transformToCopy.position );
                    thisTransform.position = transformToCopy.position;
                }
                if( copyRotation )
                {
                    //Debug.Log( "UpdateValues() rotation = " + transformToCopy.localEulerAngles );
                    thisTransform.localEulerAngles = transformToCopy.localEulerAngles;
                }
                if( copyScale )
                {
                    //Debug.Log( "UpdateValues() scale = " + transformToCopy.localScale );
                    thisTransform.localScale = transformToCopy.localScale;
                }
            }

        } //END UpdateValues

        //-------------------------------------------------------------//
        public void Copy()
        //-------------------------------------------------------------//
        {

            UpdateValues();

        } //END Copy

        //-------------------------------------------------------------//
        public void CopyPosition()
        //-------------------------------------------------------------//
        {
            if( transformToCopy != null && thisTransform != null )
            {
                thisTransform.position = transformToCopy.position;
            }

        } //END CopyPosition

        //-------------------------------------------------------------//
        public void CopyRotation()
        //-------------------------------------------------------------//
        {
            if( transformToCopy != null && thisTransform != null )
            {
                thisTransform.localEulerAngles = transformToCopy.localEulerAngles;
            }

        } //END CopyRotation

        //-------------------------------------------------------------//
        public void CopyScale()
        //-------------------------------------------------------------//
        {
            if( transformToCopy != null && thisTransform != null )
            {
                thisTransform.localScale = transformToCopy.localScale;
            }

        } //END CopyScale


    } //END Class

} //END Namespace