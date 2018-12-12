using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace BrandXR
{
    public class UIColliderManager: MonoBehaviour
    {
        public List<Collider> colliders;
        public List<Button> buttons;
        public List<Image> images;

        //-----------------------------------------//
        public void SetColliders( bool turnOn )
        //-----------------------------------------//
        {

            foreach( Collider co in colliders )
            {
                co.enabled = turnOn;
            }

            foreach( Button button in buttons )
            {
                button.enabled = turnOn;
            }

            foreach( Image image in images )
            {
                image.enabled = turnOn;
                image.gameObject.SetActive( turnOn );
            }


        } //END SetColliders

        //-----------------------------------------//
        public void FlipColliders()
        //-----------------------------------------//
        {

            foreach( Collider co in colliders )
            {
                co.enabled = !co.enabled;
            }

            foreach( Button button in buttons )
            {
                button.enabled = !button.enabled;
            }

            foreach( Image image in images )
            {
                image.enabled = !image.enabled;
                image.gameObject.SetActive( !image.gameObject.activeSelf );
            }


        } //END FlipColliders


    } //END Class

} //END Namespace