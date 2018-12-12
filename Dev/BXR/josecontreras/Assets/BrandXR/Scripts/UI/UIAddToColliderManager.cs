using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

namespace BrandXR
{
    public class UIAddToColliderManager: MonoBehaviour
    {

        public UIColliderManager uiColliderManager_AddToList;
        private List<Collider> colliders = new List<Collider>();
        private List<Button> buttons = new List<Button>();
        private List<Image> images = new List<Image>();

        //--------------------------------------//
        public void Awake()
        //--------------------------------------//
        {

            //Find the list to add these color tweeners to, or make a new one
            if( uiColliderManager_AddToList == null && this.GetComponent<UIColliderManager>() != null )
            {
                uiColliderManager_AddToList = this.GetComponent<UIColliderManager>();
            }
            else if( uiColliderManager_AddToList == null && this.GetComponent<UIColliderManager>() == null )
            {
                uiColliderManager_AddToList = this.gameObject.AddComponent<UIColliderManager>();
            }



            //Add any colliders we find to the list
            List<Collider> _colliders = this.GetComponentsInChildren<Collider>().ToList();

            if( _colliders != null && _colliders.Count > 0 )
            {
                colliders = _colliders;
            }

            //Add any buttons we find to the list
            List<Button> _buttons = this.GetComponentsInChildren<Button>().ToList();

            if( _buttons != null && _buttons.Count > 0 )
            {
                buttons = _buttons;
            }

            //Add any images with the name "collider" we find to the list
            List<Image> _images = this.GetComponentsInChildren<Image>().ToList();

            if( _images != null && _images.Count > 0 )
            {
                foreach( Image image in _images )
                {
                    if( image.gameObject.name.ToLower().Contains( "collider" ) )
                    {
                        images.Add( image );
                    }
                }
            }






            //Add any colliders we created to the manager
            if( uiColliderManager_AddToList != null && colliders != null && colliders.Count > 0 )
            {
                //If our manager already has colliders, check each collider we want to add and make sure it hasn't already been added
                if( uiColliderManager_AddToList.colliders != null && uiColliderManager_AddToList.colliders.Count > 0 )
                {
                    foreach( Collider collider in colliders )
                    {
                        if( !uiColliderManager_AddToList.colliders.Contains( collider ) )
                        {
                            uiColliderManager_AddToList.colliders.Add( collider );
                        }
                    }
                }

                //If the manager has colliders but the list is empty
                else if( uiColliderManager_AddToList.colliders != null && uiColliderManager_AddToList.colliders.Count == 0 )
                {
                    uiColliderManager_AddToList.colliders = colliders;
                }

                //If the manager has no colliders (not instantiated)
                else if( uiColliderManager_AddToList.colliders == null )
                {
                    uiColliderManager_AddToList.colliders = colliders;
                }
            }

            //Add any buttons we created to the manager
            if( uiColliderManager_AddToList != null && buttons != null && buttons.Count > 0 )
            {
                //If our manager already has buttons, check each collider we want to add and make sure it hasn't already been added
                if( uiColliderManager_AddToList.buttons != null && uiColliderManager_AddToList.buttons.Count > 0 )
                {
                    foreach( Button button in buttons )
                    {
                        if( !uiColliderManager_AddToList.buttons.Contains( button ) )
                        {
                            uiColliderManager_AddToList.buttons.Add( button );
                        }
                    }
                }

                //If the manager has buttons but the list is empty
                else if( uiColliderManager_AddToList.buttons != null && uiColliderManager_AddToList.buttons.Count == 0 )
                {
                    uiColliderManager_AddToList.buttons = buttons;
                }

                //If the manager has no buttons (not instantiated)
                else if( uiColliderManager_AddToList.buttons == null )
                {
                    uiColliderManager_AddToList.buttons = buttons;
                }
            }


            //Add any images we found with the name "collider" to the manager
            if( uiColliderManager_AddToList != null && images != null && images.Count > 0 )
            {
                //If our manager already has images, check each collider we want to add and make sure it hasn't already been added
                if( uiColliderManager_AddToList.images != null && uiColliderManager_AddToList.images.Count > 0 )
                {
                    foreach( Image image in images )
                    {
                        if( !uiColliderManager_AddToList.images.Contains( image ) )
                        {
                            uiColliderManager_AddToList.images.Add( image );
                        }
                    }
                }

                //If the manager has buttons but the list is empty
                else if( uiColliderManager_AddToList.images != null && uiColliderManager_AddToList.images.Count == 0 )
                {
                    uiColliderManager_AddToList.images = images;
                }

                //If the manager has no buttons (not instantiated)
                else if( uiColliderManager_AddToList.images == null )
                {
                    uiColliderManager_AddToList.images = images;
                }
            }



        } //END Awake



    } //END Class

} //END Namespace