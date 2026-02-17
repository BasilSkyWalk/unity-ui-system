using System;

namespace GOC.UISystem
{
    public interface IUIInputHandler
    {
        void SwitchToUI();
        void SwitchToPlayer();
        void BindCancelAction(Action onCancel);
    }
}
