using UnityEngine;

public class JumpState : PlayerBaseState
{
    public JumpState(PlayerStateMachine ctx) : base(ctx) { }

    public override void Enter()
    {
        player.rb.AddForce(Vector3.up * player.jumpForce, ForceMode.Impulse);
        player.ConsumeJump();
    }

    public override void Exit() { }

    public override void UpdateState()
    {
        if (!player.isGrounded && player.rb.linearVelocity.y < 0f)
            ctx.TransitionTo(ctx.fallState);
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