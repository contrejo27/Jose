/* BlockText.cs
 * 
 * Displays text with advanced customization options
 */
using Sirenix.OdinInspector;
using UnityEngine;

namespace BrandXR
{
    public class BlockText: Block
    {

        public override BlockType GetBlockType() { return BlockType.Text; }

        [Space( 10f ), TitleGroup( "Block Text", "Shows Text when a Block Button is activated, or can be set to always show" )]
        public int titleDummy = 0;

        [FoldoutGroup("Hooks")]
        public UIScaleTweener uiScaleTweener_TextAndBackgroundParent;

        [FoldoutGroup( "Hooks" )]
        public UITextField uiTextField_Text;

        [FoldoutGroup( "Hooks" )]
        public UIColorTweenManager uiColorTweenManager_Text;


        //--------------------------------//
        public override void Show()
        //--------------------------------//
        {
            base.Show();

            uiScaleTweener_TextAndBackgroundParent.Play( UITweener.TweenValue.Show );

            //Debug.Log( "BlockText.cs Show()" );
            uiColorTweenManager_Text.Play( UITweener.TweenValue.Show );

        } //END Show

        //--------------------------------//
        public override void Hide()
        //--------------------------------//
        {
            base.Hide();

            uiScaleTweener_TextAndBackgroundParent.Play( UITweener.TweenValue.Hide );

            //Debug.Log( "BlockText.cs Hide()" );
            uiColorTweenManager_Text.Play( UITweener.TweenValue.Hide );

        } //END Hide

        //--------------------------------//
        public override void ForceShow()
        //--------------------------------//
        {
            base.ForceShow();

            uiScaleTweener_TextAndBackgroundParent.Force( UITweener.TweenValue.Show );

            //Debug.Log( "BlockText.cs ForceShow()" );
            uiColorTweenManager_Text.Force( UITweener.TweenValue.Show );

        } //END ForceShow

        //--------------------------------//
        public override void ForceHide()
        //--------------------------------//
        {
            base.ForceHide();

            uiScaleTweener_TextAndBackgroundParent.Force( UITweener.TweenValue.Hide );

            //Debug.Log( "BlockText.cs ForceHide()" );
            uiColorTweenManager_Text.Force( UITweener.TweenValue.Hide );

        } //END ForceHide


        

        //--------------------------------//
        public override void PrepareForDestroy()
        //--------------------------------//
        {

            //Stops any running tweens or timers associated with this block


        } //END PrepareForDestroy
        

        


    } //END Class

} //END Namespace