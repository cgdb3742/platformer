using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fan : MonoBehaviour {

	public float fanPower = 50.0f;
	public float fanAngle = 90.0f;
	public float fanMaxDistance = Mathf.Infinity;

	protected Vector3 fanDirection = Vector3.zero;

	public LayerMask collisionMask;
	public int rayCount = 15;
	protected BoxCollider boxCollider;

	protected Vector2 raycastStart;
	protected Vector2 raycastEnd;

	protected virtual void Start ()
	{
		boxCollider = GetComponent<BoxCollider> ();

		fanAngle = Mathf.Repeat (fanAngle, 360.0f);
		fanDirection = new Vector3 (Mathf.Cos (fanAngle * Mathf.Deg2Rad), Mathf.Sin (fanAngle * Mathf.Deg2Rad), 0.0f);
	}

	protected virtual void Update() {
		CalculateRayOrigins ();
		Collisions ();
	}

	protected void CalculateRayOrigins ()
	{
		Bounds bounds = boxCollider.bounds;
		bounds.Expand (0.15f * -2);

		if (fanAngle > 45.0f && fanAngle <= 135.0f) { //Up
			raycastStart = new Vector2 (bounds.min.x, bounds.min.y);
			raycastEnd = new Vector2 (bounds.max.x, bounds.min.y);
		} else if (fanAngle > 135.0f && fanAngle <= 225.0f) { //Left
			raycastStart = new Vector2 (bounds.max.x, bounds.max.y);
			raycastEnd = new Vector2 (bounds.max.x, bounds.min.y);
		} else if (fanAngle > 225.0f && fanAngle <= 315.0f) { //Down
			raycastStart = new Vector2 (bounds.min.x, bounds.max.y);
			raycastEnd = new Vector2 (bounds.max.x, bounds.max.y);
		} else { //Right
			raycastStart = new Vector2 (bounds.min.x, bounds.max.y);
			raycastEnd = new Vector2 (bounds.min.x, bounds.min.y);
		}
	}

	void Collisions() {
		List<PlayerInput> moved = new List<PlayerInput> ();

		for (int i = 0; i < rayCount; i++) {
			Vector2 rayOrigin = raycastStart + (raycastEnd - raycastStart) * i / (rayCount - 1);
			RaycastHit hitInfo;
			bool hit = Physics.Raycast (rayOrigin, fanDirection, out hitInfo, fanMaxDistance, collisionMask);

			Debug.DrawRay (rayOrigin, 0.15f * fanDirection, Color.red);

			if (hit) {
				PlayerInput input = hitInfo.collider.GetComponent<PlayerInput> ();

				if (input != null) {
					if (!moved.Contains (input)) {
						input.velocity += fanPower * Time.deltaTime * fanDirection;
						moved.Add (input);
					}
				}
			}
		}
	}
}
