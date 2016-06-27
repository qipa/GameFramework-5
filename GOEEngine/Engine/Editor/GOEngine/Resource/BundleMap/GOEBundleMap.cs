using System;
using System.IO;
using System.Collections.Generic;
namespace GOEngine.Implement
{
    public class GOEBundleMapItem
    {
        const char splitchar = ':';
        string _name = string.Empty, _finalName = string.Empty, _md5 = string.Empty;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

        public string FinalName
        {
            get
            {
                return _finalName;
            }
            set
            {
                _finalName = value;
            }
        }
        public int Size { get; set; }
        public string MD5
        {
            get
            {
                return _md5;
            }
            set
            {
                _md5 = value;
            }
        }

#if UNITY_EDITOR
        public bool Create (string path)
        {
            _name = Path.GetFileName(path);
            _finalName = string.Empty;
            Size = (int)EngineFileUtil.GetFileLength(path);
            MD5 = string.Empty;
            FileStream fs = new FileStream (path, FileMode.Open);
            if (null != fs) {
                MD5 = SysUtil.GetMD5Str (fs);
                fs.Close ();
            }
            if (MD5 == string.Empty) {
                return false;
            }
            return true;
        }
#endif
        public void FromString(string str)
        {
            string[] strs = JsonUtil.StringToStringArray(str, splitchar);
            _name = strs[0];
            _finalName = strs[1];
            Size = int.Parse(strs[2]);
            _md5 = strs[3];
        }

        public override string ToString()
        {
            return Name
                + splitchar + FinalName
                    + splitchar + Size
                        + splitchar + MD5;
        }
    }

    public struct GOEBundleInfo
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public HashSet<string> Assets { get; set; }

        public HashSet<string> Dependencies { get; set; }
    }

    public class GOEBundleMap
    {
        Dictionary<string, GOEBundleMapItem> _bundleMap = new Dictionary<string, GOEBundleMapItem>();
        public Dictionary<string, GOEBundleMapItem> BundleMap
        {
            get
            {
                return _bundleMap;
            }
        }
        public const string BUNDLE_INFO_PREFIX = "&BundleInfo%";

        public bool Read(string stream)
        {
            _bundleMap.Clear();
            return AppendRead(stream);
        }


        public bool AppendRead(string stream)
        {
            StringReader sr = new StringReader(stream);
            string line = sr.ReadLine();
            while (null != line)
            {
                if (line.StartsWith(BUNDLE_INFO_PREFIX))
                {
                    byte[] buf = Convert.FromBase64String(line.Substring(BUNDLE_INFO_PREFIX.Length + 1));
                    MemoryStream ms = new MemoryStream(buf);
                    BinaryReader br = new BinaryReader(ms);

                    int bundleCnt = br.ReadInt32();
                    for (int i = 0; i < bundleCnt; i++)
                    {
                        string name = br.ReadString();
                        int size = br.ReadInt32();
                        int assetCnt = br.ReadInt32();
                        HashSet<string> assets = new HashSet<string>();
                        string firstAsset = null;
                        for (int j = 0; j < assetCnt; j++)
                        {
                            string assetName = br.ReadString();

                            if (firstAsset == null)
                                firstAsset = assetName;
                            ResourceMgr.Instance().RegisterBundleIdx(assetName, name, size);
                        }
                        GOEBundle bundle = ResourceMgr.Instance().GetBundle(name);
                        bundle.FirstAsset = firstAsset;
                        int dependCnt = br.ReadInt32();
                        for (int j = 0; j < dependCnt; j++)
                        {
                            string depName = br.ReadString();
                            bundle.DependsOn.Add(depName);
                        }
                    }
                }
                else
                {
                    GOEBundleMapItem bmi = new GOEBundleMapItem();
                    bmi.FromString(line);
                    if (!_bundleMap.ContainsKey(bmi.Name))
                    {
                        _bundleMap.Add(bmi.Name, bmi);
                    }
                }
                line = sr.ReadLine();
            }
            return true;
        }

        public string ToString(List<GOEBundleInfo> lst)
        {
            StringWriter sw = new StringWriter();
            sw.Write(ToString());
            sw.Write(BUNDLE_INFO_PREFIX);
            sw.Write(':');
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(lst.Count);
            foreach (var i in lst)
            {
                bw.Write(i.Name);
                bw.Write(i.Size);
                bw.Write(i.Assets.Count);
                
                foreach (var j in i.Assets)
                {
                    bw.Write(j);
                }
                bw.Write(i.Dependencies.Count);
                foreach (var j in i.Dependencies)
                {
                    bw.Write(j);
                }
            }
            sw.WriteLine(Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length));
            return sw.ToString();
        }

        public override string ToString()
        {
            StringWriter sw = new StringWriter();
            foreach (KeyValuePair<string, GOEBundleMapItem> kvp in _bundleMap)
            {
                GOEBundleMapItem bmi = kvp.Value;
                sw.WriteLine(bmi.ToString());
            }
            return sw.ToString();
        }
    }
}

