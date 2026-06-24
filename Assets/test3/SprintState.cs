public class SprintState : WalkState
{
    public SprintState(PlayerStateMachine ctx) : base(ctx) { }

    public override void UpdateState()
    {
        if (player.jumpPressed && player.isGrounded)
            ctx.TransitionTo(ctx.jumpState);

        if (player.moveInput.sqrMagnitude < 0.01f)
            ctx.TransitionTo(ctx.idleState);
        else if (!player.sprintHeld)
            ctx.TransitionTo(ctx.walkState);
    }

    public override void FixedUpdateState()
    {
        ApplyGroundedMovement(player.sprintSpeed);
    }
}