using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FightingGame.Combat
{
    public class ComboSystem : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private ComboData[] availableCombos;
        [SerializeField] private int maxBufferSize = 8;

        [Header("Отладка")]
        [SerializeField] private bool logCombos = false;

        [Header("События")]
        [Tooltip("Вызывается при успешном завершении комбо. Передаёт выполненное ComboData.")]
        public UnityEvent<ComboData> OnComboExecuted;

        private readonly List<InputRecord> _inputBuffer = new List<InputRecord>();
        private ICombatInputProvider _inputProvider;

        private struct InputRecord
        {
            public ComboActionType action;
            public float timestamp;
        }

        public void Initialize(ICombatInputProvider inputProvider)
        {
            if (_inputProvider != null)
                _inputProvider.OnActionPerformed -= OnActionPerformed;

            _inputProvider = inputProvider;
            if (_inputProvider != null)
                _inputProvider.OnActionPerformed += OnActionPerformed;
        }

        private void OnDestroy()
        {
            if (_inputProvider != null)
                _inputProvider.OnActionPerformed -= OnActionPerformed;
        }

        private void OnActionPerformed(ComboActionType action)
        {
            Debug.Log(action.ToString());

            AddToBuffer(action);

            CheckForCompletedCombo();
        }

        private void AddToBuffer(ComboActionType action)
        {
            _inputBuffer.Add(new InputRecord
            {
                action = action,
                timestamp = Time.time
            });

            while (_inputBuffer.Count > maxBufferSize)
                _inputBuffer.RemoveAt(0);
        }

        private void CheckForCompletedCombo()
        {
            var currentSequence = GetCurrentActionSequence();

            foreach (var combo in availableCombos)
            {
                if (combo.steps.Length != currentSequence.Count)
                    continue;

                bool match = true;
                for (int i = 0; i < currentSequence.Count; i++)
                {
                    if (combo.steps[i].action != currentSequence[i])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    if (IsTimingValid(combo))
                    {
                        if (logCombos)
                            Debug.Log($"Combo executed: {combo.comboName}");

                        OnComboExecuted?.Invoke(combo);
                        _inputBuffer.Clear();
                        return;
                    }
                }
            }
        }

        private bool IsTimingValid(ComboData combo)
        {
            float firstTime = _inputBuffer[0].timestamp;
            float lastTime = _inputBuffer[_inputBuffer.Count - 1].timestamp;
            if (lastTime - firstTime > combo.overallComboWindow)
                return false;

            for (int i = 1; i < _inputBuffer.Count; i++)
            {
                float window = combo.steps[i].customStepWindow > 0
                    ? combo.steps[i].customStepWindow
                    : combo.defaultStepWindow;

                if (_inputBuffer[i].timestamp - _inputBuffer[i - 1].timestamp > window)
                    return false;
            }

            return true;
        }

        private List<ComboActionType> GetCurrentActionSequence()
        {
            var sequence = new List<ComboActionType>(_inputBuffer.Count);
            foreach (var record in _inputBuffer)
                sequence.Add(record.action);
            return sequence;
        }

        private void Update()
        {
            if (_inputBuffer.Count == 0)
                return;

            bool isPrefixForAny = false;
            var currentSequence = GetCurrentActionSequence();

            foreach (var combo in availableCombos)
            {
                if (combo.MatchesPrefix(currentSequence))
                {
                    isPrefixForAny = true;

                    int nextStepIndex = currentSequence.Count;
                    if (nextStepIndex < combo.steps.Length)
                    {
                        float stepWindow = combo.steps[nextStepIndex].customStepWindow > 0
                            ? combo.steps[nextStepIndex].customStepWindow
                            : combo.defaultStepWindow;

                        float timeSinceLast = Time.time - _inputBuffer[_inputBuffer.Count - 1].timestamp;
                        if (timeSinceLast > stepWindow)
                        {
                            ClearBuffer("Step window expired");
                            return;
                        }
                    }

                    float totalTime = Time.time - _inputBuffer[0].timestamp;
                    if (totalTime > combo.overallComboWindow)
                    {
                        ClearBuffer("Overall window expired");
                        return;
                    }
                }
            }

            if (!isPrefixForAny)
            {
                ClearBuffer("Not a prefix for any combo");
            }
        }

        private void ClearBuffer(string reason)
        {
            if (logCombos)
                Debug.Log($"Buffer cleared: {reason}");
            _inputBuffer.Clear();
        }

        public void ResetBuffer()
        {
            _inputBuffer.Clear();
        }
    }
}