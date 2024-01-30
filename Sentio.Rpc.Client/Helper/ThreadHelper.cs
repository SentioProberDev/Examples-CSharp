using System;
using System.Windows;
using System.Windows.Threading;

namespace Sentio.Rpc.Client.Helper
{
    public static class ThreadExtension
    {
        /// <summary>
        /// Gets the UI dispatcher.
        /// </summary>
        /// <value>The UI dispatcher.</value>
        private static Dispatcher UiDispatcher => Application.Current != null ? Application.Current.Dispatcher : Dispatcher.CurrentDispatcher;

        /// <summary>
        /// Dispatches to UI.
        /// </summary>
        /// <param name="caller">The caller.</param>
        /// <param name="method">The method.</param>
        /// <param name="priority">The priority.</param>
        public static void DispatchToUi(this object caller, Action method, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (UiDispatcher.CheckAccess())
            {
                method.Invoke();
            }
            else
            {
                UiDispatcher.Invoke(method, priority);
            }
        }

        public static void DispatchToUiAsync(this object caller, Action method, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            if (UiDispatcher.CheckAccess())
            {
                // <ibg 2019-02-21> Dear past me: You are an idiot! DO NOT USE BeginInvoke! It will fuck things up!
                // If I'm on the main thread Invoke should be used. I'm already on the main thread! No deadlock potential! 
                // BeginInvoke will dispatch to ANOTHER thread than the UI thread!
                //
                // I'll leave my original comment as warning for the next time I feel like changing this code:
                // <ibg 2018-06-08/> Changed to BeginInvoke. (From Invoke; Invoke seems dubious here. I Suspect a copy & paste error)
                method.Invoke();
                // </ibg>
            }
            else
            {
                UiDispatcher.BeginInvoke(method, priority);
            }
        }
    }
}
