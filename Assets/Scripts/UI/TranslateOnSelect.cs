using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class TranslateOnSelect : MonoBehaviour, ISelectHandler, IDeselectHandler {
	[SerializeField] private Vector2 distance;
	[SerializeField] private float duration;
	[SerializeField] private Transform target;

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
		Stop();
	}

	public void OnSelect(BaseEventData eventData) {
		if (isSelected || !selectable.interactable) return;
		Stop();
		StartCoroutine(CoSelect());
		isSelected = true;
	}

	public void OnDeselect(BaseEventData eventData) {
		if (!isSelected) return;
		Stop();
		StartCoroutine(CoDeselect());
	}

	private void Stop() {
		StopAllCoroutines();
		SetLocalPosition(Vector3.zero);
		isSelected = false;
	}

	private void SetLocalPosition(Vector3 position) {
		target.localPosition = initialLocalPosition + position;
	}

	private IEnumerator CoSelect() {
		for (float time = 0; time < duration; time += Time.deltaTime) {
			SetLocalPosition(distance * MathX.EaseOutQuad(0, 1, time / duration));
			yield return null;
		}
	}

	private IEnumerator CoDeselect() {
		for (float time = 0; time < duration; time += Time.deltaTime) {
			SetLocalPosition(distance * MathX.EaseInQuad(1, 0, time / duration));
			yield return null;
		}
	}

}