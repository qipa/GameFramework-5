﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace AStar
{
    /**
     * Calculate the diagonal distance to goal when 
     * a straight step costs 1 and diagonal step costs sqrt(2).
     */
    class DiagonalHeuristic : AStarHeuristic
    {
        public float getEstimatedDistanceToGoal(int startX, int startY, int goalX, int goalY)
        {
            float h_diagonal = (float)Math.Min(Math.Abs(startX - goalX), Math.Abs(startY - goalY));
            float h_straight = (float)(Math.Abs(startX - goalX) + Math.Abs(startY - goalY));
            float h_result = (float)(Math.Sqrt(2) * h_diagonal + (h_straight - 2 * h_diagonal));

            /**
             * Breaking ties: Adding a small value to the heuristic to avoid A* fully searching all equal length paths
             * We only want 1 shortest path, not all of them.
             * 
             * @param p The small value we add to the heuristic. Should be p < (minimum cost of taking one step) / (expected maximum path length) to avoid 
             */

            float p = (1 / 10000);
            h_result *= (1.0f + p);

            return h_result;
        }
    }
}
