using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using System;

namespace BrandXR
{
    public class UIColorAnimation: UITweenAnimation
    {
        
        public bxrTweenColor.RendererType rendererType = bxrTweenColor.RendererType.RawImage;

        [ShowIf("rendererType", bxrTweenColor.RendererType.Renderer)]
        public Renderer[] renderers;
        [ShowIf( "rendererType", bxrTweenColor.RendererType.Image )]
        public Image[] images;
        [ShowIf( "rendererType", bxrTweenColor.RendererType.RawImage )]
        public RawImage[] rawImages;
        [ShowIf( "rendererType", bxrTweenColor.RendererType.SpriteRenderer )]
        public SpriteRenderer[] spriteRenderers;
        [ShowIf( "rendererType", bxrTweenColor.RendererType.Text )]
        public Text[] texts;
        [ShowIf( "rendererType", bxrTweenColor.RendererType.Material )]
        public Material[] materials;
        [ShowIf( "rendererType", bxrTweenColor.RendererType.CanvasGroup )]
        public CanvasGroup[] canvasGroups;

        public Color[] colorsToAnimateTo;

        //-----------------------------------//
        protected override void FindInitialValues()
        //-----------------------------------//
        {

            if( renderers == null && images == null && rawImages == null && spriteRenderers == null && texts == null && materials == null && canvasGroups == null )
            {
                if( transform.GetComponent<MeshRenderer>() != null )
                {
                    renderers = new Renderer[] { transform.GetComponent<Renderer>() };
                    rendererType = bxrTweenColor.RendererType.Renderer;
                }
                else if( transform.GetComponent<Image>() != null )
                {
                    images = new Image[] { transform.GetComponent<Image>() };
                    rendererType = bxrTweenColor.RendererType.Image;
                }
                else if( transform.GetComponent<RawImage>() != null )
                {
                    rawImages = new RawImage[] { transform.GetComponent<RawImage>() };
                    rendererType = bxrTweenColor.RendererType.RawImage;
                }
                else if( transform.GetComponent<SpriteRenderer>() != null )
                {
                    spriteRenderers = new SpriteRenderer[] { transform.GetComponent<SpriteRenderer>() };
                    rendererType = bxrTweenColor.RendererType.SpriteRenderer;
                }
                else if( transform.GetComponent<Text>() != null )
                {
                    texts = new Text[] { transform.GetComponent<Text>() };
                    rendererType = bxrTweenColor.RendererType.Text;
                }
                else if( transform.GetComponent<Material>() != null )
                {
                    materials = new Material[] { transform.GetComponent<Material>() };
                    rendererType = bxrTweenColor.RendererType.Material;
                }
                else if( transform.GetComponent<CanvasGroup>() != null )
                {
                    canvasGroups = new CanvasGroup[] { transform.GetComponent<CanvasGroup>() };
                    rendererType = bxrTweenColor.RendererType.CanvasGroup;
                }
            }

        } //END FindInitialValues

        //-----------------------------------//
        protected override void ForceDefaults()
        //-----------------------------------//
        {

            base.ForceDefaults();

            if( colorsToAnimateTo == null )
            {
                colorsToAnimateTo = new Color[] { Color.white, Color.white };
            }

        } //END ForceDefaults

        //--------------------------------------------//
        public override void Play()
        //--------------------------------------------//
        {

            base.Play();

            if( currentAnimationCounter >= colorsToAnimateTo.Length )
            {
                currentAnimationCounter = 0;
            }

            UnityEvent _event = new UnityEvent();
            _event.AddListener( Play );
            
            if( rendererType == bxrTweenColor.RendererType.Renderer )
            {
                foreach( Renderer renderer in renderers )
                {
                    bxrTween tween = renderer.Color( colorsToAnimateTo[ currentAnimationCounter ], animationSpeeds[ currentAnimationCounter ], easeTypes[ currentAnimationCounter ], renderer.sharedMaterial.color, delays[ currentAnimationCounter ], false, _event );
                    tween.PreventDestroyOnComplete();
                    tweens.Add( tween );
                }
            }
            else if( rendererType == bxrTweenColor.RendererType.Image )
            {
                foreach( Image renderer in images )
                {
                    bxrTween tween = renderer.Color( colorsToAnimateTo[ currentAnimationCounter ], animationSpeeds[ currentAnimationCounter ], easeTypes[ currentAnimationCounter ], renderer.color, delays[ currentAnimationCounter ], false, _event );
                    tween.PreventDestroyOnComplete();
                    tweens.Add( tween );
                }
            }
            else if( rendererType == bxrTweenColor.RendererType.RawImage )
            {
                foreach( RawImage renderer in rawImages )
                {
                    bxrTween tween = renderer.Color( colorsToAnimateTo[ currentAnimationCounter ], animationSpeeds[ currentAnimationCounter ], easeTypes[ currentAnimationCounter ], renderer.color, delays[ currentAnimationCounter ], false, _event );
                    tween.PreventDestroyOnComplete();
                    tweens.Add( tween );
                }
            }
            else if( rendererType == bxrTweenColor.RendererType.SpriteRenderer )
            {
                foreach( SpriteRenderer renderer in spriteRenderers )
                {
                    bxrTween tween = renderer.Color( colorsToAnimateTo[ currentAnimationCounter ], animationSpeeds[ currentAnimationCounter ], easeTypes[ currentAnimationCounter ], renderer.color, delays[ currentAnimationCounter ], false, _event );
                    tween.PreventDestroyOnComplete();
                    tweens.Add( tween );
                }
            }
            else if( rendererType == bxrTweenColor.RendererType.Text )
            {
                foreach( Text renderer in texts )
                {
                    bxrTween tween = renderer.Color( colorsToAnimateTo[ currentAnimationCounter ], animationSpeeds[ currentAnimationCounter ], easeTypes[ currentAnimationCounter ], renderer.color, delays[ currentAnimationCounter ], false, _event );
                    tween.PreventDestroyOnComplete();
                    tweens.Add( tween );
                }
            }
            else if( rendererType == bxrTweenColor.RendererType.Material )
            {
                foreach( Material renderer in materials )
                {
                    bxrTween tween = renderer.Color( colorsToAnimateTo[ currentAnimationCounter ], animationSpeeds[ currentAnimationCounter ], easeTypes[ currentAnimationCounter ], renderer.color, delays[ currentAnimationCounter ], false, _event );
                    tween.PreventDestroyOnComplete();
                    tweens.Add( tween );
                }
            }
            else if( rendererType == bxrTweenColor.RendererType.CanvasGroup )
            {
                foreach( CanvasGroup renderer in canvasGroups )
                {
                    bxrTween tween = renderer.Color( colorsToAnimateTo[ currentAnimationCounter ], animationSpeeds[ currentAnimationCounter ], easeTypes[ currentAnimationCounter ], new Color( Color.white.r, Color.white.g, Color.white.b, renderer.alpha ), delays[ currentAnimationCounter ], false, _event );
                    tween.PreventDestroyOnComplete();
                    tweens.Add( tween );
                }
            }

            currentAnimationCounter++;

        } //END Play


    } //END Class

} //END Namespace