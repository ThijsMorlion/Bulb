using System;

namespace Create.Helpers
{
    public static class EventDispatcher
    {
        public static void Dispatch<T>(EventHandler<T> handler, T args = null, object sender = null) where T : EventArgs
        {
            if (handler != null)
            {
                if (args == null)
                {
                    args = (T)EventArgs.Empty;
                }

                handler(sender, args);
            }
        }
    }
}