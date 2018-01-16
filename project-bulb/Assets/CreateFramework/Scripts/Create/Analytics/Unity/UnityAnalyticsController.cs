using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;

namespace Create.Analytics
{
    public class UnityAnalyticsController : IAnalyticsController
    {
        public string GetAnalyticsPlatformID()
        {
            return "UNITY ANALYTICS";
        }

        public bool LogScreen(string title)
        {
            if (string.IsNullOrEmpty(title))
                Debug.LogErrorFormat("[{0}] No title given!", GetAnalyticsPlatformID());

            var result = UnityEngine.Analytics.Analytics.CustomEvent(title);
            if (result != AnalyticsResult.Ok)
            {
                Debug.LogFormat("[{0}] Failed to log screen: {1}", GetAnalyticsPlatformID(), result.ToString());
                return false;
            }

            return true;
        }

        public bool LogCustomEvent(string eventCategory, string eventAction, string eventLabel, params KeyValuePair<string, object>[] list)
        {
            if (string.IsNullOrEmpty(eventLabel))
            {
                Debug.LogErrorFormat("[{0}] Please provide an EventLabel!", GetAnalyticsPlatformID());
                return false;
            }

            var eventName = string.Empty;
            if (string.IsNullOrEmpty(eventCategory) == false)
                eventName += eventCategory + "/";

            if (string.IsNullOrEmpty(eventAction) == false)
                eventName += eventAction + "/";

            eventName += eventLabel;

            var dictionary = new List<KeyValuePair<string, object>>(list).ToDictionary(pair => pair.Key, pair => pair.Value);
            var result = UnityEngine.Analytics.Analytics.CustomEvent(eventName, dictionary);

            if (result != AnalyticsResult.Ok)
            {
                Debug.LogFormat("[{0}] Failed to send custom event: {1}", GetAnalyticsPlatformID(), result.ToString());
                return false;
            }

            return true;
        }

        public bool LogTime(string timingCategory, long timingInterval, string timingName, string timingLabel, params KeyValuePair<string, object>[] list)
        {
            if (string.IsNullOrEmpty(timingLabel))
            {
                Debug.LogErrorFormat("[{0}] Please provide an TimingLabel!", GetAnalyticsPlatformID());
                return false;
            }

            var eventName = string.Empty;
            if (string.IsNullOrEmpty(timingCategory) == false)
                eventName += timingCategory + "/";

            if (string.IsNullOrEmpty(timingName) == false)
                eventName += timingName + "/";

            eventName += timingLabel;

            var dictionary = new List<KeyValuePair<string, object>>(list).ToDictionary(pair => pair.Key, pair => pair.Value);
            dictionary.Add("TimingInterval", timingInterval);

            var result = UnityEngine.Analytics.Analytics.CustomEvent(eventName, dictionary);

            if (result != AnalyticsResult.Ok)
            {
                Debug.LogFormat("[{0}] Failed to send timing event: {1}", GetAnalyticsPlatformID(), result.ToString());
                return false;
            }

            return true;
        }
    }
}
