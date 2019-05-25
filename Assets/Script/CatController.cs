using UnityEngine;
using System.Collections;

public class CatController : MonoBehaviour {

	// cat instance
	public GameObject[] CatPrefabs;
	// cat wow audio
	public AudioClip CatWowAudio;
	// add cat audio
	public AudioClip AddCatAudio;
	// cat leave audio
	public AudioClip CatLeaveAudio;
	// 掉落金币音效
	public AudioClip CoinAudio;

	// Use this for initialization
	void Start () {
		// hide all 
		foreach (var cat in CatPrefabs) 
		{
			cat.GetComponent<Renderer>().enabled = false;
		}
	}

	// miss will hide all cat
	public void HideAllCat(bool playAudio = true) 
	{
		bool enableOne = false;
		foreach (var cat in CatPrefabs) 
		{
			if (cat.GetComponent<Renderer>().enabled) 
			{
				enableOne = true;
				cat.GetComponent<CatScript>().WalkOut();
			}
		}
		// Play leave audio
		if (enableOne && playAudio) 
		{
			GetComponent<AudioSource> ().clip = CatLeaveAudio;
			GetComponent<AudioSource> ().Play ();
		}
	}

	// add a random new cat, if all in show do nothing
	public void AddCat() 
	{
		bool existHide = false; // if exist hide cat
		foreach (var cat in CatPrefabs) 
		{
			if (cat.GetComponent<Renderer>().enabled == false) 
			{
				existHide = true;
				break;
			}
		}
		if (!existHide) // no hide, do nothing
		{
			return;
		}
		int randomPos = 0; // find a hide cat
		do 
		{
			randomPos = Random.Range(0, CatPrefabs.Length);
		} while (CatPrefabs[randomPos].GetComponent<Renderer>().enabled == true);
		// show and walk
		var hideCat = CatPrefabs [randomPos];
		hideCat.GetComponent<CatScript>().WalkIn();
		// play add audio
		AudioSource.PlayClipAtPoint (AddCatAudio, new Vector2 (0, 0), 0.75f);
	}

	// make all cat a wow
	public void MakeAllWow() 
	{
		bool existGold = false;
		bool existCat = false;
		int goldNum = 0;
		int wowNum = 0;
		foreach (var cat in CatPrefabs) 
		{
			if (cat.GetComponent<Renderer>().enabled) // all enabled cat wow
			{
				existCat = true;
				if (Random.value > 0.1) 
				{
					cat.GetComponent<CatScript>().MakeWOW();
					wowNum++;
				}
				else 
				{
					cat.GetComponent<CatScript>().MakeGold();
					existGold = true;
					goldNum++;
				}
			}
		}
		if (wowNum > 0)
		{
			GetComponent<SceneController> ().WowScore += wowNum; // add score in scenecontroller
			GetComponent<AchievementController> ().AddWowNum (wowNum); // achievement
		}
		if (goldNum > 0)
		{
			GetComponent<GoldController> ().Gold += goldNum; // add score in scenecontroller
			GetComponent<AchievementController> ().AddGoldNum (goldNum);
		}

		// Play wow audio
		if (existCat)
		{
			GetComponent<AudioSource> ().clip = CatWowAudio;
			GetComponent<AudioSource> ().Play ();
		}
		// Play coin audio
		if (existGold) 
		{
			AudioSource.PlayClipAtPoint(CoinAudio, new Vector2(0, 0), 0.75f);
		}
	}

}
