using UnityEngine;

namespace GOC.UISystem
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIPopup : MonoBehaviour
    {
        public PopupId PopupId { get; private set; }
        public CanvasGroup CanvasGroup { get; private set; }

        public virtual void OnInitialize(PopupId id)
        {
            PopupId = id;
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void OnShow()
        {
        }

        public virtual void OnShow<TData>(TData data)
        {
            OnShow();
        }

        public virtual void OnDismiss()
        {
        }

        public virtual void OnDispose()
        {
        }
    }
}
