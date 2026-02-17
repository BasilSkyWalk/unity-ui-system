using UnityEngine;

namespace GOC.UISystem
{
    [CreateAssetMenu(menuName = "UI/Screen Transition")]
    public class ScreenTransition : ScriptableObject
    {
        public TransitionType Type = TransitionType.Fade;
        public float Duration = 0.25f;
        public AnimationCurve Curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    }
}
