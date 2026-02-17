using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOC.UISystem
{
    public class PopupHandler : MonoBehaviour
    {
        private struct PendingPopup
        {
            public PopupId Id;
            public int Priority;
            public object Data;
            public int Order;
        }

        private readonly Dictionary<string, UIPopup> _popupCache = new Dictionary<string, UIPopup>();
        private readonly List<PendingPopup> _pendingQueue = new List<PendingPopup>();
        private Transform _popupCanvas;
        private GameObject _blockerPanel;
        private UIPopup _currentPopup;
        private Coroutine _popupRoutine;
        private IUIInputHandler _inputHandler;
        private UISystemConfig _config;
        private ScreenTransition _popupTransition;
        private int _orderCounter;

        public bool IsPopupVisible => _currentPopup != null;

        public void Initialize(Transform popupCanvas, GameObject blockerPanel, UISystemConfig config, IUIInputHandler inputHandler)
        {
            _popupCanvas = popupCanvas;
            _blockerPanel = blockerPanel;
            _config = config;
            _inputHandler = inputHandler;

            _popupTransition = _config != null ? _config.DefaultPopupTransition : null;
        }

        public void ShowPopup(PopupId id, int priority = 0)
        {
            ShowPopupInternal(id, null, priority);
        }

        public void ShowPopup<TData>(PopupId id, TData data, int priority = 0)
        {
            ShowPopupInternal(id, data, priority);
        }

        public void DismissPopup()
        {
            if (_currentPopup == null)
                return;

            if (_popupRoutine != null)
                StopCoroutine(_popupRoutine);

            _popupRoutine = StartCoroutine(DismissPopupRoutine(_currentPopup));
        }

        public void DisposeAll()
        {
            foreach (var popup in _popupCache.Values)
            {
                if (popup != null)
                    popup.OnDispose();
            }
        }

        private void ShowPopupInternal(PopupId id, object data, int priority)
        {
            if (_currentPopup != null)
            {
                Enqueue(id, data, priority);
                return;
            }

            if (_popupRoutine != null)
                StopCoroutine(_popupRoutine);

            _popupRoutine = StartCoroutine(ShowPopupRoutine(id, data));
        }

        private void Enqueue(PopupId id, object data, int priority)
        {
            var pending = new PendingPopup
            {
                Id = id,
                Priority = priority,
                Data = data,
                Order = _orderCounter++
            };

            _pendingQueue.Add(pending);
            _pendingQueue.Sort((a, b) =>
            {
                int p = b.Priority.CompareTo(a.Priority);
                if (p != 0)
                    return p;
                return a.Order.CompareTo(b.Order);
            });
        }

        private PendingPopup Dequeue()
        {
            var pending = _pendingQueue[0];
            _pendingQueue.RemoveAt(0);
            return pending;
        }

        private IEnumerator ShowPopupRoutine(PopupId id, object data)
        {
            if (_blockerPanel != null)
                _blockerPanel.SetActive(true);

            var popup = GetOrCreatePopup(id);
            if (popup == null)
            {
                if (_blockerPanel != null)
                    _blockerPanel.SetActive(false);
                yield break;
            }

            _currentPopup = popup;
            popup.gameObject.SetActive(true);
            yield return FadeIn(popup.CanvasGroup, _popupTransition);

            if (data != null)
                popup.OnShow<object>(data);
            else
                popup.OnShow();

            UIEvents.RaisePopupOpened(popup.PopupId);

            if (_inputHandler != null)
                _inputHandler.SwitchToUI();
        }

        private IEnumerator DismissPopupRoutine(UIPopup popup)
        {
            yield return FadeOut(popup.CanvasGroup, _popupTransition);
            popup.OnDismiss();
            popup.gameObject.SetActive(false);
            var closedId = popup.PopupId;
            _currentPopup = null;

            if (_pendingQueue.Count > 0)
            {
                var next = Dequeue();
                _popupRoutine = StartCoroutine(ShowPopupRoutine(next.Id, next.Data));
            }
            else
            {
                if (_blockerPanel != null)
                    _blockerPanel.SetActive(false);

                UpdateInputAfterPopup();
            }

            UIEvents.RaisePopupClosed(closedId);
        }

        private void UpdateInputAfterPopup()
        {
            if (_inputHandler == null)
                return;

            var manager = UIManager.Instance;
            if (manager == null)
                return;

            var currentKey = manager.CurrentScreen.Key;
            var screenConfig = _config != null ? _config.GetScreenConfig(currentKey) : null;

            if (screenConfig != null && screenConfig.UsesPlayerInput)
                _inputHandler.SwitchToPlayer();
            else
                _inputHandler.SwitchToUI();
        }

        private UIPopup GetOrCreatePopup(PopupId id)
        {
            if (_popupCache.TryGetValue(id.Key, out var cached) && cached != null)
                return cached;

            var prefab = Resources.Load<GameObject>($"UI/Popups/{id.Key}");
            if (prefab == null)
                return null;

            var instance = Instantiate(prefab, _popupCanvas);
            var popup = instance.GetComponent<UIPopup>();
            if (popup == null)
            {
                Destroy(instance);
                return null;
            }

            popup.OnInitialize(id);
            instance.SetActive(false);
            _popupCache[id.Key] = popup;
            return popup;
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
    }
}
