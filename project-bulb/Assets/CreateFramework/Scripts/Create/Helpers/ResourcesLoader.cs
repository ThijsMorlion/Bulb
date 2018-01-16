using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

namespace Create.Helpers
{
    public class ResourcesLoader : MonoBehaviour
    {
        private static Dictionary<string, ResourceRequest> _requestedResources = new Dictionary<string, ResourceRequest>();

        /// <summary>
        /// Requests the resource, or if it is already loaded, returns the result right away.
        /// </summary>
        /// <param name="name">Path must be relative to Resource, using forward slashes only, and without extension.</param>
        public T GetResource<T>(string name, out ResourceRequest request, out bool isAlreadyRequesting) where T : UnityEngine.Object
        {
            if (_requestedResources.ContainsKey(name))
            {
                request = _requestedResources[name];
                isAlreadyRequesting = true;
                if (_requestedResources[name].isDone)
                {
                    return (T)_requestedResources[name].asset;
                }
            }
            else
            {
                _requestedResources.Add(name, Resources.LoadAsync<T>(name));
                request = _requestedResources[name];
                isAlreadyRequesting = false;
                if (request.isDone && request.asset == null)
                {
                    Debug.LogWarningFormat("Requested resource \"{0}\" does not exist.", name);
                }
            }

            return null;
        }

        public static IEnumerator RunAwaitSpriteIntoImage(ResourceRequest request, Image image)
        {
            if (request == null || image == null)
                yield break;

            yield return request;
            image.sprite = request.asset as Sprite;
        }
    }
}