using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {

	public float hotMaterialTimer = 1.0f;
	protected float currentHotMaterialTimer = 0.0f;

	public enum ObstacleType {
		Normal = 0,
		Bouncy = 1,
		TraversablePlatform = 2,
		Hot = 3
	}

	public ObstacleType obstacleType = ObstacleType.Normal;

	static float[] jumpVelocityFactors;

	void Start ()
	{
		jumpVelocityFactors = new float [] { 1f, 2f, 1f, 1f};
	}

	void Update() {
		if (obstacleType == ObstacleType.Hot) {
			currentHotMaterialTimer = Mathf.Repeat (currentHotMaterialTimer + Time.deltaTime, 2.0f * hotMaterialTimer);

			GetComponent<MeshRenderer> ().material.color = new Color (1.0f, Mathf.PingPong (currentHotMaterialTimer, hotMaterialTimer), Mathf.PingPong (currentHotMaterialTimer, hotMaterialTimer));
		}
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
