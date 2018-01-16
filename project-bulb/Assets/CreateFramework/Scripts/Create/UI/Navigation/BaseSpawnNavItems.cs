using Create.Localization.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Create.UI.Navigation
{
    [RequireComponent(typeof(ToggleGroup))]
    public class BaseSpawnNavItems<T> : MonoBehaviour where T : struct, IConvertible
    {
        public GameObject Prefab;
        public T[] PagesToSpawn;

        void Start()
        {
            SpawnItems();
        }

        private void SpawnItems()
        {
            if (Prefab == null)
            {
                Debug.LogWarningFormat("[{0}] No prefab assigned in {1}.", GetType(), name);
                return;
            }

            if (PagesToSpawn == null)
            {
                PagesToSpawn = Enum.GetValues(typeof(T)) as T[];
            }
            var toggleGroup = GetComponent<ToggleGroup>();
            foreach (var page in PagesToSpawn)
            {
                var navItem = Instantiate(Prefab, transform, false);

                // Set toggle group.
                var toggle = navItem.GetComponentInChildren<Toggle>();
                if (toggle != null)
                {
                    toggle.group = toggleGroup;
                }
                else
                {
                    WarnPrefabComponentRequired(typeof(Toggle));
                }

                // Set binding target page.
                var binding = navItem.GetComponent<BaseBindPageState<T>>();
                if (binding != null)
                {
                    binding.TargetPage = page;
                }
                else
                {
                    WarnPrefabComponentRequired(typeof(BaseBindPageState<T>));
                }

                // Set localized label.
                var interfaceText = navItem.GetComponentInChildren<DisplayInterfaceText>();
                if (interfaceText != null)
                {
                    interfaceText.ChangeKey(string.Format("Pages.{0}", page.ToString()));
                }
            }
        }

        private void WarnPrefabComponentRequired(Type type)
        {
            Debug.LogWarningFormat("[{0}] Prefab must have a {1} component.", GetType(), type);
        }
    }
}