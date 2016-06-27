using System;
namespace GOEngine
{
    public interface IEntity
    {
        /// <summary>
        /// 添加附件，比如头顶名字，需要资源为prefab
        /// </summary>
        /// <param name="name">资源名</param>
        /// <param name="bd">绑点名</param>
        /// <returns></returns>
        global::GOEngine.IEntity AddAttach(string name, string bd);
        /// <summary>
        /// 添加附件，比如头顶名字，需要资源为prefab
        /// </summary>
        /// <param name="name">资源名</param>
        /// <param name="tm">绑点transform</param>
        /// <returns></returns>
        global::GOEngine.IEntity AddAttach(string name, global::UnityEngine.Transform tm);
        /// <summary>
        /// 将其他Entity添加附件，比如头顶名字
        /// </summary>
        /// <param name="attach">Entity</param>
        /// <param name="bd">绑点名</param>
        /// <returns></returns>
        void AddAttach(IEntity attach, string bd);
        /// <summary>
        /// 将其他Entity添加附件，比如头顶名字
        /// </summary>
        /// <param name="attach">Entity</param>
        /// <param name="tm">绑点transform</param>
        /// <returns></returns>
        void AddAttach(IEntity attach, global::UnityEngine.Transform tm);
        /// <summary>
        /// 根据id获取附件
        /// </summary>
        /// <param name="id">附件id</param>
        /// <returns></returns>
        global::GOEngine.IEntity GetAttchById(string id);
        /// <summary>
        /// 根据资源名获取附件
        /// </summary>
        /// <param name="name">附件资源名</param>
        /// <returns></returns>
        global::GOEngine.IEntity GetAttchByName(string name);




        /// <summary>
        /// 设置材质中指定名字参数的颜色值//
        /// </summary>
        /// <param name="name">color名字</param>
        /// <param name="nodeName">需要部分更换颜色节点的名字</param>
        /// <returns></returns>
        void SetColor(string name, UnityEngine.Color color, string nodeName = "");
		/// <summary>
		/// 根据shader名字把entity的shader换掉,包括所有挂接的物体的shader //
		/// </summary>
		/// <param name="name">shader名字</param>
		/// <returns></returns>
		void ReplaceShader(global::GOEngine.MaterialEffectInfo info);
		/// <summary>
		/// 还原原始的shader
		/// </summary>
		void OriginalShader();
        /// <summary>
        ///替换原始的shader
        /// </summary>
        /// <param name="info"></param>
        void ReplaceOriginalShader(global::GOEngine.MaterialEffectInfo info);
        /// <summary>
        /// 停止某个shader
        /// </summary>
        void StopShader(string name, bool destory);
        /// <summary>
        /// 是否是原始shader
        /// </summary>
        bool IsOriginalShader();

        /// <summary>
        /// 更新Cache的原材质
        /// </summary>
        void UpdateSharedMaterials(string[] repalceShaderName);

        /// 当前shader名字
        /// </summary>
        string CurShaderName();

        /// <summary>
        /// 删除附件
        /// </summary>
        /// <param name="attach"></param>
        /// <param name="noDelete">是否不销毁附件Entity（可用于自行缓存）</param>
        void RemoveAttach(global::GOEngine.IEntity attach, bool noDelete = false);
        /// <summary>
        /// 加载优先级
        /// </summary>
        LoadPriority Priority { get; set; }
        void ShowBoundLineRenderer(bool showLine);
        /// <summary>
        /// 实体的GameObject对象，尽量不要直接修改此对象
        /// </summary>
        UnityEngine.GameObject GameObject { get; }
        /// <summary>
        /// 资源名
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// 加载资源
        /// </summary>
        void Load();

        /// <summary>
        /// 重新加载资源
        /// </summary>
        void Reload();
        /// <summary>
        /// ID，由引擎自动生成
        /// </summary>
        string ID { get; }
        int Layer { get; set; }
        global::UnityEngine.Vector3 Orientation { get; set; }
        global::UnityEngine.Transform Parent { get; set; }
        global::UnityEngine.Vector3 Position { get; set; }
        global::UnityEngine.Quaternion Rotation { get; set; }
        global::UnityEngine.Vector3 Scale { get; set; }
        bool Visible { get; set; }
        /// <summary>
        /// 设置当前物件是否渲染
        /// </summary>
        bool RenderVisible { get; set; }
        bool GlobalVisible { get; }
        bool EnablePhysics { get; set; }
        bool EnableCollid { get; set; }
        float Weight { get; set; }
        float CollisionRadius { get; set; }

        bool IgnorePathFindingCollision { get; set; }
        global::GOEngine.CastShadows CastShadows { get; set; }

        /// <summary>
        /// 添加资源加载完成的代理
        /// </summary>
        Action OnLoadResource { get; set; }
        void Pause();
        void Restart();
        void SetSpeed(float speed);
        void ApplyForce(float forceMagtitude, global::UnityEngine.Vector3 localForceDir, float mass = 20);
        /// <summary>
        /// 取得绑点
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        UnityEngine.Transform GetBindPoint(string name);
        /// <summary>
        /// 设置Sorting order
        /// </summary>
        bool SetSortingOrder(int order);
    }
}
