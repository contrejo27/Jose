using UnityEngine;

namespace BrandXR
{
    public class bxrCubemap
    {
        public Texture2D front;
        public Texture2D back;
        public Texture2D left;
        public Texture2D right;
        public Texture2D top;
        public Texture2D bottom;

        //--------------------------------------------//
        public Texture2D GetTexture( XRSkyboxFactory.CubeSide side )
        //--------------------------------------------//
        {
            switch( side )
            {
                case XRSkyboxFactory.CubeSide.front:
                return front;
                case XRSkyboxFactory.CubeSide.back:
                return back;
                case XRSkyboxFactory.CubeSide.left:
                return left;
                case XRSkyboxFactory.CubeSide.right:
                return right;
                case XRSkyboxFactory.CubeSide.top:
                return top;
                case XRSkyboxFactory.CubeSide.bottom:
                return bottom;
                default: return null;
            }

        } //END GetTexture

        //--------------------------------------------//
        public bool CheckIfTextureIsMissing()
        //--------------------------------------------//
        {
            if( front == null || back == null || left == null || right == null || top == null || bottom == null )
                return true;
            else
                return false;

        } //END CheckIfTextureIsMissing

    } //END Class

} //END Namespace