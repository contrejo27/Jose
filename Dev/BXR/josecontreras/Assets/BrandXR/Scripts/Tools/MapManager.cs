//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace BrandXR
{
    public class MapManager: SerializedMonoBehaviour
    {
        #region GETTERS/SETTERS
        public GameObject CurrentMap { get { return currentMap; } }
        #endregion

        [FoldoutGroup( "Map References" )]
        public GameObject zoomableMap;
        [FoldoutGroup( "Map References" )]
        public GameObject locationMap;

        [SerializeField]
        private GameObject currentMap;

        private void Awake()
        {
            #if MAPBOX == false
                if( currentMap ) Destroy( currentMap.gameObject );
            #endif
        }

        public void SpawnMap( GameObject mapToSpawn )
        {
            if( currentMap )
                DestroyCurrentMap();

            currentMap = Instantiate( mapToSpawn );
        }

        [BoxGroup( "Map Debugs" ), Button( ButtonSizes.Medium )]
        public void SpawnZoomableMap()
        {
            if( zoomableMap )
                SpawnMap( zoomableMap );
            else
                Debug.LogError( "No Zoomable Map set in [MapManager]" );
        }

        [BoxGroup( "Map Debugs" ), Button( ButtonSizes.Medium )]
        public void SpawnLocationMap()
        {
            if( locationMap )
                SpawnMap( locationMap );
            else
                Debug.LogError( "No Location Map set in [MapManager]" );
        }

        [BoxGroup( "Map Debugs", order: 0 ), Button( ButtonSizes.Medium ), ShowIf( "IsCurrentMap" )]
        public void DestroyCurrentMap()
        {
            if( Application.isPlaying )
                Destroy( currentMap );
            else
                DestroyImmediate( currentMap );
        }

        private bool IsCurrentMap()
        {
            return currentMap != null;
        }

    } //END Class

} //END Namespace