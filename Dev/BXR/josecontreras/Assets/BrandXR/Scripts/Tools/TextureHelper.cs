using UnityEngine;

namespace BrandXR
{
    class TextureHelper
    {

        //--------------------------------//
        public static Sprite CreateSpriteFromTexture( ref Texture texture )
        //--------------------------------//
        {
            Sprite sprite = Sprite.Create( (Texture2D)texture, new Rect( 0f, 0f, texture.width, texture.height ), new Vector2( .5f, .5f ) );
            sprite.name = texture.name;

            return sprite;

        } //END CreateSpriteFromTexture

    }
}
