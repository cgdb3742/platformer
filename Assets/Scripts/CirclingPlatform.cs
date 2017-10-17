using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirclingPlatform : MovingPlatform {
	public Transform center;
	public float radialSpeed = 15.0f;
	protected float currentAngle;
	protected float dist;

	protected override void Start () {
		base.Start ();

		Vector3 dir = transform.position - center.position;
		currentAngle = Mathf.Atan2 (dir.y, dir.x);
		dist = dir.magnitude;
	}

	protected override void Update ()
	{
		objectsToMove = new List<Transform> ();
		CalculateRayOrigins ();
		VerticalCollisions ();
		HorizontalLeftCollisions ();
		HorizontalRightCollisions ();

		RotationMove ();
	}

	protected void RotationMove()
	{
		currentAngle = Mathf.Repeat (currentAngle + Time.deltaTime * radialSpeed * Mathf.Deg2Rad, 2.0f * Mathf.PI);

		lastPosition = transform.position;

		transform.position = center.position + dist * new Vector3 (Mathf.Cos (currentAngle), Mathf.Sin (currentAngle), 0.0f);

		foreach (Transform objectToMove in objectsToMove) {
			objectToMove.position += transform.position - lastPosition;
		}
	}
}
