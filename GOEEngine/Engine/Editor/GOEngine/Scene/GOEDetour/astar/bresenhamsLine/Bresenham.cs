using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace AStar.astar.bresenhamsLine
{
    /**
     * Implementation of the Bresenham line algorithm.
     * @author fragkakis
     *
     */
    class Bresenham
    {
        /**
	 * Returns the list of array elements that comprise the line. 
	 * @param start the starting point
	 * @param goal the finishing point
	 * @return the line as a list of array elements
	 */
        public static List<Point> getCellsOnLine(Point start, Point goal)
        {

            List<Point> line = new List<Point>();

            int dx = Math.Abs(goal.x - start.x);
            int dy = Math.Abs(goal.y - start.y);

            int sx = start.x < goal.x ? 1 : -1;
            int sy = start.y < goal.y ? 1 : -1;

            float r = (float)dy / (float)dx;
            int lasyfy = start.y;
            for (int tx = start.x; tx != goal.x-sx; tx += sx)
            {
                int fy = (int)(r * (tx+1) + start.y);
                lasyfy = fy;
                for( int cur = lasyfy; cur <= fy; ++cur)
                {
                    line.Add( new Point( tx, cur));
                }
            }

          
            return line;
        }
    }
}
