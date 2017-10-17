using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {

	public enum ObstacleType {
		Normal = 0,
		Bouncy = 1,
		TraversablePlatform = 2
	}

	public ObstacleType obstacleType = ObstacleType.Normal;

	static float[] jumpVelocityFactors;

	void Start ()
	{
		jumpVelocityFactors = new float [] {1f, 2f, 1f};
	}

	public static float GetJumpVelocityFactor (ObstacleType type)
	{
		return jumpVelocityFactors [(int)type];
	}

	public void Disable (float t)
	{
		StartCoroutine ("DisableForSeconds", t);
	}

	IEnumerator DisableForSeconds (float t)
	{
		this.enabled = false;
		yield return new WaitForSeconds (t);
		this.enabled = true;
	}
}
