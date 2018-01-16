using System.Collections.Generic;
using UnityEngine;

namespace Create.Helpers.ImageLoader
{
    /// <summary>
    /// An external component that can manage texture memory for ImageLoaders.
    /// </summary>
    public class ImageLoaderTextureSource : MonoBehaviour
    {
        protected Dictionary<ImageSizes, Dictionary<string, Texture2D>> _loadedTextures = new Dictionary<ImageSizes, Dictionary<string, Texture2D>>();
        public Dictionary<ImageSizes, Dictionary<string, Texture2D>> LoadedTextures
        {
            get { return _loadedTextures; }
            protected set { _loadedTextures = value; }
        }

        protected virtual void OnDisable()
        {
            UnloadTextures();
        }

        public virtual void UnloadTextures()
        {
            if (LoadedTextures != null)
            {
                foreach (var imageSizeGroup in LoadedTextures)
                {
                    foreach (var idAndTexture in imageSizeGroup.Value)
                    {
#if UNITY_EDITOR
                        DestroyImmediate(idAndTexture.Value);
#else
                    Destroy(idAndTexture.Value);
#endif
                    }
                }

                LoadedTextures.Clear();
            }
        }
    }
}