using UnityEngine;

namespace GOC.UISystem
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIScreen : MonoBehaviour
    {
        public ScreenId ScreenId { get; private set; }
        public CanvasGroup CanvasGroup { get; private set; }

        public virtual void OnInitialize(ScreenId id)
        {
            ScreenId = id;
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void OnShow()
        {
        }

        public virtual void OnShow<TData>(TData data)
        {
            OnShow();
        }

        public virtual void OnHide()
        {
        }

        public virtual void OnDispose()
        {
        }
    }
}
