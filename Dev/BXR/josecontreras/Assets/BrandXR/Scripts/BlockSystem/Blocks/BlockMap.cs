using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if MAPBOX
using Mapbox;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Factories;
using Mapbox.Unity.Utilities;
#endif

using Sirenix.OdinInspector;

namespace BrandXR
{
    public abstract class BlockMap : Block
    {
        protected override bool ShowFaceCameraEvent() { return false; }

        protected abstract void PlaceMarker();

        public override void Update()
        {
            base.Update();
            
            if (Input.GetMouseButtonUp(1))
                PlaceMarker();
        }
    }
}
