using UnityEngine;

namespace FightingGame.Combat
{
    public class ComboExecutor : MonoBehaviour
    {
        [SerializeField] private ComboSystem comboSystem;

        private void Start()
        {
            comboSystem.OnComboExecuted.AddListener(OnCombo);
        }

        private void OnDestroy()
        {
            comboSystem.OnComboExecuted.RemoveListener(OnCombo);
        }

        private void OnCombo(ComboData combo)
        {
            Debug.Log($"Исполняется комбо: {combo.comboName}");
        }
    }
}