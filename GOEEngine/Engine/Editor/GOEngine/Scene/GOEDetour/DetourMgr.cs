using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using UnityEngine;
using AStar;
using AStar.astar;
using dtPolyRef = System.UInt32;
using AStar.astar.core;

namespace GOEngine
{
	public interface IDetourSender
	{
		int GetMotionAilityFlags();
	}

	public enum SamplePolyFlags
	{
		SAMPLE_POLYFLAGS_WALK = 0x01,		// Ability to walk (ground, grass, road)
		SAMPLE_POLYFLAGS_SWIM = 0x02,		// Ability to swim (water).
		SAMPLE_POLYFLAGS_DOOR = 0x04,		// Ability to move through doors.
		SAMPLE_POLYFLAGS_JUMP = 0x08,		// Ability to jump.
		SAMPLE_POLYFLAGS_DISABLED = 0x10,		// Disabled polygon
		SAMPLE_POLYFLAGS_BLOCK1 = 0x20,
		SAMPLE_POLYFLAGS_BLOCK2 = 0x40,
		SAMPLE_POLYFLAGS_BLOCK3 = 0x80,
		SAMPLE_POLYFLAGS_BLOCK4 = 0x100,
		SAMPLE_POLYFLAGS_BLOCK5 = 0x200,
		SAMPLE_POLYFLAGS_ALL = 0xffff	// All abilities.
	};
}

namespace GOEngine.Implement
{
	/// <summary>
	/// 导航路径
	/// </summary>
#if UNITY_EDITOR
    public
#else
	internal
#endif
	class NavPath
	{
		public const int MAX_POLYS = 512;
		public float[] startPos = new float[3];
		public float[] endPos = new float[3];

		public float[] straightPath = new float[MAX_POLYS * 3];
		public byte[] straightPathFlags = new byte[MAX_POLYS];
		public dtPolyRef[] straightPathPolys = new dtPolyRef[MAX_POLYS];
		public int straightPathCount = 0;
	};

	/// <summary>
	/// 导航状态
	/// </summary>
#if UNITY_EDITOR
    public
#else
	internal
#endif
	class DetourState
	{
		public const int DT_SUCCESS = 0;
		public const int DT_ERR_NO_NEAR_START = -1;
		public const int DT_ERR_NO_NEAR_END = -2;
		public const int DT_ERR_NO_PATH = -3;
		public const int DT_ERR_NO_MESH = -4;
	};

	/// <summary>
	/// 行走类型
	/// </summary>
#if UNITY_EDITOR
    public
#else
	internal
#endif
	enum SamplePolyAreas
	{
		SAMPLE_POLYAREA_GROUND,
		SAMPLE_POLYAREA_WATER,
		SAMPLE_POLYAREA_ROAD,
		SAMPLE_POLYAREA_DOOR,
		SAMPLE_POLYAREA_GRASS,
		SAMPLE_POLYAREA_JUMP,
	};


	/// <summary>
	/// 导航基类
	/// </summary>
#if UNITY_EDITOR
    public
#else
	internal
#endif
	class DetourBase
	{
		internal virtual void Load(object data) { }
		internal virtual void UnLoad() { }
		internal virtual int FindStraightPath(IDetourSender sender, Vector3 start, Vector3 end, out List<Vector3> nodeList, float[] extents = null) 
		{ 
			nodeList = null;
			return DetourState.DT_SUCCESS; 
		}

		internal virtual int Raycast(IDetourSender sender, Vector3 start, Vector3 end, out Vector3 hitpoint)
		{
			hitpoint = Vector3.zero;
			return DetourState.DT_SUCCESS; 
		}

		internal virtual float GetHeight(IDetourSender sender, Vector3 point)
		{
			return 0f;
		}

		internal virtual bool IsPositionWalkable(IDetourSender sender, Vector3 point)
		{
			return false;
		}

		internal float RaycastDistance(IDetourSender sender, Vector3 start, Vector3 dir)
		{
			Vector3 hitpoint;
			Vector3 end = start + dir * 10;

			int result = Raycast(sender, start, end, out hitpoint);

			if (result == 1)
				return (hitpoint - start).magnitude;
			else if (result == 0)
				return (hitpoint - start).magnitude;
			return 0.0f;
		}
	}

	
	/// <summary>
	/// AStart 导航
	/// </summary>
#if UNITY_EDITOR
    public
#else
	internal
#endif
	class AStartDetour : DetourBase
	{
		public const float TILE_SIZE = 30f;
		public const int ROL_COUNT = 5;
		public const int COL_COUNT = 5;

		private AStarMap astarMap;
		private PathFinder pathFinder;
		internal override void Load(object data)
		{
			List<AStarTile> datas = data as List<AStarTile>;

			//int maxCol = 4;// int.MinValue;
			//int maxRow = 4;// int.MinValue;

			AStarDataMgr astarDatas = new AStarDataMgr();
			foreach (var d in datas)
			{
				astarDatas.initstring(d.mRol, d.mCol, d.mData);
				//if (d.mRol > maxRow)
				//	maxRow = d.mRol;
				//if (d.mCol > maxCol)
				//	maxCol = d.mCol;
			}

			AStarMap aSMap = new AStarMultiTileMap(astarDatas, COL_COUNT, ROL_COUNT);

			this.astarMap = aSMap;
			this.pathFinder = new PathFinder(astarMap);

		}

		internal override void UnLoad()
		{
		}

		internal override int FindStraightPath(IDetourSender sender, Vector3 startPos, Vector3 endPos, out List<Vector3> nodeList, float[] extents = null)
		{
			Vector3 originStartPos = startPos;
			Vector3 originEndPos = endPos;
			startPos = UnityToAStarDetour(startPos);
			endPos = UnityToAStarDetour(endPos);
			Vector2 startPos2D = new Vector2(startPos.x, startPos.y);
			Vector2 endPos2D = new Vector2(endPos.x, endPos.y);
			List<Vector2> path2D = pathFinder.findStraightPath(startPos2D, endPos2D, astarMap.getCellSize());
			List<Vector3> path3D = new List<Vector3>();
			if (path2D == null)
			{
				Vector3 hitPoint;
				Raycast(null, originStartPos, originEndPos, out hitPoint);
				if (IsPositionWalkable(null, hitPoint))
				{
					nodeList = path3D;
					return DetourState.DT_SUCCESS;
				}
				else
				{
					return FindStraightPath(null, startPos, hitPoint, out nodeList);
				}
			}

			foreach (Vector2 p2D in path2D)
			{
				Vector3 p3D = new Vector3(p2D.x, p2D.y, 0);
				p3D = AStarDetourToUnity(p3D);
				path3D.Add(fixHeight(p3D));
			}

			nodeList = path3D;
			return DetourState.DT_SUCCESS;
		}

		internal override int Raycast(IDetourSender sender, Vector3 startPos, Vector3 endPos, out Vector3 hitpoint)
		{
			startPos = UnityToAStarDetour(startPos);
			endPos = UnityToAStarDetour(endPos);
			Vector2 startPos2D = new Vector2(startPos.x, startPos.y);
			Vector2 endPos2D = new Vector2(endPos.x, endPos.y);
			Vector2 hitPos = Raycast(startPos2D, endPos2D);
			Vector3 hitPosOut = AStarDetourToUnity(new Vector3(hitPos.x, hitPos.y, 0));
			hitpoint = fixHeight(hitPosOut);
			return DetourState.DT_SUCCESS;
		}

		public Vector2 Raycast(Vector2 startPos, Vector2 endPos)
		{
			Point startCell = PathFinder.posToCell(startPos, astarMap.getCellSize());
			Point goalCell = PathFinder.posToCell(endPos, astarMap.getCellSize());

			// exception: start is obstacle. Now just return start
			if (AStarCell.isObstacle(astarMap.getCell(startCell.x, startCell.y)))
			{
				return startPos;
			}

			if (startCell.Equals(goalCell))
			{
				return endPos;
			}

			Point hitCell = pathFinder.raycast(startPos, endPos, astarMap.getCellSize());
			Vector2 hitPos = PathFinder.cellToPos(hitCell, astarMap.getCellSize());
			Vector2 dir = (endPos - startPos).normalized;
			hitPos = PathFinder.getNextPosBeforeNextCell(hitPos, dir, astarMap.getCellSize());
			return hitPos;
		}

		internal override float GetHeight(IDetourSender sender, Vector3 pos)
		{
			return fixHeight(pos).z;
		}

		public Vector3 fixHeight(Vector3 pos)
		{
			pos = UnityToAStarDetour(pos);
			int cellX = (int)Math.Floor(pos.x / astarMap.getCellSize());
			int cellY = (int)Math.Floor(pos.y / astarMap.getCellSize());
			float height = astarMap.getHeight(cellX, cellY);
			pos = new Vector3(pos.x, pos.y, height);
			return AStarDetourToUnity(pos);
		}

		internal override bool IsPositionWalkable(IDetourSender sender, Vector3 pos)
		{
			pos = UnityToAStarDetour(pos);
			return pathFinder.getCellWithErrorTolerate(pos.x, pos.y) == null;
		}

		internal Vector3 AStarDetourToUnity(Vector3 vec)
		{
			return new Vector3(vec.x, vec.z, (TILE_SIZE * ROL_COUNT) - vec.y);
		}

		internal Vector3 UnityToAStarDetour(Vector3 vec)
		{
			return new Vector3(vec.x, (TILE_SIZE * ROL_COUNT) - vec.z, vec.y);
		}
	}

	/// <summary>
	/// 导航网 导航
	/// </summary>
#if UNITY_EDITOR
    public
#else
	internal
#endif
	class RecastDetour : DetourBase
	{
		#region DllImport
#if UNITY_IPHONE
         [DllImport("__Internal")]
        private static extern bool LoadNavData( byte[] data);

        [DllImport("__Internal")]
        private static extern bool FindNavPath(float[] start, float[] end, int jflag, ref IntPtr outarr, ref int len);

        [DllImport("__Internal")]
        private static extern bool RayCast(float[] start, float[] end, int jflag, ref IntPtr outarr);

        [DllImport("__Internal")]
        private static extern bool IsPosInBlock(IntPtr jpos);

        [DllImport("__Internal")]
        private static extern void ClearIntPtr(IntPtr pBuffer);

        [DllImport("__Internal")]
        public static extern bool IsWalkable(float[] start, int flag);

        [DllImport("__Internal")]
        public static extern void FindNearestPoly(float[] start, float[] end, int flag, ref int nearRef,ref IntPtr pt );

        [DllImport("__Internal")]
        public static extern float GetPolyHeight(float[] point, int flag);

        //[DllImport("__Internal")]
        //public static extern void SetDebugFunction(IntPtr fp);
#else
		[DllImport("PathFinding")]
		private static extern bool LoadNavData(byte[] data);

		[DllImport("PathFinding")]
		private static extern bool FindNavPath(float[] start, float[] end, int jflag, ref IntPtr outarr, ref int len);

		[DllImport("PathFinding")]
		private static extern bool RayCast(float[] start, float[] end, int jflag, ref IntPtr outarr);

		[DllImport("PathFinding")]
		private static extern bool IsPosInBlock(IntPtr jpos);

		[DllImport("PathFinding")]
		private static extern void ClearIntPtr(IntPtr pBuffer);

		[DllImport("PathFinding")]
		public static extern bool IsWalkable(float[] start, int flag);

		[DllImport("PathFinding")]
		public static extern void FindNearestPoly(float[] start, float[] end, int flag, ref int nearRef, ref IntPtr pt);

		[DllImport("PathFinding")]
		public static extern float GetPolyHeight(float[] point, int flag);

		//[DllImport("PathFinding")]
		//public static extern void SetDebugFunction(IntPtr fp);
#endif
		#endregion
		private static readonly float[] DT_EXTEND = new float[] { 2, 4, 2 };

		internal override void Load(object data)
		{
			LoadNavData(data as byte[]);
		}

		internal override void UnLoad()
		{

		}

		internal override int FindStraightPath(IDetourSender sender, Vector3 start, Vector3 end, out List<Vector3> nodeList, float[] extents = null)
		{
			if (extents == null)
			{
				extents = DT_EXTEND;
			}

			int flags = 0;
			if (sender != null)
			{
				flags = sender.GetMotionAilityFlags();
			}

			nodeList = new List<Vector3>();

			float[] starts = new float[3];
			float[] ends = new float[3];
			Vector3UnityToDetour(starts, start);
			Vector3UnityToDetour(ends, end);
			int len = 0;

			IntPtr list = IntPtr.Zero;
			FindNavPath(starts, ends, flags, ref list, ref len);
			float[] path = new float[len];
			Marshal.Copy(list, path, 0, len);
			ClearIntPtr(list);
			int num = len / 3;
			for (int i = 0; i < num; i++)
			{
				Vector3 point;
				point = new Vector3(path[i * 3 + 2], path[i * 3 + 1], path[i * 3 + 0]);
				nodeList.Add(point);
			}

			return DetourState.DT_SUCCESS;
		}

		internal override int Raycast(IDetourSender sender, Vector3 start, Vector3 end, out Vector3 hitpoint)
		{
			int flags = 0;
			if (sender != null)
			{
				flags = sender.GetMotionAilityFlags();
			}

			float[] starts = new float[3];
			float[] ends = new float[3];
			Vector3UnityToDetour(starts, start);
			Vector3UnityToDetour(ends, end);

			IntPtr p = IntPtr.Zero;
			RayCast(starts, ends, flags, ref p);
			float[] ps = new float[3];
			Marshal.Copy(p, ps, 0, 3);
			ClearIntPtr(p);
			Vector3 point = Vector3DetourToUnity(ps);
			hitpoint = point;
			return DetourState.DT_SUCCESS;
		}
		internal override float GetHeight(IDetourSender sender, Vector3 point)
		{
			int flags = 0;
			if (sender != null)
			{
				flags = sender.GetMotionAilityFlags();
			}

			float[] vec = new float[3];
			Vector3UnityToDetour(vec, point);
			return GetPolyHeight(vec, flags);
		}

		internal override bool IsPositionWalkable(IDetourSender sender, Vector3 point)
		{
			int flags = 0;
			if (sender != null)
			{
				flags = sender.GetMotionAilityFlags();
			}

			float[] vec = new float[3];
			Vector3UnityToDetour(vec, point);
			return IsWalkable(vec, flags);
		}

		internal Vector3 Vector3DetourToUnity(float[] vec)
		{
			return new Vector3(vec[2], vec[1], vec[0]);
		}

		internal void Vector3UnityToDetour(float[] vec, Vector3 point)
		{
			vec[0] = point.z;
			vec[1] = point.y;
			vec[2] = point.x;
		}
	}

	/// <summary>
	/// DetourManager
	/// </summary>
#if UNITY_EDITOR
    public
#else
	internal
#endif
	class AStarTile
	{
		public AStarTile(int rol, int col, string data)
		{
			mRol = rol;
			mCol = col;
			mData = data;
		}
		public int mRol;
		public int mCol;
		public string mData;
	}


	/// <summary>
	/// DetourManager
	/// </summary>
#if UNITY_EDITOR
    public
#else
	internal
#endif
 class DetourMgr : GOESceneComponent
	{
		private DetourBase mDetour;
		public DetourBase Detour { get { return mDetour; } }
		internal bool LoadNavMesh(byte[] data)
		{
			mDetour = new RecastDetour();
			mDetour.Load(data);
			return true;
		}

		internal bool LoadAStar(List<AStarTile> data)
		{
			mDetour = new AStartDetour();
			mDetour.Load(data);
			return true;
		}

		internal void UnLoad() 
		{
            if (mDetour != null)
                mDetour.UnLoad();
		}
		internal int FindStraightPath(IDetourSender sender, Vector3 start, Vector3 end, out List<Vector3> nodeList, float[] extents = null)
		{
			return mDetour.FindStraightPath(sender, start, end, out nodeList, extents);
		}

		internal int Raycast(IDetourSender sender, Vector3 start, Vector3 end, out Vector3 hitpoint)
		{
			return mDetour.Raycast(sender, start, end, out hitpoint);
		}

		internal float GetHeight(IDetourSender sender, Vector3 point)
		{
			return mDetour.GetHeight(sender, point);
		}

		internal bool IsPositionWalkable(IDetourSender sender, Vector3 point)
		{
			return mDetour.IsPositionWalkable(sender, point);
		}

		internal bool IsDetourAStar()
		{
			return mDetour is AStartDetour;
		}
		
		internal override void OnLeave()
		{
			base.OnLeave();
		}
	}
}
