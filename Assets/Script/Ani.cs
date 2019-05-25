using UnityEngine;
using System.Collections;

public class Ani : MonoBehaviour {

	public void OnAniExit(string aniName) 
	{
		var achievementController = GameObject.Find ("SceneController").GetComponent<AchievementController>();	
		achievementController.StopAni (GameObject.Find ("Animation/" + aniName));
	}

}
