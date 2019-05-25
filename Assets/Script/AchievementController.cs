using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AchievementController : MonoBehaviour {
	
	private Dictionary<int, int> records; // 本局成就记录
	public int GameAddGold; // 本局金币

	public GameObject AniConditionPass; // 条件通过动画
	public GameObject AniLevelUp; // 升级动画
	public GameObject AniConditionProgress; // 条件进度+1动画

	private Queue<AniInfo> _nextAni = new Queue<AniInfo>(); // 需要播放的下一个动画

	private bool _isPlaying = false; // 是否正在播放
	public bool IsPlaying 
	{
		get 
		{
			return _isPlaying;
		}
	}
	public bool EndGameAfterPlay = false; // 播放完成后结束游戏

	// Use this for initialization
	void Start () {

		// 动画先都隐藏
		AniConditionPass.SetActive (false);
		AniLevelUp.SetActive (false);
		AniConditionProgress.SetActive (false);

		// 初始化记录
		this.RestartRecord ();
	}

	// 测试用，重置等级
	private void Reset() 
	{
		PlayerPrefs.SetInt(GameConsts.HISTORY_SCORE_LOCAL, 0);
		GetComponent<LevelController> ().Level = 1;
		this.ResetAchievement ();
	}

	// 重新记录, 在一局重新开始或者升级时调用
	public void RestartRecord() 
	{
		if (records == null) 
		{
			records = new Dictionary<int, int> ();
		} 
		else 
		{
			records.Clear();
		}
		
		this.NewHighestScore (GetComponent<ScoreController> ().RecordScore); // 特殊处理，最高分直接检测
	}

	// achievement
	public bool GetAchievement(int id)
	{
		return PlayerPrefs.GetInt (GameConsts.ACHIEVEMENT_LOCAL_PREFIX + id) > 0;
	}

	// achievement num
	public int GetAchievementNum(int id)
	{
		if (id == 3003) // 只有最高分是直接检测
		{
			return GetComponent<ScoreController>().RecordScore;
		}
		return records.ContainsKey (id) ? records [id] : 0;
	}
	
	public void SetAchievement(int id, bool passed) 
	{
		if (!passed) 
		{
			PlayerPrefs.DeleteKey (GameConsts.ACHIEVEMENT_LOCAL_PREFIX + id);
		} 
		else 
		{
			PlayerPrefs.SetInt (GameConsts.ACHIEVEMENT_LOCAL_PREFIX + id, 1);
		}

	}

	public void ResetAchievement() 
	{
		Level nextLevel = GetComponent<LevelController> ().NextLevelConfig;
		if (nextLevel == null) // 没有下一级了。放弃
			return;
		var conditions = nextLevel.conditions;
		foreach (var condition in conditions) 
		{
			this.SetAchievement(condition.id, false);
		}
		this.RestartRecord (); // 重新记录
	}

	// check and save achievement,返回是否升级
	private void CheckNum(int id, int num, int oldNum, bool checkLevel) 
	{
		int needNum = this.GetAchievementNeedNum (id);
		if (needNum == -1)	return;
		if (num >= needNum) // passed
		{
			SetAchievement (id, true);
			// 播放通过条件动画
			AniInfo aniInfo = new AniInfo (AniConditionPass);
			string txt = LevelConfig.GetAchievement (id).GetNumText(this.GetAchievementNeedNum(id, true));
			this.PlayAni (aniInfo.AddParam ("condition", txt)); 

			if (checkLevel) 
			{
				this.CheckLevel(); // 检查等级
			}
		} 
		else if (id != 3003) 
		{
			// 播放进度动画
			string txt = LevelConfig.GetAchievement (id).GetNumText(this.GetAchievementNeedNum(id, true));
			AniInfo aniInfo = new AniInfo (AniConditionProgress);
			this.PlayAni (aniInfo.AddParam("condition", txt).AddParam("oldNum", oldNum.ToString()).AddParam("newNum", num.ToString()));
		}
	}

	// 检查一个等级是否通过
	public bool CheckLevel() 
	{
		if (GetComponent<LevelController> ().NextLevelConfig == null) // 已经到了最高级
		{
			return false;
		}
		var conditions = GetComponent<LevelController> ().NextLevelConfig.conditions; // check this level condition
		bool allPassed = true;
		foreach (var condition in conditions)
		{
			if (!this.GetAchievement (condition.id))
			{
				allPassed = false;
				break;
			}
		}
		if (allPassed) // all passed, level up
		{
			GetComponent<LevelController> ().LevelUp ();
			this.ResetAchievement ();
			// 播放升级动画
			AniInfo aniInfo = new AniInfo (AniLevelUp);
			Level curLevel = GetComponent<LevelController> ().CurLevelConfig;
			this.PlayAni (aniInfo.AddParam ("level", curLevel.name).AddParam ("quality", curLevel.quality.ToString ()));
			this.NewHighestScore(GetComponent<ScoreController>().CurScore); // 再判断一次最高分
		}
		return allPassed;
	}

	// add num in one game record
	private void AddNum(int id, int num, bool checkLevel) 
	{
		int oldNum;
		if (records.ContainsKey (id)) 
		{
			oldNum = records [id];
		} 
		else 
		{
			oldNum = 0;
		}
		records [id] = oldNum + num;
		this.CheckNum (id, records [id], oldNum, checkLevel);
	}

	// wow
	public void AddWowNum(int num) 
	{
		this.AddNum (3001, num, true);
	}

	// bullet
	public void AddBulletNum(int num) 
	{
		this.AddNum (3002, num, true);
	}

	// highest score
	public void NewHighestScore(int score) 
	{
		this.CheckNum (3003, score, 0, true);
	}

	// hallow num
	public void AddHallowNum(int num) 
	{
		if (num == 0)
			return;
		this.AddNum (3004, num, false);
	}

	// angry shot
	public void AddAngryShotNum(int num) 
	{
		if (num == 0)
			return;
		this.AddNum (3005, num, false);
	}

	// triple
	public void AddTripleNum() 
	{
		this.AddNum (3006, 1, false);
	}

	// rebound
	public void AddReboundNum(int num)
	{
		if (num == 0)
			return;
		this.AddNum (3007, num, false);
	}

	// triple hallow
	public void AddTripleHallowNum()
	{
		this.AddNum (3008, 1, false);
	}

	// angry shot hallow
	public void AddAngryShotHallowNum(int num)
	{
		if (num == 0)
			return;
		this.AddNum (3009, num, false);
	}

	// rebound hallow
	public void AddReboundHallowNum(int num) 
	{
		if (num == 0)
			return;
		this.AddNum (3010, num, false);
	}

	// triple angry shot
	public void AddTripleAngryShotNum() 
	{
		this.AddNum (3011, 1, false);
	}

	// triple angry hallow
	public void AddTripleHallowAngryShotNum()
	{
		this.AddNum (3012, 1, false);
	}

	// add time
	public void AddTimeNum(int addTime)
	{
		if (addTime == 0)
			return;
		this.AddNum (3013, addTime, false);
	}

	// final shot 
	public void AddFinalShotNum()
	{
		this.AddNum (3014, 1, true);
	}

	// gold num
	public void AddGoldNum(int num)
	{
		GameAddGold += num;
	}

	// 播放某个动画
	public void PlayAni(AniInfo aniInfo) 
	{
		if (!_isPlaying)
		{
			_isPlaying = true;
			var animator = aniInfo.Ani.GetComponent<Animator> ();
			aniInfo.Ani.SetActive (true); // 变为显示
			switch (aniInfo.Ani.name) 
			{
			case "ConditionPass": // 通过一个条目
			{
				var text = aniInfo.Ani.transform.Find("Canvas/Text");
				text.GetComponent<Text>().text = aniInfo.Params["condition"];
				break;
			}
			case "LevelUp": // 升级
			{
				var text = aniInfo.Ani.transform.Find("Canvas/Text");
				text.GetComponent<Text>().text = aniInfo.Params["level"];
				TextController.ReplaceSprite("UI/level_" + aniInfo.Params["quality"], aniInfo.Ani.transform.Find("level_5").gameObject);
				break;
			}
			case "ConditionProgress": // 条目进度
			{
				var text1 = aniInfo.Ani.transform.Find("Canvas/Text");
				text1.GetComponent<Text>().text = aniInfo.Params["condition"];
				var text2 = aniInfo.Ani.transform.Find("Canvas/Text (1)");
				text2.GetComponent<Text>().text = aniInfo.Params["oldNum"];
				var text3 = aniInfo.Ani.transform.Find("Canvas (2)/Text");
				text3.GetComponent<Text>().text = aniInfo.Params["newNum"];
				break;
			}
			}
			animator.Play ("Playing"); // 开始播放
		} 
		else
		{
			lock (_nextAni) 
			{
				_nextAni.Enqueue(aniInfo);
			}
		}
	}

	// 停止某个动画
	public void StopAni(GameObject ani)
	{
		var animator = ani.GetComponent<Animator> ();
		ani.SetActive (false); // 隐藏显示
		_isPlaying = false;
		lock (_nextAni)
		{
			if (_nextAni.Count > 0 && _nextAni.Peek ().Ani == ani) // 动画弹出
			{
				_nextAni.Dequeue ();
			}
			if (_nextAni.Count > 0) // 播放下一个预期动画
			{
				this.PlayAni (_nextAni.Peek ());
			} else if (EndGameAfterPlay)
			{
				GetComponent<SceneController> ().EndGame ();
			}
		}
	}

	// current level achievement need num, if not exist or passed, return -1
	private int GetAchievementNeedNum(int id, bool onlyNum = false) 
	{
		Level levelConfig = GetComponent<LevelController>().NextLevelConfig;
		if (levelConfig == null) // 不存在下一个等级
		{
			return -1;
		}
		foreach (var condition in levelConfig.conditions) 
		{
			if (condition.id == id && (onlyNum || !this.GetAchievement(id)))
			{
				return condition.num; // find num
			}
		}
		return -1; // not exist in current level
	}
}

// 动画的信息
public class AniInfo 
{
	public GameObject Ani;
	public Dictionary<string, string> Params; 

	public AniInfo(GameObject ani)
	{
		Ani = ani;
		Params = new Dictionary<string, string> ();
	}

	public AniInfo AddParam(string key, string value) 
	{
		Params [key] = value;
		return this;
	}
}