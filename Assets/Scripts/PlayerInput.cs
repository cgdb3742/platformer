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

	[HideInInspector]
	public Vector3 platformVelocity;

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
		if (controller.collisions.above.isColliding || controller.collisions.below.isColliding) {
			velocity.y = 0f;
		}

		if (controller.collisions.below.isColliding  && doubleJumped) {
			doubleJumped = false;
		}

		if ((controller.collisions.above.isColliding || controller.collisions.below.isColliding || controller.collisions.left.isColliding || controller.collisions.right.isColliding) && dashed) {
			dashed = false;
			dashing = false;
			currentDashDuration = 0.0f;
		}

		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

		bool clingLeftWall = false;
		bool clingRightWall = false;

		if (Input.GetButton ("Cling")) {
			if (controller.collisions.left.isColliding) {
				clingLeftWall = true;
			} else if (controller.collisions.right.isColliding) {
				clingRightWall = true;
			}
		}

		if (Input.GetButtonDown ("Jump")) {
			if (controller.collisions.below.isColliding) {
				velocity.y = jumpVelocity * Obstacle.GetJumpVelocityFactor (controller.collisions.below.obstacleType);
			} else if ((input.x > 0 || clingLeftWall) && controller.collisions.left.isColliding) {
				velocity.x = wallJumpImpulseX;
				velocity.y = wallJumpImpulseY;
			} else if ((input.x < 0 || clingRightWall) && controller.collisions.right.isColliding) {
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
				velocity.x = platformVelocity.x + (clingLeftWall ? -0.5f : 0.5f);
				velocity.y = platformVelocity.y;
			} else {
				float targetVelocityX = input.x * moveSpeed;

				targetVelocityX += platformVelocity.x;

				velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref currentVelocityX, (controller.collisions.below.isColliding) ? smoothX : 2f * smoothX);
				velocity.y += gravity * Time.deltaTime + platformVelocity.y;
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

