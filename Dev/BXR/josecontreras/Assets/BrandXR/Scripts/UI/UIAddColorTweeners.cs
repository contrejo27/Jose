using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

namespace BrandXR
{
    public class UIAddColorTweeners: MonoBehaviour
    {

        public UIColorTweenManager uiColorTweenManager_AddToList;
        private List<UIColorTweener> colorTweeners = new List<UIColorTweener>();

        //--------------------------------------//
        public void Awake()
        //--------------------------------------//
        {

            //Find the list to add these color tweeners to, or make a new one
            if( uiColorTweenManager_AddToList == null && this.GetComponent<UIColorTweenManager>() != null )
            {
                uiColorTweenManager_AddToList = this.GetComponent<UIColorTweenManager>();
            }
            else if( uiColorTweenManager_AddToList == null && this.GetComponent<UIColorTweenManager>() == null )
            {
                uiColorTweenManager_AddToList = this.gameObject.AddComponent<UIColorTweenManager>();
            }



            //Add color tweeners to any image components we find
            List<Image> images = this.GetComponentsInChildren<Image>().ToList();

            if( images != null && images.Count > 0 )
            {
                foreach( Image image in images )
                {
                    if( image.GetComponent<UIColorTweener>() == null )
                    {
                        UIColorTweener colorTweener = image.gameObject.AddComponent<UIColorTweener>();
                        colorTweeners.Add( colorTweener );
                        colorTweener.color_Show = image.color;
                        colorTweener.color_Hide = new Color( image.color.r, image.color.g, image.color.b, 0f );
                        colorTweener.color_Default = image.color;
                    }
                }
            }
            



            //Add any color tweeners we created to the manager
            if( uiColorTweenManager_AddToList != null && colorTweeners != null && colorTweeners.Count > 0 )
            {
                //If our manager already has tweeners, check each tweener we want to add and make sure it hasn't already been added
                if( uiColorTweenManager_AddToList.tweeners != null && uiColorTweenManager_AddToList.tweeners.Count > 0 )
                {
                    foreach( UIColorTweener tweener in colorTweeners )
                    {
                        if( !uiColorTweenManager_AddToList.tweeners.Contains( tweener ) )
                        {
                            uiColorTweenManager_AddToList.tweeners.Add( tweener );
                        }
                    }
                }

                //If the manager has tweeners but the list is empty
                else if( uiColorTweenManager_AddToList.tweeners != null && uiColorTweenManager_AddToList.tweeners.Count == 0 )
                {
                    uiColorTweenManager_AddToList.tweeners = colorTweeners;
                }

                //If the manager has no tweeners (not instantiated)
                else if( uiColorTweenManager_AddToList.tweeners == null )
                {
                    uiColorTweenManager_AddToList.tweeners = colorTweeners;
                }
            }


        } //END Awake

    } //END Class

} //END Namespace