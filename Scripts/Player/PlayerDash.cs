using UnityEngine;

public class 
    PlayerDash : PlayerAbility, IRewindable {  
    [Range(0, 10)] public int MaxDashes = 2;
    [Range(100, 300)] public float DashVelocity = 1f;
    [Range(0, 10)] public float DashLength = 1f;
    
    private int _dashesInSession;
    private bool _dashingRight;
    private float _currentDashTime;
    
    public override string AbilityName() {
        return "Dash";
    }

    public override void PlayerUpdate(ref Vector2 velocity) {
        if (!enabled) {
            return;
        }

        // Reset Dashes
        if (Core.Collisions.Below || Core.Collisions.Left || Core.Collisions.Right) {
            _dashesInSession = 0;
        }

        if (PlayerInput.Frame.IsDashKeyDown && CanDash()) {
            StartDash();
        }

        if (!IsDashing()) {
            return;
        }

        _currentDashTime += Time.deltaTime;
        velocity.y = 0; // lock y for dash
        
        if (Core.Collisions.Left || Core.Collisions.Right) {
            StopDash();
        }
        else if (_dashingRight && _currentDashTime <= DashLength) {
            velocity.x = DampenedDashVelocity();
        }
        else if (!_dashingRight && _currentDashTime <= DashLength) {
            velocity.x = -1 * DampenedDashVelocity();
        }
        else {
            StopDash();
        }
    }

    private void StartDash() {
        _dashingRight = PlayerInput.Frame.IsTiltedRight();
        _currentDashTime = 0;
        _dashesInSession += 1;
        Core.MovementState = PlayerMovementState.Dashing;
        SoundManager.PlayEffect(SoundEffect.Dash);
    }

    private void StopDash() {
        DetermineMovementState();
    }
   
    private float DampenedDashVelocity() {
        var dashPercentageCompleted = (_currentDashTime / DashLength);
        return DashVelocity - (DashVelocity * dashPercentageCompleted);
    }
    
    private bool CanDash() {
        if (IsDashing()) {
            return false;
        }

        if (!IsInAir()) {
            return false; 
        }

        if (_dashesInSession >= MaxDashes) {
            return false;
        }

        if (PlayerInput.Frame.JoystickNotReceivingInput()) {
            return false; 
        }
        
        return true;
    }
    
    // IRewindable 

    public RewindFrame BuildRewindFrame() {
        var dashesInSession = _dashesInSession;
        var dashingRight =  _dashingRight;
        var currentDashTime = _currentDashTime;
        
        return delegate(IRewindable instance) {
            var inst = ((PlayerDash) instance);
            inst._dashesInSession = dashesInSession;
            inst._dashingRight = dashingRight;
            inst._currentDashTime = currentDashTime;
        };
    }

    public void ApplyRewindFrame(RewindFrame frame) {
        frame(this);
    }
}