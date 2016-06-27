using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOEngine.Implement
{
    class LocaleResource : ResComponent
    {
        private Dictionary<string, string> localeValues = new Dictionary<string, string>();
        public void AddLanguageResource(string text, char split = '=')
        {
            if (text == null || text.Length == 0)
                return;
            text = text.Replace("\r", "");
            string[] lines = text.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach(string line in lines)
            {
                if (line.Length == 0 || line.IndexOf("#") != -1 || line.IndexOf("//") != -1)
                    continue;
                string[] kandv = line.Split(split);
                if (kandv.Length != 2)
                    continue;
                if (!localeValues.ContainsKey(kandv[0]))
                    localeValues.Add(kandv[0], kandv[1].Replace("\\n", "\n"));
                else
                    UnityEngine.Debug.LogWarning("Duplicate locale text for:" + kandv[0]);
            }
        }

        public string GetStringByKey(string key)
        {
            string result = string.Empty;
            localeValues.TryGetValue(key, out result);
            return result;

        }

        public string GetStringByKey(string key, params string[] param)
        {
            string result = GetStringByKey(key);
            int index = 0;
            foreach (string p in param )
               result = result.Replace('{' + index.ToString() + '}', p);

            return result;
        }
    }
}
