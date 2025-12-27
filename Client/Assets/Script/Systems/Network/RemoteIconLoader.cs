using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class RemoteIconLoader
{
    private static readonly Dictionary<string, Sprite> _cache = new();

    public static IEnumerator LoadInto(Image target, string baseUrl, string iconKey, int version,
                                       int pixelsPerUnit = 100, bool setNativeSize = false,
                                       Action<Sprite> onLoaded = null, Action<string> onError = null)
    {
        if (target == null) yield break;
        var url = $"{baseUrl.TrimEnd('/')}/icons/{iconKey}.png?v={version}";

        if (_cache.TryGetValue(url, out var cached) && cached != null)
        {
            target.sprite = cached; target.enabled = true;
            if (setNativeSize) target.SetNativeSize();
            onLoaded?.Invoke(cached);
            yield break;
        }

        using var req = UnityWebRequestTexture.GetTexture(url, true);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            target.enabled = false;
            onError?.Invoke($"Icon load failed: {req.error} ({url})");
            yield break;
        }

        var tex = DownloadHandlerTexture.GetContent(req);
        tex.wrapMode = TextureWrapMode.Clamp;
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        _cache[url] = sprite;

        target.sprite = sprite; target.enabled = true;
        if (setNativeSize) target.SetNativeSize();
        onLoaded?.Invoke(sprite);
    }
}
