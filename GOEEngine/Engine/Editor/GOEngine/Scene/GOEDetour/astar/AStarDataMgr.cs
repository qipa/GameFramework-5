using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace AStar.astar
{
	class AStarDataMgr
	{
		private Dictionary<int, AStarData> astarDatas = new Dictionary<int, AStarData>();
		private AStarData _nullTile = null;
		public void Clear()
		{
			astarDatas.Clear();
		}

		public void initstring(int tileRol, int tileCol, string str)
		{
			int tileId = makeTileId(tileRol, tileCol);
			AStarData data;
			if (astarDatas.TryGetValue(tileId, out data))
			{
				return;
			}
			string[] strs = str.Split(new char[] { '\n', '\r' });

			AStarData astarData = AStarData.Parse(strs);
			astarDatas.Add(tileId, astarData);

			if (_nullTile == null)
			{
				_nullTile = new AStarDataNull(astarData.getWidthInCells(), astarData.getHeightInCells(), astarData.getCellSize());
			}
		}

		public AStarData getAstarData(int tileRol, int tileCol)
		{
			int tileId = makeTileId(tileRol, tileCol);
			AStarData data;
			if (!astarDatas.TryGetValue(tileId, out data))
			{
				return _nullTile;
			}
			return data;
		}

		private int makeTileId(int x, int y)
        {
            return y << 16 | x;
        }
	}
}
