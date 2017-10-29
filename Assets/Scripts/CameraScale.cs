using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScale : MonoBehaviour {

	public float currentResScale = 16.0f / 9.0f;
	public float maxResScale = 16.0f / 9.0f;

	// Use this for initialization
	void Start () {
		float newResScale = Mathf.Min (maxResScale, (float)Screen.width / (float)Screen.height);
		if (currentResScale != newResScale) {
			GetComponent<Camera> ().orthographicSize *= currentResScale / newResScale;
			currentResScale = newResScale;
		}
	}

	void Update() {
		float newResScale = Mathf.Min (maxResScale, (float)Screen.width / (float)Screen.height);
		if (currentResScale != newResScale) {
			GetComponent<Camera> ().orthographicSize *= currentResScale / newResScale;
			currentResScale = newResScale;
		}
	}
}
