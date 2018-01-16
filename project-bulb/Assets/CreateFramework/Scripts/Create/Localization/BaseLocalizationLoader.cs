using System.Collections;
using UnityEngine;

namespace Create.Localization
{
    public abstract class BaseLocalizationLoader : MonoBehaviour
    {
        IEnumerator Start()
        {
            yield return LoadLanguages();
            yield return LoadLocalizationItems();
        }

        protected abstract IEnumerator LoadLocalizationItems();
        protected abstract IEnumerator LoadLanguages();
    }
}