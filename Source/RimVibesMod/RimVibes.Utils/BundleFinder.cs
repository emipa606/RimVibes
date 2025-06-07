using UnityEngine;
using Verse;

namespace RimVibes.Utils;

public static class BundleFinder
{
    public static AssetBundle GetByName(this ModAssetBundlesHandler h, string name)
    {
        foreach (var loadedAssetBundle in h.loadedAssetBundles)
        {
            if (loadedAssetBundle.name == name)
            {
                return loadedAssetBundle;
            }
        }

        return null;
    }
}