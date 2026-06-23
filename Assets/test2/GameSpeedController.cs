using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GameSystems
{
    public class GameSpeedController : MonoBehaviour
    {
        [Header("Настройки переходов")]
        [SerializeField, Range(0.1f, 10f)] private float transitionDuration = 0.5f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("События")]
        public UnityEvent<bool> OnPauseChanged;
        public UnityEvent<float> OnTimeScaleChanged;

        private static GameSpeedController _instance;
        public static GameSpeedController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<GameSpeedController>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[GameSpeedController]");
                        _instance = go.AddComponent<GameSpeedController>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private Coroutine _currentTransition;
        private float _targetTimeScale = 1f;
        private bool _isPaused;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                Time.timeScale = 1f;
                _instance = null;
            }
        }

        public void SetTimeScale(float scale, bool instant = false)
        {
            scale = Mathf.Clamp(scale, 0f, 100f);
            _targetTimeScale = scale;

            if (instant)
            {
                StopTransition();
                Time.timeScale = scale;
                Time.fixedDeltaTime = 0.02f * scale;
                OnTimeScaleChanged?.Invoke(scale);
                UpdatePauseState(scale == 0f);
            }
            else
            {
                StartTransition(scale);
            }
        }

        public void TogglePause()
        {
            if (_isPaused) Resume();
            else Pause();
        }

        public void Pause(bool instant = false) => SetTimeScale(0f, instant);
        public void Resume() => SetTimeScale(1f);

        public void SetSlowMotion(float slowFactor = 0.3f) => SetTimeScale(slowFactor);

        private void StartTransition(float toScale)
        {
            StopTransition();
            _currentTransition = StartCoroutine(TransitionCoroutine(Time.timeScale, toScale));
        }

        private void StopTransition()
        {
            if (_currentTransition != null)
            {
                StopCoroutine(_currentTransition);
                _currentTransition = null;
            }
        }

        private IEnumerator TransitionCoroutine(float from, float to)
        {
            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = transitionCurve.Evaluate(Mathf.Clamp01(elapsed / transitionDuration));
                float current = Mathf.Lerp(from, to, t);
                Time.timeScale = current;
                Time.fixedDeltaTime = 0.02f * current;
                OnTimeScaleChanged?.Invoke(current);
                UpdatePauseState(current <= 0.001f);
                yield return null;
            }

            Time.timeScale = to;
            Time.fixedDeltaTime = 0.02f * to;
            OnTimeScaleChanged?.Invoke(to);
            UpdatePauseState(to <= 0f);
            _currentTransition = null;
        }

        private void UpdatePauseState(bool paused)
        {
            if (_isPaused != paused)
            {
                _isPaused = paused;
                OnPauseChanged?.Invoke(paused);
            }
        }
    }
}