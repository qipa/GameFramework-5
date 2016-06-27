using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOEngine.Implement
{
    public class GOEBundle
    {
        private int mSize = 0;
        private HashSet<string> mSetFile = new HashSet<string>();
        HashSet<string> dependOn = new HashSet<string>();
        public string mName = string.Empty;

        public string FirstAsset { get; set; }

        public bool BundlePersistent { get; set; }

        public bool AssetPersistent { get; set; }

        public bool IsDependency { get; set; }

        public int Size
        {
            set { mSize = value; }
            get { return mSize; }
        }

        internal void AddFile(string file)
        {
            mSetFile.Add( file );
        }

        public HashSet<string> Files
        {
            get { return mSetFile; }
        }

        public HashSet<string> DependsOn
        {
            get { return dependOn; }
        }

    }
}
