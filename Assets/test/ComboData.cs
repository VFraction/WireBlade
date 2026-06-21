using UnityEngine;

namespace FightingGame.Combat
{
    [CreateAssetMenu(fileName = "NewCombo", menuName = "Combat/Combo Data")]
    public class ComboData : ScriptableObject
    {
        [Tooltip("Уникальное имя комбо для отладки.")]
        public string comboName;

        [Tooltip("Допустимое время между любыми двумя шагами комбо.")]
        public float defaultStepWindow = 0.5f;

        [Tooltip("Максимальное время на ввод всего комбо от первого нажатия.")]
        public float overallComboWindow = 2f;

        [Tooltip("Последовательность действий, составляющая комбо.")]
        public ComboStep[] steps;

        public bool MatchesPrefix(System.Collections.Generic.IReadOnlyList<ComboActionType> actions)
        {
            if (actions.Count > steps.Length)
                return false;

            for (int i = 0; i < actions.Count; i++)
            {
                if (steps[i].action != actions[i])
                    return false;
            }
            return true;
        }
    }

    [System.Serializable]
    public struct ComboStep
    {
        public ComboActionType action;

        [Tooltip("Если > 0, переопределяет defaultStepWindow для перехода к следующему шагу.")]
        public float customStepWindow;
    }
}