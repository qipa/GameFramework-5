using UnityEngine;
using System;
using System.Collections.Generic;
using AStar.astar;
using AStar;

namespace GOEngine.Implement
{
	public class TileData
	{
        public int tileMapId;
		public string assetName;
		public string litMapName;
		public string aStarDataName;
		public int row;
		public int col;
	}

	#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOEScene : GOEBaseScene
    {
        public int mRayDistance = 4096;
        private GameObject terrain = null;

        public GOEScene()
        {
            this.AddComponent<DetourMgr>();
            //this.AddComponent<ClientHeightMap>();
        }

        //===============================public=property==================================//

        public GameObject Terrain
        {
            get { return terrain; }
        }

        private Action _onEnterScene;
        public Action OnEnterScene
        {
            get { return _onEnterScene; }
            set { _onEnterScene = value; }
        }

        private Action _onDynamicLoadedOk;
        public Action OnDynamicLoadedOk
        {
            get { return _onDynamicLoadedOk; }
            set { _onDynamicLoadedOk = value; }
        }

        private Action _onLeaveScene;
        public Action OnLeaveScene
        {
            get { return _onLeaveScene; }
            set { _onLeaveScene = value; }
        }


        //===============================public=function==================================//
        public void LoadScene(string name, bool withDetour = true, bool clearRes = true)
        {
            if (clearRes)
                leaveScene();
            if (withDetour)
            {
                string navName = "navmesh_" + name + EngineFileUtil.m_bytesExt;
                Create(name, navName);
            }
            else
            {
                Create(name);
            }
        }

        internal void DeInit()
        {
            terrain = null;
           
			this.GetComponent<DetourMgr>().UnLoad();
            //this.GetComponent<ClientHeightMap>().Clear();
        }

        private void terrainXCheck( )
        {
            
        }

        private void Create(string name)
        {
            GOERoot.ResMgrImp.GetScene(name, null);
			//Debug.Log ( GOERoot.FrameNum +   " getscene " + name);
        }

        private void Create(string name, string navmesh)
        {
            GOERoot.ResMgrImp.GetAsset(navmesh, this.OnNavmeshLoaded);
            Create(name);
        }

        private void leaveScene()
        {
            OnLeave();
        }
        
        private void OnNavmeshLoaded(string name, UnityEngine.Object obj)
        {
            TextAsset ta = obj as TextAsset;
            if(ta == null)
            {
                Logger.GetFile( LogFile.Global ).LogError("asset is null : " + name);
                return;
            }
            
            _initTerrain(ta.bytes);
        }

        //private void OnHmdLoaded(string name, string content)
        //{
        //    if (content != null)
        //        this.GetComponent<ClientHeightMap>().ReadData(content);
        //}
        private void _initTerrain(byte[] navmesh)
        {
            this.GetComponent<DetourMgr>().LoadNavMesh(navmesh);
        }

        private GameObject GetTerrainGroup()
        {
            return GameObject.Find( "terrain" );
        }

        public bool getIntersectTerrainPos(IDetourSender sender, ref Vector3 terrainPos)
        {
            float height = this.GetComponent<DetourMgr>().GetHeight(sender, terrainPos);
            if( (height - 0.0f) > 0.0001f )
            {
                terrainPos.y = height;
                return true;
            }
            //Debug.Log("heright = 0.0f");
            return false;
        }

        protected bool getIntersectTerrainPos(Ray ray, ref Vector3 terrainPos, ref Vector3 terrainNormal)
        {
            return GetIntersectTerrainPos(ray, ref terrainPos, ref terrainNormal, mRayDistance);
        }

        public static bool GetIntersectTerrainPos(Camera camera, Vector3 screenPos, ref Vector3 terrainPos,ref Vector3 terrainNormal, int rayDistance = 1024)
        {
            Ray ray = camera.ScreenPointToRay(screenPos);
            return GetIntersectTerrainPos(ray, ref terrainPos, ref terrainNormal, rayDistance);
        }

        public static bool GetIntersectTerrainPos(Ray ray, ref Vector3 terrainPos,ref Vector3 terrainNormal, int rayDistance = 1024)
        {
            RaycastHit[] list = Physics.RaycastAll(ray, rayDistance, 1 << LayerDef.Walk_Surface);

            float dis = rayDistance; //far enough
            foreach (RaycastHit hit in list)
            {
                if (hit.distance < dis)
                {
                    dis = hit.distance;
                    terrainPos = hit.point;
                    terrainNormal = hit.normal;
                    return true;
                }
            }
            return false;
        }


        internal override void OnEnter()
        {
            this.terrain = this.GetTerrainGroup();
            if (terrain == null)
            {
                Logger.GetFile(LogFile.Global).LogWarning("can not find terrain");
                return;
            }

            base.OnEnter();
            AudioListener listener = terrain.GetComponentInChildren<AudioListener>();
            if (GOERoot.GOEQualityMgr.CurQualityType() == QualityType.Height)
            {
                GOERoot.GOEQualityMgr.HeightQuality();
            }
            if (GOERoot.GOEQualityMgr.CurQualityType() == QualityType.Mid)
            {
                GOERoot.GOEQualityMgr.MidQuality();
            }
            if (GOERoot.GOEQualityMgr.CurQualityType() == QualityType.Low)
            {
                GOERoot.GOEQualityMgr.LowQuality();
            }


            if (listener != null)
                listener.enabled = false;

            bool isRandomTile = false;
            GOESceneRandomTile rancomp = this.GetComponent<GOESceneRandomTile>();
            if (rancomp != null)
            {
                isRandomTile = rancomp.GetTileCount() > 0;
            }
            bool isBigTerrainX = false;
            string levelName = Application.loadedLevelName;
            if (levelName.Contains("_pack"))
            {
                isBigTerrainX = true;
            }

            if (OnEnterScene != null)
                OnEnterScene();

            if (isRandomTile || isBigTerrainX)
            {
                Debug.Log("enter level but delay enter scene");
            }
            else
            {
                if (OnDynamicLoadedOk != null)
                {
                    OnDynamicLoadedOk();
                }
            }
        }

        internal override void OnLeave()
        {
            base.OnLeave();

            if (OnLeaveScene != null)
                OnLeaveScene();
            
            GOERoot.ResMgrImp.OnLeaveScene();
            this.DeInit();
        }
    }
}