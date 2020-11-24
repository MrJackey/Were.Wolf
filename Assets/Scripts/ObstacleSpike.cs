using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObstacleSpike : MonoBehaviour {
	public void OnCollisionEnter2D(Collision2D other) {
		if (other.rigidbody.CompareTag("Player")) {
			Scene currentScene = SceneManager.GetActiveScene();
			SceneManager.LoadScene(currentScene.buildIndex);
		}
	}
}
