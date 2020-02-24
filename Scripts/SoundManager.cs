using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    private static SoundManager _instance = null;
    private readonly Dictionary<SoundEffect, AudioSource> _soundEffects = new Dictionary<SoundEffect, AudioSource>();

    private void Awake() {
        if (_instance != null) {
            return;
        }

        _instance = this;
        Init();
    }

    private void Init() {
        foreach (Transform child in transform) {
            switch (child.name) {
                case "jump":
                    _soundEffects.Add(SoundEffect.Jump, child.GetComponent<AudioSource>());
                    // Use the same sound as jumping for now
                    _soundEffects.Add(SoundEffect.ResumePlayAfterRewind, child.GetComponent<AudioSource>());
                    break;
                case "dash":
                    _soundEffects.Add(SoundEffect.Dash, child.GetComponent<AudioSource>());
                    break;
                case "death":
                    _soundEffects.Add(SoundEffect.Death, child.GetComponent<AudioSource>());
                    break;
                case "landing":
                    _soundEffects.Add(SoundEffect.Landing, child.GetComponent<AudioSource>());
                    break;
                case "checkpoint":
                    _soundEffects.Add(SoundEffect.Checkpoint, child.GetComponent<AudioSource>());
                    break;
                case "rewind":
                    _soundEffects.Add(SoundEffect.Rewind, child.GetComponent<AudioSource>());
                    break;
                case "background":
                    _soundEffects.Add(SoundEffect.Background, child.GetComponent<AudioSource>());
                    break;
            }
        }
    }

    private void PlaySoundInstance(SoundEffect effect) {
        if (_soundEffects[effect].loop && _soundEffects[effect].isPlaying) {
            return;
        }
        _soundEffects[effect].Play();
    }
    
    private void StopSoundInstance(SoundEffect effect) {
        _soundEffects[effect].Stop();
    }

    // Static 
    
    public static void PlayEffect(SoundEffect effect) {
        _instance.PlaySoundInstance(effect);
    }

    public static void StopEffect(SoundEffect effect) {
        _instance.StopSoundInstance(effect);
    }

}

public enum SoundEffect {
    Background, Jump, Dash, Death, Landing, Checkpoint,
    Rewind, ResumePlayAfterRewind
}