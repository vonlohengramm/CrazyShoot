using UnityEngine;
using System.Collections;

public class HumanController : MonoBehaviour {
	
	private GameObject[] _humans; // 所有的人物
	// 人物的资源前缀
	private string[] _prefixs = new string[]{"Human/boy1_", "Human/boy2_", "Human/boy3_", 
		"Human/man1_", "Human/man2_", "Human/man3_", 
		"Human/women1_", "Human/women2_", "Human/women3_"}; 

	public AudioClip BangAudio;
	public AudioClip LeaveAudio;
	public AudioClip AddHumanAudio;
	public AudioClip HumanScoreAudio;
	public AudioClip CoinAudio;

	// Use this for initialization
	void Start () {
		// 初始化所有人物
		var controllers = GameObject.Find ("Human").GetComponentsInChildren<HumanScript> ();
		_humans = new GameObject[controllers.Length];
		int i = 0;
		foreach (var c in controllers)
		{
			_humans[i++] = c.gameObject;
		}
		// hide all 
		foreach (var human in _humans) 
		{
			human.GetComponent<Renderer>().enabled = false;
		}
	}

	// 把所有的人都走掉
	public void HideAllHuman(bool playAudio = true) 
	{
		bool enableOne = false;
		foreach (var human in _humans) 
		{
			if (human.GetComponent<Renderer>().enabled) 
			{
				enableOne = true;
				human.GetComponent<HumanScript>().WalkOut();
			}
		}
		// Play leave audio
		if (enableOne && playAudio) 
		{
			GetComponent<AudioSource> ().clip = LeaveAudio;
			GetComponent<AudioSource> ().Play ();
		}
	}

	// 随机增加一个人,所有人都显示无效
	public void AddHuman() 
	{
		bool existHide = false; // if exist hide cat
		foreach (var human in _humans) 
		{
			if (human.GetComponent<Renderer>().enabled == false) 
			{
				existHide = true;
				break;
			}
		}
		if (!existHide) // no hide, do nothing
		{
			return;
		}
		int randomPos = 0; // find a hide human
		do 
		{
			randomPos = Random.Range(0, _humans.Length);
		} while (_humans[randomPos].GetComponent<Renderer>().enabled == true);
		// 决定使用哪个人
		int randomHuman = Random.Range (0, _prefixs.Length);
		// 从屏幕边缘走进来
		var hideHuman = _humans [randomPos];
		hideHuman.GetComponent<HumanScript> ().HumanPrefix = _prefixs [randomHuman];
		hideHuman.GetComponent<HumanScript>().WalkIn();
		// play add audio
		AudioSource.PlayClipAtPoint (AddHumanAudio, new Vector2 (0, 0), 0.5f);
	}

	// 进行一次奖励
	public void MakeAllHumanReward() 
	{
		bool existGold = false;
		bool existHuman = false;
		int bangNum = 0;
		int goldNum = 0;
		foreach (var human in _humans) 
		{
			if (human.GetComponent<Renderer>().enabled) // 只有显示出来的人才能进行奖励
			{
				existHuman = true;
				if (Random.value > 0.1) 
				{
					human.GetComponent<HumanScript>().MakeBang();
					bangNum++;
				}
				else 
				{
					human.GetComponent<HumanScript>().MakeGold();
					existGold = true;
					goldNum++;
				}
			}
		}
		if (bangNum > 0)
		{
			GetComponent<SceneController> ().BulletScore += bangNum; // add score in scenecontroller
			GetComponent<AchievementController> ().AddBulletNum (bangNum); // achievement
		}
		if (goldNum > 0)
		{
			GetComponent<GoldController> ().Gold += goldNum; // add score in scenecontroller
			GetComponent<AchievementController> ().AddGoldNum (goldNum);
		}

		// Play bang audio
		if (existHuman)
		{
			GetComponent<AudioSource> ().clip = HumanScoreAudio;
			GetComponent<AudioSource> ().Play ();
			
			// play score audio
			AudioSource.PlayClipAtPoint (BangAudio, new Vector2 (0, 0), 0.1f);
		}
		// Play coin audio
		if (existGold) 
		{
			AudioSource.PlayClipAtPoint(CoinAudio, new Vector2(0, 0), 0.5f);
		}
	}

}
