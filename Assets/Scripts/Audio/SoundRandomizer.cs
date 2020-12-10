using UnityEngine;

public class SoundRandomizer : MonoBehaviour {
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private AudioClip[] sounds;

	public AudioSource Source => audioSource;
	public AudioClip CurrentClip { get; private set; }

	public AudioClip[] Sounds => sounds;

	private void Awake() {
		if (audioSource == null)
			audioSource = GetComponent<AudioSource>();

		if (audioSource == null)
			Debug.LogError($"{nameof(SoundRandomizer)} is missing an AudioSource.");
	}

	[ContextMenu("PlayRandom")]
	public void PlayRandom() {
		CurrentClip = sounds[Random.Range(0, sounds.Length)];
		audioSource.clip = CurrentClip;
		audioSource.Play();
	}

	public void Stop() {
		audioSource.Stop();
	}
}