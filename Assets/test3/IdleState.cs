using UnityEngine;

public class IdleState : PlayerBaseState
{
    public IdleState(PlayerStateMachine ctx) : base(ctx) { }

    public override void Enter() { }
    public override void Exit() { }

    public override void UpdateState()
    {
        if (player.jumpPressed && player.isGrounded)
            ctx.TransitionTo(ctx.jumpState);

        if (player.moveInput.sqrMagnitude > 0.01f)
        {
            if (player.sprintHeld)
                ctx.TransitionTo(ctx.sprintState);
            else
                ctx.TransitionTo(ctx.walkState);
        }
    }

    public override void FixedUpdateState()
    {
        Vector3 vel = player.rb.linearVelocity;
        vel.x = Mathf.Lerp(vel.x, 0f, 0.2f);
        vel.z = Mathf.Lerp(vel.z, 0f, 0.2f);
        player.rb.linearVelocity = vel;
    }
}