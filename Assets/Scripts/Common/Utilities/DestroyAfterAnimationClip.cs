using System;
using UnityEngine;

public class DestroyAfterAnimationClip : MonoBehaviour {
	[SerializeField] private AnimationClip clip;

	private void Start() {
		Destroy(gameObject, clip.length);
	}
}