using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class TranslateOnSelect : MonoBehaviour, ISelectHandler, IDeselectHandler {
	[SerializeField] private Vector2 distance;
	[SerializeField] private float duration;
	[SerializeField] private Transform target;

	private float t;
	private bool isSelected;
	private Vector3 initialLocalPosition;
	private Selectable selectable;

	private void Awake() {
		selectable = GetComponent<Selectable>();

		if (target == null)
			target = transform;

		initialLocalPosition = target.localPosition;
	}

	private void OnDisable() {
		StopAllCoroutines();
		SetLocalPosition(Vector3.zero);
		t = 0;
		isSelected = false;
	}

	public void OnSelect(BaseEventData eventData) {
		if (isSelected || !selectable.interactable) return;
		StopAllCoroutines();
		StartCoroutine(CoSelect());
		isSelected = true;
	}

	public void OnDeselect(BaseEventData eventData) {
		if (!isSelected) return;
		StopAllCoroutines();
		StartCoroutine(CoDeselect());
		isSelected = false;
	}

	private void SetLocalPosition(Vector3 position) {
		target.localPosition = initialLocalPosition + position;
	}

	private IEnumerator CoSelect() {
		for (float time = t * duration; time < duration; time += Time.deltaTime) {
			t = time / duration;
			SetLocalPosition(distance * MathX.EaseOutQuad(0, 1, t));
			yield return null;
		}

		t = 1;
		SetLocalPosition(distance);
	}

	private IEnumerator CoDeselect() {
		for (float time = t * duration; time >= 0; time -= Time.deltaTime) {
			t = time / duration;
			SetLocalPosition(distance * MathX.EaseOutQuad(0, 1, t));
			yield return null;
		}

		t = 0;
		SetLocalPosition(Vector3.zero);
	}
}