using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FightingGame.Combat
{
    public class ComboInputSimulator : MonoBehaviour, ICombatInputProvider
    {
        public event Action<ComboActionType> OnActionPerformed;
        [SerializeField] private ComboSystem comboSystem;

        [Header("Настройки тестовой последовательности")]
        [SerializeField] private bool playTest = false;
        [SerializeField] private float startDelay = 0.5f;
        [SerializeField] private List<ActionStep> testSequence = new List<ActionStep>();

        private Coroutine _currentSequenceRoutine;

        [System.Serializable]
        public struct ActionStep
        {
            public ComboActionType action;
            [Tooltip("Задержка перед этим действием (относительно предыдущего).")]
            public float delayBeforeAction;
        }

        public void PlayTestSequence()
        {
            if (_currentSequenceRoutine != null)
                StopCoroutine(_currentSequenceRoutine);
            _currentSequenceRoutine = StartCoroutine(PlaySequence(testSequence));
        }

        public void PlayAnotherSequence(List<ActionStep> steps)
        {
            if (_currentSequenceRoutine != null)
                StopCoroutine(_currentSequenceRoutine);
            _currentSequenceRoutine = StartCoroutine(PlaySequence(steps));
        }

        public void SimulateAction(ComboActionType action)
        {
            OnActionPerformed?.Invoke(action);
        }

        public void SimulateActionWithDelay(ComboActionType action, float delay)
        {
            StartCoroutine(DelayedAction(action, delay));
        }

        private IEnumerator PlaySequence(List<ActionStep> steps)
        {
            foreach (var step in steps)
            {
                if (step.delayBeforeAction > 0f)
                    yield return new WaitForSeconds(step.delayBeforeAction);

                SimulateAction(step.action);
            }
            _currentSequenceRoutine = null;
        }

        private IEnumerator DelayedAction(ComboActionType action, float delay)
        {
            yield return new WaitForSeconds(delay);
            SimulateAction(action);
        }

        private void Start()
        {
            if (playTest)
            {
                comboSystem.Initialize(this);
                StartCoroutine(PlayAfterStartDelay());
            }
        }

        private IEnumerator PlayAfterStartDelay()
        {
            yield return new WaitForSeconds(startDelay);
            PlayTestSequence();
        }
    }
}