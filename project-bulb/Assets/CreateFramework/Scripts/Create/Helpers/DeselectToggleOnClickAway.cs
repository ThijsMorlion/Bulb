using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Create.Helpers
{
    [RequireComponent(typeof(Toggle))]
    public class DeselectToggleOnClickAway : MonoBehaviour
    {
        [Tooltip("If set, the toggle will not be deselected if the clicked object is also parented to this transform. Else, this object's transform will be used.")]
        public GameObject CommonRoot;

        private Toggle _toggle;

        void Start()
        {
            _toggle = GetComponent<Toggle>();
        }

        // Update is called once per frame
        void Update()
        {
            if (_toggle.isOn && Input.GetMouseButtonDown(0))
            {
                // Deactivate the toggle when we don't click on any of the toggle's children.
                List<RaycastResult> hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = Input.mousePosition }, hits);

                foreach (var hit in hits)
                {
                    var hitObject = hit.gameObject;
                    while (hitObject != null)
                    {
                        if (!CommonRoot && hitObject == gameObject || CommonRoot && hitObject == CommonRoot)
                            return;

                        if (hitObject.transform.parent == null)
                            break;
                        hitObject = hitObject.transform.parent.gameObject;
                    }
                }

                _toggle.isOn = false;
            }
        }
    }
}