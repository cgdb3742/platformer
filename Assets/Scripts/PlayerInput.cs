using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (PlayerController))]
public class PlayerInput : MonoBehaviour {

	public float moveSpeed = 6f;
	public float smoothX = 0.2f;
	public float gravity = -20f;
	public float jumpVelocity = 8f;

	public float wallJumpImpulseX = 15.0f;
	public float wallJumpImpulseY = 20.0f;

	public int dashDirections = 8;

	public float damageImpulse = 5.0f;

	[HideInInspector]
	//public Vector3 platformVelocity;

	Vector3 velocity;
	float currentVelocityX;

	bool doubleJumped = false;

	public float dashSpeed = 40.0f;
	public float dashDuration = 0.1f;
	float currentDashDuration = 0.0f;

	Vector2 dashDirection = Vector2.zero;
	bool dashing = false;
	bool dashed = false;

	PlayerController controller;

	void Start ()
	{
		controller = GetComponent<PlayerController> ();
	}

	void Update ()
	{
		if (controller.collisions.damagedLeft || controller.collisions.damagedRight || controller.collisions.damagedAbove || controller.collisions.damagedBelow) {
			Vector2 damageDir = Vector2.zero;

			if (controller.collisions.damagedLeft) {
				damageDir.x += 1.0f;
			}

			if (controller.collisions.damagedRight) {
				damageDir.x -= 1.0f;
			}

			if (controller.collisions.damagedBelow) {
				damageDir.y += 1.0f;
			}

			if (controller.collisions.damagedAbove) {
				damageDir.y -= 1.0f;
			}

			if (damageDir != Vector2.zero) {
				damageDir *= damageImpulse / damageDir.magnitude;
			}

			velocity = damageDir;
			controller.Move (Time.deltaTime * velocity);

			return;
		}

		if (controller.collisions.blockedAbove || controller.collisions.blockedBelow) {
			velocity.y = 0f;
		}

		if (controller.collisions.blockedBelow  && doubleJumped) {
			doubleJumped = false;
		}

		if ((controller.collisions.blockedAbove || controller.collisions.blockedBelow || controller.collisions.blockedLeft || controller.collisions.blockedRight) && dashed) {
			dashed = false;
			dashing = false;
			currentDashDuration = 0.0f;
		}

		float horizontalInput = Mathf.Clamp (Input.GetAxisRaw ("Horizontal"), -1.0f, 1.0f);
		float verticalInput = Mathf.Clamp (Input.GetAxisRaw ("Vertical"), -1.0f, 1.0f);

		Vector2 input = new Vector2 (horizontalInput, verticalInput);

		bool clingLeftWall = false;
		bool clingRightWall = false;

		if (Input.GetButton ("Cling")) {
			if (controller.collisions.blockedLeft) {
				clingLeftWall = true;
			} else if (controller.collisions.blockedRight) {
				clingRightWall = true;
			}
		}

		if (Input.GetButtonDown ("Jump")) {
			if (controller.collisions.blockedBelow) {
				if (input.y < 0 && controller.collisions.below.obstacle.obstacleType == Obstacle.ObstacleType.TraversablePlatform) {
					controller.collisions.below.obstacle.Disable (0.5f);
					doubleJumped = true;
				} else {
					velocity.y = jumpVelocity * Obstacle.GetJumpVelocityFactor (controller.collisions.below.obstacle.obstacleType);
				}
			} else if ((input.x > 0 || clingLeftWall) && controller.collisions.blockedLeft) {
				velocity.x = wallJumpImpulseX;
				velocity.y = wallJumpImpulseY;
			} else if ((input.x < 0 || clingRightWall) && controller.collisions.blockedRight) {
				velocity.x = -wallJumpImpulseX;
				velocity.y = wallJumpImpulseY;
			} else if (!doubleJumped) {
				velocity.y = jumpVelocity;
				doubleJumped = true;
			}

			dashing = false;
			currentDashDuration = 0.0f;
			clingLeftWall = false;
			clingRightWall = false;
		}

		if (Input.GetButtonDown ("Dash") && !dashed) {
			Vector2 dashDir = GetDashDirection (input);

			if (dashDir != Vector2.zero) {
				dashed = true;
				dashing = true;

				dashDirection = dashDir;
				currentDashDuration = 0.0f;
			}

			clingLeftWall = false;
			clingRightWall = false;
		}

		if (dashing) {
			float frameDashDuration = Mathf.Min (Time.deltaTime, Mathf.Max (0.0f, dashDuration - currentDashDuration));

			//Debug.Log (frameDashDuration);

			velocity = dashDirection * dashSpeed;

			controller.Move (frameDashDuration * velocity);

			currentDashDuration += Time.deltaTime;

			if (currentDashDuration >= dashDuration) {
				dashing = false;
				currentDashDuration = 0.0f;
				//velocity = Vector3.zero;
			}
		} else {
			if (clingLeftWall || clingRightWall) {
				velocity = Vector3.zero;
				//velocity.x = platformVelocity.x + (clingLeftWall ? -0.5f : 0.5f);
				//velocity.y = platformVelocity.y;
				velocity.x = clingLeftWall ? -0.5f : 0.5f;
			} else {
				float targetVelocityX = input.x * moveSpeed;

				//targetVelocityX += platformVelocity.x;

				velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref currentVelocityX, (controller.collisions.blockedBelow) ? smoothX : 2f * smoothX);
				//velocity.y += gravity * Time.deltaTime + platformVelocity.y;
				velocity.y += gravity * Time.deltaTime;
			}

			controller.Move (Time.deltaTime * velocity);
		}
	}

	Vector2 GetDashDirection(Vector2 input) {
		if (input == Vector2.zero) {
			return Vector2.zero;
		}

		if (dashDirections <= 0) {
			return input.normalized;
		}

		float inputAngle = Mathf.Atan2 (input.y, input.x);

		float sectorAngle = 2.0f * Mathf.PI / dashDirections;

		float dashAngle = Mathf.Floor (Mathf.Repeat ((inputAngle + sectorAngle / 2.0f), 2.0f * Mathf.PI) / sectorAngle) * sectorAngle;

		//Debug.Log (input + " " + dashAngle * Mathf.Rad2Deg);

		return new Vector2 (Mathf.Cos (dashAngle), Mathf.Sin (dashAngle));
	}
}

