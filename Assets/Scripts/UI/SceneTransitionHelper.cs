using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionHelper : MonoBehaviour {
	[SerializeField] private AnimationClip transitionClip;
	[SerializeField] private SceneHelper sceneHelper;

	private int sceneToLoad = -1;
	private AsyncOperation loadedScene;

	public int SceneToLoad { set => sceneToLoad = value; }

	private void Start() {
		if (sceneToLoad == -1) return;

		loadedScene = SceneManager.LoadSceneAsync(sceneToLoad);
		loadedScene.allowSceneActivation = false;

		StartCoroutine(CoDelay());
	}

	private IEnumerator CoDelay() {
		if (sceneToLoad == -1) yield break;

		yield return new WaitForSeconds(transitionClip.length);
		loadedScene.allowSceneActivation = true;
		Destroy(gameObject, 1f);
	}
}
