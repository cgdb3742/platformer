using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public float smoothing = 0.1f;
	public Vector3 offset;
	Vector3 currentVelocity;
	Transform target;

	void Start ()
	{
		target = GameObject.FindGameObjectWithTag ("Player").transform;
	}

	void Update ()
	{
		Vector3 targetPosition = new Vector3 (target.position.x, target.position.y, transform.position.z) + offset;
		transform.position = Vector3.SmoothDamp (transform.position, targetPosition, ref currentVelocity, smoothing);
	}
}
