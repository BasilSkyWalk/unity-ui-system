using System;

namespace GOC.UISystem
{
    [Serializable]
    public class TransitionOverride
    {
        public string FromScreenKey;
        public string ToScreenKey;
        public ScreenTransition Transition;
    }
}
