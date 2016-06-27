using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
////using System.Threading.Tasks;

namespace AStar.astar
{
    class AStarData
    {
        private const String SPLIT_SUFFIX = ",";

        private int widthInCells;
        private int heightInCells;
        private float cellSize;
	    private int[,] obstacleInfo;
	    private float[,] heightInfo;

        public AStarData(int widthInCells, int heightInCells, float cellSize)
        {
            this.widthInCells = widthInCells;
            this.heightInCells = heightInCells;
            this.cellSize = cellSize;
        }


	    public static AStarData Parse(String[] str)
        {
            //String fileName = System.IO.Path.GetFileName(file);
            //StreamReader sr = new StreamReader(file, Encoding.Default);
		    try
            {			
			    String[] headerInfo = str[0].Split(SPLIT_SUFFIX.ToCharArray());
			    if(headerInfo.Length != 3)
                {
				    //throw new ApplicationException("astar文件头数据不合法，文件:" + fileName);
			    }
			
			    int widthInCells = int.Parse(headerInfo[0]);
			    int heightInCells = int.Parse(headerInfo[1]);
			    float cellSize = float.Parse(headerInfo[2]);

				AStarData astarData = new AStarData(widthInCells, heightInCells, cellSize);

				astarData.obstacleInfo = new int[heightInCells, widthInCells];
				astarData.heightInfo = new float[heightInCells, widthInCells];
			
			    for(int h = 0; h < heightInCells; ++h)
                {
                    String[] lineData = str[h+1].Split(SPLIT_SUFFIX.ToCharArray());
				    if(lineData.Length < widthInCells * 2)
                    {
                        //throw new ApplicationException("astar寻路文件错误：读取文件错误。文件:" + fileName + ",行:" + (h + 2));
				    }
				
				    for(int w = 0; w < widthInCells; ++w)
                    {
						astarData.obstacleInfo[h, w] = int.Parse(lineData[w * 2]);
						astarData.heightInfo[h, w] = float.Parse(lineData[w * 2 + 1]);
				    }
			    }

				return astarData;
		    }
            catch (IOException e)
            {
                //throw new ApplicationException("astar寻路文件错误：读取文件错误。文件:" + fileName);
		    }
            finally
            {
                //sr.Close();
			}
			return null;
	    }

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

	    public int getObstacle(int x, int y)
        {
		    try
            {
			    return obstacleInfo[y, x];
		    }
            catch (Exception e)
            {
			    return 1;
		    }
	    }
	
	    public float getHeight(int x, int y)
        {
		    try
            {
			    return heightInfo[y, x];
		    }
            catch (Exception e)
            {
			    return 0f;
		    }
	    }
    }

	class AStarDataNull : AStarData
	{
		public AStarDataNull(int widthInCells, int heightInCells, float cellSize)
			: base(widthInCells, heightInCells, cellSize)
		{

		}

		public int getObstacle(int x, int y)
		{
			return 0;
		}

		public float getHeight(int x, int y)
		{
			return 0f;
		}
	}
}
