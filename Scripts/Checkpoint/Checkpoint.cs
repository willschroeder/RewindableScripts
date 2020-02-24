using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {
	public int World;
	public int Level;
	public int SubLevel;
	public int RewindsForThisSection;
	
	private bool _triggered;
	private readonly CheckpointDetails _checkpointDetails = new CheckpointDetails();
	private readonly List<ICheckpointHit> _checkpointListeners = new List<ICheckpointHit>();
	private Rewinder _rewinder;


	private void Start() {
		_checkpointDetails.World = World;
		_checkpointDetails.Level = Level;
		_checkpointDetails.SubLevel = SubLevel;
		_checkpointDetails.RewindsForThisSection = RewindsForThisSection;
		_checkpointDetails.checkpointGameObject = this;
		
		foreach (var go in FindObjectsOfType<GameObject>()) {
			if (go == this) {
				continue;
			}
			foreach (var comp in go.GetComponents<Component>()) {
				var checkpointListener = comp as ICheckpointHit;
				if (checkpointListener != null) {
					_checkpointListeners.Add(checkpointListener);
				}
			}
		}

		_rewinder = GameObject.Find("__Scene__").GetComponent<Rewinder>();
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (_triggered) {
			return;
		}
		
		FlipTrigger();
		_checkpointDetails.SaveGameAtThisCheckpoint();
		_rewinder.WipeHistory();
		foreach (var checkpointListener in _checkpointListeners) {
			checkpointListener.CheckpointHit(_checkpointDetails);
		}
	}

	public void FlipTrigger(bool isFlippedByLoad=false) {
		_triggered = true; 
		GetComponent<Renderer>().material.color = Color.magenta;

		if (isFlippedByLoad) {
			return;
		}
		
		SoundManager.PlayEffect(SoundEffect.Checkpoint);
	}

	public Vector2 CheckpointCenter() {
		return GetComponent<Renderer>().bounds.center;
	}


	public CheckpointDetails CheckpointDetails() {
		return _checkpointDetails;
	}
}

public class CheckpointDetails {
	public int World;
	public int Level;
	public int SubLevel;
	public int RewindsForThisSection;
	public Checkpoint checkpointGameObject;
	
	public static readonly string WorldKey = "world";
	public static readonly string LevelKey = "level";
	public static readonly string SubLevelKey = "sublevel";

	public void SaveGameAtThisCheckpoint() {
		PlayerPrefs.SetInt(WorldKey, World);
		PlayerPrefs.SetInt(LevelKey, Level);
		PlayerPrefs.SetInt(SubLevelKey, SubLevel);
	}
}

public interface ICheckpointHit {
	void CheckpointHit(CheckpointDetails details);
}