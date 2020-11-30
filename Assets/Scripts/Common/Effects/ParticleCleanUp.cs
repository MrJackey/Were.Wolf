using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCleanUp : MonoBehaviour {
	private new ParticleSystem particleSystem;

	private void Start() {
		particleSystem = GetComponent<ParticleSystem>();
		Destroy(gameObject, particleSystem.main.duration);
	}
}
