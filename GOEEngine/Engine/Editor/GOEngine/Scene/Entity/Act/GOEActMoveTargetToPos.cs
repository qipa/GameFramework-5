using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.MoveTargetToPos)]
    [DisplayName("将目标移动到目标点")]
    [System.Reflection.Obfuscation(Exclude = true)]
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOEActMoveTargetToPos : GOEActComponent
    {
        Entity target;
        GOEEntityMoveToDirect move;
        [DisplayName("移动时间")]
        public float MoveTime { get; set; }
        internal override void OnTrigger()
        {
            base.OnTrigger();
            List<Entity> listTarget = this.EntityAct.TargetList;
            if (listTarget.Count > 0)
            {
                target = listTarget[0];
                move = target.AddComponent<GOEEntityMoveToDirect>();
                move.MaxTime = MoveTime;
                move.StartPos = target.Position;
                move.EndPos = EntityAct.TargetPos;
            }
        }

        internal override void GetResAsset(HashSet<string> setRes)
        {
            
        }

        internal override void PreLoad()
        {
            
        }

        internal override void Pause()
        {
            base.Pause();
            if (move != null)
                move.Pause();
        }

        internal override void Restart()
        {
            base.Restart();
            if (move != null)
                move.Restart();
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();
            if (move != null)
                move.LifeOver = true;
        }
    }
}
