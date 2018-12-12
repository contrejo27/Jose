/* BlockImage.cs
 * 
 * Customizable image with advanced features such as Stereo 3D support and animation (spritesheet) playback
 */
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BrandXR
{
    public class BlockImage: Block
    {

        public override BlockType GetBlockType() { return BlockType.Image; }

        [Space( 10f ), TitleGroup( "Block Image", "Shows an Image when a Block Button is activated, or can be set to always show" )]
        public int dummy = 0;

        [FoldoutGroup("Hooks")]
        public UIScaleTweener uiScaleTweener_Image;

        [FoldoutGroup( "Hooks" )]
        public UIColorTweenManager uiColorTweenManager_Image;

        [FoldoutGroup( "Hooks" )]
        public RawImage rawImage;


        private bool showingImage;

        //--------------------------------//
        public override void Show()
        //--------------------------------//
        {
            base.Show();

            uiScaleTweener_Image.Play( UITweener.TweenValue.Show );
            uiColorTweenManager_Image.Play( UITweener.TweenValue.Show );

            showingImage = true;

        } //END Show

        //--------------------------------//
        public override void Hide()
        //--------------------------------//
        {
            base.Hide();

            uiScaleTweener_Image.Play( UITweener.TweenValue.Hide );
            uiColorTweenManager_Image.Play( UITweener.TweenValue.Hide );

            showingImage = false;

        } //END Hide

        //--------------------------------//
        public override void ForceShow()
        //--------------------------------//
        {
            base.ForceShow();

            uiScaleTweener_Image.Force( UITweener.TweenValue.Show );
            uiColorTweenManager_Image.Force( UITweener.TweenValue.Show );

            showingImage = true;

        } //END ForceShow

        //--------------------------------//
        public override void ForceHide()
        //--------------------------------//
        {
            base.ForceHide();

            uiScaleTweener_Image.Force( UITweener.TweenValue.Hide );
            uiColorTweenManager_Image.Force( UITweener.TweenValue.Hide );

            showingImage = false;

        } //END ForceHide


        

        //--------------------------------//
        public override void PrepareForDestroy()
        //--------------------------------//
        {

            //Stops any running tweens or timers associated with this block


        } //END PrepareForDestroy
        
        

        //--------------------------------------//
        public Texture GetTexture()
        //--------------------------------------//
        {

            if( rawImage != null )
            {
                return rawImage.texture;
            }
            else
            {
                return null;
            }

        } //END GetTexture

        


    } //END Class

} //END Namespace