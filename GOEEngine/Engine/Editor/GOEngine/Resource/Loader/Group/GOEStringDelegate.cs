using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GOEngine.Implement
{
    internal class GOEStringDelegate : ResComponent
	{
        private Dictionary<string, Action<string, string>> mTxtHandlerDic = new Dictionary<string, Action<string, string>>();
        internal string GetString(string name, Action<string, string> callback = null)
        {
            if (mTxtHandlerDic.ContainsKey(name))
                mTxtHandlerDic[name] += callback;
            else
                mTxtHandlerDic.Add(name, callback);
            GOERoot.ResMgrImp.GetAsset(name, OnGetAsset);
            return null;
        }

        private void OnGetAsset(string name, UnityEngine.Object obj)
        {
            if (mTxtHandlerDic.ContainsKey(name))
            {
                mTxtHandlerDic[name](name, (obj as UnityEngine.TextAsset).text);
                mTxtHandlerDic.Remove(name);
            }
        }
	}
}
