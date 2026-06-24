using UnityEngine;

public class WalkState : PlayerBaseState
{
    public WalkState(PlayerStateMachine ctx) : base(ctx) { }

    public override void Enter() { }
    public override void Exit() { }

    public override void UpdateState()
    {
        if (player.jumpPressed && player.isGrounded)
            ctx.TransitionTo(ctx.jumpState);

        if (player.moveInput.sqrMagnitude < 0.01f)
            ctx.TransitionTo(ctx.idleState);
        else if (player.sprintHeld)
            ctx.TransitionTo(ctx.sprintState);
    }

    public override void FixedUpdateState()
    {
        ApplyGroundedMovement(player.walkSpeed);
    }

    protected void ApplyGroundedMovement(float speed)
    {
        Vector3 targetVelocity = player.GetMoveDirection() * speed;
        Vector3 currentVel = player.rb.linearVelocity;
        Vector3 newVel = Vector3.MoveTowards(
            new Vector3(currentVel.x, 0, currentVel.z),
            targetVelocity,
            player.acceleration * Time.fixedDeltaTime
        );

        if (player.isGrounded && player.groundAngle <= player.slopeLimit)
            newVel = Vector3.ProjectOnPlane(newVel, player.groundNormal);

        player.rb.linearVelocity = new Vector3(newVel.x, player.rb.linearVelocity.y, newVel.z);
    }
}