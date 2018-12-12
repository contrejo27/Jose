using UnityEngine;
using FFE;

namespace BrandXR
{
    public class VideoCopy: MonoBehaviour
    {

        public BlockVideo blockVideo;
        public Material copyMaterial;
        
        //----------------------------------//
        public void Start()
        //----------------------------------//
        {

            if( blockVideo == null && transform.GetComponentInParent<BlockVideo>() != null )
            {
                blockVideo = transform.GetComponentInParent<BlockVideo>();
            }

            if( copyMaterial == null &&
                blockVideo != null &&
                blockVideo.videoPlane1 != null &&
                ( blockVideo.Is3DSideBySide() || blockVideo.Is3DTopBottom() ) )
            {
                copyMaterial = blockVideo.videoPlane1.material;
            }
            else if( copyMaterial == null &&
                blockVideo != null &&
                blockVideo.videoSphere1 != null &&
                ( blockVideo.Is3DSideBySide() || blockVideo.Is3DTopBottom() ) )
            {
                copyMaterial = blockVideo.videoSphere1.material;
            }

        } //END Start
        

    } //END Class

} //END Namespace