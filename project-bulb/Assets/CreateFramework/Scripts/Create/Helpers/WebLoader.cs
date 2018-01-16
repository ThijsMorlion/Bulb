using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Create.Helpers.ImageLoader;

namespace Create.Helpers
{
    public static class WebLoader
    {
        /// <summary>
        /// To be filled in by settings.
        /// </summary>
        public static Dictionary<string, string> AuthHeader = new Dictionary<string, string>() { { "Authorization", "Basic NjAzSWgyczRWVDNwMjc0bTFUYXF0NE8wQjF6MmwwOU86OGY0MXFQYk9pTGhOM0x1eklpMVZWVzBBYVF6YW5KbVc=" } };
        /// <summary>
        /// To be filled in by settings.
        /// </summary>
        public static string DownloadsAPI { get; set; }
        /// <summary>
        /// To be filled in by settings.
        /// </summary>
        public static string ImagesAPI { get; set; }

        public static IEnumerator LoadTextFile(string url, Dictionary<string, string> postData = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogWarning("Tried to load null or empty url.");
                yield break;
            }

            // Create www.
            WWW www = null;

            try
            {
                if (postData != null)
                {
                    WWWForm form = new WWWForm();
                    foreach (var pair in postData)
                    {
                        form.AddField(pair.Key, pair.Value);
                    }
                    www = new WWW(url, form.data, AuthHeader);
                }
                else
                {
                    www = new WWW(url, null, AuthHeader);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("Failed to access {0} : {1}\r\n{2}.", url, ex.Message, ex.StackTrace));
                yield break;
            }

            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogWarning(string.Format("{0} returned error: {1}", url, www.error));
                yield break;
            }

            // Get www text.
            if (string.IsNullOrEmpty(www.text))
            {
                Debug.LogWarning(string.Format("{0} returned null or empty string.", url));
                yield break;
            }

            yield return www.text;
        }

        public static IEnumerator SendRequest(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogWarning("Tried to load null or empty url.");
                yield break;
            }

            // Create www.
            WWW www = null;
            try
            {
                www = new WWW(url, null, AuthHeader);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("Failed to access {0} : {1}\r\n{2}.", url, ex.Message, ex.StackTrace));
                yield break;
            }

            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogWarning(string.Format("{0} returned error: {1}", url, www.error));
                yield return false;
                yield break;
            }

            yield return true;
        }

        /// <summary>
        /// Loads a Texture2D from the given image URI (not the full URL). Can alternatively provide a TextureAndBytes struct so the byte array can be used directly for image caching, preventing the need to re-encode the texture.
        /// </summary>
        public static IEnumerator LoadImage(string uri, Vector2 size = new Vector2(), bool readable = false, bool alsoReturnBytes = false, ImageFitMode fitMode = ImageFitMode.Contain)
        {
            if (string.IsNullOrEmpty(uri))
            {
                Debug.LogWarning("Tried to load null or empty url.");
                yield break;
            }

            // Create www.
            string url = string.Format("{0}/{1}?w={2}&h={3}&fit={4}", ImagesAPI, uri, size.x, size.y, fitMode.ToString().ToLower());
            if (size == Vector2.zero)
                url = uri;
            WWW www = null;
            try
            {
                www = new WWW(url, null, AuthHeader);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("Failed to access {0} : {1}\r\n{2}.", url, ex.Message, ex.StackTrace));
                yield break;
            }

            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogWarning(string.Format("{0} returned error: {1}", url, www.error));
                yield break;
            }

            Texture2D texture = null;
            // Default to non readable texture so the texture doesn't have to be kept in CPU accessible memory.
            if (!readable)
            {
                texture = www.textureNonReadable;
            }
            else
            {
                texture = www.texture;
            }

            if (alsoReturnBytes)
            {
                yield return new TextureAndBytes(www.bytes, texture);
            }
            else
            {
                yield return texture;
            }
        }

        public static IEnumerator LoadFileBytes(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                Debug.LogWarning("Tried to load null or empty url.");
                yield break;
            }

            // Create www using TAUTH.
            string url = string.Format("{0}/{1}", DownloadsAPI, uri);
            WWW www = null;
            try
            {
                www = new WWW(url);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("Failed to access {0} : {1}\r\n{2}.", url, ex.Message, ex.StackTrace));
                yield break;
            }

            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogWarning(string.Format("{0} returned error: {1}", url, www.error));
                yield break;
            }

            yield return www.bytes;
        }
    }

    public struct TextureAndBytes
    {
        public byte[] Bytes;
        public Texture2D Texture;

        public TextureAndBytes(byte[] bytes, Texture2D texture)
        {
            Bytes = bytes;
            Texture = texture;
        }
    }
}