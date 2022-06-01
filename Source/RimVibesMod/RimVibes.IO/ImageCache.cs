using RimVibes.UI;
using UnityEngine;
using Verse;

namespace RimVibes.IO;

[StaticConstructorOnStartup]
public static class ImageCache
{
    private static string currentUrl = "";

    private static Texture2D current;

    private static uint currentDownloadIndex;

    private static Texture2D MissingImage => HUD.MissingAlbumArt;

    public static Texture2D GetImage(string url, int w, int h)
    {
        if (string.IsNullOrWhiteSpace(url) || w <= 0 || h <= 0)
        {
            return MissingImage;
        }

        if (url == currentUrl)
        {
            return current ?? MissingImage;
        }

        currentDownloadIndex++;
        if (current != null)
        {
            Object.Destroy(current);
        }

        current = null;
        currentUrl = url;
        ImageDownloader.Download(w, h, url, currentDownloadIndex, delegate(object token, Texture2D tex)
        {
            if (!(tex == null) && (uint)token == currentDownloadIndex)
            {
                current = tex;
            }
        });
        return MissingImage;
    }
}