using UnityEngine;

public class FallState : PlayerBaseState
{
    public FallState(PlayerStateMachine ctx) : base(ctx) { }

    public override void Enter() { }
    public override void Exit() { }

    public override void UpdateState()
    {
        if (player.isGrounded)
        {
            if (player.moveInput.sqrMagnitude > 0.01f)
            {
                if (player.sprintHeld)
                    ctx.TransitionTo(ctx.sprintState);
                else
                    ctx.TransitionTo(ctx.walkState);
            }
            else
            {
                ctx.TransitionTo(ctx.idleState);
            }
        }
    }

    public override void FixedUpdateState()
    {
        Vector3 moveDir = player.GetMoveDirection();

        if (moveDir.sqrMagnitude > 0.01f)
        {
            Vector3 horizontalVelocity = new Vector3(player.rb.linearVelocity.x, 0f, player.rb.linearVelocity.z);
            Vector3 desiredVelocity = moveDir * player.maxAirSpeed;
            Vector3 velocityChange = desiredVelocity - horizontalVelocity;
            float maxChange = player.airAcceleration * Time.fixedDeltaTime;
            velocityChange = Vector3.ClampMagnitude(velocityChange, maxChange);
            player.rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }
}