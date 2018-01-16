using System.Collections.Generic;
using Bulb.Core;
using TMPro;
using UnityEngine;

namespace Bulb.LevelEditor.Popups
{
    public class CellInfoPopup : BasePopup
    {
        public static CellInfoPopup Instance;

        public RectTransform InfoItemPrefab;
        public RectTransform InfoItemContainer;

        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }

        public void AddInfoItems(List<CellInfoPopupItem> items)
        {
            foreach(RectTransform child in InfoItemContainer)
            {
                child.gameObject.SetActive(false);
            }

            for(var i = 0; i < items.Count; ++i)
            {
                var item = items[i];
                RectTransform child = null;
                if(i < InfoItemContainer.childCount - 1)
                {
                    child = transform.GetChild(i).GetComponent<RectTransform>();
                    child.gameObject.SetActive(true);
                }
                else
                {
                    child = Instantiate(InfoItemPrefab, InfoItemContainer, false);
                }

                var labelVisual = child.GetChild(0).GetComponent<TextMeshProUGUI>();
                var valueVisual = child.GetChild(1).GetComponent<TextMeshProUGUI>();

                labelVisual.text = item.Label;
                valueVisual.text = item.Value;
            }
        }
    }

    public struct CellInfoPopupItem
    {
        public string Label;
        public string Value;
    }
}