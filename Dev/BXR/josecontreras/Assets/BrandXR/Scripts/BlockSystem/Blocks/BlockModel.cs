/* BlockModel.cs
 * 
 * Shows a 3D model in world space.
 * 
 * Instantiates a gameObject prefab on Awake() and childs it to this component
 */
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#if LEANTOUCH
using Lean.Touch;
#endif

namespace BrandXR
{
    public class BlockModel: Block
    {
        [TitleGroup( "Block Model", "Shows a 3D Model" )]
        public int titleDummy = 0;

        public override BlockType GetBlockType() { return BlockType.Model; }
        
        [Space(15f), InfoBox("The model prefab to instantiate on Awake()\n\nYou can change this out at runtime using a BlockEvent with a BlockEventBase of 'BlockModel'")]
        public GameObject model;

        [Space( 15f ), FoldoutGroup( "Hooks" )]
        public UIColorTweenManager uiColorTweenManager = null;

        [Space( 15f ), FoldoutGroup( "Hooks" )]
        public MeshCollider _meshCollider;
        
        [Space( 15f ), FoldoutGroup( "Hooks" )]
        public Rigidbody _rigidbody;
        private float dragEndEaseSpeed = 3f;

        [Space( 15f ), FoldoutGroup( "Hooks" )]
        private Transform transformToMove;

        [Space( 15f ), FoldoutGroup( "Hooks" )]
        private Transform transformToPinch;

        [Space( 15f ), FoldoutGroup( "Hooks" )]
        private Transform transformToTwist;

#if LEANTOUCH
        private LeanForceRigidbody leanForceRigidbody;

        private LeanFingerSwipe leanFingerSwipe;
#endif

        [FoldoutGroup( "Interaction & Physics Options" ), InfoBox( "Enable this to enforce that any models added to this BlockModel have a Mesh Collider attached" )]
        public bool addMeshCollider = false;

        [ShowIf( "addMeshCollider", true ), Space( 15f ), FoldoutGroup("Interaction & Physics Options"), InfoBox("Enable this to enforce that any models added to this BlockModel have a RigidBody attached")]
        public bool addRigidbody = false;


        //---------------------------------//
        public override void Start()
        //---------------------------------//
        {
            base.Start();

            CheckIfWeShouldChangeModelOnStart();
            
        } //END Start

        //--------------------------------//
        private void CheckIfWeShouldChangeModelOnStart()
        //--------------------------------//
        {

            //Check if we should Instantiate the model
            if( faceCameraTransform.childCount == 0 &&
                model != null )
            {
                ChangeModel( model );
            }

        } //END CheckIfWeShouldChangeModelOnStart

        //--------------------------------//
        /// <summary>
        /// Replaces the existing instantiated 3D model with the one passed in. Expects a prefab/gameObject reference to clone
        /// </summary>
        /// <param name="newModelToClone">The GameObject prefab we want to clone</param>
        public void ChangeModel( GameObject newModelToClone )
        //--------------------------------//
        {
#if LEANTOUCH
            leanForceRigidbody = null;
            leanFingerSwipe = null;
#endif

            //Destroy any existing model(s)
            if( faceCameraTransform.childCount > 0 )
            {
#if UNITY_EDITOR
                //Wait a moment before calling DestroyImmediate to make sure no logic is running
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    DestroyImmediate( faceCameraTransform.GetChild(0).gameObject );
                };
#else
                Destroy( faceCameraTransform.GetChild(0).gameObject );
#endif
            }

            //Instantiate the new model to show
            if( newModelToClone != null )
            {
                model = Instantiate( newModelToClone, faceCameraTransform );
                
                //Remove (Clone) from the models name
                if( model.name.Contains( "(Clone)" ))
                {
                    string[] words = model.name.Split( '(' );
                    model.name = words[ 0 ];
                }
                
                model.SetActive( true );

                transformToMove = this.transform;
                transformToPinch = this.transform;
                transformToTwist = this.transform;
                /*
                transformToMove = model.transform;
                transformToPinch = model.transform;
                transformToTwist = model.transform;
                */

            }

            //Add a ColorTweenManager to the new model, and link all of the renderers to it
            if ( model.GetComponent<UIColorTweenManager>() == null )
            {
                uiColorTweenManager = model.AddComponent<UIColorTweenManager>();
            }
            else
            {
                uiColorTweenManager = model.GetComponent<UIColorTweenManager>();
            }

            //Find all of the Renderers on the object, attach a UIColorTweener to those gameObjects and link them to the uiColorTweenManager
            if( model.GetComponentInChildren<Renderer>() != null )
            {
                uiColorTweenManager.tweeners = new List<UIColorTweener>();

                foreach( Renderer rend in model.GetComponentsInChildren<Renderer>().ToList() )
                {
                    if ( rend != null && rend.gameObject.GetComponent<UIColorTweener>() == null )
                    {
                        uiColorTweenManager.tweeners.Add( rend.gameObject.AddComponent<UIColorTweener>() );
                    }
                    else if( rend != null && rend.gameObject.GetComponent<UIColorTweener>() != null )
                    {
                        uiColorTweenManager.tweeners.Add( rend.gameObject.GetComponent<UIColorTweener>() );
                    }
                }
            }

            CheckIfWeShouldAddMeshColliderToModel();

            CheckIfWeShouldAddRigidbodyToModel();

        } //END ChangeModel
        
        //---------------------------------------//
        private void CheckIfWeShouldAddMeshColliderToModel()
        //---------------------------------------//
        {
            //Check if we should add a Mesh Collider to the model
            if( model != null && addMeshCollider )
            {
                //Only add a MeshCollider if there isn't one already
                if( model.GetComponentInChildren<MeshCollider>() == null )
                {
                    //Add the MeshCollider to the first MeshRenderer we can find
                    if( model.GetComponentInChildren<MeshRenderer>() != null )
                    {
                        MeshRenderer meshRenderer = model.GetComponentInChildren<MeshRenderer>();

                        _meshCollider = meshRenderer.gameObject.AddComponent<MeshCollider>();

                        //Make the Mesh Collider convex
                        _meshCollider.convex = true;
                    }
                }
                else
                {
                    _meshCollider = model.GetComponentInChildren<MeshCollider>();
                }
            }

        } //END CheckIfWeShouldAddMeshColliderToModel

        //---------------------------------------//
        private void CheckIfWeShouldAddRigidbodyToModel()
        //---------------------------------------//
        {

            //Check if we should add a Rigidbody to the model
            if( model != null && addMeshCollider && addRigidbody && _meshCollider != null )
            {
                //Only add a Rigidbody if there isn't one already
                if( model.GetComponentInChildren<Rigidbody>() == null )
                {
                    _rigidbody = _meshCollider.gameObject.AddComponent<Rigidbody>();
                }
                else
                {
                    _rigidbody = model.GetComponentInChildren<Rigidbody>();
                }

                //Disable gravity until the floor plane has been created, otherwise it will fall through the floor!
                _rigidbody.isKinematic = false;
                _rigidbody.useGravity = false;
                _rigidbody.mass = 1f;
                _rigidbody.drag = dragEndEaseSpeed;
                _rigidbody.angularDrag = dragEndEaseSpeed;
                _rigidbody.freezeRotation = true;

                //Setup the other features we'll need to apply physics
#if LEANTOUCH
                //Add a LeanTouchRigidbody component to the model
                if( leanForceRigidbody == null )
                {
                    leanForceRigidbody = _meshCollider.gameObject.AddComponent<LeanForceRigidbody>();
                    leanForceRigidbody.cachedBody = _rigidbody;
                }
                else
                {
                    Debug.Log( "model = " + model + ", leanForceRigidbody != null... " + leanForceRigidbody );
                }

                //Add a LeanFingerSwipe component to the model
                if( leanFingerSwipe == null )
                {
                    leanFingerSwipe = _meshCollider.gameObject.AddComponent<LeanFingerSwipe>();
                    leanFingerSwipe.Clamp = LeanFingerSwipe.ClampType.ScaledDelta;
                }

                //Connect the finger swipe event to the Rigidbody
                if( leanFingerSwipe != null && leanForceRigidbody != null )
                {
                    if( leanFingerSwipe.OnSwipeDelta == null ) { leanFingerSwipe.OnSwipeDelta = new LeanFingerSwipe.Vector2Event(); }
                    leanFingerSwipe.OnSwipeDelta.AddListener( leanForceRigidbody.Apply );
                }
#endif
            }

        } //END CheckIfWeShouldAddRigidbodyToModel

        //---------------------------------------//
        public void EnableGravity()
        //---------------------------------------//
        {

            if( _rigidbody != null )
            {
                _rigidbody.useGravity = true;
            }

        } //END EnableGravity

        //---------------------------------------//
        public void DisableGravity()
        //---------------------------------------//
        {

            if( _rigidbody != null )
            {
                _rigidbody.useGravity = false;
            }

        } //END DisableGravity

        //---------------------------------------//
        public void DragEventOccuring()
        //---------------------------------------//
        {
            CancelRigidbodyForce();

        } //END DragEventOccuring

        //---------------------------------------//
        public void PressEventEnd()
        //---------------------------------------//
        {

            CancelRigidbodyForce();

        } //END PressEventEnd


        //---------------------------------------//
        private void CancelRigidbodyForce()
        //---------------------------------------//
        {

            //Reset any movement on the rigidbody
            if( addMeshCollider && addRigidbody )
            {
                if( model != null )
                {
                    if( _rigidbody != null )
                    {
                        _rigidbody.velocity = Vector3.zero;
                        _rigidbody.angularVelocity = Vector3.zero;
                    }
                    
                }
            }

        } //END CancelRigidbodyForce


        //---------------------------------------//
        /// <summary>
        /// Call this to inform the BlockModel that a DragEvent that was effecting this BlockModel has ended.
        /// Checks to see if we should impact our rigidbody
        /// </summary>
        public void DragEventEnd()
        //---------------------------------------//
        {

            if( addMeshCollider && addRigidbody )
            {
#if LEANTOUCH
                if( leanFingerSwipe != null && 
                    LeanTouch.Fingers != null && LeanTouch.Fingers.Count > 0 )
                {
                    //Debug.Log( "BlockModel.cs DragEventEnd() Scaled Delta = " + LeanTouch.Fingers[0].ScaledDelta + ", delta = " + LeanTouch.Fingers[0].SwipeScreenDelta );
                    leanFingerSwipe.FingerSwipe( LeanTouch.Fingers[ 0 ] );
                }
#endif
            }

        } //END DragEventEnd

        //---------------------------------------//
        public void AddForce( Vector3 force )
        //---------------------------------------//
        {

#if LEANTOUCH
            if( leanForceRigidbody != null )
            {
                leanForceRigidbody.ApplyTo( force );
            }
#else
            if( _rigidbody != null )
            {
                _rigidbody.AddForce( force );
            }
#endif

        } //END AddForce

        //---------------------------------------//
        public override void ForceShow()
        //---------------------------------------//
        {
            base.Show();

            if( uiColorTweenManager != null && uiColorTweenManager.tweeners.Count > 0 )
            {
                uiColorTweenManager.ForceAlpha( 1f );
            }

        } //END ForceShow

        //---------------------------------------//
        public override void ForceHide()
        //---------------------------------------//
        {
            base.Show();

            if( uiColorTweenManager != null && uiColorTweenManager.tweeners.Count > 0 )
            {
                uiColorTweenManager.ForceAlpha( 0f );
            }

        } //END ForceHide

        //---------------------------------------//
        public override void Show()
        //---------------------------------------//
        {
            base.Show();

            if( uiColorTweenManager != null && uiColorTweenManager.tweeners.Count > 0 )
            {
                uiColorTweenManager.PlayAlpha( 1f, 1f );
            }

        } //END Show

        //---------------------------------------//
        public override void Hide()
        //---------------------------------------//
        {
            base.Hide();

            if( uiColorTweenManager != null && uiColorTweenManager.tweeners.Count > 0 )
            {
                uiColorTweenManager.PlayAlpha( 0f, 1f );
            }

        } //END Hide

        //--------------------------------------//
        public Transform GetMoveTransform()
        //--------------------------------------//
        {

            if( transformToMove != null )
            {
                return transformToMove;
            }
            else
            {
                return transform;
            }

        } //END GetMoveTransform

        
        //--------------------------------------//
        public Transform GetPinchTransform()
        //--------------------------------------//
        {

            if( transformToPinch != null )
            {
                return transformToPinch;
            }
            else
            {
                return transform;
            }

        } //END GetPinchTransform

        //--------------------------------------//
        public Transform GetTwistTransform()
        //--------------------------------------//
        {

            if( transformToTwist != null )
            {
                return transformToTwist;
            }
            else
            {
                return transform;
            }

        } //END GetTwistTransform

        //-----------------------------//
        public void SetDragEndEaseOutSpeed( float speed )
        //-----------------------------//
        {

            dragEndEaseSpeed = speed;

            if( _rigidbody != null )
            {
                _rigidbody.drag = speed;
                _rigidbody.angularDrag = speed;
            }

        } //END SetDragEndEaseOutSpeed

    } //END Class

} //END Namespace