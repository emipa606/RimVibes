using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using UnityEngine;
using Verse;
using Random = System.Random;

namespace RimVibes.IO;

public static class ImageDownloader
{
    private static WebClient client;

    private static readonly Dictionary<string, string> urlToPath = new Dictionary<string, string>();

    public static void Download(int width, int height, string url, object token, Action<object, Texture2D> onDownloaded)
    {
        if (client == null)
        {
            client = new WebClient();
            client.DownloadFileCompleted += OnDownloadFinished;
        }

        if (urlToPath.TryGetValue(url, out var filePath))
        {
            try
            {
                var userState = new DownloadRequest
                {
                    OnDownloaded = onDownloaded,
                    FilePath = filePath,
                    Width = width,
                    Height = height,
                    Token = token
                };
                OnDownloadFinished(null, new AsyncCompletedEventArgs(null, false, userState));
                return;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return;
            }
        }

        var random = new Random();
        var temporaryCachePath = Application.temporaryCachePath;
        string text;
        do
        {
            var path = $"{random.Next(0, 99999)}.png";
            text = Path.Combine(temporaryCachePath, path);
        } while (File.Exists(text));

        urlToPath.Add(url, text);
        try
        {
            client.DownloadFileAsync(new Uri(url), text, new DownloadRequest
            {
                OnDownloaded = onDownloaded,
                FilePath = text,
                Width = width,
                Height = height,
                Token = token
            });
        }
        catch (Exception ex2)
        {
            Log.Error("Failed to start image download:");
            Log.Error(ex2.ToString());
        }
    }

    private static void OnDownloadFinished(object sender, AsyncCompletedEventArgs e)
    {
        var downloadRequest = e.UserState as DownloadRequest;
        var action = downloadRequest?.OnDownloaded;
        if (action == null)
        {
            return;
        }

        if (e.Error != null)
        {
            Log.Warning($"Failed to download texture: [{e.Error.GetType().Name}] {e.Error.Message}");
            action(downloadRequest.Token, null);
            return;
        }

        if (e.Cancelled)
        {
            action(downloadRequest.Token, null);
            return;
        }

        if (!File.Exists(downloadRequest.FilePath))
        {
            Log.Warning("Failed to download texture, downloaded but file missing...");
            action(downloadRequest.Token, null);
            return;
        }

        try
        {
            var texture2D = new Texture2D(downloadRequest.Width, downloadRequest.Height, TextureFormat.RGB24, false,
                true);
            texture2D.LoadImage(File.ReadAllBytes(downloadRequest.FilePath));
            action(downloadRequest.Token, texture2D);
        }
        catch (Exception ex)
        {
            Log.Warning($"Failed to download texture, exception loading from file: [{ex.GetType().Name}] {ex.Message}");
        }
    }

    public static void Dispose()
    {
        client?.Dispose();
        client = null;
    }

    private class DownloadRequest
    {
        public string FilePath;

        public int Height;
        public Action<object, Texture2D> OnDownloaded;

        public object Token;

        public int Width;
    }
}