using TMPro;
using UnityEngine;

namespace Settings.List
{
    public class SettingsListCategorySeparator : MonoBehaviour
    {
        private string _categoryName;
        public string CategoryName
        {
            get
            {
                return _categoryName;
            }
            set
            {
                _categoryName = value;
                ShowCategoryName();
            }
        }

        private void ShowCategoryName()
        {
            GetComponentInChildren<TextMeshProUGUI>().text = _categoryName;
        }
    }
}