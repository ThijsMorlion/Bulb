using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Create.Helpers.ImageLoader
{
    [RequireComponent(typeof(RawImage))]
    public class ImageLoader : MonoBehaviour, INotifyPropertyChanged, ILoader
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// If checked, the image will not be unloaded on disable but instead kept in the static images dictionary.
        /// </summary>
        [Tooltip("If checked, the image will not be unloaded on disable but instead kept in the static images dictionary.")]
        public bool KeepInMemory;
        /// <summary>
        /// If set, the external texture source will keep textures in memory and be in charge of releasing them.
        /// </summary>
        [Tooltip("If set, the external texture source will keep textures in memory and be in charge of releasing them.")]
        public ImageLoaderTextureSource ExternalTextureSource;
        public ImageSizes ImageSize;
        public bool CacheImage = true;
        public ImageFitMode FitMode;

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            private set
            {
                if (_isLoading == value)
                    return;
                _isLoading = value;
                RaisePropertyChanged("IsLoading");
            }
        }

        public static Dictionary<ImageSizes, string> CachedRelativePaths = new Dictionary<ImageSizes, string>()
        {
            { ImageSizes.Thumbnail, "images/thumbs" },
            { ImageSizes.Medium, "images/medium" },
            { ImageSizes.Full, "images/full" },
        };

            public static Dictionary<ImageSizes, Vector2> Dimensions = new Dictionary<ImageSizes, Vector2>()
        {
            { ImageSizes.Thumbnail, new Vector2(256,256) },
            { ImageSizes.Medium, new Vector2(840,600) },
            { ImageSizes.Full, new Vector2(1920,1080) }
        };

        /// <summary>
        /// Loaded images by filename which are to be kept in memory.
        /// </summary>
        private static Dictionary<ImageSizes, Dictionary<string, Texture2D>> _loadedImages = new Dictionary<ImageSizes, Dictionary<string, Texture2D>>();

        private string _imageUri;
        private Texture2D _texture;
        private IEnumerator _loadRoutine;

        void OnDisable()
        {
            if (_loadRoutine != null)
            {
                _imageUri = null;
                StopCoroutine(_loadRoutine);
            }

            // Unload texture from memory if so desired.
            UnloadTexture();
        }

        void OnValidate()
        {
            if (ExternalTextureSource != null)
            {
                KeepInMemory = false;
            }
        }

        public void LoadImage(string imageUri)
        {
            if (_imageUri == imageUri)
                return;

            if (_loadRoutine != null)
            {
                StopCoroutine(_loadRoutine);
            }

            _imageUri = imageUri;

            // Unset current image.
            GetComponent<RawImage>().texture = null;
            // Unload the previously loaded texture before loading the new one.
            UnloadTexture();

            if (imageUri == null || string.IsNullOrEmpty(imageUri))
                return;

            // Check if the target image is already loaded.
            if (_loadedImages.ContainsKey(ImageSize) && _loadedImages[ImageSize].ContainsKey(_imageUri))
            {
                // Apply the texture to the raw image, but don't set _texture - otherwise we might destroy the texture which is set to be kept in memory from a different object which loaded the same image but without the keep in mem flag set.
                ApplyImage(_loadedImages[ImageSize][_imageUri]);
            }
            // Check if the optional external texture source contains the target texture.
            else if (ExternalTextureSource != null && ExternalTextureSource.enabled && ExternalTextureSource.LoadedTextures != null
                && ExternalTextureSource.LoadedTextures.ContainsKey(ImageSize) && ExternalTextureSource.LoadedTextures[ImageSize].ContainsKey(_imageUri))
            {
                ApplyImage(ExternalTextureSource.LoadedTextures[ImageSize][_imageUri]);
            }
            // Load the target image otherwise.
            else
            {
                _loadRoutine = RunLoadImage();
                StartCoroutine(_loadRoutine);
            }
        }

        public static bool CachedImageExists(string imageFilename, ImageSizes imageSize)
        {
            return File.Exists(GetCachedImagePath(imageFilename, imageSize));
        }

        private IEnumerator RunLoadImage()
        {
            IsLoading = true;

            // Check if a cached version exists.
            if (File.Exists(GetCachedImagePath()))
            {
                yield return RunLoadImageFromCache();
            }
            // Else, download from the API.
            else
            {
                yield return RunLoadImageFromWeb();
            }
        }

        private IEnumerator RunLoadImageFromCache()
        {
            var imageToLoad = _imageUri;
            var www = new WWW("file:///" + GetCachedImagePath());
            yield return www;
            IsLoading = false;

            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogWarningFormat("Failed to load cached image {0}: {1}", _imageUri, www.error);
                yield break;
            }

            // Safety check to see if we didn't change files while the download was happening.
            Texture2D tex = www.textureNonReadable;
            // Clamp the texture to prevent artifacts on raw images.
            tex.wrapMode = TextureWrapMode.Clamp;
            if (_imageUri != imageToLoad)
            {
                if (tex != null)
                {
                    DestroyTexture(tex);
                }
                yield break;
            }

            if (tex == null)
                yield break;

            _texture = tex;
            // Apply the texture to the raw image.
            ApplyImage(_texture);
            // Add to in memory list if so desired.
            AddToMemoryList();
        }

        private IEnumerator RunLoadImageFromWeb()
        {
            var fileToLoad = _imageUri;

            // Start image download.
            var loader = WebLoader.LoadImage(_imageUri, size: Dimensions[ImageSize], alsoReturnBytes: CacheImage, fitMode: FitMode);
            yield return loader;

            IsLoading = false;

            // Safety check to see if we didn't change files while the download was happening.
            if (_imageUri != fileToLoad)
            {
                // Unload the no longer needed texture.
                if (loader.Current is TextureAndBytes)
                {
                    DestroyTexture(((TextureAndBytes)loader.Current).Texture);
                }
                else if (loader.Current is Texture2D)
                {
                    DestroyTexture((Texture2D)loader.Current);
                }
                yield break;
            }

            // Retrieve texture, or texture and raw bytes if we intend to cache the image.
            Texture2D texture = null;
            byte[] bytes = null;
            if (CacheImage)
            {
                if (loader.Current is TextureAndBytes)
                {
                    texture = ((TextureAndBytes)loader.Current).Texture;
                    bytes = ((TextureAndBytes)loader.Current).Bytes;
                }
            }
            else
            {
                texture = loader.Current as Texture2D;
            }

            if (texture == null)
                yield break;

            _texture = texture;
            // Clamp the texture to prevent artifacts on raw images.
            _texture.wrapMode = TextureWrapMode.Clamp;
            // Apply the texture to the raw image.
            ApplyImage(_texture);

            // Add the texture to the dictionary if it is flagged to be kept in memory.
            AddToMemoryList();

            // Cache the image if desired.
            if (CacheImage && bytes != null)
            {
                if (GetCachedImagePath() != null)
                {
                    File.WriteAllBytes(GetCachedImagePath(), bytes);
                }
            }
        }

        private void ApplyImage(Texture2D texture)
        {
            GetComponent<RawImage>().texture = texture;
            var fitter = GetComponent<AspectRatioFitter>();
            if (fitter != null)
            {
                fitter.aspectRatio = (float)texture.width / texture.height;
            }
        }

        private void AddToMemoryList()
        {
            if (_texture == null)
                return;

            // Add to external texture source if so desired.
            if (ExternalTextureSource != null)
            {
                // Texture is already being kept in static image loader memory - don't add it to the external list. 
                if (_loadedImages.ContainsKey(ImageSize) && _loadedImages[ImageSize].ContainsKey(_imageUri))
                    return;

                // External source was disabled before storing the texture - unload the texture.
                if (ExternalTextureSource.LoadedTextures == null || !ExternalTextureSource.enabled)
                {
                    UnloadTexture();
                    return;
                }

                if (!ExternalTextureSource.LoadedTextures.ContainsKey(ImageSize))
                {
                    ExternalTextureSource.LoadedTextures.Add(ImageSize, new Dictionary<string, Texture2D>());
                }
                if (!ExternalTextureSource.LoadedTextures[ImageSize].ContainsKey(_imageUri))
                {
                    ExternalTextureSource.LoadedTextures[ImageSize].Add(_imageUri, _texture);
                }
                // Texture was added by another ImageLoader while this one was loading - destroy it and revert to the existing one.
                else
                {
                    DestroyTexture(_texture);
                    _texture = ExternalTextureSource.LoadedTextures[ImageSize][_imageUri];
                    ApplyImage(_texture);
                }
            }
            // Keep in global ImageLoader memory otherwise, if so desired.
            else if (KeepInMemory)
            {
                if (!_loadedImages.ContainsKey(ImageSize))
                {
                    _loadedImages.Add(ImageSize, new Dictionary<string, Texture2D>());
                }
                if (!_loadedImages[ImageSize].ContainsKey(_imageUri))
                {
                    _loadedImages[ImageSize].Add(_imageUri, _texture);
                }
                // Texture was added by another ImageLoader while this one was loading - destroy it and revert to the existing one.
                else
                {
                    DestroyTexture(_texture);
                    _texture = _loadedImages[ImageSize][_imageUri];
                    ApplyImage(_texture);
                }
            }
        }

        private void UnloadTexture()
        {
            if (KeepInMemory || (ExternalTextureSource != null && ExternalTextureSource.enabled))
                return;

            if (_texture != null)
            {
                DestroyTexture(_texture);
                _texture = null;
            }
        }

        private string GetCachedImagePath()
        {
            return GetCachedImagePath(_imageUri, ImageSize);
        }

        public static string GetCachedImagePath(string uri, ImageSizes imageSize)
        {
            if (uri == null)
                return null;

            string rootPath = Directory.GetParent(Application.dataPath).FullName;
            string uriDir = Path.GetDirectoryName(uri);
            string cachedPath = Path.Combine(Path.Combine(rootPath, CachedRelativePaths[imageSize]), uriDir);
            if (!Directory.Exists(cachedPath))
            {
                Directory.CreateDirectory(cachedPath);
            }

            return Path.Combine(rootPath, Path.Combine(CachedRelativePaths[imageSize], uri));
        }

        private void RaisePropertyChanged(string propname)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propname));
        }

        private void DestroyTexture(Texture2D texture)
        {
#if UNITY_EDITOR
            DestroyImmediate(texture);
#else
            Destroy(texture);
#endif
        }
    }

    public enum ImageSizes
    {
        Thumbnail,
        Medium,
        Full
    }

    public enum ImageFitMode
    {
        Contain,
        Max,
        Fill,
        Stretch,
        Crop
    }
}