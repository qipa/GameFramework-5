using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
////using System.Threading.Tasks;
using System.Collections;

namespace AStar
{
    class AStarCell : IComparable<AStarCell>, IEquatable<AStarCell>
    {
        /* Cells that this is connected to */
        private float distanceFromStart;
        private float heuristicDistanceFromGoal;
        private AStarCell previousCell;
        private int x;
        private int y;

        public AStarCell(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.distanceFromStart = Int32.MaxValue;
        }

        public float getDistanceFromStart()
        {
            return distanceFromStart;
        }

        public void setDistanceFromStart(float f)
        {
            this.distanceFromStart = f;
        }

        public AStarCell getPreviousCell()
        {
            return previousCell;
        }

        public void setPreviousCell(AStarCell previousCell)
        {
            this.previousCell = previousCell;
        }

        public float getHeuristicDistanceFromGoal()
        {
            return heuristicDistanceFromGoal;
        }

        public void setHeuristicDistanceFromGoal(float heuristicDistanceFromGoal)
        {
            this.heuristicDistanceFromGoal = heuristicDistanceFromGoal;
        }

        public int getX()
        {
            return x;
        }

        public int getY()
        {
            return y;
        }

        public Point getPoint()
        {
            return new Point(x, y);
        }

        public bool isObstacle()
        {
            return false;
        }

        public bool Equals(AStarCell other)
        {
            return (x == other.x) && (y == other.y);
        }

        public int CompareTo(AStarCell otherCell)
        {
            float thisTotalDistanceFromGoal = heuristicDistanceFromGoal + distanceFromStart;
            float otherTotalDistanceFromGoal = otherCell.getHeuristicDistanceFromGoal() + otherCell.getDistanceFromStart();

            if(Math.Abs(thisTotalDistanceFromGoal - otherTotalDistanceFromGoal) < 0.01)
            {
                return 0;
            }
            else if (thisTotalDistanceFromGoal < otherTotalDistanceFromGoal)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        public void reset()
        {
            this.distanceFromStart = Int32.MaxValue;
            this.heuristicDistanceFromGoal = 0;
            this.previousCell = null;
        }

        public static bool isObstacle(AStarCell cell)
        {
            return (cell == null) || cell.isObstacle();
        }
    }
}
