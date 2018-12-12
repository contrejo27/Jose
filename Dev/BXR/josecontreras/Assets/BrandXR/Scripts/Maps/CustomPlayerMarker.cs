/* CustomPlayerMarker.cs
 * Allows you to customize the player marker visual on the map.  
 * Can use either a sprite or a premade prefab.
 * Profides ability to modify the marker's scale, rotation, and Y offset 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace BrandXR
{
    //------------------------------------------------------//
    public class CustomPlayerMarker : SerializedMonoBehaviour
    //------------------------------------------------------//
    {
        public enum ESpawnTimes
        {
            AWAKE,
            START,
            MANUAL
        }

#pragma warning disable
        // Required References
        [FoldoutGroup("Hooks"), InfoBox("Don't Modify!", InfoMessageType.Warning)]
        [FoldoutGroup("Hooks"), Required, SerializeField] private Transform targetParent = null;
        [FoldoutGroup("Hooks"), Required, SerializeField] private Transform prefabParent = null;
        [FoldoutGroup("Hooks"), Required, SerializeField] private SpriteRenderer spriteRend = null;

        // Time to spawn Player Marker
        [BoxGroup("Spawn Time"), SerializeField, EnumToggleButtons] private ESpawnTimes spawnTime = ESpawnTimes.AWAKE;

        [BoxGroup("Target Type")]
        // Dictates the target's type
        [SerializeField] private bool isPrefab = false;

        // References for target depending on the target's type
        [BoxGroup("Target Type/Prefab"), ShowIf("isPrefab"), SerializeField] private GameObject targetPrefab;
        [BoxGroup("Target Type/Sprite"), HideIf("isPrefab"), SerializeField] private Sprite targetSprite;

        // Modifications for the target's transform 
        [BoxGroup("Target Transform"), SerializeField, InlineButton("ScaleTarget", "Apply")] private Vector3 targetScale = new Vector3(1.0f, 1.0f, 1.0f);
        [BoxGroup("Target Transform"), SerializeField, InlineButton("RotateTarget", "Apply")] private Vector3 targetRotation = new Vector3(0.0f, 0.0f, 0.0f);
        [BoxGroup("Target Transform"), SerializeField, InlineButton("OffsetTargetY", "Apply")] private float targetYOffset = 0.0f;
#pragma warning restore

        [SerializeField] private bool showDebug = false;

        //-----------------//
        private void Awake()
        //-----------------//
        {
            if (spawnTime == ESpawnTimes.AWAKE)
            {
                SpawnPlayerTarget();
            }

        }// END Awake

        //-----------------//
        private void Start()
        //-----------------//
        {
            if (spawnTime == ESpawnTimes.START)
            {
                SpawnPlayerTarget();
            }

        } // END Start

        //-------------------------------------------------------------------------------------------//
        [BoxGroup("Spawn Time"), Button(ButtonSizes.Medium), ShowIf("spawnTime", ESpawnTimes.MANUAL)]
        public void SpawnPlayerTarget()
        //-------------------------------------------------------------------------------------------//
        {
            DestroyPreviousTarget();

            if(showDebug) { Debug.Log("CustomPlayerMarker.cs SpawnPlayerTarget() Spawning player target..."); }

            if (isPrefab)
            {
                if (targetPrefab)
                {
                    if (prefabParent)
                    {
                        Instantiate(targetPrefab, prefabParent);
                    }
                    else if(showDebug)
                    {
                        Debug.LogError( "CustomPlayerMarker.cs SpawnPlayerTarget() Unable to Instantiate Prefab, Null Prefab parent on [Player Target]" );
                    }

                }
                else if(showDebug)
                {
                    Debug.LogError( "CustomPlayerMarker.cs SpawnPlayerTarget() Unable to Instantiate Prefab, Null Target Prefab on [Player Target]" );
                }
            }
            else
            {
                if (spriteRend)
                {
                    if (targetSprite)
                    {
                        spriteRend.sprite = targetSprite;
                    }
                    else if(showDebug)
                    {
                        Debug.LogError( "CustomPlayerMarker.cs SpawnPlayerTarget() Unable to set SpriteRenderer, Null Target Sprite on [Player Target]" );
                    }
                }
                else if(showDebug)
                {
                    Debug.LogError( "CustomPlayerMarker.cs SpawnPlayerTarget() Unable to set SpriteRenderer, Null SpriteRend on [Player Target]" );
                }
            }

            ScaleTarget();
            RotateTarget();

        } // END SpawnPlayerTarget

        //---------------------------------//
        private void DestroyPreviousTarget()
        //---------------------------------//
        {
            if (prefabParent != null )
            {
                foreach (Transform child in prefabParent)
                {
                    if( child != null && child.gameObject != null )
                    {
                        if( Application.isEditor )
                        {
                            DestroyImmediate( child.gameObject );

                        }
                        else
                        {
                            Destroy( child.gameObject );
                        }
                    }
                }
            }

            if (spriteRend != null)
            {
                spriteRend.sprite = null;
            }

        } // END DestroyPreviousTarget

        //------------------------//
        private void ScaleTarget()
        //------------------------//
        {
            if (targetParent == null)
            {
                return;
            }

            targetParent.localScale = targetScale;

        } // END ScaleTarget

        //------------------------//
        private void RotateTarget()
        //------------------------//
        {
            if (targetParent == null)
            {
                return;
            }

            targetParent.eulerAngles = targetRotation;

        } // END Rotate Target

        //------------------------//
        private void OffsetTargetY()
        //------------------------//
        {
            if (targetParent == null)
            {
                return;
            }

            targetParent.transform.localPosition = new Vector3(0, 0 + targetYOffset, 0);

        } // END Offset Target Y

        //------------------------------------------------------//
        [Button(ButtonSizes.Medium), BoxGroup("Target Transform")]
        public void ResetTransform()
        //------------------------------------------------------//
        {
            targetRotation = Vector3.zero;
            targetScale = new Vector3(1.0f, 1.0f, 1.0f);
            targetYOffset = 0;

            RotateTarget();
            ScaleTarget();
            OffsetTargetY();

        } // END ResetTransform

    } // END Class

} // END Namespace