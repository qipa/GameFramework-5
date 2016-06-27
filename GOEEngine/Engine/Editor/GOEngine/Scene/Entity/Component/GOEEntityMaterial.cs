using System;
using UnityEngine;
using System.Collections.Generic;
namespace GOEngine
{
    public struct MaterialEffectInfo
    {
        public static Dictionary<string, Texture2D> ReplaceTextureDic;
        public string name;
        public string shaderName;
        public string behaviourName;
        public Dictionary<string, Color> materialColorParamDic;
        public Dictionary<string, Vector4> materialVectorParamDic;
        public Dictionary<string, Texture2D> materialTextureParamDic;
        public Dictionary<string, float> materialFloatParamDic;
        public List<string> materialCopyTextureParamDic;
        public List<string> materialCopyColorParamDic;
    }
}
namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
        internal
#endif
 class MaterialEffect
    {
        public MaterialEffectInfo mInfo;
        private UnityEngine.Component mBehaviour = null;
        private Dictionary<Renderer, Material[]> mNewMatDic = new Dictionary<Renderer, Material[]>();
        private Entity mEntity = null;
        private Material mMaterial;
        public MaterialEffect(MaterialEffectInfo info, Entity entity)
        {
            mInfo = info;
            mEntity = entity;
            Init();
        }

        private void Init()
        {
            // 初始化 //
            Shader shader = Shader.Find(mInfo.shaderName);
            if (shader == null)
            {
                Logger.GetFile(LogFile.Res).LogWarning("can not find shader render " + mInfo.shaderName);
                return;
            }
            if (mEntity == null)
            {
                return;
            }
            mMaterial = new Material(shader);
            //UnityEngine.Object.DontDestroyOnLoad(mMaterial);

            // 设置材质参数 //
            {
                if (mInfo.materialColorParamDic != null && mInfo.materialColorParamDic.Count > 0)
                {
                    foreach (KeyValuePair<string, Color> kayValue in mInfo.materialColorParamDic)
                    {
                        mMaterial.SetColor(kayValue.Key, kayValue.Value);
                    }
                }

                if (mInfo.materialVectorParamDic != null && mInfo.materialVectorParamDic.Count > 0)
                {
                    foreach (KeyValuePair<string, Vector4> kayValue in mInfo.materialVectorParamDic)
                    {
                        mMaterial.SetVector(kayValue.Key, kayValue.Value);
                    }
                }

                if (mInfo.materialFloatParamDic != null && mInfo.materialFloatParamDic.Count > 0)
                {
                    foreach (KeyValuePair<string, float> kayValue in mInfo.materialFloatParamDic)
                    {
                        mMaterial.SetFloat(kayValue.Key, kayValue.Value);
                    }
                }

                if (mInfo.materialTextureParamDic != null && mInfo.materialTextureParamDic.Count > 0)
                {
                    foreach (KeyValuePair<string, Texture2D> kayValue in mInfo.materialTextureParamDic)
                    {
                        if (kayValue.Value == null)
                        {
                            Texture2D tex;
                            if (MaterialEffectInfo.ReplaceTextureDic.TryGetValue(kayValue.Key, out tex))
                            {
                                mMaterial.SetTexture(kayValue.Key, tex);
                            }
                        }
                        else
                        {
                            mMaterial.SetTexture(kayValue.Key, kayValue.Value);
                        }                            
                        
                    }
                }

                // 挂上脚本 //
                if (!string.IsNullOrEmpty(mInfo.behaviourName))
                {
                    //mBehaviour = mEntity.GameObject.AddComponent(mInfo.behaviourName);
                }

            }

            GameObject go = mEntity.GameObject;
            Renderer[] rendererArray = go.GetComponentsInChildren<Renderer>(true) as Renderer[];
            for (int i = 0; i < rendererArray.Length; ++i)
            {
                Renderer r = rendererArray[i];

                if ( LayerDef.Effect == r.transform.gameObject.layer)
                {
                    continue;
                }

                if (!(r is MeshRenderer || r is SkinnedMeshRenderer))
                {
                    continue;
                }
                Material[] ms = null;


                if (!mNewMatDic.TryGetValue(r, out ms))
                {
                    ms = new Material[r.materials.Length];
                    for (int j = 0; j < r.materials.Length; ++j)
                    {
                        if (r.materials[j] != null)
                        {
                            Material m = GameObject.Instantiate(mMaterial) as Material;
                 
                            m.SetTexture("_MainTex", r.materials[j].mainTexture);
                            if(mInfo.materialCopyTextureParamDic != null)
                            {
                                
                                for(int l = 0; l < mInfo.materialCopyTextureParamDic.Count; l ++)
                                {
                                    string p = mInfo.materialCopyTextureParamDic[l];
                                    if (r.materials[j].HasProperty(p))
                                    {
                                        m.SetTexture(p, r.materials[j].GetTexture(p));
                                    }
                                }
                            }

                            if (mInfo.materialCopyColorParamDic != null)
                            {
                                for (int l = 0; l < mInfo.materialCopyColorParamDic.Count; l++)
                                {
                                    string p = mInfo.materialCopyColorParamDic[l];
                                    if (r.materials[j].HasProperty(p))
                                    {
                                        m.SetColor(p, r.materials[j].GetColor(p));
                                    }
                                }
                            }
                            ms[j] = m;
                        }
                    }
                    mNewMatDic.Add(r, ms);
                }
            }
        }

        //更新参数，不包括贴图
        private void UpdateMaterialParamDic(Material material)
        {
            //替换材质的参数
            if (mInfo.materialColorParamDic != null && mInfo.materialColorParamDic.Count > 0)
            {
                foreach (KeyValuePair<string, Color> kayValue in mInfo.materialColorParamDic)
                {
                    material.SetColor(kayValue.Key, kayValue.Value);
                }
            }

            if (mInfo.materialVectorParamDic != null && mInfo.materialVectorParamDic.Count > 0)
            {
                foreach (KeyValuePair<string, Vector4> kayValue in mInfo.materialVectorParamDic)
                {
                    material.SetVector(kayValue.Key, kayValue.Value);
                }
            }

            if (mInfo.materialFloatParamDic != null && mInfo.materialFloatParamDic.Count > 0)
            {
                foreach (KeyValuePair<string, float> kayValue in mInfo.materialFloatParamDic)
                {
                    material.SetFloat(kayValue.Key, kayValue.Value);
                }
            }

        }
        /// <summary>
        /// 重新传入info,调用此方法z 
        /// </summary>
        /// <param name="updateInfo"></param>
        internal void ReplaceMaterial(bool updateInfo = false)
        {
            // 替换材质 //
            if (mNewMatDic == null || mNewMatDic.Count <= 0)
            {
                return;
            }

            GameObject go = mEntity.GameObject;
            Renderer[] rendererArray = go.GetComponentsInChildren<Renderer>(true) as Renderer[];
            for (int i = 0; i < rendererArray.Length; ++i)
            {
                Renderer r = rendererArray[i];

                if (LayerDef.Effect == r.transform.gameObject.layer || LayerDef.FakeShadow == r.transform.gameObject.layer || LayerDef.LIGHT_FACE == r.transform.gameObject.layer)
                {
                    continue;
                }

                if (!(r is MeshRenderer || r is SkinnedMeshRenderer))
                {
                    continue;
                }
                Material[] ms = null;
                if (mNewMatDic.TryGetValue(r, out ms))
                {
                    if (updateInfo)
                    {
                        for (int mi =0;  mi< ms.Length;mi++)
                        {
                            UpdateMaterialParamDic(ms[mi]);
                        }
                    }
                    r.materials = ms;
                }

            }

        }

        internal void DestoryMaterial()
        {
            if (mNewMatDic == null)
            {
                return;
            }

            foreach (KeyValuePair<Renderer, Material[]> kayValue in mNewMatDic)
            {
                Material[] mats = kayValue.Value;
                for (int i = 0; i < mats.Length; ++i)
                {
                    Material.Destroy(mats[i]);
                    mats[i] = null;
                }
            }

            mNewMatDic.Clear();
            mNewMatDic = null;

            Material.Destroy(mMaterial);
            mMaterial = null;

            // 卸掉脚本 //
            if (mBehaviour != null)
            {
                GameObject.DestroyObject(mBehaviour);
                mBehaviour = null;
            }
        }

        internal void UpdateNewMatDic()
        {
            if (mEntity == null)
            {
                return;
            }
            GameObject go = mEntity.GameObject;
            Renderer[] rendererArray = go.GetComponentsInChildren<Renderer>(true) as Renderer[];
            for (int i = 0; i < rendererArray.Length; ++i)
            {
                Renderer r = rendererArray[i];

                if (LayerDef.Effect == r.transform.gameObject.layer)
                {
                    continue;
                }

                if (!(r is MeshRenderer || r is SkinnedMeshRenderer))
                {
                    continue;
                }
                Material[] ms = null;


                if (!mNewMatDic.TryGetValue(r, out ms))
                {
                    ms = new Material[r.materials.Length];
                    for (int j = 0; j < r.materials.Length; ++j)
                    {
                        if (r.materials[j] != null)
                        {
                            Material m = GameObject.Instantiate(mMaterial) as Material;
                            m.SetTexture("_MainTex", r.materials[j].mainTexture);
                            if (mInfo.materialCopyTextureParamDic != null)
                            {

                                for (int l = 0; l < mInfo.materialCopyTextureParamDic.Count; l++)
                                {
                                    string p = mInfo.materialCopyTextureParamDic[l];
                                    if (r.materials[j].HasProperty(p))
                                    {
                                        m.SetTexture(p, r.materials[j].GetTexture(p));
                                    }
                                }
                            }

                            if (mInfo.materialCopyColorParamDic != null)
                            {
                                for (int l = 0; l < mInfo.materialCopyColorParamDic.Count; l++)
                                {
                                    string p = mInfo.materialCopyColorParamDic[l];
                                    if (r.materials[j].HasProperty(p))
                                    {
                                        m.SetColor(p, r.materials[j].GetColor(p));
                                    }
                                }
                            }
                            ms[j] = m;
                        }
                    }
                    mNewMatDic.Add(r, ms);
                }
            }
        }
    }




#if UNITY_EDITOR
    public
#else
    internal
#endif
 class GOEEntityMaterial : GOEEntityComponent
    {
        private Dictionary<string, MaterialEffect> mMatEffectDic = new Dictionary<string, MaterialEffect>();
        private Dictionary<Renderer, Material[]> mNewMatDic = new Dictionary<Renderer, Material[]>(); //@Const 为要改变的材质手动拷贝原材质，拷贝出来的材质保存这里, 不这样做会有材质泄露 //
        //  private bool mIsChgModelAndChgShareMat = false; //@dpc 为了坐骑的人物模型是否需要重新加载而设//
        private bool mIsOriginalshader = true;
        private string mCurShaderName = "GOE";// GOE不是一个shader名，用来标记该材质的shader是原始的//

        public bool IsOriginalshader
        {
            get { return mIsOriginalshader; }
        }

        public string CurShaderName
        {
            get { return mCurShaderName; }
        }

        public GOEEntityMaterial()
        {
            mEnable = true;
            mIsOriginalshader = true;
        }


        ~GOEEntityMaterial()
        {

        }



        protected override void OnEnabled()
        {
            base.OnEnabled();
        }

        protected override void OnDisabled()
        {
            //  把材质还原 //
            OriginalShader();
            base.OnDisabled();
        }

        internal void SetColor(string name, Color color, string nodeName)
        {
            if (mNewMatDic.Count <= 0)
            {
                // 创建新的材质，防止材质的泄露 //
                ChangeSharedMaterials();
            }

            foreach (KeyValuePair<Renderer, Material[]> kvp in mNewMatDic)
            {
                if (string.IsNullOrEmpty(nodeName) || !string.IsNullOrEmpty(nodeName) && kvp.Key.gameObject.name.Equals(nodeName))
                {
                    for (int j = 0; j < kvp.Value.Length; ++j)
                    {
                        if (kvp.Value[j] != null)
                        {
                            if (kvp.Value[j].HasProperty(name))
                            {
                                kvp.Value[j].SetColor(name, color);
                            }
                        }
                    }
                }
            }

        }

        internal void ChangeSharedMaterials()
        {
            //if (Entity.ResStatus != ResStatus.OK)
            //{
            //    mIsChangedShareMaterials = false;
            //    return;
            //}
            //if (mIsChangedShareMaterials)
            //{
            //    return;
            //}
           
            foreach (Renderer r in Entity.GameObject.GetComponentsInChildren<Renderer>(true))
            {
                if (LayerDef.Effect == r.transform.gameObject.layer)
                {
                    continue;
                }
                if (!(r is MeshRenderer || r is SkinnedMeshRenderer))
                {
                    continue;
                }
                Material[] ms = r.materials;// new Material[r.sharedMaterials.Length];
                //for (int i = 0; i < r.sharedMaterials.Length; ++i)
                //{
                //    if (r.sharedMaterials[i] != null)
                //    {
                //        ms[i] = new Material(r.sharedMaterials[i]);
                //    }
                //    else
                //    {
                //        ms[i] = null;
                //    }
                //}
                if (mNewMatDic.ContainsKey(r))
                    mNewMatDic[r] = ms;
                else
                    mNewMatDic.Add(r, ms);
                //r.sharedMaterials = ms;
            }
            // mIsChangedShareMaterials = true;
        }

        internal void UpdateSharedMaterials(string[] repalceShaderName)
        {
            foreach (Renderer r in Entity.GameObject.GetComponentsInChildren<Renderer>(true))
            {
                if (LayerDef.Effect == r.transform.gameObject.layer)
                {
                    continue;
                }
                if (!(r is MeshRenderer || r is SkinnedMeshRenderer))
                {
                    continue;
                }
                Material[] ms = r.materials;// new Material[r.sharedMaterials.Length];

                if (!mNewMatDic.ContainsKey(r))
                {
                    mNewMatDic.Add(r, ms);
                }
            }

            if (repalceShaderName != null)
            {
                int count = repalceShaderName.Length;
                for (int i = 0; i < count; i++)
                {
                    MaterialEffect outEffect = null;
                    if (mMatEffectDic.TryGetValue(repalceShaderName[i], out outEffect))
                    {
                        outEffect.UpdateNewMatDic();
                    }
                }
            }

        }
        internal void RecoverMaterial()
        {
            if (mNewMatDic == null || mNewMatDic.Count <= 0)
            {
                return;
            }
            mIsOriginalshader = true;
            mCurShaderName = "GOE";
            GameObject go = this.Entity.GameObject;
            Renderer[] rendererArray = go.GetComponentsInChildren<Renderer>(true) as Renderer[];
            for (int i = 0; i < rendererArray.Length; ++i)
            {
                if (LayerDef.Effect == rendererArray[i].transform.gameObject.layer)
                {
                    continue;
                }
                if (!(rendererArray[i] is MeshRenderer || rendererArray[i] is SkinnedMeshRenderer))
                {
                    continue;
                }

                if (!mNewMatDic.ContainsKey(rendererArray[i]))
                {
                    continue;
                }
                rendererArray[i].materials = mNewMatDic[rendererArray[i]];

            }
        }


        internal void ReplaceShader(MaterialEffectInfo info)
        {
            if (mNewMatDic.Count <= 0)
            {
                // 创建新的材质，防止材质的泄露 //
                ChangeSharedMaterials();
            }

            mIsOriginalshader = false;
            mCurShaderName = info.shaderName;
            MaterialEffect outEffect = null;
            if (!mMatEffectDic.TryGetValue(info.name, out outEffect))
            {
                outEffect = new MaterialEffect(info, this.Entity);
                mMatEffectDic.Add(info.name, outEffect);
                outEffect.ReplaceMaterial();
            }else
            {
                outEffect.mInfo = info;
                outEffect.ReplaceMaterial(true);
            }
        }


        internal void StopShader(string name, bool destory)
        {
            if (!mMatEffectDic.ContainsKey(name))
                return;
            OriginalShader();

            if (destory)
            {
                mMatEffectDic[name].DestoryMaterial();
                mMatEffectDic[name] = null;
                mMatEffectDic.Remove(name);
            }
        }


        internal void OriginalShader()
        {
            RecoverMaterial();
        }

        internal void ClearEffect()
        {
            mNewMatDic.Clear();
            mNewMatDic = null;


            // 删除效果上的材质 //
            foreach (KeyValuePair<string, MaterialEffect> kayValue in mMatEffectDic)
            {
                MaterialEffect me = kayValue.Value;
                me.DestoryMaterial();
                me = null;
            }
            mMatEffectDic.Clear();
            mMatEffectDic = null;
        }

        internal override void OnDestroy()
        {
            // 删除所有材质 //
            // 删除new shared  Material //
            foreach (KeyValuePair<Renderer, Material[]> kayValue in mNewMatDic)
            {
                for (int j = 0; j < kayValue.Value.Length; ++j)
                {
                    if (kayValue.Value[j] != null)
                    {
                        Material.Destroy(kayValue.Value[j]);
                        kayValue.Value[j] = null;
                    }
                }
            }
            mNewMatDic.Clear();
            mNewMatDic = null;


            // 删除效果上的材质 //
            foreach (KeyValuePair<string, MaterialEffect> kayValue in mMatEffectDic)
            {
                MaterialEffect me = kayValue.Value;
                me.DestoryMaterial();
                me = null;
            }
            mMatEffectDic.Clear();
            mMatEffectDic = null;

            base.OnDestroy();
        }

    }

}

