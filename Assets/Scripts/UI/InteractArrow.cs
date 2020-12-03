using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractArrow : MonoBehaviour {
	[SerializeField] private float bobDistance = 0.5f, bobSpeed = 0.1f;
	private float bobStart, bobCurrent, bobEnd;
	private bool isIncreasing = false;

	private void Start() {
		Initialize();
	}

	private void Update() {
		if (bobCurrent <= bobStart)
			isIncreasing = true;
		else if (bobCurrent >= bobEnd)
			isIncreasing = false;

		if (isIncreasing) 
			bobCurrent += bobSpeed * Time.deltaTime;
		else
			bobCurrent -= bobSpeed * Time.deltaTime;

		transform.position = new Vector3(transform.position.x, bobCurrent, transform.position.z);
	}

	public void Initialize() {
		bobCurrent = transform.position.y;
		bobStart = transform.position.y;
		bobEnd = transform.position.y + bobDistance;
	}
}