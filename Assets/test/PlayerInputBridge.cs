using UnityEngine;

namespace FightingGame.Combat
{
    public class PlayerInputBridge : MonoBehaviour, ICombatInputProvider
    {
        public event System.Action<ComboActionType> OnActionPerformed;

        [SerializeField] private ComboSystem comboSystem;

        private InputSystem_Actions _inputActions;

        private void Awake()
        {
            _inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _inputActions.Enable();
            _inputActions.Combat.LMB.performed += ctx => Notify(ComboActionType.Lmb);
            _inputActions.Combat.RMB.performed += ctx => Notify(ComboActionType.Rmb);
            _inputActions.Combat.FAbility.performed += ctx => Notify(ComboActionType.Z);
            _inputActions.Combat.SAbility.performed += ctx => Notify(ComboActionType.X);
            _inputActions.Combat.TAbility.performed += ctx => Notify(ComboActionType.C);
        }

        private void OnDisable()
        {
            _inputActions.Disable();
            _inputActions.Combat.LMB.performed -= ctx => Notify(ComboActionType.Lmb);
            _inputActions.Combat.RMB.performed -= ctx => Notify(ComboActionType.Rmb);
            _inputActions.Combat.FAbility.performed -= ctx => Notify(ComboActionType.Z);
            _inputActions.Combat.SAbility.performed -= ctx => Notify(ComboActionType.X);
            _inputActions.Combat.TAbility.performed -= ctx => Notify(ComboActionType.C);
        }

        private void Start()
        {
            if (comboSystem != null)
                comboSystem.Initialize(this);
        }

        private void Notify(ComboActionType action)
        {
            OnActionPerformed?.Invoke(action);
        }
    }
}