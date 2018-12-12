using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

namespace BrandXR
{
    public class UIButton: MonoBehaviour
    {

        public UIScaleAnimation uiScaleAnimation_ScaleInAndOut;
        public UIScaleTweener uiScaleTweener_ScaleParent;
        public UIColorTweenManager uiColorTweenManager_Background;
        public UIColorTweenManager uiColorTweenManager_Icon;
        public UIColorTweenManager uiColorTweenManager_FillImage;

        public Image image_Icon;

        public Image image_IconFill;

        public AudioClip audioClipFillImage;
        private AudioSource audioSourceFillImage;

        public AudioClip audioClipSelect;
        private AudioSource audioSourceSelect;

        public float float_EnterColliderSpeed = 1f;

        protected Coroutine handle_FadeOutFillSFX;
        protected Coroutine handle_StartEnterTimer;

        public Collider buttonCollider;
        
        public UIColorTweenManager uiColorTweenManager_TextDescription;
        public UITextField uiTextField_TextDescription;

        public bool tryingToActivateButton = false;

        [System.Serializable]
        public class CallOnSelect: UnityEvent { }

        [SerializeField]
        public CallOnSelect callOnSelect;

        //--------------------------------//
        public virtual void Awake()
        //--------------------------------//
        {

            if( image_IconFill != null )
            {
                image_IconFill.fillAmount = 0f;
            }

            if( audioClipFillImage != null )
            {
                if( audioSourceFillImage == null )
                {
                    audioSourceFillImage = new AudioSource();
                }

                audioSourceFillImage.clip = audioClipFillImage;
            }

            if( audioClipSelect != null )
            {
                if( audioSourceSelect == null )
                {
                    audioSourceSelect = new AudioSource();
                }

                audioSourceSelect.clip = audioClipSelect;
            }

        } //END Awake

        //--------------------------------//
        public virtual void Show()
        //--------------------------------//
        {
            //if( gameObject.name == "Load Block" ) { Debug.Log( "UIButton.cs Show() : " + gameObject.name ); }

            if( uiScaleAnimation_ScaleInAndOut != null )
            {
                uiScaleAnimation_ScaleInAndOut.Play();
            }

            if( uiColorTweenManager_Background != null )
            {
                uiColorTweenManager_Background.Play( UITweener.TweenValue.Show );
            }

            if( uiColorTweenManager_Icon != null )
            {
                uiColorTweenManager_Icon.Play( UITweener.TweenValue.Show );
            }

            if( uiColorTweenManager_FillImage != null )
            {
                uiColorTweenManager_FillImage.Play( UITweener.TweenValue.Show );
            }

            if( uiColorTweenManager_TextDescription != null )
            {
                uiColorTweenManager_TextDescription.Play( UITweener.TweenValue.Show );
            }

            if( buttonCollider != null )
            {
                buttonCollider.enabled = true;
            }

        } //END Show

        //--------------------------------//
        public virtual void Hide()
        //--------------------------------//
        {
            //if( gameObject.name == "Load Block" ) { Debug.Log( "UIButton.cs Hide() : " + gameObject.name ); }

            if( uiScaleAnimation_ScaleInAndOut != null )
            {
                uiScaleAnimation_ScaleInAndOut.Stop();
            }

            if( uiScaleTweener_ScaleParent != null )
            {
                uiScaleTweener_ScaleParent.Play( UITweener.TweenValue.Hide );
            }

            if( uiColorTweenManager_Background != null )
            {
                uiColorTweenManager_Background.Play( UITweener.TweenValue.Hide );
            }

            if( uiColorTweenManager_Icon != null )
            {
                uiColorTweenManager_Icon.Play( UITweener.TweenValue.Hide );
            }

            if( uiColorTweenManager_FillImage != null )
            {
                uiColorTweenManager_FillImage.Play( UITweener.TweenValue.Hide );
            }

            if( uiColorTweenManager_TextDescription != null )
            {
                uiColorTweenManager_TextDescription.Play( UITweener.TweenValue.Hide );
            }

            if( buttonCollider != null )
            {
                buttonCollider.enabled = false;
            }

        } //END Hide

        //--------------------------------//
        public virtual void ForceShow()
        //--------------------------------//
        {
            //if( gameObject.name == "Load Block" ) { Debug.Log( "UIButton.cs ForceShow() : " + gameObject.name ); }

            if( uiScaleAnimation_ScaleInAndOut != null )
            {
                uiScaleAnimation_ScaleInAndOut.Play();
            }

            if( uiColorTweenManager_Background != null )
            {
                uiColorTweenManager_Background.Force( UITweener.TweenValue.Show );
            }

            if( uiColorTweenManager_Icon != null )
            {
                uiColorTweenManager_Icon.Force( UITweener.TweenValue.Show );
            }

            if( uiColorTweenManager_FillImage != null )
            {
                uiColorTweenManager_FillImage.Force( UITweener.TweenValue.Show );
            }

            if( uiColorTweenManager_TextDescription != null )
            {
                uiColorTweenManager_TextDescription.Force( UITweener.TweenValue.Show );
            }

            if( buttonCollider != null )
            {
                buttonCollider.enabled = true;
            }

        } //END ForceShow

        //--------------------------------//
        public virtual void ForceHide()
        //--------------------------------//
        {
            //if( gameObject.name == "Load Block" ) { Debug.Log( "UIButton.cs ForceHide() : " + gameObject.name ); }

            if( uiScaleAnimation_ScaleInAndOut != null )
            {
                uiScaleAnimation_ScaleInAndOut.Stop();
            }

            if( uiScaleTweener_ScaleParent != null )
            {
                uiScaleTweener_ScaleParent.Force( UITweener.TweenValue.Hide );
            }

            if( uiColorTweenManager_Background != null )
            {
                uiColorTweenManager_Background.Force( UITweener.TweenValue.Hide );
            }

            if( uiColorTweenManager_Icon != null )
            {
                uiColorTweenManager_Icon.Force( UITweener.TweenValue.Hide );
            }

            if( uiColorTweenManager_FillImage != null )
            {
                uiColorTweenManager_FillImage.Force( UITweener.TweenValue.Hide );
            }

            if( uiColorTweenManager_TextDescription != null )
            {
                uiColorTweenManager_TextDescription.Force( UITweener.TweenValue.Hide );
            }

            if( buttonCollider != null )
            {
                buttonCollider.enabled = false;
            }

        } //END ForceHide


        //---------------------------------//
        public virtual void Grow()
        //---------------------------------//
        {
            if( uiScaleTweener_ScaleParent != null )
            {
                uiScaleTweener_ScaleParent.Play( UITweener.TweenValue.Show );
            }

        } //END Grow

        //---------------------------------//
        public virtual void Shrink()
        //---------------------------------//
        {
            if( uiScaleTweener_ScaleParent != null )
            {
                uiScaleTweener_ScaleParent.Play( UITweener.TweenValue.Hide );
            }

        } //END Shrink

        //-------------------------------//
        public virtual void OnLookAt()
        //-------------------------------//
        {

            //Figure out how much time is remaining to fill in the fill circle (it doesn't reset to zero when a player exits the collider, we continue where we left off)
            float enterColliderSpeed = float_EnterColliderSpeed;

            if( image_IconFill != null )
            {
                enterColliderSpeed = float_EnterColliderSpeed - image_IconFill.fillAmount;
            }

            //Increase the size of the block's icon only, not the collider
            Grow();

            //Tween the fill circle
            if( image_IconFill != null )
            {
                image_IconFill.Kill();
                image_IconFill.fillAmount = 0f;
                image_IconFill.ImageFill( 1f, enterColliderSpeed, EaseCurve.Linear );
            }

            //Play the fill sfx
            FadeOutFillSFX();
            
            if( audioSourceFillImage != null && audioSourceFillImage.clip != null )
            {
                audioSourceFillImage.volume = 1f;
                audioSourceFillImage.loop = true;
                audioSourceFillImage.Play();
                audioSourceFillImage.AudioPitch( 1.25f, enterColliderSpeed, EaseCurve.Linear );
            }

            //Set a boolean to keep track of when we're loading and not loading the pin
            tryingToActivateButton = true;

            //Set a timer to go off, when it does we'll fade out the fill sfx
            handle_FadeOutFillSFX = Timer.instance.In( enterColliderSpeed - .15f, FadeOutFillSFX, gameObject );

            //Set a timer to go off, when it finishes we'll call OnSelect
            handle_StartEnterTimer = Timer.instance.In( enterColliderSpeed + .01f, OnSelect, gameObject );

        } //END OnLookAt

        //---------------------------------//
        protected virtual void FadeOutFillSFX()
        //---------------------------------//
        {

            //Stop the fill up sfx and play the exit sfx
            if( audioSourceFillImage != null && audioSourceFillImage.clip != null && audioSourceFillImage.isPlaying )
            {
                AudioHelper.instance.Fade( audioSourceFillImage, 0f, .25f, 0f );
            }

        } //END FadeOutFillSFX

        //-------------------------------//
        public virtual void OnLookAway()
        //-------------------------------//
        {

            if( tryingToActivateButton )
            {
                //Set a boolean to keep track of when we're loading and not loading the button
                tryingToActivateButton = false;

                //Cancel the existing timers
                if( handle_FadeOutFillSFX != null )
                {
                    Timer.instance.Cancel( handle_FadeOutFillSFX );
                }

                FadeOutFillSFX();

                if( handle_StartEnterTimer != null )
                {
                    Timer.instance.Cancel( handle_StartEnterTimer );
                }

                //Shrink the icon image, not the collider
                Shrink();

                //Make sure the block icon is showing
                Show();

                //Reset the fill circle
                if( image_IconFill != null )
                {
                    //Have the fill circle tween back down to zero
                    image_IconFill.Kill();
                    image_IconFill.ImageFill( 0f, .5f, EaseCurve.Linear );
                }

                //AudioManager.Instance.PlaySound( AudioManager.AudioClipNameType.BlockExit, AudioManager.AudioSourceType.Unity, false, 1f, 1f, 0f, .25f, null );
            }

        } //END OnLookAway

        //-------------------------------//
        public virtual void OnSelect()
        //-------------------------------//
        {

            //Set a boolean to keep track of when we're loading and not loading the button
            tryingToActivateButton = false;

            //Hide the fill circle
            if( image_IconFill != null )
            {
                image_IconFill.Kill();
                image_IconFill.fillAmount = 0f;
            }

            //Fade out the fill in sfx
            FadeOutFillSFX();

            //Play the button sfx
            if( audioSourceSelect != null && audioSourceSelect.clip != null )
            {
                audioSourceSelect.volume = 1f;
                audioSourceSelect.loop = false;
                audioSourceSelect.Play();
            }

            //Kill any timers (to prevent OnSelect from being called again)
            KillTimers();

            if( callOnSelect != null )
            {
                callOnSelect.Invoke();
            }

        } //END OnSelect

        //-------------------------------//
        public virtual void KillTimers()
        //-------------------------------//
        {

            if( handle_FadeOutFillSFX != null )
            {
                Timer.instance.Cancel( handle_FadeOutFillSFX );
            }

            if( handle_StartEnterTimer != null )
            {
                Timer.instance.Cancel( handle_StartEnterTimer );
            }

        } //END KillTimers

    } //END Class

} //END Namespace