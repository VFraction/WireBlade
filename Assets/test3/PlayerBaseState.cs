public abstract class PlayerBaseState
{
    protected PlayerStateMachine ctx;
    protected PlayerController player;

    public PlayerBaseState(PlayerStateMachine stateMachine)
    {
        ctx = stateMachine;
        player = stateMachine.player;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void UpdateState();
    public abstract void FixedUpdateState();
}