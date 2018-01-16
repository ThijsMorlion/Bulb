using UnityEngine;
using Settings.Model;
using System.Collections.Generic;
using Settings.Binding.SpecificTypeBindings;

namespace Settings.List
{
    public class SettingsManagerList : MonoBehaviour
    {
        public GameObject CategorySeparatorObject;
        public GameObject TargetTransformObject;
        public GameObject SettingsManagerTogglePrefab;
        public GameObject SettingsManagerSliderPrefab;
        public GameObject SetttingsManagerStringPrefab;
        public GameObject SetttingsManagerIntPrefab;
        public GameObject SettingsManagerFloatPrefab;
        public GameObject SettingsManagerVector2Prefab;
        public GameObject SettingsManagerVector3Prefab;
        public GameObject SettingsManagerButtonPrefab;

        private void OnEnable()
        {
            SettingsManager.PropertyChanged += UnitySettingsManager_PropertyChanged;
        }

        private void OnDisable()
        {
            SettingsManager.PropertyChanged -= UnitySettingsManager_PropertyChanged;
        }

        private void UnitySettingsManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Settings")
            {
                ClearAllChildrenFromList();
                CreateSettingsElementsList();
            }
        }

        private void ClearAllChildrenFromList()
        {
            int numberOfChildren = TargetTransformObject.transform.childCount;
            for (int i = 0; i < numberOfChildren; i++)
            {
                Destroy(TargetTransformObject.transform.GetChild(i).gameObject);
            }
        }

        private void CreateSettingsElementsList()
        {
            /*
            TO DO
            - add array datatype (as list of inputfields with number of inputfields)
            */

            string previousCategoryName = "";

            //foreach categoryname present in the Settings: check all the fields of the Settings if its name is the same
            //→ if it is, create a gameobject which represents the gameobject
            //checking foreach categoryname divides the fields in the categories
            foreach (string categoryName in GetAllCategories())
            {
                if (categoryName != previousCategoryName)
                {
                    GameObject newPrefab = Instantiate(CategorySeparatorObject, TargetTransformObject.transform, false);
                    newPrefab.GetComponent<SettingsListCategorySeparator>().CategoryName = categoryName;
                }

                //get each propertytype from the SettingsManager
                var fields = SettingsManager.Settings.GetType().GetFields();
                foreach (var field in fields)
                {
                    if (field.Name == "SettingsCreatedVersion" || field.Name == "CurrentSettingsVersion") //don't create list entry for version
                    {
                        continue; //skip to other iteration
                    }

                    string fieldCategoryName = "";
                    try
                    {
                        //get the categoryname of the field
                        fieldCategoryName = (string)field.FieldType.GetProperty("Category").GetValue(field.GetValue(SettingsManager.Settings), null);
                    }
                    catch
                    {
                        Debug.LogWarning(string.Format("Could not retrieve 'Category' property of field '{0}'", field.Name));
                    }

                    //create the gameobject if its categoryname matches the categoryname we're checking for
                    if (fieldCategoryName == categoryName)
                    {
                        GameObject newPrefab = null;

                        //if field is of type Field<GenericArgument>
                        if (field.FieldType.IsGenericType)
                        {
                            var type = field.FieldType.GetGenericArguments()[0];

                            if (type == typeof(bool))
                            {
                                newPrefab = Instantiate(SettingsManagerTogglePrefab, TargetTransformObject.transform, false);
                            }
                            else if (type == typeof(float))
                            {
                                newPrefab = Instantiate(SettingsManagerFloatPrefab, TargetTransformObject.transform, false);
                            }
                            else if (type == typeof(string))
                            {
                                newPrefab = Instantiate(SetttingsManagerStringPrefab, TargetTransformObject.transform, false);
                            }
                            else if (type == typeof(int))
                            {
                                newPrefab = Instantiate(SetttingsManagerIntPrefab, TargetTransformObject.transform, false);
                            }
                            else if (type == typeof(Vector2))
                            {
                                newPrefab = Instantiate(SettingsManagerVector2Prefab, TargetTransformObject.transform, false);
                            }
                            else if (type == typeof(Vector3))
                            {
                                newPrefab = Instantiate(SettingsManagerVector3Prefab, TargetTransformObject.transform, false);
                            }
                        }
                        else
                        {
                            if (field.FieldType == typeof(MinMaxSetting))
                            {
                                newPrefab = Instantiate(SettingsManagerSliderPrefab, TargetTransformObject.transform, false);
                                newPrefab.GetComponent<ManagerSettingsPropertMinMaxFloat>().Field = field;
                            }

                            else if (field.FieldType == typeof(ButtonSetting))
                            {
                                newPrefab = Instantiate(SettingsManagerButtonPrefab, TargetTransformObject.transform, false);
                                newPrefab.GetComponent<ManagerSettingsPropertyButton>().Field = field;
                            }
                        }

                        //check if newPrefab has been instantiated
                        if (newPrefab != null)
                        {
                            //if so, assing Field property
                            newPrefab.GetComponent<ManagerSettingsProperty>().Field = field;
                        }
                        else
                        {
                            //if not, show warning
                            if (field.FieldType.IsGenericType)
                            {
                                Debug.LogWarning(string.Format("Field of type '{0}' not yet implemented.", field.FieldType.GetGenericArguments()[0]));
                            }
                            else
                            {
                                Debug.LogWarning(string.Format("Field of type '{0}' not yet implemented.", field.FieldType.Name));
                            }
                        }
                    }
                }
            }
        }

        private List<string> GetAllCategories()
        {
            List<string> categories = new List<string>();
            categories.Add("default"); //ensures the default category is first in the list

            var fields = SettingsManager.Settings.GetType().GetFields();
            foreach (var field in fields)
            {
                if (field.Name == "SettingsCreatedVersion" || field.Name == "CurrentSettingsVersion")
                {
                    continue;
                }

                try
                {
                    string categoryName = (string)field.FieldType.GetProperty("Category").GetValue(field.GetValue(SettingsManager.Settings), null);
                    if (!categories.Contains(categoryName))
                    {
                        categories.Add(categoryName);
                    }
                }
                catch
                {
                    Debug.LogWarning(string.Format("Could not retrieve Category property of field '{0}'.", field.Name));
                }
            }

            return categories;
        }
    }
}