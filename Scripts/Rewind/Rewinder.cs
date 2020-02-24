using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void RewindFrame(IRewindable instance);

public class Rewinder : MonoBehaviour {
	public RewindStatus Status { get; internal set; }
	public static float SafeTime { get; internal set; }

	private readonly Dictionary<IRewindable, Stack<RewindFrame>> _rewindables = new Dictionary<IRewindable, Stack<RewindFrame>>();
	private readonly List<IRewindStatus> _statusListeners = new List<IRewindStatus>();

	public const float WaitBeforeResumeTime = 1;
	private float _timeWaitedForResume;
	
	void Start () {
		Status = RewindStatus.Recording;

		foreach (var go in FindObjectsOfType<GameObject>()) {
			if (go == this) {
				continue;
			}
			foreach (var comp in go.GetComponents<Component>()) {
				// Find all rewindable objects
				var rewindable = comp as IRewindable;
				if (rewindable != null) {
					_rewindables.Add(rewindable, new Stack<RewindFrame>());
				}

				// Find all listeners who care about status updates 
				var listener = comp as IRewindStatus;
				if (listener != null) {
					_statusListeners.Add(listener);
				}
			}
		}
	}
	
	void FixedUpdate () {		
		switch (Status) {
			case RewindStatus.Recording:
				RecordFrame();
				break;
			case RewindStatus.Rewinding:
				if (PlayerInput.Frame.IsRewindKeyHeld && HasRewindableFrames()) {
					RewindFrame();
				}
				else {
					ChangeStatus(RewindStatus.SafeFreeze);
				}
				break;
			case RewindStatus.SafeFreeze:
				// If rewound while still frozen, start rewinding again
				if (PlayerInput.Frame.IsRewindKeyHeld && HasRewindableFrames()) {
					ChangeStatus(RewindStatus.Rewinding);
					_timeWaitedForResume = 0;
				}
				
				// Have safety time period where a rewind can start again 
				if (_timeWaitedForResume < WaitBeforeResumeTime) {
					_timeWaitedForResume += Time.deltaTime;
				}
				else {
					_timeWaitedForResume = 0;
					ChangeStatus(RewindStatus.Recording);
				}
				
				break;
			case RewindStatus.RewindableDeathFreeze:
				if (PlayerInput.Frame.IsRewindKeyHeld && HasRewindableFrames()) {
					ChangeStatus(RewindStatus.Rewinding);
				}
				break;
			case RewindStatus.ResetRequiredDeathFreeze:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public void DeathFreeze(bool hasRewindsLeft) {
		if (Status != RewindStatus.Recording) {
			return;
		}

		if (hasRewindsLeft) {
			ChangeStatus(RewindStatus.RewindableDeathFreeze);
		}
		else {
			ChangeStatus(RewindStatus.ResetRequiredDeathFreeze);
		}
	}

	public void WipeHistory() {
		foreach(var entry in _rewindables) {
			entry.Value.Clear();
		}
	}
	
	public void StartRecording() {
		ChangeStatus(RewindStatus.Recording);
	}

	public bool IsFrozen() {
		return Status == RewindStatus.SafeFreeze ||
		       Status == RewindStatus.RewindableDeathFreeze ||
		       Status == RewindStatus.ResetRequiredDeathFreeze;
	}

	private void ChangeStatus(RewindStatus status) {
		if (Status == status) {
			return;
		}

		Status = status;

		switch (status) {
			case RewindStatus.Rewinding:
				break;
		}
		
		foreach (var listener in _statusListeners) {
			listener.RewindStatusChanged(Status);
		}	
	}

	private void RecordFrame() {
		SafeTime += Time.deltaTime;
		
		foreach(var entry in _rewindables) {
			entry.Value.Push(entry.Key.BuildRewindFrame());
		}
	}

	private void RewindFrame() {
		SafeTime -= Time.deltaTime;

		foreach(var entry in _rewindables) {
			if (entry.Value.Count == 0) {
				continue;
			}
			entry.Key.ApplyRewindFrame(entry.Value.Pop());
		}
	}

	private bool HasRewindableFrames() {
		return _rewindables.First().Value.Count > 0;
	}
}

public enum RewindStatus {
	Recording,
	Rewinding,
	SafeFreeze,
	RewindableDeathFreeze,
	ResetRequiredDeathFreeze
}