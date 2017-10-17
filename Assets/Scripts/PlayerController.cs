using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (BoxCollider))]
public class PlayerController : MonoBehaviour {

	public LayerMask collisionMask;

	const float raycastOriginOffset = 0.15f;

	public int horizontalRayCount;
	public int verticalRayCount;

	float horizontalRaySpacing;
	float verticalRaySpacing;

	BoxCollider boxCollider;
	RaycastOrigins raycastOrigins;
	public Collisions collisions;

	void Start ()
	{
		boxCollider = GetComponent<BoxCollider> ();

		CalculateRaySpacing ();
	}

	public void Move (Vector3 velocity)
	{
		CalculateRayOrigins ();
		collisions.Reset ();

		if (velocity.x != 0) {
			HorizontalCollisions (ref velocity);
		}
		if (velocity.y != 0) {
			VerticalCollisions (ref velocity);
		}

		transform.Translate (velocity);
	}

	void CalculateRayOrigins ()
	{
		Bounds bounds = boxCollider.bounds;
		bounds.Expand (0.15f * -2);

		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}

	void CalculateRaySpacing ()
	{
		Bounds bounds = boxCollider.bounds;
		bounds.Expand (raycastOriginOffset * -2);

		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);

		horizontalRaySpacing = bounds.size.x / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.y / (verticalRayCount - 1);
	}

	void HorizontalCollisions (ref Vector3 velocity)
	{
		float directionX = Mathf.Sign (velocity.x);
		float rayLength = Mathf.Abs (velocity.x) + raycastOriginOffset;

		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += (i * horizontalRaySpacing + velocity.y) * Vector2.up;						// + velocity.y pour prévoir la collision
			RaycastHit hitInfo;
			bool hit = Physics.Raycast (rayOrigin, directionX * Vector3.right, out hitInfo, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, rayLength * directionX * Vector3.right, Color.red);

			if (hit) {
				if (directionX == -1) {
					collisions.left.obstacle = hitInfo.collider.gameObject.GetComponent<Obstacle> ();
					if (collisions.left.obstacle.obstacleType == Obstacle.ObstacleType.TraversablePlatform) {
						return;
					}
					collisions.left.isColliding = true;
				}
				if (directionX == 1) {
					collisions.right.obstacle = hitInfo.collider.gameObject.GetComponent<Obstacle> ();
					if (collisions.right.obstacle.obstacleType == Obstacle.ObstacleType.TraversablePlatform) {
						return;
					}
					collisions.right.isColliding = true;
				}

				velocity.x = (hitInfo.distance - raycastOriginOffset) * directionX;
				rayLength = hitInfo.distance;										// pour ne pas détecter de collisions plus loin que l'obstacle
			}
		}
	}

	void VerticalCollisions (ref Vector3 velocity)
	{
		float directionY = Mathf.Sign (velocity.y);
		float rayLength = Mathf.Abs (velocity.y) + raycastOriginOffset;

		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += (i * verticalRaySpacing + velocity.x) * Vector2.right;						// + velocity.x pour prévoir la collision
			RaycastHit hitInfo;
			bool hit = Physics.Raycast (rayOrigin, directionY * Vector3.up, out hitInfo, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, rayLength * directionY * Vector3.up, Color.red);

			if (hit) {
				if (directionY == -1) {
					collisions.below.obstacle = hitInfo.collider.gameObject.GetComponent<Obstacle> ();
					if (!collisions.below.obstacle.enabled) {
						return;
					}
					collisions.below.isColliding = true;
				}
				if (directionY == 1) {
					collisions.above.obstacle = hitInfo.collider.gameObject.GetComponent<Obstacle> ();
					if (collisions.above.obstacle.obstacleType == Obstacle.ObstacleType.TraversablePlatform) {
						return;
					}
					collisions.above.isColliding= true;
				}

				velocity.y = (hitInfo.distance - raycastOriginOffset) * directionY;
				rayLength = hitInfo.distance;										// pour ne pas détecter de collisions plus loin que l'obstacle
			}
		}
	}

	struct RaycastOrigins {
		public Vector2 bottomLeft, bottomRight, topLeft, topRight;
	}

	public struct Collisions {
		public CollisionInfo above, below, left, right;

		public void Reset ()
		{
			above.isColliding = false;
			below.isColliding = false;
			left.isColliding = false;
			right.isColliding = false;
		}
	}

	public struct CollisionInfo {
		public bool isColliding;
		public Obstacle obstacle;
	}
}
