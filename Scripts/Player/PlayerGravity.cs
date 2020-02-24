using UnityEngine;

[RequireComponent(typeof(PlayerCore))]
public class PlayerGravity : PlayerAbility {
    [Range(0, 15)] public float Gravity = 6;

    public override string AbilityName() {
        return "Gravity";
    }
    
    public override void PlayerUpdate(ref Vector2 velocity) {
        if (!enabled) {
            return;
        }

        // Don't apply while dashing
        if (IsDashing()) {
            return;
        }

        velocity.y += -Gravity;

        // If colliding below, dont let a massive well of velocity build up
        // this can cause us to fall off ledges very quickly
        if (Core.Collisions.Below) {
            velocity.y = -.1f;
        }
    }
}