using System;
using UnityEngine;

[RequireComponent(typeof(PlayerCore))]
public class PlayerJump : PlayerAbility, IRewindable {
    private float _currentJumpUpTime;
    private int _jumpsInSession;
    private JumpType _jumpType;

    [Range(0, 30)] public float AirMoveSpeed = 6f;
    [Range(0, 20)] public float GainRate = .5f;
    [Range(0, 20)] public float LossRate = .5f;
    [Range(0, 50)] public float InitalJumpVelocity = 6f;
    [Range(0, 300)] public float JumpFallMultiplyer = 30f;
    [Range(.1f, 3)] public float MaxUpTimeBoost = .2f;
    [Range(0, 20)] public int MaxJumps = 3;
    public Vector2 WallRepel = Vector2.one;
    public Vector2 InAirBoosts = Vector2.one;
    [Range(0, 1)] public float WallJumpInputWait = .3f;


    public override string AbilityName() {
        return "Jump";
    }

    public override void PlayerUpdate(ref Vector2 velocity) {
        if (!enabled) {
            return;
        }
        
        // PRE-JUMP
        
        // See if we are mid-air, if so, we are in a jumping state
        if (!Core.Collisions.HasCollision() && !Core.Collisions.IsCollidingWithWall() && !IsDashing()) {
            Core.MovementState = PlayerMovementState.Air;
        }
        
        // Reset jump limits
        if (Core.Collisions.Below || Core.Collisions.Left || Core.Collisions.Right) {
            // Don't reset session if the jump is held, otherwise you can "boost" off of walls 
            if (!PlayerInput.Frame.IsJumpKeyHeld) {
                _currentJumpUpTime = 0;
                _jumpsInSession = 0;
            }
            
            DetermineMovementState();   
        }
        
        // JUMP 
        
        // Start Jump
        if (PlayerInput.Frame.IsJumpKeyDown && CanJump()) {
            _currentJumpUpTime = 0; // Reset jump timer in case we are double jumping
            _jumpsInSession += 1;
            SoundManager.PlayEffect(SoundEffect.Jump);

            // Jump away from wall if needed
            if (Core.Collisions.Left) {
                velocity.y = WallRepel.y;
                velocity.x = WallRepel.x;
                _jumpType = JumpType.FromLeftWall;
            }           
            else if (Core.Collisions.Right) {
                velocity.y = WallRepel.y;
                velocity.x = -WallRepel.x;
                _jumpType = JumpType.FromRightWall;
            }
            // Normal Jump
            else {
                velocity.y = InitalJumpVelocity;
                _jumpType = JumpType.Normal;
            }
            
            Core.MovementState = PlayerMovementState.Air;
        }
        
        // Continue Jump
        if (!Core.Collisions.Below) {
            // Increment jump timer
            _currentJumpUpTime += Time.deltaTime;
            
            // Boost jump while holding button down
            if (PlayerInput.Frame.IsJumpKeyHeld && _currentJumpUpTime < MaxUpTimeBoost) {
                velocity.y += InAirBoosts.y;

                if (_jumpType == JumpType.FromLeftWall) {
                    velocity.x += InAirBoosts.x;
                }
                else if (_jumpType == JumpType.FromRightWall) {
                    velocity.x += -InAirBoosts.x;
                }
            }
            // Otherwise, force a pressing down 
            else {
                velocity.y += -JumpFallMultiplyer;
            }
        }

        // POST-JUMP
        
        // If top of charter is hit, kill the Y velocity
        if (Core.Collisions.Above) {
            velocity.y = 0;
        }
        
        // Nothing below happens unless jumping
        if (!IsInAir()) {
            return;
        }
        
        // Block input if a wall jump so we have time to break away smoothly
        if (_jumpType == JumpType.FromLeftWall || _jumpType == JumpType.FromRightWall) {
            if (_currentJumpUpTime < WallJumpInputWait) {
                return;
            }
        }
        
        // In Air Controls  
        velocity.x += PlayerInput.Frame.Joystick.x * GainRate;
            
        // Cap max move speed
        if (velocity.x > AirMoveSpeed) {
            velocity.x = AirMoveSpeed;
        }
        else if (velocity.x < -AirMoveSpeed) {
            velocity.x = -AirMoveSpeed;
        }
        
        if (!PlayerInput.Frame.JoystickIsReceivingInput()) {
            velocity.x = Util.BringToZero(velocity.x, LossRate);
        }
    }

    private bool CanJump() {
        if (MaxJumps == 0) {
            return true;
        }
        
        if (_jumpsInSession < MaxJumps) {
            return true;
        }

        return false; 
    }
    
    // IRewindable 

    public RewindFrame BuildRewindFrame() {
        var  currentJumpUpTime = _currentJumpUpTime;
        var jumpsInSession = _jumpsInSession;
        var jumpType = _jumpType;
        
        return delegate(IRewindable instance) {
            var inst = ((PlayerJump) instance);
            inst._currentJumpUpTime = currentJumpUpTime;
            inst._jumpsInSession = jumpsInSession;
            inst._jumpType = jumpType;
        };
    }

    public void ApplyRewindFrame(RewindFrame frame) {
        frame(this);
    }

    enum JumpType {
        Normal, FromLeftWall, FromRightWall
    }
}