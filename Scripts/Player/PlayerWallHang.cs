using UnityEngine;

public class PlayerWallHang : PlayerAbility {
    [Range(0, 10)] public float GainRate = .5f;
    [Range(0, 10)] public float FallSpeed = 3f;
    public bool BlockXInputWhileHanging;
    public bool AllowFastFalling;
    
    public override string AbilityName() {
        return "WallHang";
    }
    
    public override void PlayerUpdate(ref Vector2 velocity) {
        if (!enabled) return;

        // This stops a player from being stuck in a corner
        if (Core.Collisions.Below || Core.Collisions.Above) {
            return;
        }
       
        if (Core.Collisions.Left || Core.Collisions.Right) {            
            Core.MovementState = PlayerMovementState.WallContact;
            
            //If down is held, allow normal gravity 
            if (AllowFastFalling && PlayerInput.Frame.IsTiltingDown()) {
                return;
            }
            
            // Dont unstick from the wall 
            if (BlockXInputWhileHanging && !PlayerInput.Frame.IsJumpKeyHeld) {
                velocity.x = 0;
            }
            
            if (velocity.y < -.1f) {
                velocity.y += -GainRate;
            }

            if (velocity.y < -FallSpeed) {
                velocity.y = -FallSpeed;
            }
        }  
    }
}