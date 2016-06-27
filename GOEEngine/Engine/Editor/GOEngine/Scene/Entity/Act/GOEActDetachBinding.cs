using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.DetachBinding)]
    [DisplayName("解除绑点下物件的绑定")]
    [System.Reflection.Obfuscation(Exclude = true)]
#if UNITY_EDITOR
    public
#else
    internal
#endif
 class GOEActDetachBinding : GOEActComponent
    {
        private string mAttachNode = string.Empty;
        UnityEngine.Transform node;
        UnityEngine.Transform[] transforms;
        UnityEngine.Vector3[] localPos;
        UnityEngine.Quaternion[] localRot;
        
        [DisplayName("绑点")]
        [JsonFieldAttribute(JsonFieldTypes.BindPoint)]
        public string AttachNode
        {
            set { mAttachNode = value; }
            get { return mAttachNode; }
        }

        
        internal override void OnTrigger()
        {
            base.OnTrigger();
            node = Entity.GetBindPoint(mAttachNode);
            if (node != null)
            {
                int cnt = node.childCount;
                transforms = new UnityEngine.Transform[cnt];
                localPos = new UnityEngine.Vector3[cnt];
                localRot = new UnityEngine.Quaternion[cnt];

                for (int i = 0; i < cnt; i++)
                {
                    UnityEngine.Transform t = node.GetChild(i);
                    transforms[i] = t;
                    localPos[i] = t.localPosition;
                    localRot[i] = t.localRotation;
                }

                node.DetachChildren();
            }
        }

        internal override void GetResAsset(HashSet<string> setRes)
        {
            
        }

        internal override void PreLoad()
        {
            
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();
            if (transforms != null)
            {
                for (int i = 0; i < transforms.Length; i++)
                {
                    if ((transforms[i]) == null)
                        continue;
                    transforms[i].parent = node;
                    transforms[i].localPosition = localPos[i];
                    transforms[i].localRotation = localRot[i];
                }
            }
        }
    }
}
