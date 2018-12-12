using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace BrandXR
{
    public class UIColorTweener: UITweener
    {
        
        public bxrTweenColor.RendererType rendererType = bxrTweenColor.RendererType.RawImage;

        [ShowIf( "rendererType", bxrTweenColor.RendererType.Renderer )]
        public Renderer Renderer;
        [ShowIf( "rendererType", bxrTweenColor.RendererType.Image )]
        public Image image;
        [ShowIf( "rendererType", bxrTweenColor.RendererType.RawImage )]
        public RawImage rawImage;
        [ShowIf( "rendererType", bxrTweenColor.RendererType.SpriteRenderer )]
        public SpriteRenderer spriteRenderer;
        [ShowIf( "rendererType", bxrTweenColor.RendererType.Text )]
        public Text text;
        [ShowIf( "rendererType", bxrTweenColor.RendererType.Material )]
        public Material material;
        [ShowIf( "rendererType", bxrTweenColor.RendererType.CanvasGroup )]
        public CanvasGroup canvasGroup;

        [FoldoutGroup("Colors")]
        public Color color_Default = Color.white;
        [FoldoutGroup( "Colors" )]
        public Color color_Hide = new Color( Color.white.r, Color.white.g, Color.white.b, 0f );
        [FoldoutGroup( "Colors" )]
        public Color color_Show = Color.white;
        [FoldoutGroup( "Colors" )]
        public Color color_HoverEnter = Color.white;
        [FoldoutGroup( "Colors" )]
        public Color color_HoverExit = Color.white;
        [FoldoutGroup( "Colors" )]
        public Color color_Pressed = Color.white;
        [FoldoutGroup( "Colors" )]
        public Color color_Hold = Color.white;
        [FoldoutGroup( "Colors" )]
        public Color color_Enabled = Color.white;
        [FoldoutGroup( "Colors" )]
        public Color color_Disabled = Color.white;

        //-----------------------------------//
        protected override void FindInitialValues()
        //-----------------------------------//
        {

            if( Renderer == null && image == null && rawImage == null && spriteRenderer == null && text == null && material == null )
            {
                if( transform.GetComponent<Renderer>() != null ) { Renderer = transform.GetComponent<Renderer>(); }
                else if( transform.GetComponent<Image>() != null ) { image = transform.GetComponent<Image>(); }
                else if( transform.GetComponent<RawImage>() != null ) { rawImage = transform.GetComponent<RawImage>(); }
                else if( transform.GetComponent<SpriteRenderer>() != null ) { spriteRenderer = transform.GetComponent<SpriteRenderer>(); }
                else if( transform.GetComponent<Text>() != null ) { text = transform.GetComponent<Text>(); }
                else if( transform.GetComponent<Material>() != null ) { material = transform.GetComponent<Material>(); }
                else if( transform.GetComponent<CanvasGroup>() != null ) { canvasGroup = transform.GetComponent<CanvasGroup>(); }
            }


            if( Renderer != null ) { rendererType = bxrTweenColor.RendererType.Renderer; }
            else if( image != null ) { rendererType = bxrTweenColor.RendererType.Image; }
            else if( rawImage != null ) { rendererType = bxrTweenColor.RendererType.RawImage; }
            else if( spriteRenderer != null ) { rendererType = bxrTweenColor.RendererType.SpriteRenderer; }
            else if( text != null ) { rendererType = bxrTweenColor.RendererType.Text; }
            else if( material != null ) { rendererType = bxrTweenColor.RendererType.Material; }
            else if( canvasGroup != null ) { rendererType = bxrTweenColor.RendererType.CanvasGroup; }

        } //END FindInitialValues

        //------------------------------------------//
        protected override void ForceDefaults()
        //------------------------------------------//
        {
            SetColor( GetDefaultColorFromRendererType(), TweenValue.Default );

        } //END ForceDefaults

        //-----------------------------------//
        private Color GetDefaultColorFromRendererType()
        //-----------------------------------//
        {
            Color color = Color.white;

            if( rendererType == bxrTweenColor.RendererType.Renderer ) { color = Renderer.sharedMaterial.color; }
            else if( rendererType == bxrTweenColor.RendererType.Image ) { color = image.color; }
            else if( rendererType == bxrTweenColor.RendererType.RawImage ) { color = rawImage.color; }
            else if( rendererType == bxrTweenColor.RendererType.SpriteRenderer ) { color = spriteRenderer.color; }
            else if( rendererType == bxrTweenColor.RendererType.Text ) { color = text.color; }
            else if( rendererType == bxrTweenColor.RendererType.Material ) { color = material.color; }
            else if( rendererType == bxrTweenColor.RendererType.CanvasGroup ) { color = new Color( Color.white.r, Color.white.g, Color.white.b, canvasGroup.alpha ); }

            return color;

        } //END GetDefaultColorFromRendererType




        //-----------------------------------//
        public void Force( Color newColor, TweenValue tweenValue )
        //-----------------------------------//
        {
            SetColor( newColor, tweenValue );
            Force( tweenValue );

        } //END Force

        //-----------------------------------//
        public override void Force( TweenValue tweenValue )
        //-----------------------------------//
        {
            base.Force( tweenValue );

            Color color = GetEndColor( tweenValue );

            if( rendererType == bxrTweenColor.RendererType.Renderer && Renderer != null && Renderer.sharedMaterial != null ) { Renderer.sharedMaterial.color = color; }
            else if( rendererType == bxrTweenColor.RendererType.Image && image != null ) { image.color = color; }
            else if( rendererType == bxrTweenColor.RendererType.RawImage && rawImage != null ) { rawImage.color = color; }
            else if( rendererType == bxrTweenColor.RendererType.SpriteRenderer && spriteRenderer != null ) { spriteRenderer.color = color; }
            else if( rendererType == bxrTweenColor.RendererType.Text && text != null ) { text.color = color; }
            else if( rendererType == bxrTweenColor.RendererType.Material && material != null ) { material.color = color; }
            else if( rendererType == bxrTweenColor.RendererType.CanvasGroup && canvasGroup != null ) { canvasGroup.alpha = color.a; }

        } //END Force





        //------------------------------------//
        public void ForceAlpha( float alpha )
        //------------------------------------//
        {

            Color color = GetColorFromRendererWithNewAlpha( alpha );

            if( rendererType == bxrTweenColor.RendererType.Renderer ) { Renderer.sharedMaterial.color = color; }
            else if( rendererType == bxrTweenColor.RendererType.Image ) { image.color = color; }
            else if( rendererType == bxrTweenColor.RendererType.RawImage ) { rawImage.color = color; }
            else if( rendererType == bxrTweenColor.RendererType.SpriteRenderer ) { spriteRenderer.color = color; }
            else if( rendererType == bxrTweenColor.RendererType.Text ) { text.color = color; }
            else if( rendererType == bxrTweenColor.RendererType.Material ) { material.color = color; }
            else if( rendererType == bxrTweenColor.RendererType.CanvasGroup ) { canvasGroup.alpha = color.a; }

        } //END ForceAlpha

        //------------------------------------//
        public void PlayAlpha( float alpha, float tweenSpeed )
        //------------------------------------//
        {
            Stop();
            
            BeginPlayingAlpha( alpha, tweenSpeed, 0f, easeType_Show );

        } //END PlayAlpha

        //------------------------------------//
        public void PlayAlpha( float alpha, float tweenSpeed, float delay )
        //------------------------------------//
        {
            Stop();

            BeginPlayingAlpha( alpha, tweenSpeed, delay, easeType_Show );

        } //END PlayAlpha

        //------------------------------------//
        public void PlayAlpha( float alpha, float tweenSpeed, UnityEvent CallOnComplete )
        //------------------------------------//
        {
            Stop();
            
            BeginPlayingAlpha( alpha, tweenSpeed, 0f, easeType_Show );

        } //END PlayAlpha

        //------------------------------------//
        public void PlayAlpha( float alpha, float tweenSpeed, float delay, EaseCurve.EaseType easeType, UnityEvent CallOnComplete )
        //------------------------------------//
        {
            Stop();
            
            BeginPlayingAlpha( alpha, tweenSpeed, delay, easeType );

        } //END PlayAlpha

        //------------------------------------//
        private void BeginPlayingAlpha( float alpha, float tweenSpeed, float delay, EaseCurve.EaseType easeType )
        //------------------------------------//
        {
            Color startColor = GetStartColor( rendererType );
            Color endColor = GetColorFromRendererWithNewAlpha( alpha );

            if( rendererType == bxrTweenColor.RendererType.Renderer )           { tween = Renderer.Color( endColor, tweenSpeed, easeType, startColor, delay, false, onCompleteOrLoop ); }
            else if( rendererType == bxrTweenColor.RendererType.Image )         { tween = image.Color( endColor, tweenSpeed, easeType, startColor, delay, false, onCompleteOrLoop ); }
            else if( rendererType == bxrTweenColor.RendererType.RawImage )      { tween = rawImage.Color( endColor, tweenSpeed, easeType, startColor, delay, false, onCompleteOrLoop ); }
            else if( rendererType == bxrTweenColor.RendererType.SpriteRenderer ){ tween = spriteRenderer.Color( endColor, tweenSpeed, easeType, startColor, delay, false, onCompleteOrLoop ); }
            else if( rendererType == bxrTweenColor.RendererType.Text )          { tween = text.Color( endColor, tweenSpeed, easeType, startColor, delay, false, onCompleteOrLoop ); }
            else if( rendererType == bxrTweenColor.RendererType.Material )      { tween = material.Color( endColor, tweenSpeed, easeType, startColor, delay, false, onCompleteOrLoop ); }
            else if( rendererType == bxrTweenColor.RendererType.CanvasGroup )   { tween = canvasGroup.Color( endColor, tweenSpeed, easeType, startColor, delay, false, onCompleteOrLoop ); }

        } //END BeginPlayingAlpha

        //-----------------------------------//
        public void SetTexture( Texture texture )
        //-----------------------------------//
        {

            if( rendererType == bxrTweenColor.RendererType.Renderer )           { Renderer.sharedMaterial.mainTexture = texture; }
            else if( rendererType == bxrTweenColor.RendererType.Image )         { image.sprite = Sprite.Create( texture as Texture2D, new Rect( 0, 0, texture.width, texture.height ), new Vector2( .5f, .5f ) ); }
            else if( rendererType == bxrTweenColor.RendererType.RawImage )      { rawImage.texture = texture; }
            else if( rendererType == bxrTweenColor.RendererType.SpriteRenderer ){ spriteRenderer.sprite = Sprite.Create( texture as Texture2D, new Rect( 0, 0, texture.width, texture.height ), new Vector2( .5f, .5f ) ); }
            else if( rendererType == bxrTweenColor.RendererType.Material )      { material.mainTexture = texture; }

        } //END SetTexture

        //-----------------------------------//
        public void SetTexture( Texture2D texture )
        //-----------------------------------//
        {

            if( rendererType == bxrTweenColor.RendererType.Renderer ) { Renderer.sharedMaterial.mainTexture = texture; }
            else if( rendererType == bxrTweenColor.RendererType.Image ) { image.sprite = Sprite.Create( texture, new Rect( 0, 0, texture.width, texture.height ), new Vector2( .5f, .5f ) ); }
            else if( rendererType == bxrTweenColor.RendererType.RawImage ) { rawImage.texture = texture; }
            else if( rendererType == bxrTweenColor.RendererType.SpriteRenderer ) { spriteRenderer.sprite = Sprite.Create( texture, new Rect( 0, 0, texture.width, texture.height ), new Vector2( .5f, .5f ) ); }
            else if( rendererType == bxrTweenColor.RendererType.Material ) { material.mainTexture = texture; }

        } //END SetTexture

        //-----------------------------------//
        public void SetColor( Color color, TweenValue tweenValue )
        //-----------------------------------//
        {

            if( tweenValue == TweenValue.Default ) { color_Default = color; }
            else if( tweenValue == TweenValue.Hide ) { color_Hide = color; }
            else if( tweenValue == TweenValue.Show ) { color_Show = color; }
            else if( tweenValue == TweenValue.HoverEnter ) { color_HoverEnter = color; }
            else if( tweenValue == TweenValue.HoverExit ) { color_HoverExit = color; }
            else if( tweenValue == TweenValue.Pressed ) { color_Pressed = color; }
            else if( tweenValue == TweenValue.Hold ) { color_Hold = color; }
            else if( tweenValue == TweenValue.Enabled ) { color_Enabled = color; }
            else if( tweenValue == TweenValue.Disabled ) { color_Disabled = color; }

        } //END SetColor

        //------------------------------------//
        public Color GetColorFromRenderer()
        //------------------------------------//
        {

            Color color = Color.white;

            if( rendererType == bxrTweenColor.RendererType.Renderer ) { color = Renderer.sharedMaterial.color; }
            else if( rendererType == bxrTweenColor.RendererType.Image ) { color = image.color; }
            else if( rendererType == bxrTweenColor.RendererType.RawImage ) { color = rawImage.color; }
            else if( rendererType == bxrTweenColor.RendererType.SpriteRenderer ) { color = spriteRenderer.color; }
            else if( rendererType == bxrTweenColor.RendererType.Text ) { color = text.color; }
            else if( rendererType == bxrTweenColor.RendererType.Material ) { color = material.color; }
            else if( rendererType == bxrTweenColor.RendererType.CanvasGroup ) { color = new Color( Color.white.r, Color.white.g, Color.white.b, canvasGroup.alpha ); }

            return color;

        } //END GetColorFromRenderer

        //-----------------------------------//
        public Color GetEndColor( TweenValue tweenValue )
        //-----------------------------------//
        {

            if( tweenValue == TweenValue.Default ) { return color_Default; }
            else if( tweenValue == TweenValue.Hide ) { return color_Hide; }
            else if( tweenValue == TweenValue.Show ) { return color_Show; }
            else if( tweenValue == TweenValue.HoverEnter ) { return color_HoverEnter; }
            else if( tweenValue == TweenValue.HoverExit ) { return color_HoverExit; }
            else if( tweenValue == TweenValue.Pressed ) { return color_Pressed; }
            else if( tweenValue == TweenValue.Hold ) { return color_Hold; }
            else if( tweenValue == TweenValue.Enabled ) { return color_Enabled; }
            else if( tweenValue == TweenValue.Disabled ) { return color_Disabled; }

            return color_Default;

        } //END GetEndColor

        //------------------------------------//
        public Color GetStartColor( bxrTweenColor.RendererType rendererType )
        //------------------------------------//
        {

            return GetColorFromRenderer();

        } //END GetStartColor
        

        //------------------------------------//
        public Color GetColorFromRendererWithNewAlpha( float alpha )
        //------------------------------------//
        {

            Color color = GetColorFromRenderer();
            return new Color( color.r, color.g, color.b, alpha );

        } //END GetColorFromRendererWithNewAlpha

        //------------------------------------//
        public Color GetColorWithNewAlpha( float alpha, TweenValue tweenValue )
        //------------------------------------//
        {

            Color color = GetEndColor( tweenValue );
            return new Color( color.r, color.g, color.b, alpha );

        } //END GetColorWithNewAlpha

        //------------------------------------//
        public void Play( Color color, TweenValue tweenValue )
        //------------------------------------//
        {
            SetColor( color, tweenValue );

            base.Play( tweenValue );

        } //END Play
        
        //------------------------------------//
        public void Play( TweenValue tweenValue, Color color, float tweenSpeed, float delay, EaseCurve.EaseType easeCurve, UnityEvent CallOnComplete )
        //------------------------------------//
        {
            SetColor( color, tweenValue );
            SetTweenSpeed( tweenSpeed, tweenValue );
            SetDelay( delay, tweenValue );
            SetEaseType( easeCurve, tweenValue );
            SetOnCompleteEvent( CallOnComplete );

            base.Play( tweenValue, CallOnComplete );

        } //END Play

        //------------------------------------//
        protected override void CallTween( TweenValue tweenValue )
        //------------------------------------//
        {
            base.CallTween( tweenValue );

            Color startColor = GetStartColor( rendererType );
            Color endColor = GetEndColor( tweenValue );
            float tweenSpeed = GetTweenSpeed( tweenValue );
            EaseCurve.EaseType easeType = GetEaseType( tweenValue );
            float delay = GetDelay( tweenValue );
            
            //Debug.Log( "UIColorTweener.cs CallTween( " + tweenValue + " ) object.name = " + name + ", Renderer = " + Renderer + ", Renderer.enabled = " + Renderer.enabled + ", rendererType = " + rendererType + ", endColor = " + endColor + ", onCompleteOrLoop = " + onCompleteOrLoop );

            if( rendererType == bxrTweenColor.RendererType.Renderer )           { tween = Renderer.Color( endColor, tweenSpeed, easeType, startColor, delay, false, onCompleteOrLoop ); }
            else if( rendererType == bxrTweenColor.RendererType.Image )         { tween = image.Color( endColor, tweenSpeed, easeType, startColor, delay, false, onCompleteOrLoop ); }
            else if( rendererType == bxrTweenColor.RendererType.RawImage )      { tween = rawImage.Color( endColor, tweenSpeed, easeType, startColor, delay, false, onCompleteOrLoop ); }
            else if( rendererType == bxrTweenColor.RendererType.SpriteRenderer ){ tween = spriteRenderer.Color( endColor, tweenSpeed, easeType, startColor, delay, false, onCompleteOrLoop ); }
            else if( rendererType == bxrTweenColor.RendererType.Text )          { tween = text.Color( endColor, tweenSpeed, easeType, startColor, delay, false, onCompleteOrLoop ); }
            else if( rendererType == bxrTweenColor.RendererType.Material )      { tween = material.Color( endColor, tweenSpeed, easeType, startColor, delay, false, onCompleteOrLoop ); }
            else if( rendererType == bxrTweenColor.RendererType.CanvasGroup )   { tween = canvasGroup.Color( endColor, tweenSpeed, easeType, startColor, delay, false, onCompleteOrLoop ); }
            
        } //END CallTween



    } //END Class

} //END Namespace