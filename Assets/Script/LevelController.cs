using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Umeng;

public class LevelController : MonoBehaviour {

	// player level
	private int _level;
	public int Level
	{
		get 
		{
			if (_level == 0) // load level
			{
				_level = PlayerPrefs.GetInt(GameConsts.LEVEL_LOCAL);
				if (_level == 0) // no level, need init level, reset
				{
					Level = 1;
				}
			}
			return _level;
		}
		set 
		{
			if (_level == value) return;
			_level = value;
			PlayerPrefs.SetInt(GameConsts.LEVEL_LOCAL, value);
			PlayerPrefs.Save();
		}
	}

	// level config
	public Level CurLevelConfig
	{
		get 
		{
			return LevelConfig.GetLevel(Level);
		}
	}

	// next level config
	public Level NextLevelConfig
	{
		get
		{
			return LevelConfig.GetLevel(Level + 1);
		}
	}

	// canvas
	public GameObject LevelCanvas;
	public GameObject PauseCanvas;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ShowLevelCanvas() 
	{
		var sceneController = GetComponent<SceneController> ();
		sceneController.ShowOneUI ("Level");
		var levels = LevelCanvas.transform.Find ("UILevels"); // all level item ui
		int itemNum = levels.childCount; // level item num
		for (int i=0; i<itemNum; i++) 
		{
			var levelItem = levels.GetChild(i).gameObject;
			this.DealLevelItem(levelItem, i + 1, i + 1 > _level);
		}

		this.DealMaxLevelUI (LevelCanvas);

		var curLevelUI = LevelCanvas.transform.Find ("UICurLevel").gameObject; // level ui
		this.DealLevelItem (curLevelUI, _level, false);
		var nextLevelUI = LevelCanvas.transform.Find ("UINextLevel").gameObject; // next level ui
		this.DealLevelItem (nextLevelUI, _level + 1, true);
		this.DealCondition (_level + 1, LevelCanvas); // condition
		TouchHandler.PlayClickAudio (); // 播放按钮声音
	}

	public void ShowPauseCanvas()
	{
		GetComponent<SceneController> ().ShowOneUI ("Pause"); // 切换ui

		this.DealMaxLevelUI (PauseCanvas);

		var curLevelUI = PauseCanvas.transform.Find ("UICurLevel").gameObject; // level ui
		var nextLevelUI = PauseCanvas.transform.Find ("UINextLevel").gameObject; // next level ui
		this.DealLevelItem (curLevelUI, _level, false); // 调整显示
		this.DealLevelItem (nextLevelUI, _level + 1, true);
		this.DealCondition (_level + 1, PauseCanvas); 
	}

	private void DealMaxLevelUI(GameObject canvas) 
	{
		var curLevelUI = canvas.transform.Find ("UICurLevel").gameObject; // level ui
		var nextLevelUI = canvas.transform.Find ("UINextLevel").gameObject; // next level ui
		var levelText = canvas.transform.Find ("UILevelButton/Text").gameObject;
		var levelButton = canvas.transform.Find ("UILevelButton").gameObject;
		var congratulation = canvas.transform.Find ("UICongratulation").gameObject;
		var role = canvas.transform.Find ("UIRole").gameObject;
		var conditions = canvas.transform.Find ("UIConditions").gameObject;
		var arrow = canvas.transform.Find ("UIArrow").gameObject;
		
		bool isMaxLevel = GetComponent<LevelController> ().NextLevelConfig == null;
		curLevelUI.SetActive (!isMaxLevel);
		nextLevelUI.SetActive (!isMaxLevel);
		conditions.SetActive (!isMaxLevel);
		arrow.SetActive (!isMaxLevel);
		levelButton.SetActive (isMaxLevel);
		congratulation.SetActive (isMaxLevel);		
		role.SetActive (isMaxLevel);
		if (isMaxLevel) // 满级之后替换显示
		{
			// 替换等级名称
			Level levelConfig = GetComponent<LevelController> ().CurLevelConfig;
			levelText.GetComponent<Text> ().text = levelConfig.name;
			// 替换等级图片
			var sprite = Resources.Load ("UI/level_" + levelConfig.quality, typeof(Sprite)) as Sprite; // replace sprite color
			levelButton.GetComponent<Image> ().sprite = sprite;
			return;
		} 
	}

	private void DealCondition(int level, GameObject canvas) 
	{
		Level levelConfig = LevelConfig.GetLevel (level);
		if (levelConfig == null) 
		{
			levelConfig = new Level();
			levelConfig.conditions = new Condition[0];
		}
		var conditions = canvas.transform.Find ("UIConditions"); // all level item ui
		int itemNum = conditions.childCount; // level item num
		for (int i=0; i<itemNum; i++) 
		{
			var conditionItem = conditions.GetChild(i).gameObject;
			var text = conditionItem.transform.Find("Text");
			var recordText = conditionItem.transform.Find("RecordText"); // 需要显示当前进度
			var finish1 = conditionItem.transform.Find("Finish1").gameObject; // 完成打钩
			var finish2 = conditionItem.transform.Find("Finish2").gameObject;
			if (recordText != null) 
			{
				recordText.gameObject.SetActive(false);
			}
			string resource = "UI/condition_";
			if (i >= levelConfig.conditions.Length) // empty condition
			{
				resource += "off";
				text.GetComponent<Text>().text = "";
				finish1.SetActive(false); // 不打钩
				finish2.SetActive(false);
			}
			else 
			{
				int achievementId = levelConfig.conditions[i].id;
				if (GetComponent<AchievementController>().GetAchievement(achievementId)) 
				{
					resource += "on";
					finish1.SetActive(true); // 打钩
					finish2.SetActive(true);
				}
				else
				{
					resource += "off";
					finish1.SetActive(false); // 不打钩
					finish2.SetActive(false);
					if (recordText != null)  // 显示进度数字
					{
						int record = GetComponent<AchievementController>().GetAchievementNum(achievementId);
						recordText.GetComponent<Text>().text = record.ToString();
						recordText.gameObject.SetActive(true);
					}
				}
				Achievement achievementConfig = LevelConfig.GetAchievement(achievementId);
				text.GetComponent<Text>().text = achievementConfig.GetNumText(levelConfig.conditions[i].num);
			}
			var sprite = Resources.Load(resource, typeof(Sprite)) as Sprite; // replace sprite color
			conditionItem.GetComponent<Image>().sprite = sprite;
		}
	}

	private void DealLevelItem(GameObject levelItem, int level, bool needMask) 
	{
		Level levelConfig = LevelConfig.GetLevel (level);
		var mask = levelItem.transform.Find("Mask").gameObject; // unreachable mask
		var text = levelItem.transform.Find("Text").gameObject; // name text
		if (levelConfig == null) // not exist level
		{
			mask.SetActive(true);
			text.GetComponent<Text>().text = "";
			return;
		}
		mask.SetActive(needMask);
		text.GetComponent<Text>().text = levelConfig.name; // name
		var sprite = Resources.Load("UI/level_" + levelConfig.quality, typeof(Sprite)) as Sprite; // replace sprite color
		levelItem.GetComponent<Image>().sprite = sprite;
	}

	// player level up
	public void LevelUp() 
	{
		if (this.NextLevelConfig == null) // 没有下一级了，放弃
			return;
		Level++;

		// umeng
		GA.SetUserLevel (Level);
	}
}
