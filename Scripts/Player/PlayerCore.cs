using System;
using System.Globalization;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerCore : MonoBehaviour, IRewindable {
    public float SpeedLimit = 10f;
    
    private Vector2 _bottomLeft;
    private Vector2 _bottomRight;
    private Bounds _bounds;
    private BoxCollider2D _boxCollider;
    private float _heightRaySpacing;
    private LayerMask _inversePlayerMask;
    private PlayerAbility[] _playerAbilities;
    private readonly int _raysPerDirection = 5;
    private Rewinder _rewinder;

    // inset used so if touching a surface, we still have room to fire rays
    private readonly float _skinWidth = 0.01f; 
    // This is how far rays stick out from the edge, allowing detection of being against a wall and not just next to it
    private readonly float _rayPeekOutDistance = .1f; 
    private Vector2 _topLeft;
    private Vector2 _topRight;
    private Vector2 _velocity;
    private float _widthRaySpacing;

    public PlayerCollisions Collisions { get; private set; }
    public PlayerMovementState MovementState { get; set; }

    private void Start() {
        MovementState = PlayerMovementState.Air;
        _rewinder = GameObject.Find("__Scene__").GetComponent<Rewinder>();
       
        _boxCollider = GetComponent<BoxCollider2D>();
        Collisions = new PlayerCollisions();
        _playerAbilities = GetComponents<PlayerAbility>();

        if (_raysPerDirection > 1) {
            _bounds = _boxCollider.bounds;
            _widthRaySpacing = _bounds.size.x / _raysPerDirection;
            _heightRaySpacing = _bounds.size.y / _raysPerDirection;
        }

        _inversePlayerMask = ~(1 << 8);
    }

    private void FixedUpdate() {
        if (_rewinder.IsFrozen()) {
            return;
        }
        
        UpdateBounds();

        var abilityVelocity = _velocity;
        
        foreach (var playerAbility in _playerAbilities) {
            if (playerAbility.enabled) {
                playerAbility.PlayerUpdate(ref abilityVelocity);
            }
        }

        // Apply speed limit 
        if (abilityVelocity.x > SpeedLimit) {
            abilityVelocity.x = SpeedLimit;
        }
        else if (abilityVelocity.x < -SpeedLimit) {
            abilityVelocity.x = -SpeedLimit;
        }
        else if (abilityVelocity.y > SpeedLimit) {
            abilityVelocity.y = SpeedLimit;
        }        
        else if (abilityVelocity.y < -SpeedLimit) {
            abilityVelocity.y = -SpeedLimit;
        }                        
        
        _velocity = abilityVelocity;
        
        // newPosition is movement to be made this frame
        // so while velocity is accumulating above
        // our position is still scaled to time delta 
        // otherwise we would hit warp speed 
        // basically, this is just the velocity scaled by this frame 
        var newPosition = _velocity * Time.deltaTime;

        // Reset any collisions or state
        Collisions.ResetCollisions();

        //cast rays in a clockwise direction
        UpCollision(ref newPosition);
        RightCollision(ref newPosition);
        DownCollision(ref newPosition);
        LeftCollision(ref newPosition);
                
        // Correct velocities if a collision was detected
        if (Collisions.Left || Collisions.Right) {
            _velocity.x = 0;
        }

        if (Collisions.Above || Collisions.Below) {
            _velocity.y = 0;
        }
                
        transform.Translate(newPosition);
    }

    private void UpdateBounds() {
        _bounds = _boxCollider.bounds;
        _bounds.Expand(_skinWidth * -2f); //-2f because for some reason that makes it collide on the edge

        _bottomLeft = new Vector2(_bounds.min.x, _bounds.min.y);
        _topLeft = new Vector2(_bounds.min.x, _bounds.max.y);
        _bottomRight = new Vector2(_bounds.max.x, _bounds.min.y);
        _topRight = new Vector2(_bounds.max.x, _bounds.max.y);
    }

    private void UpCollision(ref Vector2 newPosition) {
        var rayDistance = Mathf.Abs(newPosition.y) - _skinWidth;

        for (var i = 0; i < _raysPerDirection; i++) {
            var offset = Vector2.right * _widthRaySpacing * i; // right for direction each origin should be placed
            var rayOrigin = _topLeft + offset;
            var rayDirection = Vector2.up;

            var hit = RayCast(rayOrigin, rayDirection, rayDistance, _inversePlayerMask, Color.green);
            if (hit && !hit.collider.isTrigger && !IsDamage(hit.transform.gameObject)) {
                var hitDistance = hit.point.y - rayOrigin.y;
                hitDistance -= _skinWidth;

                rayDistance = hitDistance;

                if (newPosition.y > 0) {
                    newPosition.y = hitDistance;
                }

                Collisions.Above = true;
            }
        }
    }

    private void RightCollision(ref Vector2 newPosition) {
        var rayDistance = Mathf.Abs(newPosition.x) - _skinWidth + _rayPeekOutDistance;

        for (var i = 0; i < _raysPerDirection; i++) {
            var offset = Vector2.down * _heightRaySpacing * i;
            var rayOrigin = _topRight + offset;
            var rayDirection = Vector2.right;

            var hit = RayCast(rayOrigin, rayDirection, rayDistance, _inversePlayerMask, Color.yellow);
            if (hit && !hit.collider.isTrigger && !IsDamage(hit.transform.gameObject)) {
                var hitDistance = hit.point.x - rayOrigin.x;
                hitDistance -= _skinWidth;

                rayDistance = hitDistance;

                if (newPosition.x > 0) {
                    newPosition.x = hitDistance;
                }

                Collisions.Right = true;
            }
        }
    }

    private void DownCollision(ref Vector2 newPosition) {
        var rayDistance = Mathf.Abs(newPosition.y) + _skinWidth;

        for (var i = 0; i < _raysPerDirection; i++) {
            var offset = Vector2.left * _widthRaySpacing * i;
            var rayOrigin = _bottomRight + offset;
            var rayDirection = Vector2.down;

            var hit = RayCast(rayOrigin, rayDirection, rayDistance, _inversePlayerMask, Color.magenta);
            if (hit && !hit.collider.isTrigger && !IsDamage(hit.transform.gameObject)) {
                var hitDistance = hit.point.y - rayOrigin.y; // hit will be lower than origin going down			
                hitDistance += _skinWidth;

                // now distance to cast will be shorter, so we wont pass though the object
                rayDistance = hitDistance;

                if (newPosition.y < 0) {
                    newPosition.y = hitDistance;
                }

                Collisions.Below = true;
            }
        }
    }

    private void LeftCollision(ref Vector2 newPosition) {
        var rayDistance = Mathf.Abs(newPosition.x) + _skinWidth + _rayPeekOutDistance;

        for (var i = 0; i < _raysPerDirection; i++) {
            var offset = Vector2.up * _heightRaySpacing * i;
            var rayOrigin = _bottomLeft + offset;
            var rayDirection = Vector2.left;

            var hit = RayCast(rayOrigin, rayDirection, rayDistance, _inversePlayerMask, Color.cyan);
            if (hit && !hit.collider.isTrigger && !IsDamage(hit.transform.gameObject)) {
                var hitDistance = hit.point.x - rayOrigin.x;
                hitDistance += _skinWidth;

                rayDistance = hitDistance;

                // we are both detecting a collision but only snapping to the side if there is velocity
                if (newPosition.x < 0) {
                    newPosition.x = hitDistance;
                }

                Collisions.Left = true;
            }
        }
    }
    
    private static RaycastHit2D RayCast(Vector2 rayOriginPoint, Vector2 rayDirection, float rayDistance, LayerMask mask,
        Color color, bool drawGizmo = true) {
        if (drawGizmo) Debug.DrawRay(rayOriginPoint, rayDirection * rayDistance, color);

        return Physics2D.Raycast(rayOriginPoint, rayDirection, rayDistance, mask);
    }

    // This is a hack that slips collision with damaging triggers, Box2D will detect and stop player
    private bool IsDamage(GameObject obj) {
        return obj.CompareTag("Damage");
    }

    // IRewindable 
    
    public RewindFrame BuildRewindFrame() {
        var positionCopy = transform.position;
        var velocityCopy = _velocity;
        
        return delegate(IRewindable instance) {
            var inst = ((PlayerCore)instance); 
            inst.transform.position = positionCopy;
            inst._velocity = velocityCopy;
        };
    }

    public void ApplyRewindFrame(RewindFrame frame) {
        frame(this);
    }
} 