using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class XRMeshTrigger: MonoBehaviour
    {
        [InfoBox("Attach this script to a 3D collider to recieve mouse/touch events")]
        [Tooltip( "Mouse entered the collider" )]
        public UnityEvent onMouseEnter;

        [Tooltip( "Mouse is hovering on collider" )]
        public UnityEvent onMouseOver;

        [Tooltip( "Mouse exited on collider" )]
        public UnityEvent onMouseExit;

        [Tooltip("Mouse clicked on collider")]
        public UnityEvent onMouseDown;
        

        //---------------------------------//
        public void OnMouseEnter()
        //---------------------------------//
        {
            //Debug.Log( "Mouse Entered the trigger" );

            if( onMouseEnter != null ) { onMouseEnter.Invoke(); }

        } //END OnMouseEnter

        //---------------------------------//
        public void OnMouseOver()
        //---------------------------------//
        {
            //Debug.Log( "Mouse Over the trigger" );

            if( onMouseOver != null ) { onMouseOver.Invoke(); }

        } //END OnMouseOver

        //---------------------------------//
        public void OnMouseExit()
        //---------------------------------//
        {
            //Debug.Log( "Mouse Exited the trigger" );

            if( onMouseExit != null ) { onMouseExit.Invoke(); }

        } //END OnMouseExit

        //---------------------------------//
        public void OnMouseDown()
        //---------------------------------//
        {
            //Debug.Log( "Mouse clicked the trigger" );

            if( onMouseDown != null ) { onMouseDown.Invoke(); }
            
        } //END OnMouseSelect


    } //END Class

} //END Namespace