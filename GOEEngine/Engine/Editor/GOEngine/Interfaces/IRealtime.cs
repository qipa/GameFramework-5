using System;
namespace GOEngine
{
    public interface IRealtime
    {
        /// <summary>
        /// 每帧的间隔
        /// </summary>
        float DeltaTime { get; }

        /// <summary>
        /// 当前的时间 秒为单位
        /// </summary>
        float RealTime { get; }

        /// <summary>
        /// 当前的时间 毫秒为单位
        /// </summary>
        long RealTimeMill { get; }
    }
}
