using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace AStar.astar.utils
{
    class MergeSortClass
    {
        public static void MergeSort(List<AStarCell> array)
        {
            MergeSort(array, 0, array.Count());
        }

        static void MergeSort(List<AStarCell> array, int p, int r)
        {
            if (p < r - 1)
            {
                int q = (p + r) / 2;
                MergeSort(array, p, q);
                MergeSort(array, q, r);
                Merge(array, p, q, r);
            }
        }

        static void Merge(List<AStarCell> array, int p, int q, int r)
        {
            if (p + 1 >= r)
                return;
            int i = p;
            int j = q;
            List<AStarCell> newarray = new List<AStarCell>(r - p);
            while (i < q && j < r)
            {
                AStarCell left = array[i];
                AStarCell right = array[j];
                if (left.CompareTo(right) <= 0)
                {
                    newarray.Add(left);
                    i++;
                }
                else
                {
                    newarray.Add(right);
                    j++;
                }
            }
            if (i >= q)
                while (j < r)
                    newarray.Add(array[j++]);
            else
                while (i < q)
                    newarray.Add(array[i++]);

            int index = 0;
            for (i = p; i < r; i++)
                array[i] = newarray[index++];
        }

    }
}
