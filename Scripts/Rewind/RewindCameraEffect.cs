using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

//	https://github.com/Unity-Technologies/PostProcessing/wiki/Manipulating-the-Stack

public class RewindCameraEffect : MonoBehaviour, IRewindStatus {

	PostProcessVolume _postProcessVolume;
	LensDistortion _lensDistortion;
	private Bloom _bloom;
	private ColorGrading _colorGrading;

	private const float BloomIntensityNormal = 18;
	private const float BloomIntensityHigh = 23;
	private const float LensDistortionAmount = -25f;


	void Start()
	{
		_lensDistortion = ScriptableObject.CreateInstance<LensDistortion>();
		
		_bloom = ScriptableObject.CreateInstance<Bloom>();
		_bloom.enabled.Override(true);
		_bloom.intensity.Override(BloomIntensityNormal);
		_bloom.diffusion.Override(6.5f);
		
		_colorGrading = ScriptableObject.CreateInstance<ColorGrading>();
		_colorGrading.enabled.Override(true);
		_colorGrading.saturation.Override(100);
		_colorGrading.contrast.Override(7);
		_colorGrading.temperature.Override(7);

		
		_postProcessVolume = PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, _bloom);
		_postProcessVolume = PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, _colorGrading);
		_postProcessVolume = PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, _lensDistortion);
	}

	void OnDestroy()
	{
		RuntimeUtilities.DestroyVolume(_postProcessVolume, true, true);
	}
	
	// IRewindStatus
	
	public void RewindStatusChanged(RewindStatus changingTo) {
		switch (changingTo) {
			case RewindStatus.Recording:
				break;
			case RewindStatus.Rewinding:
				StartCoroutine(nameof(LensDistortFadeIn));
				break;
			case RewindStatus.SafeFreeze:
				StartCoroutine(nameof(LensDistortFadeOut));
				break;
			case RewindStatus.RewindableDeathFreeze:
				break;
		}	
	}

	private IEnumerator LensDistortFadeIn() 
	{
		_lensDistortion.enabled.Override(true);

		while (true) {
			var correctionsBeingMade = false; 
			
			if (_lensDistortion.intensity.value > LensDistortionAmount) {
				_lensDistortion.intensity.Override(_lensDistortion.intensity.value - (100 * Time.deltaTime));
				correctionsBeingMade = true;
			}

			if (_bloom.intensity.value < BloomIntensityHigh) {
				_bloom.intensity.Override(_bloom.intensity.value + (100 * Time.deltaTime));
				correctionsBeingMade = true;
			}

			if (!correctionsBeingMade) {
				break;
			}
			
			yield return null;	
		}
	}

	private IEnumerator LensDistortFadeOut() 
	{
		while (true) {
			var correctionsBeingMade = false; 

			if (_lensDistortion.intensity.value < 0) {
				_lensDistortion.intensity.Override(_lensDistortion.intensity.value + (60 * Time.deltaTime));
				correctionsBeingMade = true;
			}

			if (_bloom.intensity.value > BloomIntensityNormal) {
				_bloom.intensity.Override(_bloom.intensity.value - (60 * Time.deltaTime));
				correctionsBeingMade = true;
			}
			
			if (!correctionsBeingMade) {
				break;
			}
			
			yield return null;	
		}
		
		_lensDistortion.enabled.Override(false);
	}
}
