using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using AStar.astar;

namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
 class GOESceneRandomTile : GOESceneComponent
    {
        protected List<TileData> tileDatas = new List<TileData>();

        public void Clear()
        {
            tileDatas.Clear();
        }

        public void AddTile(TileData tile)
        {
            tileDatas.Add(tile);
        }

        public int GetTileCount()
        {
            return tileDatas.Count;
        }

        internal override void OnEnter()
        {
            base.OnEnter();
            //Debug.Log("@临时稳删 scene random tile enter at frame = " + GOERoot.FrameNum);
            if (tileDatas.Count <= 0) 
			{
				return;
			}

            GOEScene scene = this.Owner as GOEScene;
            if (tileDatas.Count > 0)
            {
				List<AStarTile> astarData = new List<AStarTile>();
                for (int i = 0; i < tileDatas.Count; ++i)
                {
                    TileData td = tileDatas[i];
                    GOERoot.ResMgrImp.GetAsset(td.aStarDataName, (string tempName, UnityEngine.Object tempObj) =>
                    {
                        TextAsset tempGO = tempObj as TextAsset;
						astarData.Add(new AStarTile(td.row, td.col, tempGO.text));
						if (astarData.Count == tileDatas.Count)
                        {
							scene.GetComponent<DetourMgr>().LoadAStar(astarData);
                        }
                    });
                }

                List<string> lightMaps = new List<string>();
                for (int i = 0; i < tileDatas.Count; ++i)
                {
                    TileData td = tileDatas[i];
                    if (!lightMaps.Contains(td.litMapName))
                    {
                        lightMaps.Add(td.litMapName);
                    }
                }

                //Debug.Log("@xulin enter random tile at frame = " + GOERoot.FrameNum);
                LightmapData[] lmds = new LightmapData[lightMaps.Count];
                int lightMapCount = lightMaps.Count;
                for (int i = 0; i < lightMaps.Count; ++i)
                {
                    string litmapName = lightMaps[i];
                    //Debug.Log("@临时稳删 get one litmap at frame = " + GOERoot.FrameNum);
                    GOERoot.ResMgrImp.GetAsset(litmapName, (string tempName, UnityEngine.Object tempObj) =>
                                               {
                                                   --lightMapCount;
                                                   Texture2D litMapTex = tempObj as Texture2D;
                                                   LightmapData lmd = new LightmapData();
                                                   lmd.lightmapFar = litMapTex;
                                                   int litIndex = lightMaps.IndexOf(tempName);
                                                   lmds[litIndex] = lmd;
                                                   //Debug.Log("@xulin loaded one litmap at frame = " + GOERoot.FrameNum);
                                                   if (lightMapCount == 0)
                                                   {
                                                       //Debug.Log("@xulin loaded all litmaps at frame = " + GOERoot.FrameNum);
                                                       LightmapSettings.lightmaps = lmds;
                                                       int tileCount = tileDatas.Count;
                                                       //Debug.Log("count " + tileCount);
                                                       for (int j = 0; j < tileDatas.Count; ++j)
                                                       {
                                                           TileData td = tileDatas[j];
                                                           //Debug.Log("xulin --- get asset " + td.assetName);
                                                           GOERoot.ResMgrImp.GetAsset(td.assetName, (string tempTileName, UnityEngine.Object tempTileObj) =>
                                                           {
                                                               //Debug.Log("@xulin loaded one prefab at frame = " + GOERoot.FrameNum);

                                                               //Debug.Log("xulin --- get asset loaded " + td.assetName);
                                                               --tileCount;
                                                               if (tileCount <= 0)
                                                               {
                                                                   //Debug.Log("@xulin loaded all prefabs at frame = " + GOERoot.FrameNum);
                                                                   if (scene != null && scene.OnDynamicLoadedOk != null)
                                                                   {
                                                                       scene.OnDynamicLoadedOk();
                                                                       //Debug.Log("OnDynamicLoadedOk ");
                                                                   }
                                                               }
                                                               GameObject tempTileGO = tempTileObj as GameObject;
                                                               if (tempTileGO == null)
                                                               {
                                                                   //Debug.Log("tempTileGO == null" + tempTileName);
                                                               }
                                                               Transform go = null;
                                                               for (int xx = 0; xx < tempTileGO.transform.childCount; ++xx)
                                                               {
                                                                   go = tempTileGO.transform.GetChild(xx);
                                                                   if (go != null && go.name.Contains("AStars"))
                                                                   {
                                                                       go.gameObject.SetActive(false);
                                                                   }
                                                               }
                                                               tempTileGO.transform.parent = scene.Terrain.transform;
															   tempTileGO.transform.localPosition = (AStartDetour.ROL_COUNT - td.row) * AStartDetour.TILE_SIZE * Vector3.forward + td.col * AStartDetour.TILE_SIZE * Vector3.right;
                                                               tempTileGO.transform.localEulerAngles = Vector3.zero;
                                                               litIndex = lightMaps.IndexOf(td.litMapName);
                                                               //Debug.Log(td.litMapName + " " + litIndex);
                                                               MeshRenderer[] mrs = tempTileGO.GetComponentsInChildren<MeshRenderer>();
                                                               foreach (MeshRenderer mr in mrs)
                                                               {
                                                                   mr.lightmapIndex = litIndex;
                                                                   MeshLightmapSetting mls = mr.gameObject.GetComponent<MeshLightmapSetting>();
                                                                   if (mls != null)
                                                                   {
                                                                       mls.lightmapIndex = litIndex;
                                                                       mls.LoadSettings();
                                                                   }
                                                               }
                                                           }, LoadPriority.MostPrior);
                                                       }
                                                   }
                                               }, LoadPriority.MostPrior);
                }


            }
        }

        internal override void Update()
        {
            base.Update();
        }

    }
}
