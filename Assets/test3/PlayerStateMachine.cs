public class PlayerStateMachine
{
    public PlayerController player;
    public PlayerBaseState CurrentState { get; private set; }

    public readonly IdleState idleState;
    public readonly WalkState walkState;
    public readonly SprintState sprintState;
    public readonly JumpState jumpState;
    public readonly FallState fallState;

    public PlayerStateMachine(PlayerController pc)
    {
        player = pc;
        idleState = new IdleState(this);
        walkState = new WalkState(this);
        sprintState = new SprintState(this);
        jumpState = new JumpState(this);
        fallState = new FallState(this);
    }

    public void Initialize(PlayerBaseState startState)
    {
        CurrentState = startState;
        CurrentState.Enter();
    }

    public void TransitionTo(PlayerBaseState newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}