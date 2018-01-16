using System.Linq;
using Bulb.Electricity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulb.LevelEditor.Widgets
{
    public class ParallelGroupWidget: MonoBehaviour
    {
        public ToggleGroup ToggleGroup;
        public RectTransform TogglePrefab;
        public RectTransform ToggleParent;
        public CurrentWalker CurrentWalker;

        public void OnEnable()
        {
            CurrentWalker.OnParallelGroupsAnalyzed += CurrentWalker_OnParallelGroupsAnalyzed;
        }

        private void CurrentWalker_OnParallelGroupsAnalyzed()
        {
            Reset();

            var index = 0;
            foreach(var group in CurrentWalker.ParallelGroups)
            {
                AddToggleGroup(index);
                ++index;
            }
        }

        public void Reset()
        {
            for(var i = 0; i < ToggleParent.childCount; ++i)
            {
                var child = ToggleParent.GetChild(i);
                child.gameObject.SetActive(false);
            }
        }

        public void AddToggleGroup(int id)
        {
            if(id < ToggleParent.childCount)
            {
                var child = ToggleParent.GetChild(id);
                child.gameObject.SetActive(true);
            }
            else
            {
                var toggleGO = Instantiate(TogglePrefab, ToggleParent, false);
                var toggle = toggleGO.GetComponent<Toggle>();
                toggle.group = ToggleGroup;
                toggle.onValueChanged.AddListener(OnToggleValueChanged);

                var name = string.Format("Parallel Group {0}", id);
                toggleGO.name = name;

                var description = toggleGO.GetChild(1).GetComponent<TextMeshProUGUI>();
                description.text = name;
            }
        }

        public void OnToggleValueChanged(bool value)
        {
            var activeToggles = ToggleGroup.ActiveToggles();
            if (activeToggles.Count() == 0)
            {
                CurrentWalker.ShowParallelGroup(-1);
                return;
            }

            if (value)
            {
                foreach (var toggle in activeToggles)
                {
                    var name = toggle.name;
                    var groupID = -1;
                    int.TryParse(name.Substring(name.Length - 1), out groupID);
                    CurrentWalker.ShowParallelGroup(groupID);
                } 
            }
        }
    }
}
