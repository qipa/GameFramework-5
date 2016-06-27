using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace AStar.astar
{
    /**
     * @author g
     * 对于多地块场景，游戏中，同一线程下的所有相同场景不能共享CellMgr中的Cell
     * 原因是在同一场景中，可能包括多块相同的地块，这样在A*寻路时会产生冲突
     * 故这里会拷贝一份CellMgr中的Cell
     *
     */
    class AStarMultiTileMap : AStarMap
    {
        private Dictionary<int, AStarCell> cells = new Dictionary<int, AStarCell>();
	
	    private int tileCountX;
	    private int tileCountY;
		private AStarDataMgr astarDatas;
	
	    private int cellCountXPerTile;
	    private int cellCountYPerTile;
	
	    public AStarMultiTileMap(AStarDataMgr datas, int tileCountX, int tileCountY)
        {
			this.astarDatas = datas;
			this.tileCountX = tileCountX;
			this.tileCountY = tileCountY;

			AStarData firstData = astarDatas.getAstarData(0, 0);
		    this.cellCountXPerTile = firstData.getWidthInCells();
		    this.cellCountYPerTile = firstData.getHeightInCells();
            this.cellSize = firstData.getCellSize();
            this.errorTolerate = cellSize * ERROR_TOLERATE_RATIO;

			this.widthInCells = tileCountX * cellCountXPerTile;
			this.heightInCells = tileCountY * cellCountYPerTile;
		
		    checkTiles();
		
		    loadTiles();
	    }

	    public override AStarCell getCell(int x, int y)
        {
		    int cellId = makeCellId(x, y);
			AStarCell cell = null;
			if (cells.TryGetValue (cellId, out cell)) 
			{
			}
			return cell;
	    }
	
	    private void checkTiles()
        {
		    for(int h = 0; h < tileCountY; ++h)
            {
			    for(int w = 0; w < tileCountX; ++w)
                {
					AStarData astarData = astarDatas.getAstarData(h, w); ;
					if (astarData == null)
					{
						throw new ApplicationException("tile invalid:" + h + "," + w);
					}
				
				    if(astarData.getWidthInCells() != cellCountXPerTile
						    || astarData.getHeightInCells() != cellCountYPerTile)
                    {
					    throw new ApplicationException("tile size invalid:" + h + "," + w);
				    }
			    }
		    }
	    }
	
	    private void loadTiles()
        {
		    for(int tileY = 0; tileY < tileCountY; ++tileY)
            {
			    for(int tileX = 0; tileX < tileCountX; ++tileX)
                {
					AStarData astarData = astarDatas.getAstarData(tileY, tileX);

				    for(int cellY = 0; cellY < cellCountYPerTile; ++cellY)
                    {
					    for(int cellX = 0; cellX < cellCountXPerTile; ++cellX)
                        {
						    int obstacle = astarData.getObstacle(cellX, cellY);
						    if(obstacle == 1)
							    continue;
						
						    int x = tileX * cellCountXPerTile + cellX;
						    int y = tileY * cellCountYPerTile + cellY;
							/*UnityEngine.GameObject go = new UnityEngine.GameObject();
							AStarHelper ash = go.AddComponent<AStarHelper>();
							ash.Length = 1;
							go.transform.position = new UnityEngine.Vector3(x,0,-y);*/
						    int cellId = makeCellId(x, y);
						    cells.Add(cellId, new AStarCell(x, y));
					    }
				    }
			    }
		    }
	    }

	    public override float getHeight(int x, int y)
        {
		    int tileX = x / cellCountXPerTile;
		    int tileY = y / cellCountYPerTile;
		    int cellX = x % cellCountXPerTile;
		    int cellY = y % cellCountYPerTile;
		
		    if(tileX >= tileCountX || tileY >= tileCountY)
            {
                throw new ApplicationException("pos invalid, x=" + x + ",y=" + y);
		    }
		
		    return astarDatas.getAstarData(tileY, tileX).getHeight(cellX, cellY);
	    }
    }
}
