using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (PlayerController))]
public class PlayerInput : MonoBehaviour {

	public float moveSpeed = 6f;
	public float smoothX = 0.2f;
	public float gravity = -20f;
	public float jumpVelocity = 8f;

	[HideInInspector]
	public Vector3 platformVelocity;

	Vector3 velocity;
	float currentVelocityX;

	bool doubleJumped = false;

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

		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

		if (Input.GetButtonDown ("Jump")) {
			if (controller.collisions.below.isColliding) {
				velocity.y = jumpVelocity * Obstacle.GetJumpVelocityFactor (controller.collisions.below.obstacleType);
			} else if (input.x > 0 && controller.collisions.left.isColliding) {
				velocity.x = 15;
				velocity.y = 20;
			} else if (input.x < 0 && controller.collisions.right.isColliding) {
				velocity.x = -15;
				velocity.y = 20;
			} else if (!doubleJumped) {
				velocity.y = jumpVelocity;
				doubleJumped = true;
			}
		}

		float targetVelocityX = input.x * moveSpeed;

		targetVelocityX += platformVelocity.x;

		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref currentVelocityX, (controller.collisions.below.isColliding)?smoothX:2f*smoothX);
		velocity.y += gravity * Time.deltaTime + platformVelocity.y;
		controller.Move (Time.deltaTime * velocity);
	}
}
