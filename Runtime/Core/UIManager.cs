using System;
using UnityEngine;
using UnityEngine.UI;

namespace GOC.UISystem
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private UISystemConfig _config;

        private ScreenHandler _screenHandler;
        private PopupHandler _popupHandler;
        private IUIInputHandler _inputHandler;
        private Transform _screenCanvas;
        private Transform _popupCanvas;
        private GameObject _blockerPanel;

        /// <summary>
        /// Game-specific cancel routing. Called when cancel is pressed and no popup is visible.
        /// </summary>
        public Action OnCancelPressed;

        public ScreenId CurrentScreen => _screenHandler != null ? _screenHandler.CurrentScreen : ScreenId.None;
        public bool IsPopupVisible => _popupHandler != null && _popupHandler.IsPopupVisible;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeCanvases();
        }

        private void OnDestroy()
        {
            if (_screenHandler != null)
                _screenHandler.DisposeAll();

            if (_popupHandler != null)
                _popupHandler.DisposeAll();

            if (Instance == this)
                Instance = null;
        }

        public void Initialize(IUIInputHandler inputHandler)
        {
            _inputHandler = inputHandler;

            var screenObject = new GameObject("ScreenHandler");
            screenObject.transform.SetParent(transform);
            _screenHandler = screenObject.AddComponent<ScreenHandler>();
            _screenHandler.Initialize(_screenCanvas, _config, _inputHandler);

            var popupObject = new GameObject("PopupHandler");
            popupObject.transform.SetParent(transform);
            _popupHandler = popupObject.AddComponent<PopupHandler>();
            _popupHandler.Initialize(_popupCanvas, _blockerPanel, _config, _inputHandler);

            _inputHandler.BindCancelAction(HandleCancel);
        }

        public void ShowScreen(ScreenId screenId)
        {
            _screenHandler.ShowScreen(screenId);
        }

        public void ShowScreen(ScreenId screenId, bool clearHistory)
        {
            _screenHandler.ShowScreen(screenId, clearHistory);
        }

        public void ShowScreen<TData>(ScreenId screenId, TData data)
        {
            _screenHandler.ShowScreen(screenId, data);
        }

        public void ShowScreen<TData>(ScreenId screenId, TData data, bool clearHistory)
        {
            _screenHandler.ShowScreen(screenId, data, clearHistory);
        }

        public void GoBack()
        {
            _screenHandler.GoBack();
        }

        public void ShowHUD()
        {
            _screenHandler.ShowHUD();
        }

        public void HideHUD()
        {
            _screenHandler.HideHUD();
        }

        public void ShowPopup(PopupId popupId, int priority = 0)
        {
            _popupHandler.ShowPopup(popupId, priority);
        }

        public void ShowPopup<TData>(PopupId popupId, TData data, int priority = 0)
        {
            _popupHandler.ShowPopup(popupId, data, priority);
        }

        public void DismissPopup()
        {
            _popupHandler.DismissPopup();
        }

        private void InitializeCanvases()
        {
            var screenCanvasObject = new GameObject("ScreenCanvas");
            screenCanvasObject.transform.SetParent(transform);
            var screenCanvas = screenCanvasObject.AddComponent<Canvas>();
            screenCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            screenCanvas.sortingOrder = 0;

            var screenScaler = screenCanvasObject.AddComponent<CanvasScaler>();
            screenScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            screenScaler.referenceResolution = new Vector2(1920f, 1080f);
            screenScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            screenCanvasObject.AddComponent<GraphicRaycaster>();
            _screenCanvas = screenCanvasObject.transform;

            var popupCanvasObject = new GameObject("PopupCanvas");
            popupCanvasObject.transform.SetParent(transform);
            var popupCanvas = popupCanvasObject.AddComponent<Canvas>();
            popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            popupCanvas.sortingOrder = 100;

            var popupScaler = popupCanvasObject.AddComponent<CanvasScaler>();
            popupScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            popupScaler.referenceResolution = new Vector2(1920f, 1080f);
            popupScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            popupCanvasObject.AddComponent<GraphicRaycaster>();
            _popupCanvas = popupCanvasObject.transform;

            _blockerPanel = CreateBlockerPanel(_popupCanvas);
        }

        private GameObject CreateBlockerPanel(Transform parent)
        {
            var blocker = new GameObject("BlockerPanel");
            blocker.transform.SetParent(parent, false);
            var rect = blocker.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = blocker.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.5f);
            image.raycastTarget = true;

            blocker.SetActive(false);
            return blocker;
        }

        private void HandleCancel()
        {
            if (_popupHandler.IsPopupVisible)
            {
                _popupHandler.DismissPopup();
                return;
            }

            OnCancelPressed?.Invoke();
        }
    }
}
