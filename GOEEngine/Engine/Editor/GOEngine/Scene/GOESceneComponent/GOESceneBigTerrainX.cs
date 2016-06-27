using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
 class GOESceneBigTerrainX : GOESceneComponent
    {
        // @xl 用于卸载的时间 //
        private float mTimeToDelete = 5;
        private bool mHaveSentEnterScene = false;
        private int mCurrID = -1;
        // 保存上一次的所有加载的lod的prefab //
        private List<string> mLastCells = new List<string>();
        // 失效的lod的prefab //
        private List<string> mUnUsedCells = new List<string>();
        // 所有加载的（缓存的）lod的prefab和其对应的Gameobject //
        private Dictionary<string, GameObject> mAllLoadObjs = new Dictionary<string, GameObject>();
        private GameObject mTerrainxObj = null;
        private TerrainBehaviour mTerrainBeh = null;
        private GOEScene mScene = null;
        private bool mNeedDynamicLoad = false;

        internal override void OnEnter()
        {
            base.OnEnter();
            mCurrID = -1;
            mAllLoadObjs.Clear();
            mUnUsedCells.Clear();
            mTerrainxObj = null;
            mTerrainBeh = null;
            mNeedDynamicLoad = false;
            mTerrainxObj = GameObject.Find("TerrainX");
            mTerrainBeh = null;
            if (mTerrainxObj != null)
            {
                mTerrainBeh = mTerrainxObj.GetComponent<TerrainBehaviour>();
                MeshRenderer[] mrs = mTerrainxObj.GetComponentsInChildren<MeshRenderer>(true);
                if (mrs.Length <= 0)
                {
                    mNeedDynamicLoad = true;
                }
            }
            mHaveSentEnterScene = false;
            mScene = this.Owner as GOEScene;
        }

        internal override void Update()
        {
            base.Update();
            
            if (!mNeedDynamicLoad)
            {
                return;
            }
            if (mTerrainxObj == null)
            {
                return;
            }
            if (mTerrainBeh == null)
            {
                return;
            }
            mTimeToDelete -= Time.deltaTime;
            if ((mTimeToDelete < 0) && (mUnUsedCells.Count > 0))
            {
                // 当时间到了就卸载删除地块 //
                string slog = "unload cells ";
                foreach (string assetName in mUnUsedCells)
                {
                    GameObject delObj = null;
                    if (mAllLoadObjs.TryGetValue(assetName, out delObj))
                    {
                        GOERoot.ResMgrImp.ReleaseAsset(assetName + ".prefab", delObj);
                        mAllLoadObjs.Remove(assetName);
                        slog += " " + assetName + ";";
                    }
                }
                //Debug.LogError("暂时log：卸载地块" + slog);
                mUnUsedCells.Clear();
            }

            int curid = mTerrainBeh.GetCenterChunkID();
            if (curid == mCurrID)
            {
                return;
            }
            mCurrID = curid;
            //Debug.Log(curid.ToString());
            string prefix = Application.loadedLevelName;
            prefix = prefix.Replace("_pack", "");
            string[] nextLodName = mTerrainBeh.GetVisibleLodArray();
            //Debug.Log("临时稳删：getvisiblelodarray.length = " + nextLodName.Length);
            //Debug.Log("临时稳删：curid = " + curid.ToString() + "mCurid = " + mCurrID.ToString());
            List<string> newCells = new List<string>();
            for (int i = 0; i < nextLodName.Length; ++i)
            {
                newCells.Add(prefix + "_" + nextLodName[i]);
            }
            foreach( string assetName in newCells )
            {
                // 如果待删除列表里有新的需要显示的则移除 //
                if( mUnUsedCells.Contains(assetName))
                {
                    mUnUsedCells.Remove(assetName);
                }
            }
            foreach( string assetName in mLastCells )
            {
                // 把上一次列表里面有的，但新的显示列表没有的添加到待删除列表 //
                if (!newCells.Contains(assetName))
                {
                    mUnUsedCells.Add(assetName);
                }
            }
            // 开始计时 //
            mTimeToDelete = 5f;
            mLastCells.Clear();
            mLastCells.AddRange(newCells);
            RefreshCells(newCells);
        }

        public void RefreshCells(List<string> newAssetNames)
        {
            // @xl 当玩家在所有的地块之外，要通知客户端关闭loading //
            if (newAssetNames.Count == 0)
            {
                if (!mHaveSentEnterScene)
                {
                    mHaveSentEnterScene = true;
                    mScene.OnDynamicLoadedOk();
                    Debug.Log("加载地块完成通知，在terrainx外...");
                }
                return;
            }
            //Debug.Log("临时稳删：第一个地块" + newAssetNames[0]);
            LoadCell(newAssetNames[0], (string backName) =>
            {
                // @xl 加载中间地块成功，通知逻辑端 //
                if (!mHaveSentEnterScene)
                {
                	mHaveSentEnterScene = true;
                    mScene.OnDynamicLoadedOk();
                    Debug.Log("加载地块完成通知...");
                }
            }, LoadPriority.MostPrior);

            for (int i = 1; i < newAssetNames.Count; ++i)
            {
                string newcell = newAssetNames[i];
                //Debug.Log("临时稳删：第 " + i.ToString() + " 个地块" + newAssetNames[i]);
                LoadCell(newcell, (string cellName) =>
                {
                }, LoadPriority.Default);
            }
            string slog = "refresh cells ";
            foreach (string cn in newAssetNames)
            {
                slog += " " + cn + ";";
            }
            //Debug.LogError("暂时log：刷新地块" + slog);

        }

        private GameObject FindGameObjectInChild( GameObject go, string name)
        {
            for( int i = 0; i < go.transform.childCount; ++i)
            {
                //if(go.transform.GetChild(i).Equals(name))
                if (go.transform.GetChild(i).name.Equals(name))
                {
                    return go.transform.GetChild(i).gameObject;
                }
            }
            return null;
        }

        private void LoadCell(string newcell, Action<string> callback, LoadPriority lp)
        {
            string lumdaName = newcell;
            if (!mAllLoadObjs.Keys.Contains(lumdaName))
            {
                // @xl lumdaName，也就是lod地块prefab的名字是这个格式"场景名_Chunk_数字_LOD_数字"
                //Debug.LogError("really load ::" + newcell);
                string pName = lumdaName + ".prefab";
                string bundleName = GOERoot.ResMgr.GetBundleName(pName);
                if( string.IsNullOrEmpty(bundleName))
                {
                    return;
                }
                GOERoot.ResMgr.GetAsset(pName, (string prefabName, UnityEngine.Object obj) =>
                {
                    callback(lumdaName);
                    string[] arr = lumdaName.Split('_');
                    if (arr.Length != 5)
                    {
                        Debug.Log("包名不符合规则 " + lumdaName);
                        return;
                    }
                    //string chunkname = arr[2] + " " + arr[3];
                    string chunkname = arr[1] + " " + arr[2];
                    // @xl 查找Chunk 3的节点
                    GameObject chunkObj = FindGameObjectInChild(mTerrainBeh.gameObject, chunkname);
                    GameObject go = obj as GameObject;
                    if (go != null)
                    {
                        // @xl 查找LOD_3 //
                        //GameObject lodObj = FindGameObjectInChild(chunkObj, arr[4] + "_" + arr[5]);
                        GameObject lodObj = FindGameObjectInChild(chunkObj, arr[3] + "_" + arr[4]);
                        go.transform.parent = lodObj.transform;
                        go.transform.localEulerAngles = Vector3.zero;
                        go.transform.localPosition = Vector3.zero;
                        MeshRenderer[] mrs = go.GetComponentsInChildren<MeshRenderer>();
                        foreach (MeshRenderer mr in mrs)
                        {
                            MeshLightmapSetting mls = mr.gameObject.GetComponent<MeshLightmapSetting>();
                            if (mls != null)
                            {
                                mls.LoadSettings();
                            }
                        }
                        BigTerrainXSetting sett = go.GetComponent<BigTerrainXSetting>();
                        if (sett != null)
                        {
                            sett.load = true;
                        }
                        mAllLoadObjs.Add(lumdaName, go);
                        mTerrainBeh.ChunkVisableUpdate(true);
                    }
                }, lp);
            }
        }
    }
}
