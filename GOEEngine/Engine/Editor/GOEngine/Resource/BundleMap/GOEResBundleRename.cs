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
        class BundleRename : ResComponent
    {
        internal GOEBundleMap bundlemap = new GOEBundleMap();

        internal void Read(string stream)
        {
            bundlemap.Read(stream);
        }

        internal void AppendRead(string stream)
        {
            bundlemap.AppendRead(stream);
        }

        internal string GetBundleNameFromOriginalName(string originalName)
        {
            GOEBundleMapItem bmi = GetBundleItemFromOriginalName(originalName);
            if (null == bmi)
                return null;
            else
                return bmi.FinalName;
        }

        internal GOEBundleMapItem GetBundleItemFromOriginalName(string originalName)
        {
            GOEBundleMapItem bmi = null;
            if ( bundlemap.BundleMap.TryGetValue( originalName, out bmi ) )
            {
                return bmi;
            }
            else
            {
                Logger.GetFile( LogFile.Res ).LogError( originalName + " not found in BundleMap" );
                return null;
            }
        }
    }
}
