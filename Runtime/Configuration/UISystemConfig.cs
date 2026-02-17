using System.Collections.Generic;
using UnityEngine;

namespace GOC.UISystem
{
    [CreateAssetMenu(menuName = "UI/UI System Config")]
    public class UISystemConfig : ScriptableObject
    {
        [Header("Transitions")]
        public ScreenTransition DefaultScreenTransition;
        public ScreenTransition DefaultPopupTransition;

        [Header("HUD")]
        public string HudScreenKey = "HUD";

        [Header("Navigation")]
        public string FallbackScreenKey = "HUD";

        [Header("Screen Configuration")]
        public List<ScreenConfig> ScreenConfigs = new List<ScreenConfig>();

        [Header("Transition Overrides")]
        public List<TransitionOverride> TransitionOverrides = new List<TransitionOverride>();

        public ScreenConfig GetScreenConfig(string key)
        {
            for (int i = 0; i < ScreenConfigs.Count; i++)
            {
                if (ScreenConfigs[i].ScreenKey == key)
                    return ScreenConfigs[i];
            }
            return null;
        }

        public ScreenTransition GetTransitionOverride(string fromKey, string toKey)
        {
            for (int i = 0; i < TransitionOverrides.Count; i++)
            {
                var over = TransitionOverrides[i];
                if (over.FromScreenKey == fromKey && over.ToScreenKey == toKey)
                    return over.Transition;
            }
            return null;
        }
    }
}
