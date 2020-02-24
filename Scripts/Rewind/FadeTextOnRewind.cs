using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FadeTextOnRewind : MonoBehaviour, IRewindStatus {
    private TMPro.TextMeshPro _textMesh;
    
    private void Start() {
        _textMesh = GetComponent<TMPro.TextMeshPro>();
        _textMesh.enabled = false;
    }

    public void RewindStatusChanged(RewindStatus changingTo) {       
        if (changingTo == RewindStatus.Recording) {
            _textMesh.enabled = false;
        }
        else {
            _textMesh.enabled = true;
        }
    }
}
