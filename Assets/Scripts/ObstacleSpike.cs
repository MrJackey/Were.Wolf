using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObstacleSpike : MonoBehaviour {
	public void OnCollisionEnter2D(Collision2D collision) {

		if (collision.gameObject.CompareTag("Player")) {
			Scene currentScene = SceneManager.GetActiveScene();
			SceneManager.LoadScene(currentScene.name);
		}
	}
}
