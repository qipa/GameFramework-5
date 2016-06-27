using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GOEngine.Implement
{
    /// <summary>
    /// 项目中未使用
    /// </summary>
    class ClientHeightMap:GOESceneComponent
    {
        private float _offX;
        private float _offY;
        private int _mapW;
        private int _mapH;
        private string[] _sceneH;
        public void ReadData(string content)
        {
            string[] sts1 = content.Split(new string[]{"\n"}, StringSplitOptions.RemoveEmptyEntries);

            string[] offs = sts1[0].Split(',');
            _offX = float.Parse(offs[0]);
            _offY = float.Parse(offs[2]);

            string[] sizes = sts1[2].Split(',');
            _mapW = int.Parse(sizes[0]);
            _mapH = int.Parse(sizes[1]);
            _sceneH = sts1[3].Split(',');
        }

        public float GetHeight( float x, float z)
        {

            if (_sceneH == null || _sceneH.Length == 0)
                return 0;
            int intx = Convert.ToInt32(x - _offX);
            int inty = Convert.ToInt32(z - _offY);
            if (intx < 0 || inty < 0)
                return 0;
            int ind = inty * _mapW + intx;
            if(ind < _sceneH.Length)
            {
                return float.Parse(_sceneH[ind]);
            }
            return 0;
        }

        public void Clear()
        {
            if (_sceneH != null)
                _sceneH = null;
        }
    }
}
