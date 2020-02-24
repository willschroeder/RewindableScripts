using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugUI : MonoBehaviour {
	private static string _toPrint = null;
	
	void OnGUI() {
		if (_toPrint == null) {
			return;
		}

		GUI.Label(new Rect(10, 10, 1000, 1000), _toPrint);
	}

	public static void SetString(string newValue) {
		_toPrint = newValue;
	}

	public static void SetBool(bool newValue) {
		SetString($"{newValue}");
	}

	public static void SetFloat(float newValue) {
		SetString($"{newValue}");
	}
}
