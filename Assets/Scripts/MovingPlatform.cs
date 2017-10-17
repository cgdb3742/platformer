using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour {

	public Transform[] waypoints;
	public float waitTime;
	public float smoothing;
	Vector3 currentVelocity;
	int currentWaypoint;
	bool readyToGo;

	Vector3 lastPosition;

	void Start ()
	{
		currentWaypoint = 0;
		readyToGo = true;
	}

	void Update ()
	{
		if (waypoints.Length > 0) {
			if (readyToGo) {
				readyToGo = false;
				currentWaypoint = (currentWaypoint == waypoints.Length - 1) ? 0 : currentWaypoint + 1;
				StartCoroutine ("GoToPosition", waypoints [currentWaypoint].position);
			}
		}
	}

	IEnumerator GoToPosition (Vector3 newPosition)
	{
		while (Vector3.Distance (transform.position, newPosition) > 0.1f) {
			transform.position = Vector3.SmoothDamp (transform.position, newPosition, ref currentVelocity, smoothing);
			lastPosition = transform.position;
			yield return null;
		}
		yield return new WaitForSeconds (waitTime);
		readyToGo = true;
	}

	void OnCollisionEnter (Collision collisionInfo)
	{
		PlayerInput player = collisionInfo.gameObject.GetComponent<PlayerInput> ();
		if (player != null) {
			player.platformVelocity = 1f / Time.deltaTime * (lastPosition - transform.position);
		}
	}
}
