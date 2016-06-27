using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace GOEngine.Implement
{
    [ActTypeName(ActType.TurnToTarget)]
    [System.Reflection.Obfuscation(Exclude = true)]
    [DisplayName("转向目标")]
#if UNITY_EDITOR
    public
#else
    internal
#endif        
    class GOEActTurnToTarget : GOEActComponent
    {
        private float _fToatolTime = 0.0f;
        [DisplayName("转向180度用的时间")]
        public float TotalTime { get { return _fToatolTime; } set { _fToatolTime = value; } }

        [DisplayName("持续保持转向")]
        public bool Continuous { get; set; }

        internal override void OnTrigger()
        {
            base.OnTrigger();

            Vector3 ori;
            if (this.EntityAct.TargetList.Count != 0 && this.EntityAct.TargetList[0] != null)
            {
                Entity target = this.EntityAct.TargetList[0];
                ori = target.Position - this.Entity.Position;
            }
            else if (this.EntityAct.TargetPos != Vector3.zero)
            {
                ori = this.EntityAct.TargetPos - this.Entity.Position;
            }
            else
                return;

            ori.y = 0;
            if (ori.magnitude <= 0.01)
            {
                return;
            }

            GOEActorEntity actorEntity = this.Entity as GOEActorEntity;
            if (actorEntity != null && actorEntity.RotateSmooth)
            {
                actorEntity.StartSmoothRotation(Quaternion.LookRotation(ori.normalized), TotalTime);
                return;
            }
                this.Entity.Orientation = ori.normalized;
        }

        internal override void Update()
        {
            if (Continuous)
                OnTrigger();
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
