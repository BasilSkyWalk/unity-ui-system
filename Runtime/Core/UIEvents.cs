using System;

namespace GOC.UISystem
{
    public static class UIEvents
    {
        public static event Action<ScreenId> OnScreenShown;
        public static event Action<ScreenId> OnScreenHidden;
        public static event Action<PopupId> OnPopupOpened;
        public static event Action<PopupId> OnPopupClosed;
        public static event Action OnBackNavigated;

        internal static void RaiseScreenShown(ScreenId id)
        {
            OnScreenShown?.Invoke(id);
        }

        internal static void RaiseScreenHidden(ScreenId id)
        {
            OnScreenHidden?.Invoke(id);
        }

        internal static void RaisePopupOpened(PopupId id)
        {
            OnPopupOpened?.Invoke(id);
        }

        internal static void RaisePopupClosed(PopupId id)
        {
            OnPopupClosed?.Invoke(id);
        }

        internal static void RaiseBackNavigated()
        {
            OnBackNavigated?.Invoke();
        }
    }
}
