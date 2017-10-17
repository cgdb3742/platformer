using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LifeControl : MonoBehaviour {
	public int maxLife = 8;
	int currentLife;

	void Start() {
		currentLife = maxLife;

		transform.Find ("LifeCurrent").GetComponent<Image> ().fillAmount = (float)currentLife / maxLife;
		transform.Find ("LifeCounter").GetComponent<Text> ().text = currentLife.ToString ();
	}

	public void LoseLife(int amount) {
		if (amount <= 0) {
			return;
		}

		currentLife = Mathf.Max (0, currentLife - amount);

		if (currentLife <= 0) {
			SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
		}

		transform.Find ("LifeCurrent").GetComponent<Image> ().fillAmount = (float)currentLife / maxLife;
		transform.Find ("LifeCounter").GetComponent<Text> ().text = currentLife.ToString ();
	}
}
