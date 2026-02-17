using System;
using UnityEngine;

namespace GOC.UISystem
{
    [Serializable]
    public class ScreenConfig
    {
        public string ScreenKey;
        public bool ShowHud;
        public bool UsesPlayerInput;
        public bool ExcludeFromHistory;
    }
}
