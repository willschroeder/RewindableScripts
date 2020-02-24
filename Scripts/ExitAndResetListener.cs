using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitAndResetListener : MonoBehaviour {
    private float _resetTime;


    private void FixedUpdate() {
        // Exit the application
        if (PlayerInput.Frame.IsMenuKeyPressed) {
            Application.Quit();
        }
        
        if (PlayerInput.Frame.IsResetKeyHeld) {
            _resetTime += Time.deltaTime;
            if (_resetTime > 1f) {
                ResetGame();
            }
        }
        else {
            _resetTime = 0;
        }

    }
    
    
    void OnCollisionEnter2D(Collision2D other) {
        ResetGame();
    }

    void ResetGame() {
        PlayerPrefs.DeleteKey("world");
        PlayerPrefs.DeleteKey("level");
        PlayerPrefs.DeleteKey("sublevel");
        SceneManager.LoadScene("world_1");
    }
}
