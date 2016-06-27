using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace AStar
{
    /**
     * The AreaMap holds information about the With, Height, 
     * Start position, Goal position and Obstacles on the map.
     * A place on the map is referred to by it's (x,y) coordinates, 
     * where (0,0) is the upper left corner, and x is horizontal and y is vertical.
     */
    abstract class AStarMap
    {
        private static float SQRT2 = (float)Math.Sqrt(2);
        // 容许的误差百分比(相当于cellSize大小的百分比)
	    protected static float ERROR_TOLERATE_RATIO = 0.1f;

        protected int widthInCells;
        protected int heightInCells;

        protected float cellSize;	// 每个单元格的大小，单位米


        // 容许的误差
        protected float errorTolerate;

        public abstract AStarCell getCell(int x, int y);

        public abstract float getHeight(int x, int y);

        public int getWidthInCells()
        {
            return widthInCells;
        }

        public int getHeightInCells()
        {
            return heightInCells;
        }

        public float getCellSize()
        {
            return cellSize;
        }

        public float getErrorTolerate()
        {
            return errorTolerate;
        }

        public List<AStarCell> getNeighborList(AStarCell cell)
        {
            List<AStarCell> neighborList = new List<AStarCell>();

            bool downWalkable = false;
            if (cell.getY() > 0)
            {// down
                AStarCell neighbor = getCell(cell.getX(), (cell.getY() - 1));
                if (!AStarCell.isObstacle(neighbor))
                {
                    neighborList.Add(neighbor);
                    downWalkable = true;
                }
            }

            bool rightWalkable = false;
            if (cell.getX() < (widthInCells - 1))
            {// right
                AStarCell neighbor = getCell(cell.getX() + 1, cell.getY());
                if (!AStarCell.isObstacle(neighbor))
                {
                    neighborList.Add(neighbor);
                    rightWalkable = true;
                }
            }

            bool upWalkable = false;
            if (cell.getY() < (heightInCells - 1))
            {// up
                AStarCell neighbor = getCell(cell.getX(), cell.getY() + 1);
                if (!AStarCell.isObstacle(neighbor))
                {
                    neighborList.Add(neighbor);
                    upWalkable = true;
                }
            }

            bool leftWalkable = false;
            if (cell.getX() > 0)
            {// left
                AStarCell neighbor = getCell(cell.getX() - 1, cell.getY());
                if (!AStarCell.isObstacle(neighbor))
                {
                    neighborList.Add(neighbor);
                    leftWalkable = true;
                }
            }

            if (downWalkable || rightWalkable)
            {
                if (cell.getX() < (widthInCells - 1) && cell.getY() > 0)
                {// down right
                    AStarCell neighbor = getCell(cell.getX() + 1, cell.getY() - 1);
                    if (!AStarCell.isObstacle(neighbor))
                    {
                        neighborList.Add(neighbor);
                    }
                }
            }

            if (upWalkable || rightWalkable)
            {
                if (cell.getX() < (widthInCells - 1) && cell.getY() < (heightInCells - 1))
                { // up right
                    AStarCell neighbor = getCell(cell.getX() + 1, cell.getY() + 1);
                    if (!AStarCell.isObstacle(neighbor))
                    {
                        neighborList.Add(neighbor);
                    }
                }
            }

            if (upWalkable || leftWalkable)
            {
                if (cell.getX() > 0 && cell.getY() < (heightInCells - 1))
                {// up left
                    AStarCell neighbor = getCell(cell.getX() - 1, cell.getY() + 1);
                    if (!AStarCell.isObstacle(neighbor))
                    {
                        neighborList.Add(neighbor);
                    }
                }
            }

            if (downWalkable || leftWalkable)
            {
                if (cell.getX() > 0 && cell.getY() > 0)
                {// down left
                    AStarCell neighbor = getCell(cell.getX() - 1, cell.getY() - 1);
                    if (!AStarCell.isObstacle(neighbor))
                    {
                        neighborList.Add(neighbor);
                    }
                }
            }
            
            return neighborList;
        }

        /**
         * Determine the distance between two neighbor Cells 
         * as used by the AStar algorithm.
         * 
         * @param cell1 any Cell
         * @param cell2 any of Cell1's neighbors
         * @return Float - the distance between the two neighbors
         */
        public static float getDistanceBetween(AStarCell cell1, AStarCell cell2)
        {
            //if the cells are on top or next to each other, return 1
            if (cell1.getX() == cell2.getX() || cell1.getY() == cell2.getY())
            {
                return 1;//*(mapHeight+mapWith);
            }
            else
            { //if they are diagonal to each other return diagonal distance: sqrt(1^2+1^2)
                return SQRT2;//*(mapHeight+mapWith);
            }
        }

        public static int makeCellId(int x, int y)
        {
            return y << 16 | x;
        }
    }
}
