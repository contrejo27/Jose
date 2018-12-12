#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
#endif

namespace BrandXR
{
#if UNITY_EDITOR
    public class BXRBuildLogic: IPreprocessBuildWithReport
#else
    public class BXRBuildLogic
#endif
    {
        public int callbackOrder { get { return 0; } }

#if UNITY_EDITOR
        //----------------------------------------------------//
        public void OnPreprocessBuild( BuildReport report )
        //----------------------------------------------------//
        {
            CheckForIncompatibleXRTargets();

        } //END OnPreprocessBuild

        //------------------------------------//
        private void CheckForIncompatibleXRTargets()
        //------------------------------------//
        {

            //Grab the XRTechnologyManager
            if( GameObject.FindObjectOfType<XRTechnologyManager>() != null )
            {
                XRTechnologyManager xrTechnologyManager = GameObject.FindObjectOfType<XRTechnologyManager>();

                //If our XR tech is set to none, then any XRTargetBase prefabs and their children will be removed
                if( xrTechnologyManager.xrTechnologyType == XRTechnologyManager.XRTechnologyType.None )
                {
                    RemoveAllXRTargetBases();
                }

                //Otherwise, remove all XRTargetBase prefabs that do not match the selected technology
                else
                {
                    RemoveAllXRTargetBases( xrTechnologyManager.xrTechnologyType );
                }
                
            }

        } //END CheckForIncompatibleXRTargets

        //-----------------------------------//
        private void RemoveAllXRTargetBases()
        //-----------------------------------//
        {
            //Grab all of the XRTargetBase components in the scene
            if( GameObject.FindObjectOfType<XRTargetBase>() != null && 
                GameObject.FindObjectsOfType<XRTargetBase>().Length > 0 )
            {
                List<XRTargetBase> xrTargetBases = GameObject.FindObjectsOfType<XRTargetBase>().ToList();

                foreach( XRTargetBase xrTargetBase in xrTargetBases )
                {
                    if( xrTargetBase != null )
                    {
                        xrTargetBase.RemoveBlockGroup();
                    }
                }
            }

        } //END RemoveAllXRTargetBases

        //-----------------------------------//
        private void RemoveAllXRTargetBases( XRTechnologyManager.XRTechnologyType keepBasesWithThisType )
        //-----------------------------------//
        {
            //Grab all of the XRTargetBase components in the scene
            if( GameObject.FindObjectOfType<XRTargetBase>() != null &&
                GameObject.FindObjectsOfType<XRTargetBase>().Length > 0 )
            {
                List<XRTargetBase> xrTargetBases = GameObject.FindObjectsOfType<XRTargetBase>().ToList();

                foreach( XRTargetBase xrTargetBase in xrTargetBases )
                {
                    if( xrTargetBase != null && 
                        !xrTargetBase.GetXRTechnologyType().Contains( keepBasesWithThisType ) )
                    {
                        xrTargetBase.RemoveBlockGroup();
                    }
                }
            }

        } //END RemoveAllXRTargetBases



#endif

    } //END Class

} //END namespace
