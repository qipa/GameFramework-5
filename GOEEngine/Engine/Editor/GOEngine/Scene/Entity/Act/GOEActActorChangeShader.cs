using System;
using UnityEngine;
using System.Collections.Generic;
namespace GOEngine.Implement
{
    [ActTypeName(ActType.ChangeShader)]
    [System.Reflection.Obfuscation(Exclude = true)]
    [DisplayName("更换Shader")]        
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOEActActorChangeShader : GOEActComponent
    {
        MaterialEffectInfo info = new MaterialEffectInfo() { name = "冰冻", shaderName = "GOE/Ice" };
        string repTexs;
        Color _color;
        [DisplayName("效果名称（不能重复）")]
        public string Name
        {
            get
            {
                return info.name;
            }
            set
            {
                info.name = value;
            }
        }

        [DisplayName("Shader名称")]       
        public string ShaderName
        {
            get { return info.shaderName; }
            set { info.shaderName = value; }
        }

        [DisplayName("替换贴图名称")]
        public string ReplaceTextures
        {
            get { return repTexs; }
            set
            {
                repTexs = value;
                string[] r = value.Split(',');
                info.materialTextureParamDic = new Dictionary<string, Texture2D>();
                foreach (var i in r)
                {
                    info.materialTextureParamDic[i] = null;
                }
            }
        }

        [DisplayName("替换主色调")]
        public Color ReplaceMainColor
        {
            get { return _color; }
            set
            {
                _color = value;
                info.materialColorParamDic = new Dictionary<string, Color>();
                info.materialColorParamDic["_Color"] = value;
            }
        }
        internal override void OnTrigger()
        {
            base.OnTrigger ();
            if ( this.Entity.ResStatus != ResStatus.OK )
            {
                return;
            }

            this.Entity.ReplaceShader(info);
            this.Enable = false;
        }

        internal override void GetResAsset(HashSet<string> setRes)
        {
            
        }

        internal override void PreLoad()
        {
            
        }

        internal override void OnDestroy()
        {
            if (this.Entity.CurShaderName() == info.shaderName)
                this.Entity.OriginalShader();
            base.OnDestroy();
        }

    }
}

