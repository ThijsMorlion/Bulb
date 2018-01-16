using System.Collections.Generic;

namespace Create.Analytics
{
    interface IAnalyticsController
    {
        string GetAnalyticsPlatformID();

        bool LogScreen(string title);

        /// <summary>
        /// Log a custom event to the analytics platform.
        /// Depending on the platform of your choice, some parameters can be omitted from the log.
        /// </summary>
        /// <param name="eventCategory">The category of the event (e.g. User Action)</param>
        /// <param name="eventAction">The type of action of the event (e.g. Switched Screen)</param>
        /// <param name="eventLabel">The name of the event itself (e.g. Main Menu)</param>
        /// <param name="list">Additional parameters</param>
        bool LogCustomEvent(string eventCategory, string eventAction, string eventLabel, params KeyValuePair<string, object>[] list);

        /// <summary>
        /// Log a time to the analytics platform.
        /// Depending on the platform of your choice, some parameters can be omitted from the log.
        /// </summary>
        /// <param name="timingCategory">The category of the timing event (e.g. Loading Resources)</param>
        /// <param name="timingInterval">The duration of the timing event (e.g. 50L)</param>
        /// <param name="timingName">The name of the event (e.g. Main Menu)</param>
        /// <param name="timingLabel">The label of th eevent (e.g. First Load)</param>
        /// <param name="list">Additional parameters</param>
        bool LogTime(string timingCategory, long timingInterval, string timingName, string timingLabel, params KeyValuePair<string, object>[] list);
    }
}
