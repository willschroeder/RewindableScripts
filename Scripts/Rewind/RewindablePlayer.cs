using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RewindablePlayer : MonoBehaviour, IRewindStatus, ICheckpointHit {
    private Rewinder _rewinder;
    private RewindStatus _currentRewindStatus;
    private int _rewindsLeft = 9999;
    private CheckpointDetails _lastCheckpoint;
    protected PlayerCore Core;

    private void Start() {
        _rewinder = GameObject.Find("__Scene__").GetComponent<Rewinder>();
        Core = GetComponent<PlayerCore>();

        WorldUIManager.DisplayBanner(WorldUiElement.LeftBanner);
        UpdateRewindsCountBanner(_rewindsLeft);
        StartCoroutine(DelayTeleport());
    }
    
    IEnumerator DelayTeleport()
    {
        // This is required because in the start frame, the checkpoint scripts are finding their own object in Start
        yield return 0;
        TeleportToStartingCheckpoint();
    }

    
    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Damage")) {
            SoundManager.PlayEffect(SoundEffect.Death);
            _rewindsLeft -= 1;
            _rewinder.DeathFreeze(_rewindsLeft >= 0);
            
            UpdateRewindsCountBanner(_rewindsLeft);
            DisplayDeathUI(_rewindsLeft >= 0);
        }
        else {
            SoundManager.PlayEffect(SoundEffect.Landing);
        }
    }
    
    void FixedUpdate() {
        if (_currentRewindStatus == RewindStatus.ResetRequiredDeathFreeze && PlayerInput.Frame.IsRewindKeyHeld) {
            TeleportToCheckpoint(_lastCheckpoint);
            _rewinder.WipeHistory();
            _rewinder.StartRecording();
            _rewindsLeft = _lastCheckpoint.RewindsForThisSection;
        }
    }
    
    Checkpoint FindCheckpoint(int world, int level, int sublevel) {
        foreach (var go in FindObjectsOfType<GameObject>()) {
            foreach (var comp in go.GetComponents<Component>()) {
                var checkpoint = comp as Checkpoint;
                if (checkpoint != null) {
                    if (checkpoint.World == world && checkpoint.Level == level && checkpoint.SubLevel == sublevel) {
                        return checkpoint;
                    }
                }
            }
        }
        throw new InvalidOperationException("No checkpoint with that level");
    }
    
    void TeleportToCheckpoint(CheckpointDetails checkpoint) {
        transform.position = checkpoint.checkpointGameObject.CheckpointCenter();
        _lastCheckpoint = checkpoint;
        _rewindsLeft = checkpoint.RewindsForThisSection;
        UpdateRewindsCountBanner(_rewindsLeft);
    }

    void TeleportToStartingCheckpoint() {
        var world = PlayerPrefs.GetInt(CheckpointDetails.WorldKey);
        var level = PlayerPrefs.GetInt(CheckpointDetails.LevelKey);
        var sublevel = PlayerPrefs.GetInt(CheckpointDetails.SubLevelKey);

        // Then we are at the beginning
        if (level == 0) {
            return;
        }

        var checkpoint = FindCheckpoint(world, level, sublevel);
        checkpoint.FlipTrigger(true);
        TeleportToCheckpoint(checkpoint.CheckpointDetails());
    }

    // IRewindStatus
    
    public void RewindStatusChanged(RewindStatus changingTo) {        
        switch (changingTo) {
            case RewindStatus.Recording:
                DOTween.PlayForwardAll();
                GetComponent<Renderer>().material.color = Color.green;
                HideDeathUI();
                break;
            case RewindStatus.Rewinding:
                DOTween.PlayBackwardsAll();
                GetComponent<Renderer>().material.color = Color.yellow;
                break;
            case RewindStatus.SafeFreeze:
                DOTween.PauseAll();
                GetComponent<Renderer>().material.color = Color.yellow;
                StartCoroutine(nameof(RewindEndingCountdownUi));
                break;
            case RewindStatus.RewindableDeathFreeze:
            case RewindStatus.ResetRequiredDeathFreeze:
                DOTween.PauseAll();
                GetComponent<Renderer>().material.color = new Color(1.0f, 0.5f, 0.0f);
                break;
        }

        // Perform anything that needs to to happen on a specific status change to another
        if (changingTo == RewindStatus.Recording) {
            SoundManager.PlayEffect(SoundEffect.Background);
        }
        else {
            SoundManager.StopEffect(SoundEffect.Background);
        }
        
        if (changingTo == RewindStatus.Rewinding) {
            SoundManager.PlayEffect(SoundEffect.Rewind);
        }
        else {
            SoundManager.StopEffect(SoundEffect.Rewind);
        }

        if (changingTo == RewindStatus.Recording && _currentRewindStatus == RewindStatus.SafeFreeze) {
            SoundManager.PlayEffect(SoundEffect.ResumePlayAfterRewind);
        }
        
        _currentRewindStatus = changingTo;
    }
    
    // Private
    
    private void DisplayDeathUI(bool hasRewindsLeft) {
        WorldUIManager.DisplayBanner(WorldUiElement.MainBanner);
        WorldUIManager.DisplayBanner(WorldUiElement.SubBanner);

        if (hasRewindsLeft) {
            WorldUIManager.SetBannerText(WorldUiElement.MainBanner,"Try That Again");
            WorldUIManager.SetBannerText(WorldUiElement.SubBanner,"Hold L1 or Backspace to rewind.");   
        }
        else {
            WorldUIManager.SetBannerText(WorldUiElement.MainBanner,"Out of Rewinds");
            WorldUIManager.SetBannerText(WorldUiElement.SubBanner,"Hold L1 or Backspace restart at checkpoint.");   
        }
    }

    private void HideDeathUI() {
        WorldUIManager.HideElement(WorldUiElement.MainBanner);
        WorldUIManager.HideElement(WorldUiElement.SubBanner);
    }

    private IEnumerator RewindEndingCountdownUi() {
        WorldUIManager.DisplayBanner(WorldUiElement.SubBanner);

        var timeWaitedForResume = 0f;

        while (true) {
            // Banner will exit when status changes for any reason 
            if (_currentRewindStatus != RewindStatus.SafeFreeze && timeWaitedForResume > 0) {
                break;
            }
            
            var countdown = Math.Round((Rewinder.WaitBeforeResumeTime - timeWaitedForResume), 1);
            countdown = countdown < 0 ? 0 : countdown;
            WorldUIManager.SetBannerText(WorldUiElement.SubBanner, countdown.ToString());
            timeWaitedForResume += Time.deltaTime;
            
            yield return null;	
        }
        
        WorldUIManager.HideElement(WorldUiElement.SubBanner);
    }

    private void UpdateRewindsCountBanner(int rewindsCount) {
        WorldUIManager.SetBannerText(WorldUiElement.LeftBanner, $"Rewinds: {rewindsCount}");
    }

    // ICheckpointHit
    
    public void CheckpointHit(CheckpointDetails details) {
        _lastCheckpoint = details;
        _rewindsLeft = details.RewindsForThisSection;
        UpdateRewindsCountBanner(_rewindsLeft);
    }
}