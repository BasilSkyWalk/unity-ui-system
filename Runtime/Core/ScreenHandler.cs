using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOC.UISystem
{
    public class ScreenHandler : MonoBehaviour
    {
        private readonly Dictionary<string, UIScreen> _screenCache = new Dictionary<string, UIScreen>();
        private readonly Stack<ScreenId> _history = new Stack<ScreenId>();
        private Transform _screenCanvas;
        private UIScreen _currentScreen;
        private ScreenId _currentScreenId = ScreenId.None;
        private Coroutine _transitionRoutine;
        private IUIInputHandler _inputHandler;
        private UISystemConfig _config;

        public ScreenId CurrentScreen => _currentScreenId;

        public void Initialize(Transform screenCanvas, UISystemConfig config, IUIInputHandler inputHandler)
        {
            _screenCanvas = screenCanvas;
            _config = config;
            _inputHandler = inputHandler;
        }

        public void ShowScreen(ScreenId id, bool clearHistory = false)
        {
            ShowScreenInternal(id, null, clearHistory, true);
        }

        public void ShowScreen<TData>(ScreenId id, TData data, bool clearHistory = false)
        {
            ShowScreenInternal(id, data, clearHistory, true);
        }

        public void GoBack()
        {
            if (_transitionRoutine != null)
                StopCoroutine(_transitionRoutine);

            if (_history.Count > 0)
            {
                var previous = _history.Pop();
                _transitionRoutine = StartCoroutine(ShowScreenRoutine(previous, null, false, false));
                UIEvents.RaiseBackNavigated();
                return;
            }

            if (_config != null && !string.IsNullOrEmpty(_config.FallbackScreenKey))
            {
                var fallback = new ScreenId(_config.FallbackScreenKey);
                if (fallback != _currentScreenId)
                {
                    _transitionRoutine = StartCoroutine(ShowScreenRoutine(fallback, null, false, false));
                    UIEvents.RaiseBackNavigated();
                }
            }
        }

        public void ShowHUD()
        {
            if (_transitionRoutine != null)
                StopCoroutine(_transitionRoutine);

            var transition = GetDefaultTransition();
            _transitionRoutine = StartCoroutine(ToggleHUD(true, transition));
        }

        public void HideHUD()
        {
            if (_transitionRoutine != null)
                StopCoroutine(_transitionRoutine);

            var transition = GetDefaultTransition();
            _transitionRoutine = StartCoroutine(ToggleHUD(false, transition));
        }

        public void DisposeAll()
        {
            foreach (var screen in _screenCache.Values)
            {
                if (screen != null)
                    screen.OnDispose();
            }
        }

        private void ShowScreenInternal(ScreenId id, object data, bool clearHistory, bool addToHistory)
        {
            if (id == _currentScreenId && !id.IsNone)
                return;

            if (_transitionRoutine != null)
                StopCoroutine(_transitionRoutine);

            _transitionRoutine = StartCoroutine(ShowScreenRoutine(id, data, clearHistory, addToHistory));
        }

        private IEnumerator ShowScreenRoutine(ScreenId id, object data, bool clearHistory, bool addToHistory)
        {
            var previousId = _currentScreenId;

            if (clearHistory)
                _history.Clear();

            if (addToHistory && !clearHistory && !previousId.IsNone && !IsExcludedFromHistory(previousId))
                _history.Push(previousId);

            var transition = GetTransition(previousId, id);

            if (_currentScreen != null)
            {
                _currentScreen.OnHide();
                yield return FadeOut(_currentScreen.CanvasGroup, transition);
                _currentScreen.gameObject.SetActive(false);
                UIEvents.RaiseScreenHidden(_currentScreen.ScreenId);
                _currentScreen = null;
            }

            bool hideHud = ShouldHideHudForScreen(id);
            bool showHud = ShouldShowHudForScreen(id);

            if (hideHud)
                yield return ToggleHUD(false, transition);

            _currentScreenId = id;

            string hudKey = _config != null ? _config.HudScreenKey : "HUD";
            UIScreen incoming = null;
            if (id.Key != hudKey && !id.IsNone)
                incoming = GetOrCreateScreen(id);

            if (incoming != null)
            {
                incoming.gameObject.SetActive(true);
                yield return FadeIn(incoming.CanvasGroup, transition);
                if (data != null)
                    incoming.OnShow<object>(data);
                else
                    incoming.OnShow();
                UIEvents.RaiseScreenShown(incoming.ScreenId);
                _currentScreen = incoming;
            }

            if (showHud)
                yield return ToggleHUD(true, transition);

            UpdateInputState();
        }

        private UIScreen GetOrCreateScreen(ScreenId id)
        {
            if (_screenCache.TryGetValue(id.Key, out var cached) && cached != null)
                return cached;

            var prefab = Resources.Load<GameObject>($"UI/Screens/{id.Key}");
            if (prefab == null)
                return null;

            var instance = Instantiate(prefab, _screenCanvas);
            var screen = instance.GetComponent<UIScreen>();
            if (screen == null)
            {
                Destroy(instance);
                return null;
            }

            screen.OnInitialize(id);
            instance.SetActive(false);
            _screenCache[id.Key] = screen;

            string hudKey = _config != null ? _config.HudScreenKey : "HUD";
            if (id.Key == hudKey)
                instance.transform.SetAsFirstSibling();

            return screen;
        }

        private IEnumerator ToggleHUD(bool visible, ScreenTransition transition)
        {
            string hudKey = _config != null ? _config.HudScreenKey : "HUD";
            var hudId = new ScreenId(hudKey);
            var hud = GetOrCreateScreen(hudId);
            if (hud == null)
                yield break;

            if (visible && !hud.gameObject.activeSelf)
            {
                hud.gameObject.SetActive(true);
                yield return FadeIn(hud.CanvasGroup, transition);
                hud.OnShow();
                UIEvents.RaiseScreenShown(hud.ScreenId);
            }
            else if (!visible && hud.gameObject.activeSelf)
            {
                hud.OnHide();
                yield return FadeOut(hud.CanvasGroup, transition);
                hud.gameObject.SetActive(false);
                UIEvents.RaiseScreenHidden(hud.ScreenId);
            }
        }

        private bool ShouldHideHudForScreen(ScreenId id)
        {
            if (id.IsNone)
                return true;

            var screenConfig = _config != null ? _config.GetScreenConfig(id.Key) : null;
            if (screenConfig != null)
                return !screenConfig.ShowHud;

            return false;
        }

        private bool ShouldShowHudForScreen(ScreenId id)
        {
            if (id.IsNone)
                return false;

            var screenConfig = _config != null ? _config.GetScreenConfig(id.Key) : null;
            if (screenConfig != null)
                return screenConfig.ShowHud;

            return false;
        }

        private bool IsExcludedFromHistory(ScreenId id)
        {
            var screenConfig = _config != null ? _config.GetScreenConfig(id.Key) : null;
            if (screenConfig != null)
                return screenConfig.ExcludeFromHistory;

            return false;
        }

        private ScreenTransition GetTransition(ScreenId from, ScreenId to)
        {
            if (_config != null)
            {
                var over = _config.GetTransitionOverride(from.Key, to.Key);
                if (over != null)
                    return over;
            }

            return GetDefaultTransition();
        }

        private ScreenTransition GetDefaultTransition()
        {
            if (_config != null && _config.DefaultScreenTransition != null)
                return _config.DefaultScreenTransition;

            return null;
        }

        private IEnumerator FadeIn(CanvasGroup group, ScreenTransition transition)
        {
            if (group == null)
                yield break;

            group.interactable = false;
            group.blocksRaycasts = false;
            yield return FadeCoroutine(group, 0f, 1f, transition, true);
        }

        private IEnumerator FadeOut(CanvasGroup group, ScreenTransition transition)
        {
            if (group == null)
                yield break;

            group.interactable = false;
            group.blocksRaycasts = false;
            yield return FadeCoroutine(group, 1f, 0f, transition, false);
        }

        private IEnumerator FadeCoroutine(CanvasGroup group, float from, float to, ScreenTransition transition, bool enableOnComplete)
        {
            if (transition == null || transition.Type == TransitionType.Instant)
            {
                group.alpha = to;
                group.interactable = enableOnComplete;
                group.blocksRaycasts = enableOnComplete;
                yield break;
            }

            float elapsed = 0f;
            group.alpha = from;

            while (elapsed < transition.Duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = transition.Curve.Evaluate(elapsed / transition.Duration);
                group.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            group.alpha = to;
            group.interactable = enableOnComplete;
            group.blocksRaycasts = enableOnComplete;
        }

        private void UpdateInputState()
        {
            if (_inputHandler == null)
                return;

            if (UIManager.Instance != null && UIManager.Instance.IsPopupVisible)
            {
                _inputHandler.SwitchToUI();
                return;
            }

            var screenConfig = _config != null ? _config.GetScreenConfig(_currentScreenId.Key) : null;
            if (screenConfig != null && screenConfig.UsesPlayerInput)
                _inputHandler.SwitchToPlayer();
            else
                _inputHandler.SwitchToUI();
        }
    }
}
