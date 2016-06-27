using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace AStar
{
    /**
     * A heuristic that uses the tile that is closest to the target
     * as the next best tile.
     */
    class ClosestHeuristic : AStarHeuristic
    {
        public float getEstimatedDistanceToGoal(int startX, int startY, int goalX, int goalY)
        {
            float dx = goalX - startX;
            float dy = goalY - startY;

            float result = (float)(Math.Sqrt((dx * dx) + (dy * dy)));

            //Optimization! Changed to distance^2 distance: (but looks more "ugly")

            //float result = (float) (dx*dx)+(dy*dy);


            return result;
        }
    }
}
