using System;
using UnityEngine;
using System.Collections.Generic;
namespace GOEngine
{
    public interface IGOEActorEntity : IEntity, IDetourSender
    {
        /// <summary>
        /// 是否需要客户端寻路，如果为false，则直线移动
        /// </summary>
        bool ClientDetour { set; get; }
        /// <summary>
        /// 是否根据地形倾斜度旋转Acto
        /// </summary>
        bool RotateUpVector { get; set; }
        /// <summary>
        /// 移动速度，单位为米/秒
        /// </summary>
        float MoveSpeed { set; get; }
        /// <summary>
        /// 
        /// </summary>
        bool RotateSmooth { set; get; }
        /// <summary>
        /// 是否自动播放子对象动作
        /// </summary>
        bool AutoPlaySubAnimation { get; set; }
        /// <summary>
        /// 如果开启了平滑旋转，在旋转中只有通过这个才能取得真实旋转
        /// </summary>
        Quaternion RealRotation { get; }

        /// <summary>
        /// 平滑转向的转向时间
        /// </summary>
        float RotationSpeed { get; set; }
        /// <summary>
        /// 转向速度是否为匀速
        /// </summary>
        bool IsRotationSpeedUniform { get; set; }
        bool MoveSmooth { set; get; }
        /// <summary>
        ///行走的动作名称
        /// </summary>
        string MoveAnimation { set; get; }

        /// <summary>
        /// 动画前缀
        /// </summary>
        string AnimationPrefix { get; set; }
        /// <summary>
        ///站立的动作名称
        /// </summary>
        string StandAnimation { set; get; }
        /// <summary>
        /// 慢走动作，可为空
        /// </summary>
        string SlowMoveAnimation { get; set; }
        /// <summary>
        /// 刹车动作，可为空
        /// </summary>
        string BreakAnimation { get; set; }

        /// <summary>
        /// 移动融合时间
        /// </summary>
        float MoveBlendTime { get; set; }

        /// <summary>
        /// 下一个动作不融合
        /// </summary>
        bool NextAnimationNoCrossfade { get; set; }

        /// <summary>
        /// 是否忽略播放移动动作
        /// </summary>
        bool IgnoreMoveAnimation { get; set; }
        /// <summary>
        /// 添加特效
        /// </summary>
        /// <param name="name">资源名</param>
        /// <param name="BindPoint">绑点名</param>
        /// <param name="time">播放时间</param>
        /// <returns></returns>
        GOEngine.IEntity AddEffect(string name, string BindPoint, float time = -1.0f);
        /// <summary>
        /// 删除特效
        /// </summary>
        /// <param name="name">特效名</param>
        void DelEffect(string name);
        /// <summary>
        /// 过度到下一个动作
        /// </summary>
        /// <param name="name">动作名</param>
        /// <param name="fadeLength">过度时间</param>
        /// <param name="mode"></param>
        /// <param name="backToStand">是否在播放完毕后自动切换回站立动作</param>
        /// <returns></returns>
        bool CrossFade(string name, float fadeLength = 0.3F, UnityEngine.PlayMode mode = PlayMode.StopSameLayer, bool backToStand = true);

        /// <summary>
        /// 过度播放序列动作，在前一个动作播完之后播放
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fadeLength">过度时间</param>
        /// <param name="queue"></param>
        /// <param name="mode"></param>
        void CrossFadeQueued(string name, float fadeLength = 0.3F, UnityEngine.QueueMode queue = QueueMode.CompleteOthers, UnityEngine.PlayMode mode = PlayMode.StopSameLayer, bool backToStand = false);
        /// <summary>
        /// 融合动作
        /// </summary>
        /// <param name="name"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        bool PlayBlend(string name, float weight, string bone);
        /// <summary>
        /// 是否正在寻路
        /// </summary>
        bool IsPathfinding { get; }
        /// <summary>
        /// 朝某个角度移动
        /// </summary>
        /// <param name="degree">摇杆的角度</param>
        void MoveDir(float degree);
        /// <summary>
        /// 屏幕点击移动
        /// </summary>
        /// <param name="x">屏幕坐标x</param>
        /// <param name="y">屏幕坐标y</param>
        List<Vector3> MoveTo(float x, float y, Action onReach = null);
        /// <summary>
        /// 朝特定点移动（世界坐标）
        /// </summary>
        /// <param name="pos">目标点</param>
        List<Vector3> MoveTo(Vector3 pos, Action onReach = null);
        /// <summary>
        /// 根据地形等设置重置旋转
        /// </summary>
        void ResetRotation();
        /// <summary>
        /// 按指定路径移动
        /// </summary>
        /// <param name="path"></param>
        /// <param name="onReach"></param>
        void MoveTo(List<Vector3> path, Action onReach);
        /// <summary>
        /// 移动到某点附近，不重叠
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="onClose"></param>
        /// <param name="dis"></param>
        /// <returns></returns>
        List<Vector3> MoveClose(Vector3 pos, Action onClose, float dis = 1.0f);
        /// <summary>
        /// 以特定速度直线移动到终点
        /// </summary>
        /// <param name="end">终点</param>
        /// <param name="speed">速度</param>
        void MoveStraight(Vector3 end, float speed = -1, bool keeFace = false, bool keepAni = false, Action onReach = null);
        /// <summary>
        /// 在特定时间内直线移动到终点
        /// </summary>
        /// <param name="end">终点</param>
        /// <param name="time">时间</param>
        void MoveStraightByTime(Vector3 end, float time, bool keepface, bool keepAni);
        /// <summary>
        /// 播放技能特效
        /// </summary>
        /// <param name="name">技能特效资源名</param>
        /// <returns></returns>
        GOEngine.IGOEEntityAct PlayAct(string name, ActType mask = ActType.None, float speed = 1, bool immediatelyRun = false);
        /// <summary>
        /// 播放动作（新版animator）
        /// </summary>
        /// <param name="type"></param>
        void PlayAnimation(GOEngine.AnimationType type);
        /// <summary>
        /// 直接播放动画
        /// </summary>
        /// <param name="name"></param>
        /// <param name="mode"></param>
        /// <param name="backToStand"></param>
        void PlayAnimation(string name, PlayMode mode = PlayMode.StopSameLayer, bool backToStand = false, float time = 0f);
        /// <summary>
        /// 添加音乐
        /// </summary>
        /// <param name="name"></param>
        /// <param name="bLoop"></param>
        void PlayAudio(string name, bool bLoop);
        /// <summary>
        /// 播放序列动作，在前一个动作播完之后播放
        /// </summary>
        /// <param name="name"></param>
        /// <param name="queue"></param>
        /// <param name="mode"></param>
        void PlayQueued(string name, UnityEngine.QueueMode queue = QueueMode.CompleteOthers, UnityEngine.PlayMode mode = PlayMode.StopSameLayer, bool backToStand = false);
        /// <summary>
        /// 停止播放动作
        /// </summary>
        void Stop();
        /// <summary>
        /// 停止播放技能特效
        /// </summary>
        bool IsMoving { get; }

        /// <summary>
        /// 停止指定ACT
        /// </summary>
        /// <param name="name"></param>
        void StopAct(string name);
        void StopAct(HashSet<string> exclude = null);
        /// <summary>
        /// 停止音乐
        /// </summary>
        /// <param name="name"></param>
        void StopAudio(string name);
        /// <summary>
        /// 停止移动
        /// </summary>
        void StopMove();
        /// <summary>
        /// 人物移动时派发此代理
        /// </summary>
        Action<Vector3, Vector3> OnUserMoveTo { set; get; }
        /// <summary>
        /// 人物移动停止时派发次代理
        /// </summary>
        Action OnUserStopMove { set; get; }

        /// <summary>
        /// 人物碰撞上动态阻挡时处理
        /// </summary>
        Action<IEntity> OnCollision { get; set; }
        /// <summary>
        /// 动画播放速率
        /// </summary>
        float AnimationSpeed { set; get; }
        /// <summary>
        /// 默认的人物移动速度
        /// </summary>
        float DefaultMoveSpeed { set; get; }
        /// <summary>
        /// 暂停技能动画
        /// </summary>
        void PauseAct();
        /// <summary>
        /// 重启技能动画
        /// </summary>
        void RestartAct();
        /// <summary>
        /// 打断角色旋转，强制设置方向
        /// </summary>
        void InterruptRotation(Quaternion target);
        /// <summary>
        /// 添加可以被融合的动作
        /// </summary>
        /// <param name="name"></param>
        void AddBlendedAnime(string name);


        /// <summary>
        /// 添加Actor附件
        /// </summary>
        /// <param name="name">资源名</param>
        /// <param name="bd">绑点</param>
        /// <returns></returns>
        IGOEActorEntity AddActorAttach(string name, string bd);
        /// <summary>
        /// 判断act剩余生命时间
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        float GetActLifeTime(string name);

        /// <summary>
        /// 设置模型 混合动作绑定点
        /// </summary>
        string BlendPoint { set; get; }

        void AddFakeShadow();
    }
}
