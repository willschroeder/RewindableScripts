using UnityEngine;

public class PlayerCollisions {
    public bool Above;
    public bool Below;
    public bool Left;
    public bool Right;

    public bool HasCollision() {
        return Below || Right || Left || Above;
    }

    public bool IsCollidingWithWall() {
        return Left || Right;
    }

    public void ResetCollisions() {
        Right = Left = Above = Below = false;
    }

    public override string ToString() {
        return $"[PlayerCollision] r: {Right}, l: {Left}, a: {Above}, b: {Below}";
    }
}