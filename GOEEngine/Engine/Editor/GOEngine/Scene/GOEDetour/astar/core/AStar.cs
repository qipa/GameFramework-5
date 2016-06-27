using AStar.astar.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

namespace AStar
{
    class AStar
    {
        private AStarMap map;
        private AStarHeuristic heuristic;
        /**
         * openList The list of Cells not searched yet, sorted by their distance to the goal as guessed by our heuristic.
         */
        private List<AStarCell> closedList = new List<AStarCell>();
        private List<AStarCell> openList = new List<AStarCell>();

        public AStar(AStarMap map, AStarHeuristic heuristic)
        {
            this.map = map;
            this.heuristic = heuristic;
        }

        public List<Point> calcShortestPath(int startX, int startY, int goalX, int goalY) 
        {
            AStarCell startCell = map.getCell(startX, startY);
            AStarCell goalCell = map.getCell(goalX, goalY);

            //Check if the start cell is also an obstacle (if it is, it is impossible to find a path)
            if (AStarCell.isObstacle(startCell))
            {
				UnityEngine.Debug.Log("start is null");
                return null;
            }

            //Check if the goal cell is also an obstacle (if it is, it is impossible to find a path there)
            if (AStarCell.isObstacle(goalCell))
			{
				UnityEngine.Debug.Log("end is null");
				return null;
            }

            startCell.reset();
		    startCell.setDistanceFromStart(0);
		    closedList.Clear();
		    openList.Clear();
		    openList.Add(startCell);

		    //while we haven't reached the goal yet
		    while(openList.Count() != 0) 
            {
			    //get the first Cell from non-searched Cell list, sorted by lowest distance from our goal as guessed by our heuristic
			    AStarCell current = openList[0];

			    // check if our current Cell location is the goal Cell. If it is, we are done.
			    if(current.getX() == goalX && current.getY() == goalY) 
                {
				    return reconstructPath(current);
			    }

			    //move current Cell to the closed (already searched) list
			    openList.Remove(current);
			    closedList.Add(current);

			    //go through all the current Cells neighbors and calculate if one should be our next step
                foreach (AStarCell neighbor in map.getNeighborList(current)) 
                {
				    bool neighborIsBetter;

				    //if we have already searched this Cell, don't bother and continue to the next one 
				    if (closedList.Contains(neighbor))
					    continue;

					// calculate how long the path is if we choose this neighbor as the next step in the path 
                    float neighborDistanceFromStart = (current.getDistanceFromStart() + AStarMap.getDistanceBetween(current, neighbor));

					//add neighbor to the open list if it is not there
					if(!openList.Contains(neighbor)) 
                    {
                        neighbor.reset();
						openList.Add(neighbor);
						neighborIsBetter = true;
						//if neighbor is closer to start it could also be better
					} 
                    else if(neighborDistanceFromStart < current.getDistanceFromStart()) 
                    {
						neighborIsBetter = true;
					} 
                    else 
                    {
						neighborIsBetter = false;
					}
					// set neighbors parameters if it is better
					if (neighborIsBetter) 
                    {
						neighbor.setPreviousCell(current);
						neighbor.setDistanceFromStart(neighborDistanceFromStart);
                        neighbor.setHeuristicDistanceFromGoal(heuristic.getEstimatedDistanceToGoal(
                            neighbor.getX(), neighbor.getY(), goalCell.getX(), goalCell.getY()));

                        // csharp List.Sort use QuickSort, which is unstable, 
                        // but in java implement ArrayList.sort use MergeSort, which is stable,
                        // so here use MergeSort to generate the same result as in java implement
                        MergeSortClass.MergeSort(openList);
					}
			    }
		    }
			
			UnityEngine.Debug.Log("path is null");
		    return null;
	    }

        private List<Point> reconstructPath(AStarCell cell)
        {
            List<Point> path = new List<Point>();
            while (!(cell.getPreviousCell() == null))
            {
                path.Insert(0, cell.getPoint());
                cell = cell.getPreviousCell();
            }
            path.Insert(0, cell.getPoint());
            return path;
        }
    }
}
