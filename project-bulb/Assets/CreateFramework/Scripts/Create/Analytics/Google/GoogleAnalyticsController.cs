using System.Collections.Generic;
using UnityEngine;

namespace Create.Analytics
{
    public class GoogleAnalyticsController //: IAnalyticsController
    {
        //private GoogleAnalyticsV4 _googleAnalytics;

        //public GoogleAnalyticsController(GoogleAnalyticsV4 analyticsObject)
        //{
        //    _googleAnalytics = analyticsObject;
        //}

        //~GoogleAnalyticsController()
        //{
        //    _googleAnalytics = null;
        //}

        //public string GetAnalyticsPlatformID()
        //{
        //    return "GOOGLE ANALYTICS";
        //}

        //public bool LogScreen(string title)
        //{
        //    var appViewHitBuilder = new AppViewHitBuilder()
        //                                    .SetScreenName(title);

        //    if (appViewHitBuilder.Validate() == null)
        //        return false;

        //    _googleAnalytics.LogScreen(appViewHitBuilder);
        //    return true;
        //}

        //public bool LogCustomEvent(string eventCategory, string eventAction, string eventLabel, params KeyValuePair<string, object>[] list)
        //{
        //    if (string.IsNullOrEmpty(eventCategory) || string.IsNullOrEmpty(eventAction))
        //    {
        //        Debug.LogErrorFormat("[{0}] A custom event needs an EventCategory and an EventAction assigned!", GetAnalyticsPlatformID());
        //        return false;
        //    }

        //    var eventHitBuilder = new EventHitBuilder()
        //                                .SetEventCategory(eventCategory)
        //                                .SetEventAction(eventAction);

        //    if (string.IsNullOrEmpty(eventLabel) == false)
        //        eventHitBuilder.SetEventLabel(eventLabel);

        //    foreach(var param in list)
        //    {
        //        if(param.Key.Equals("EventValue"))
        //        {
        //            var value = param.Value;
        //            if (value.GetType() == typeof(long))
        //            {
        //                eventHitBuilder.SetEventValue((long)value);
        //            }
        //            else
        //            {
        //                Debug.LogErrorFormat("[{0}] The given EventValue needs to be of type long!", GetAnalyticsPlatformID());
        //                return false;
        //            }
        //        }
        //    }

        //    if (eventHitBuilder.Validate() == null)
        //        return false;

        //    _googleAnalytics.LogEvent(eventHitBuilder);
        //    return true;
        //}

        //public bool LogTime(string timingCategory, long timingInterval, string timingName, string timingLabel, params KeyValuePair<string, object>[] list)
        //{
        //    if (string.IsNullOrEmpty(timingCategory))
        //    {
        //        Debug.LogErrorFormat("[{0}] To log time, you need to assign a TimingCategory!", GetAnalyticsPlatformID());
        //        return false;
        //    }

        //    var timingHitBuilder = new TimingHitBuilder()
        //                                .SetTimingCategory(timingCategory)
        //                                .SetTimingInterval(timingInterval);

        //    if (string.IsNullOrEmpty(timingName) == false)
        //        timingHitBuilder.SetTimingName(timingName);

        //    if (string.IsNullOrEmpty(timingLabel) == false)
        //        timingHitBuilder.SetTimingLabel(timingLabel);

        //    if (timingHitBuilder.Validate() == null)
        //        return false;

        //    _googleAnalytics.LogTiming(timingHitBuilder);
        //    return true;
        //}
    }
}
