using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace GOEngine.Implement
{
    [ActTypeName(ActType.TurnDegree)]
    [System.Reflection.Obfuscation(Exclude = true)]
    [DisplayName("旋转指定角度")]
#if UNITY_EDITOR
    public
#else
    internal
#endif
 class GOEActTurnDegree : GOEActComponent
    {
        [DisplayName("旋转角度")]
        public float Degree { get; set; }
        internal override void OnTrigger()
        {
            base.OnTrigger();

            Debug.Log("Old orientation:" + this.Entity.Orientation);
            var ori = Entity.Rotation * Quaternion.Euler(0, Degree, 0);
            this.Entity.Rotation = ori;
            Debug.Log("New orientation:" + this.Entity.Orientation);
            
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
        }
    }
}
