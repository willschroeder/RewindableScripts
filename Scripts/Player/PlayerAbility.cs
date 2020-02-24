using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerCore))]
public class PlayerAbility : MonoBehaviour {
    protected PlayerCore Core;

    private void Start() {
        Core = GetComponent<PlayerCore>();
    }

    public virtual void PlayerUpdate(ref Vector2 velocity) {
    }

    public virtual string AbilityName() {
        return "";
    }

    protected void DetermineMovementState() {
        if (Core.Collisions.Below) {
            Core.MovementState = PlayerMovementState.Grounded;
        }
        else if (Core.Collisions.Left || Core.Collisions.Right) {
            Core.MovementState = PlayerMovementState.WallContact;
        }
        else if (!Core.Collisions.HasCollision()) {
            Core.MovementState = PlayerMovementState.Air;
        }
        else {
            throw new NotImplementedException();
        }
    }

    protected bool IsInAir() {
        return Core.MovementState == PlayerMovementState.Air;
    }

    protected bool IsDashing() {
        return Core.MovementState == PlayerMovementState.Dashing;
    }
    
    protected bool IsWallContact() {
        return Core.MovementState == PlayerMovementState.WallContact;
    }
}