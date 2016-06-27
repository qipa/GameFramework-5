using System;
namespace GOEngine
{
    public interface IGOEGameScene
    {
        /// <summary>
        /// 添加人物类实体
        /// </summary>
        /// <param name="name">实体资源名</param>
        /// <returns></returns>
        global::GOEngine.IGOEActorEntity AddActor(string name);
        /// <summary>
        /// 添加人物类实体
        /// </summary>
        /// <returns></returns>
        global::GOEngine.IGOEActorEntity AddActor();
        /// <summary>
        /// 场景中添加特效
        /// </summary>
        /// <param name="name">资源名</param>
        /// <param name="pos">位置</param>
        /// <param name="rot">旋转</param>
        /// <param name="scale">缩放</param>
        /// <param name="time">播放时间</param>
        /// <returns></returns>
        global::GOEngine.IEntity AddEffect(string name, global::UnityEngine.Vector3 pos, global::UnityEngine.Quaternion rot, float scale = 1, float time = -1);
        /// <summary>
        /// 添加场景实体
        /// </summary>
        /// <param name="name">资源名</param>
        /// <returns></returns>
        global::GOEngine.IEntity AddEntity(string name);
        /// <summary>
        /// 添加场景实体
        /// </summary>
        /// <returns></returns>
        global::GOEngine.IEntity AddEntity();
        /// <summary>
        /// 删除场景实体
        /// </summary>
        /// <param name="obj"></param>
        void DelEntity(global::GOEngine.IEntity obj);
        /// <summary>
        /// 删除场景实体
        /// </summary>
        void DelEntity(string id);
        /// <summary>
        /// 根据id查找实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        global::GOEngine.IEntity GetEntityByID(string id);
        /// <summary>
        /// 场景出生点，需要场景有SceneConfig脚本
        /// </summary>
        /// <param name="ind"></param>
        /// <returns></returns>
        global::UnityEngine.Vector3 GetBornPoint(int ind = 0);
        /// <summary>
        /// 根据射线，获得当前点对应的地表高度
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="refY"></param>
        /// <returns></returns>
        float GetHeight(float x, float z, float refY);
        /// <summary>
        /// 检查当前坐标是否在地表上
        /// </summary>
        bool CheckPositonByRayCast(ref global::UnityEngine.Vector3 pos);
        /// <summary>
        /// 根据屏幕坐标获得当前的地表点的世界坐标
        /// </summary>
        /// <param name="screenPos"></param>
        /// <param name="terrainPos"></param>
        /// <returns></returns>
        bool GetIntersectTerrainPos(global::UnityEngine.Vector3 screenPos, ref global::UnityEngine.Vector3 terrainPos);
        /// <summary>
        /// 获取当前直线上的可以通过区域的最远点
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="cast"></param>
        /// <returns></returns>
        int GetRaycast(global::GOEngine.IDetourSender actor, global::UnityEngine.Vector3 start, global::UnityEngine.Vector3 end, out global::UnityEngine.Vector3 cast);
        /// <summary>
        /// 判断point点 是否是是在行走区域内的
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        bool IsPositionWalkable(IDetourSender sender, global::UnityEngine.Vector3 point);
        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="name"></param>
        void LoadScene(string name, bool withDetour = true, bool clearRes = true);
        /// <summary>
        /// 主角加载完成时调用，用于设置一些跟镜头或者主角有关的配置
        /// </summary>
        /// <param name="obj"></param>
        void SetHero(IEntity obj);

        /// <summary>
        /// 删除Hero ReLoad资源的时候 OnLoadResource的监听
        /// </summary>
        void RemoveHeroLoadListener();

        /// <summary>
        /// 添加Hero ReLoad资源的时候 OnLoadResource的监听
        /// </summary>
        void AddHeroLoadListener();

        /// <summary>
        /// 进入场景后派发
        /// </summary>
        Action OnEnterScene { get; set; }
        /// <summary>
        /// 离开场景派发
        /// </summary>
        Action OnLeaveScene { get; set; }

        Action OnDynamicLoadedOk { get; set; }
        /// <summary>
        /// 设置背景音乐
        /// </summary>
        /// <param name="name"></param>
        void AddBackgroundMusic(string name);
        void RemoveBackgroundMusic();

        /// <summary>
        /// 打开闪电效果 //
        /// </summary>
        void OpenFlash();

        /// <summary>
        /// 关闭闪电效果 //
        /// </summary>
        void CloseFlash();

        /// <summary>
        /// 打开下雨效果 //
        /// </summary>
        void OpenRain();

        /// <summary>
        /// 关闭下雨效果 //
        /// </summary>
        void CloseRain();

        /// <summary>
        /// Set Light //
        /// </summary>
        void SetLight(float light);

        /// <summary>
        /// 获取或设置是否开启了闪电
        /// </summary>
        bool FlashEnabled { get; set; }

        /// <summary>
        /// 获取或设置是否开启了下雨
        /// </summary>
        bool RainEnabled { get; set; }

        /// <summary>
        /// 获取或设置光照强度
        /// </summary>
        float LightBrightness { get; set; }


        /// <summary>
        /// 获取或设置月光强度
        /// </summary>
        float MoonLightScale { get; set; }


        /// <summary>
        /// 获取导航网格路径
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        int GetPath(global::UnityEngine.Vector3 start, global::UnityEngine.Vector3 end, out global::System.Collections.Generic.List<global::UnityEngine.Vector3> path, 
            IDetourSender act,float dis = 0.0f);
        /// <summary>
        /// 根据导航网格取回高度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        float GetHeightByNavmesh(IDetourSender sender, global::UnityEngine.Vector3 pos);
        /// <summary>
        /// 清除随即地图的块数据 //
        /// </summary>
        void ClearTileData();
        /// <summary>
        /// 添加数据 //
        /// </summary>
        /// <param name="tile"></param>
        void AddTileData(GOEngine.Implement.TileData tile);
    }
}
