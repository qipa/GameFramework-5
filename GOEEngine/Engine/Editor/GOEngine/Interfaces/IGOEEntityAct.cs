using System;
namespace GOEngine
{
    public interface IGOEEntityAct
    {
        /// <summary>
        /// 技能动画添加攻击对象
        /// </summary>
        /// <param name="ent">对象实体</param>
        void AddTarget(global::GOEngine.IEntity ent);
        /// <summary>
        /// 停止播放技能动画
        /// </summary>
        void Stop();
        /// <summary>
        /// 技能动画的目标点
        /// </summary>
        global::UnityEngine.Vector3 TargetPos { get; set; }

        /// <summary>
        /// 往摄像机移动的偏移量
        /// </summary>
        float DistanceTowardsCamera { get; set; }

        /// <summary>
        /// Act的生命时间
        /// </summary>
        float LiftTime { get; set; }
    }
}
