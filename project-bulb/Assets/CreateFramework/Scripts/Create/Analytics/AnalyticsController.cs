using System.Collections.Generic;
using UnityEngine;

namespace Create.Analytics
{
    public class AnalyticsController : MonoBehaviour
    {
        public enum AnalyticsPlatform
        {
            Unity,
            Google
        }

        public AnalyticsPlatform CurrentPlatform;

        private IAnalyticsController _currentAnalyticsController; 

        public void Awake()
        {
            switch(CurrentPlatform)
            {
                case AnalyticsPlatform.Google:
                    //var GAV4 = GetComponentInChildren<GoogleAnalyticsV4>();
                    //if (GAV4 == null)
                    //{
                    //    Debug.LogErrorFormat("{[0]} Did you forget to place the Google Analytics V4 script on the controller?!", "GOOGLE ANALYTICS");
                    //    return;
                    //}

                    //_currentAnalyticsController = new GoogleAnalyticsController(GAV4);
                    break;
                case AnalyticsPlatform.Unity:
                    _currentAnalyticsController = new UnityAnalyticsController();
                    break;
            }
        }

        private void OnDestroy()
        {
            _currentAnalyticsController = null;
        }

        public void LogScreen(string title)
        {
            _currentAnalyticsController.LogScreen(title);
        }

        public bool LogCustomEvent(string eventCategory, string eventAction, string eventLabel, params KeyValuePair<string, object>[] list)
        {
            return _currentAnalyticsController.LogCustomEvent(eventCategory, eventAction, eventLabel, list);
        }

        public bool LogTime(string timingCategory, long timingInterval, string timingName, string timingLabel, params KeyValuePair<string, object>[] list)
        {
            return _currentAnalyticsController.LogTime(timingCategory, timingInterval, timingName, timingLabel, list);
        }
    }
}
