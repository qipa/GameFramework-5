using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
 class ResBundleList : ResComponent
    {
        private List<GOEBundle> mListBundle = new List<GOEBundle>();
        private Dictionary<int, GOEBundle> mDicBundle = new Dictionary<int, GOEBundle>();

        internal void RegisterBundleIdx(string asset, string bundleName, int size = 0)
        {
            GOEBundle bundle = this.GetBundle(bundleName);
            if (bundle == null)
            {
                bundle = new GOEBundle();
                mListBundle.Add(bundle);
                mDicBundle.Add(bundleName.GetHashCode(), bundle);
                bundle.mName = bundleName;
                bundle.Size = size;
            }

            bundle.AddFile(asset);
        }

        internal void UnRegisterByBundleName(string bundleName)
        {
            int hashCode = bundleName.GetHashCode();
            for (int i = 0; i < mListBundle.Count; ++i)
            {
                if (mListBundle[i].mName == bundleName)
                {
                    mListBundle.RemoveAt(i);
                    break;
                }
            }

            mDicBundle.Remove(hashCode);
        }

        internal GOEBundle GetBundle(string name)
        {
            GOEBundle bundle = null;
            if (mDicBundle.TryGetValue(name.GetHashCode(), out bundle))
            {
                return bundle;
            }

            return null;
        }

        internal List<GOEBundle> GetBundleByBeginName(string beginName)
        {
            List<GOEBundle> list = new List<GOEBundle>();
            foreach (GOEBundle bundle in mListBundle)
            {
                if (bundle.mName.IndexOf(beginName) == 0)
                {
                    list.Add(bundle);
                }
            }

            return list;
        }

        internal int GetCount()
        {
            return mListBundle.Count;
        }

        internal GOEBundle GetBundleByIndex(int index)
        {
            if (index < 0 || index >= mListBundle.Count)
            {
                return null;
            }

            return mListBundle[index];
        }
    }
}
