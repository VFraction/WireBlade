using System;

namespace FightingGame.Combat
{
    public interface ICombatInputProvider
    {
        event Action<ComboActionType> OnActionPerformed;
    }
}