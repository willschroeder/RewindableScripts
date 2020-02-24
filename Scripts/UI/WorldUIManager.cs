using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldUIManager : MonoBehaviour {

	private static WorldUIManager _instance = null;
	private readonly Dictionary<WorldUiElement, GameObject> _uiElements = new Dictionary<WorldUiElement, GameObject>();

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
				case "MainBanner":
					_uiElements.Add(WorldUiElement.MainBanner, child.gameObject);
					break;
				case "SubBanner":
					_uiElements.Add(WorldUiElement.SubBanner, child.gameObject);
					break;
				case "LeftBanner":
					_uiElements.Add(WorldUiElement.LeftBanner, child.gameObject);
					break;
				case "RightBanner":
					_uiElements.Add(WorldUiElement.RightBanner, child.gameObject);
					break;
			}
		}
		
		// Hide all elements to start
		foreach (var uiElementsKey in _uiElements.Keys) {
			DisplayBannerInst(uiElementsKey, false);
		}
	}

	private void DisplayBannerInst(WorldUiElement element, bool isVisible) {
		_uiElements[element].gameObject.SetActive(isVisible);
	}
	
	private void SetBannerTextInst(WorldUiElement element, string text) {
		// This lookup should be cached, future calls will be hitting this frequently (timer for example)
		_uiElements[element].transform.GetChild(0).GetComponent<Text>().text = text;
	}
	
	// Static 

	public static void DisplayBanner(WorldUiElement element) {
		_instance.DisplayBannerInst(element, true);
	}

	public static void SetBannerText(WorldUiElement element, string text) {
		_instance.SetBannerTextInst(element, text);	
	}

	public static void HideElement(WorldUiElement element) {
		_instance.DisplayBannerInst(element, false);
	}
}

public enum WorldUiElement {
	MainBanner, SubBanner, LeftBanner, RightBanner
}