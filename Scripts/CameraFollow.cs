using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public GameObject Target;
	public bool ShowFocusBox;
	public float FocusFrameX = 5;
	public float FocusFrameY = 5;
	public float YOffset;
	
	private BoxCollider2D _targetBoxCollider;
	private Bounds _focusBounds;

	private void Start() {
		_targetBoxCollider = Target.GetComponent<BoxCollider2D>();
		_focusBounds = new Bounds(_targetBoxCollider.bounds.center, new Vector2(FocusFrameX,FocusFrameY));
	}

	void Update() {		
		var cameraRect = new Rect(_focusBounds.min, _focusBounds.size);
		var playerRect = new Rect(_targetBoxCollider.bounds.min, _targetBoxCollider.bounds.size);

		var x = 0f;
		var y = 0f;
	
		if (playerRect.xMax > cameraRect.xMax) {
			x = playerRect.xMax - cameraRect.xMax;
		}

		if (playerRect.xMin < cameraRect.xMin) {
			x = playerRect.xMin - cameraRect.xMin;
		}

		if (playerRect.yMax > cameraRect.yMax) {
			y = playerRect.yMax - cameraRect.yMax;
		}
		
		if (playerRect.yMin < cameraRect.yMin) {
			y = playerRect.yMin - cameraRect.yMin;
		}
		
		_focusBounds.center = new Vector3(_focusBounds.center.x + x, _focusBounds.center.y + y + YOffset, transform.position.z);
		transform.position = _focusBounds.center;
	}
	
	void OnDrawGizmos() {
		if (!ShowFocusBox) {
			return;
		}
		Gizmos.color = new Color(0, 1, 0, .5f);
		Gizmos.DrawCube(_focusBounds.center, _focusBounds.size);
	}
}
