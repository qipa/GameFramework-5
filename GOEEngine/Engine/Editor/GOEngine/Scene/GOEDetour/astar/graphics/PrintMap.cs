using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace AStar.astar.graphics
{
    class PrintMap
    {
        public PrintMap(AStarMap map, List<Point> shortestPath) 
        {
            StringBuilder sb = new StringBuilder();
		    AStarCell cell;
            for (int y = 0; y < map.getHeightInCells(); y++) 
            {
			    if(y == 0) 
                {
                    for (int i = 0; i <= map.getHeightInCells(); i++)
                    {
                        sb.Append("-");
                    }
				    sb.AppendLine();	
			    }
			    sb.Append("|");

                for (int x = 0; x < map.getWidthInCells(); x++) 
                {
				    cell = map.getCell(x, y);

                    if (AStarCell.isObstacle(cell)) 
                    {
					    sb.Append("X");
				    } 
                    else if(isStart(shortestPath, cell.getPoint())) 
                    {
					    sb.Append("s");
				    } 
                    else if(isGoal(shortestPath, cell.getPoint())) 
                    {
					    sb.Append("g");
				    } 
                    else if (contains(shortestPath, cell.getPoint())) 
                    {
					    sb.Append("?");
				    } 
                    else 
                    {
					    sb.Append(" ");
				    }
                    if (y == map.getHeightInCells())
                    {
                        sb.Append("_");
                    }
			    }

			    sb.Append("|");
			    sb.AppendLine();
		    }
            for (int i = 0; i <= map.getHeightInCells(); i++)
            {
                sb.Append("-");
            }
            //System.Console.SetWindowSize(System.Console.LargestWindowWidth, System.Console.LargestWindowHeight);
            //System.Console.Write(sb.ToString());
	    }

        private static bool contains(List<Point> shortestPath, Point point)
        {
            foreach (Point p in shortestPath)
            {
                if (p.Equals(point))
                    return true;
            }

            return false;
        }

        private static bool isStart(List<Point> shortestPath, Point point)
        {
            if (shortestPath.Count <= 0)
            {
                return false;
            }

            return shortestPath[0].Equals(point);
        }

        private static bool isGoal(List<Point> shortestPath, Point point)
        {
            if (shortestPath.Count <= 0)
            {
                return false;
            }

            return shortestPath[shortestPath.Count - 1].Equals(point);
        }
    }

}
