using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (BoxCollider))]
public class PlayerController : MonoBehaviour {

	protected LifeControl lifeControl = null;

	public LayerMask collisionMask;

	const float raycastOriginOffset = 0.15f;

	public bool checkAllDir = true;

	public Color neutralColor;
	public Color damagedColor;

	public float invulnerabilityTimer = 1.0f;
	public float currentInvulnerabilityTimer = 0.0f;

	public int horizontalRayCount;
	public int verticalRayCount;

	float horizontalRaySpacing;
	float verticalRaySpacing;

	BoxCollider boxCollider;
	RaycastOrigins raycastOrigins;
	public Collisions collisions;

	public Vector3 platformVelocity = Vector3.zero;

	void Start ()
	{
		lifeControl = GameObject.Find ("LifeBase").GetComponent<LifeControl> ();

		GetComponent<MeshRenderer> ().material.color = neutralColor;

		boxCollider = GetComponent<BoxCollider> ();

		CalculateRaySpacing ();
	}

	void Update()
	{
		if (currentInvulnerabilityTimer > 0.0f) {
			currentInvulnerabilityTimer -= Time.deltaTime;

			if (currentInvulnerabilityTimer <= 0.0f) {
				currentInvulnerabilityTimer = 0.0f;

				GetComponent<MeshRenderer> ().material.color = neutralColor;
			}
		}
	}

	void GetDamaged()
	{
		currentInvulnerabilityTimer = invulnerabilityTimer;

		GetComponent<MeshRenderer> ().material.color = damagedColor;

		lifeControl.LoseLife (1);
	}

	public void Move (Vector3 velocity)
	{
		CalculateRayOrigins ();
		collisions.Reset ();

		if (checkAllDir) {
			bool movingAbove = (velocity.y > 0);
			bool movingBelow = (velocity.y < 0);
			bool movingLeft = (velocity.x < 0);
			bool movingRight = (velocity.x > 0);

			LeftCollisions (ref velocity, movingLeft);
			RightCollisions (ref velocity, movingRight);
			AboveCollisions (ref velocity, movingAbove);
			BelowCollisions (ref velocity, movingBelow);
		} else {
			if (velocity.x != 0) {
				HorizontalCollisions (ref velocity);
			}
			if (velocity.y != 0) {
				VerticalCollisions (ref velocity);
			}
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

		bool damaged = false;

		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += (i * horizontalRaySpacing + velocity.y) * Vector2.up;						// + velocity.y pour prévoir la collision
			RaycastHit hitInfo;
			bool hit = Physics.Raycast (rayOrigin, directionX * Vector3.right, out hitInfo, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, rayLength * directionX * Vector3.right, Color.red);

			if (hit) {
				if (directionX == -1) {
					collisions.left.obstacleType = hitInfo.collider.gameObject.GetComponent<Obstacle> ().obstacleType;
					if (collisions.left.obstacleType == Obstacle.ObstacleType.TraversablePlatform) {
						return;
					} else if (collisions.left.obstacleType == Obstacle.ObstacleType.Hot && !damaged && currentInvulnerabilityTimer <= 0.0f) {
						GetDamaged ();
						damaged = true;
						collisions.damagedLeft = true;
					}

					collisions.left.isColliding = true;
					collisions.blockedLeft = true;

				}
				if (directionX == 1) {
					collisions.right.obstacleType = hitInfo.collider.gameObject.GetComponent<Obstacle> ().obstacleType;
					if (collisions.right.obstacleType == Obstacle.ObstacleType.TraversablePlatform) {
						return;
					} else if (collisions.right.obstacleType == Obstacle.ObstacleType.Hot && !damaged && currentInvulnerabilityTimer <= 0.0f) {
						GetDamaged ();
						damaged = true;
						collisions.damagedRight = true;
					}

					collisions.right.isColliding = true;
					collisions.blockedRight = true;
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

		bool damaged = false;

		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += (i * verticalRaySpacing + velocity.x) * Vector2.right;						// + velocity.x pour prévoir la collision
			RaycastHit hitInfo;
			bool hit = Physics.Raycast (rayOrigin, directionY * Vector3.up, out hitInfo, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, rayLength * directionY * Vector3.up, Color.red);

			if (hit) {
				if (directionY == -1) {
					collisions.below.obstacleType = hitInfo.collider.gameObject.GetComponent<Obstacle> ().obstacleType;

					if (collisions.below.obstacleType == Obstacle.ObstacleType.Hot && !damaged && currentInvulnerabilityTimer <= 0.0f) {
						GetDamaged ();
						damaged = true;
						collisions.damagedBelow = true;
					}

					collisions.below.isColliding = true;
					collisions.blockedBelow = true;
				}
				if (directionY == 1) {
					collisions.above.obstacleType = hitInfo.collider.gameObject.GetComponent<Obstacle> ().obstacleType;
					if (collisions.above.obstacleType == Obstacle.ObstacleType.TraversablePlatform) {
						return;
					} else if (collisions.above.obstacleType == Obstacle.ObstacleType.Hot && !damaged && currentInvulnerabilityTimer <= 0.0f) {
						GetDamaged ();
						damaged = true;
						collisions.damagedAbove = true;
					}

					collisions.above.isColliding = true;
					collisions.blockedAbove = true;
				}

				velocity.y = (hitInfo.distance - raycastOriginOffset) * directionY;
				rayLength = hitInfo.distance;										// pour ne pas détecter de collisions plus loin que l'obstacle
			}
		}
	}

	void AboveCollisions (ref Vector3 velocity, bool movingAbove) {
		float rayLength = Mathf.Max(0.0f, velocity.y) + raycastOriginOffset;

		bool damaged = false;

		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = raycastOrigins.topLeft;
			rayOrigin += (i * verticalRaySpacing + velocity.x) * Vector2.right;						// + velocity.x pour prévoir la collision
			RaycastHit hitInfo;
			bool hit = Physics.Raycast (rayOrigin, Vector3.up, out hitInfo, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, rayLength * Vector3.up, Color.red);

			if (hit) {
					collisions.above.obstacleType = hitInfo.collider.gameObject.GetComponent<Obstacle> ().obstacleType;
				if (collisions.above.obstacleType == Obstacle.ObstacleType.TraversablePlatform) {
					return;
				} else if (collisions.above.obstacleType == Obstacle.ObstacleType.Hot && !damaged && currentInvulnerabilityTimer <= 0.0f) {
					GetDamaged ();
					damaged = true;
					collisions.damagedAbove = true;
				}

				collisions.above.isColliding = true;

				if (movingAbove) {
					collisions.blockedAbove = true;
					velocity.y = (hitInfo.distance - raycastOriginOffset);
				}

				rayLength = hitInfo.distance;										// pour ne pas détecter de collisions plus loin que l'obstacle
			}
		}
	}

	void BelowCollisions (ref Vector3 velocity, bool movingBelow) {
		float rayLength = Mathf.Max(0.0f, -velocity.y) + raycastOriginOffset;

		bool damaged = false;

		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = raycastOrigins.bottomLeft;
			rayOrigin += (i * verticalRaySpacing + velocity.x) * Vector2.right;						// + velocity.x pour prévoir la collision
			RaycastHit hitInfo;
			bool hit = Physics.Raycast (rayOrigin, Vector3.down, out hitInfo, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, rayLength * Vector3.down, Color.red);

			if (hit) {
				collisions.below.obstacleType = hitInfo.collider.gameObject.GetComponent<Obstacle> ().obstacleType;
				if (collisions.below.obstacleType == Obstacle.ObstacleType.Hot && !damaged && currentInvulnerabilityTimer <= 0.0f) {
					GetDamaged ();
					damaged = true;
					collisions.damagedBelow = true;
				}

				collisions.below.isColliding = true;

				if (movingBelow) {
					collisions.blockedBelow = true;
					velocity.y = -(hitInfo.distance - raycastOriginOffset);
				}

				rayLength = hitInfo.distance;										// pour ne pas détecter de collisions plus loin que l'obstacle
			}
		}
	}

	void LeftCollisions (ref Vector3 velocity, bool movingLeft) {
		float rayLength = Mathf.Max(0.0f, -velocity.x) + raycastOriginOffset;

		bool damaged = false;

		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = raycastOrigins.topLeft;
			rayOrigin += (i * horizontalRaySpacing + velocity.y) * Vector2.down;						// + velocity.y pour prévoir la collision
			RaycastHit hitInfo;
			bool hit = Physics.Raycast (rayOrigin, Vector3.left, out hitInfo, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, rayLength * Vector3.left, Color.red);

			if (hit) {
				collisions.left.obstacleType = hitInfo.collider.gameObject.GetComponent<Obstacle> ().obstacleType;
				if (collisions.left.obstacleType == Obstacle.ObstacleType.TraversablePlatform) {
					return;
				} else if (collisions.left.obstacleType == Obstacle.ObstacleType.Hot && !damaged && currentInvulnerabilityTimer <= 0.0f) {
					GetDamaged ();
					damaged = true;
					collisions.damagedLeft = true;
				}

				collisions.left.isColliding = true;

				if (movingLeft) {
					collisions.blockedLeft = true;
					velocity.x = -(hitInfo.distance - raycastOriginOffset);
				}

				rayLength = hitInfo.distance;										// pour ne pas détecter de collisions plus loin que l'obstacle
			}
		}
	}

	void RightCollisions (ref Vector3 velocity, bool movingRight) {
		float rayLength = Mathf.Max(0.0f, velocity.x) + raycastOriginOffset;

		bool damaged = false;

		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = raycastOrigins.topRight;
			rayOrigin += (i * horizontalRaySpacing + velocity.y) * Vector2.down;						// + velocity.y pour prévoir la collision
			RaycastHit hitInfo;
			bool hit = Physics.Raycast (rayOrigin, Vector3.right, out hitInfo, rayLength, collisionMask);

			Debug.DrawRay (rayOrigin, rayLength * Vector3.right, Color.red);

			if (hit) {
				collisions.right.obstacleType = hitInfo.collider.gameObject.GetComponent<Obstacle> ().obstacleType;
				if (collisions.right.obstacleType == Obstacle.ObstacleType.TraversablePlatform) {
					return;
				} else if (collisions.right.obstacleType == Obstacle.ObstacleType.Hot && !damaged && currentInvulnerabilityTimer <= 0.0f) {
					GetDamaged ();
					damaged = true;
					collisions.damagedRight = true;
				}

				collisions.right.isColliding = true;

				if (movingRight) {
					collisions.blockedRight = true;
					velocity.x = (hitInfo.distance - raycastOriginOffset);
				}

				rayLength = hitInfo.distance;										// pour ne pas détecter de collisions plus loin que l'obstacle
			}
		}
	}

	struct RaycastOrigins {
		public Vector2 bottomLeft, bottomRight, topLeft, topRight;
	}

	public struct Collisions {
		public CollisionInfo above, below, left, right;
		public bool damagedAbove, damagedBelow, damagedLeft, damagedRight;
		public bool blockedAbove, blockedBelow, blockedLeft, blockedRight;

		public void Reset ()
		{
			above.isColliding = false;
			below.isColliding = false;
			left.isColliding = false;
			right.isColliding = false;

			damagedAbove = false;
			damagedBelow = false;
			damagedLeft = false;
			damagedRight = false;

			blockedAbove = false;
			blockedBelow = false;
			blockedLeft = false;
			blockedRight = false;
		}
	}

	public struct CollisionInfo {
		public bool isColliding;
		public Obstacle.ObstacleType obstacleType;
	}
}
