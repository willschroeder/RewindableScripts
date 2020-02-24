using UnityEngine;

[RequireComponent(typeof(PlayerCore))]
public class PlayerHorizontalGroundedMovement : PlayerAbility {

    [Range(0, 50)] public float MoveSpeed = 6;
    [Range(0, 20)] public float GainRate = .5f;
    [Range(0, 20)] public float LossRate = .5f;
    
    public override string AbilityName() {
        return "HorizontalGroundedMovement";
    }

    public override void PlayerUpdate(ref Vector2 velocity) {
        if (!enabled) {
            return;
        }

        if (!Core.Collisions.Below) {
            return;
        }

        Core.MovementState = PlayerMovementState.Grounded;

        velocity.x += PlayerInput.Frame.Joystick.x * GainRate;
                       
        // Cap max move speed
        if (velocity.x > MoveSpeed) {
            velocity.x = MoveSpeed;
        }
        else if (velocity.x < -MoveSpeed) {
            velocity.x = -MoveSpeed;
        }
        
        // Slow down when no input
        if (!PlayerInput.Frame.JoystickIsReceivingInput()) {
            velocity.x = Util.BringToZero(velocity.x, LossRate);
        }
    }
}