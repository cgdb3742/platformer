using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour {

	public LayerMask collisionMask;
	protected const float raycastOriginOffset = 0.15f;
	public int horizontalRayCount;
	public int verticalRayCount;
	protected float horizontalRaySpacing;
	protected float verticalRaySpacing;
	protected BoxCollider boxCollider;
	protected RaycastOrigins raycastOrigins;

	public Transform[] waypoints;
	public float waitTime;
	public float smoothing;
	protected Vector3 currentVelocity;
	protected int currentWaypoint;
	protected bool readyToGo;
	protected Vector3 lastPosition;
	protected List<Transform> objectsToMove;

	protected virtual void Start ()
	{
		boxCollider = GetComponent<BoxCollider> ();
		CalculateRaySpacing ();

		currentWaypoint = 0;
		readyToGo = true;
		objectsToMove = new List<Transform> ();
	}

	protected void CalculateRayOrigins ()
	{
		Bounds bounds = boxCollider.bounds;
		bounds.Expand (0.15f * -2);

		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
	}

	protected void CalculateRaySpacing ()
	{
		Bounds bounds = boxCollider.bounds;
		bounds.Expand (raycastOriginOffset * -2);

		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		horizontalRaySpacing = bounds.size.x / (horizontalRayCount - 1);
		verticalRayCount = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);
		verticalRaySpacing = bounds.size.y / (verticalRayCount - 1);
	}

	protected void VerticalCollisions ()
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
					PlayerController controller = hitInfo.collider.GetComponent<PlayerController> ();
					Obstacle obstacle = this.GetComponent<Obstacle> ();
					bool register = true;

					if (controller != null && obstacle != null) {
						if (controller.currentInvulnerabilityTimer <= 0.0f && obstacle.obstacleType == Obstacle.ObstacleType.Hot) {
							register = false;
						}
					}

					if (register) {
						objectsToMove.Add (hitInfo.collider.transform);
					}
				}
			}
		}
	}

	protected void HorizontalLeftCollisions ()
	{
		float rayLength = 0.1f + raycastOriginOffset;

		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = raycastOrigins.topLeft;
			rayOrigin += (i * verticalRaySpacing) * Vector2.down;
			RaycastHit hitInfo;
			bool hit = Physics.Raycast (rayOrigin, Vector3.left, out hitInfo, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, rayLength * Vector3.left, Color.red);

			if (hit) {
				if (!objectsToMove.Contains (hitInfo.collider.transform)) {
					PlayerController controller = hitInfo.collider.GetComponent<PlayerController> ();
					Obstacle obstacle = this.GetComponent<Obstacle> ();
					bool register = true;

					if (controller != null && obstacle != null) {
						if (controller.currentInvulnerabilityTimer <= 0.0f && obstacle.obstacleType == Obstacle.ObstacleType.Hot) {
							register = false;
						}
					}

					if (register) {
						objectsToMove.Add (hitInfo.collider.transform);
					}
				}
			}
		}
	}

	protected void HorizontalRightCollisions ()
	{
		float rayLength = 0.1f + raycastOriginOffset;

		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = raycastOrigins.topRight;
			rayOrigin += (i * verticalRaySpacing) * Vector2.down;
			RaycastHit hitInfo;
			bool hit = Physics.Raycast (rayOrigin, Vector3.right, out hitInfo, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, rayLength * Vector3.right, Color.red);

			if (hit) {
				if (!objectsToMove.Contains (hitInfo.collider.transform)) {
					PlayerController controller = hitInfo.collider.GetComponent<PlayerController> ();
					Obstacle obstacle = this.GetComponent<Obstacle> ();
					bool register = true;

					if (controller != null && obstacle != null) {
						if (controller.currentInvulnerabilityTimer <= 0.0f && obstacle.obstacleType == Obstacle.ObstacleType.Hot) {
							register = false;
						}
					}

					if (register) {
						objectsToMove.Add (hitInfo.collider.transform);
					}
				}
			}
		}
	}

	protected virtual void Update ()
	{
		objectsToMove = new List<Transform> ();
		CalculateRayOrigins ();
		VerticalCollisions ();
		HorizontalLeftCollisions ();
		HorizontalRightCollisions ();

		if (waypoints.Length > 0) {
			if (readyToGo) {
				readyToGo = false;
				currentWaypoint = (currentWaypoint == waypoints.Length - 1) ? 0 : currentWaypoint + 1;
				StartCoroutine ("GoToPosition", waypoints [currentWaypoint].position);
			}
		}
	}

	protected IEnumerator GoToPosition (Vector3 newPosition)
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

	protected struct RaycastOrigins {
		public Vector2 topLeft, topRight, bottomLeft, bottomRight;
	}
}
