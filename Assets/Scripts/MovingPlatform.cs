using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour {

	public LayerMask collisionMask;
	const float raycastOriginOffset = 0.15f;
	public int horizontalRayCount;
	float horizontalRaySpacing;
	BoxCollider boxCollider;
	RaycastOrigins raycastOrigins;

	public Transform[] waypoints;
	public float waitTime;
	public float smoothing;
	Vector3 currentVelocity;
	int currentWaypoint;
	bool readyToGo;
	Vector3 lastPosition;
	List<Transform> objectsToMove;

	void Start ()
	{
		boxCollider = GetComponent<BoxCollider> ();
		CalculateRaySpacing ();

		currentWaypoint = 0;
		readyToGo = true;
		objectsToMove = new List<Transform> ();
	}

	void CalculateRayOrigins ()
	{
		Bounds bounds = boxCollider.bounds;
		bounds.Expand (0.15f * -2);

		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}

	void CalculateRaySpacing ()
	{
		Bounds bounds = boxCollider.bounds;
		bounds.Expand (raycastOriginOffset * -2);

		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		horizontalRaySpacing = bounds.size.x / (horizontalRayCount - 1);
	}

	void VerticalCollisions ()
	{
		float rayLength = 0.1f + raycastOriginOffset;

		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = raycastOrigins.topLeft;
			rayOrigin += (i * horizontalRaySpacing) * Vector2.right;
			RaycastHit hitInfo;
			bool hit = Physics.Raycast (rayOrigin, Vector3.up, out hitInfo, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, rayLength * Vector3.up, Color.red);

			if (hit) {
				if (!objectsToMove.Contains (hitInfo.collider.transform)) {
					objectsToMove.Add (hitInfo.collider.transform);
				}
			}
		}
	}

	void Update ()
	{
		objectsToMove = new List<Transform> ();
		CalculateRayOrigins ();
		VerticalCollisions ();

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
			foreach (Transform objectToMove in objectsToMove) {
				objectToMove.position += transform.position - lastPosition;
			}
			lastPosition = transform.position;
			yield return null;
		}
		yield return new WaitForSeconds (waitTime);
		readyToGo = true;
	}

	struct RaycastOrigins {
		public Vector2 topLeft, topRight;
	}
}
