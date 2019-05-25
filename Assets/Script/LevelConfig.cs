using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml; 

public class LevelConfig : MonoBehaviour {

	private static Dictionary<int, Level> levels;
	private static Dictionary<int, Achievement> achievements;

	public static Level GetLevel(int level) 
	{
		if (levels == null || achievements == null) // need reload config
		{
			LoadConfig();
		}
		return levels.ContainsKey(level) ? levels [level] : null;
	}

	public static Level GetNextLevel(int level) 
	{
		return GetLevel (level + 1);
	}

	public static Achievement GetAchievement(int achievementId) 
	{
		if (levels == null || achievements == null) // need reload config
		{
			LoadConfig();
		}
		return achievements [achievementId];
	}

	private static void LoadConfig() 
	{
		XmlDocument doc = new XmlDocument(); // load xml file
		doc.Load(GameConsts.LEVEL_CONFIG_URL); 

		levels = new Dictionary<int, Level> (); // load level config
		XmlNode levelDocs = doc.SelectSingleNode ("AngryShot").SelectSingleNode ("Levels");
		foreach (XmlNode levelDoc in levelDocs) 
		{
			XmlElement _level = (XmlElement) levelDoc;
			Level level = new Level();
			level.id = int.Parse(_level.GetAttribute("id"));
			string key = _level.GetAttribute("name");
			level.name = UITextScript.GetTextByKey(key);
			level.quality = int.Parse(_level.GetAttribute("quality"));
			XmlNodeList levelConditionDocs = levelDoc.SelectSingleNode("conditions").SelectNodes("condition");
			level.conditions = new Condition[levelConditionDocs.Count];
			int i = 0;
			foreach (XmlNode levelConditionDoc in levelConditionDocs) 
			{
				XmlElement _condition = levelConditionDoc as XmlElement;
				if (_condition == null) continue;
				Condition c = new Condition();
				c.id = int.Parse(_condition.GetAttribute("id"));
				c.num = int.Parse(_condition.GetAttribute("num"));
				level.conditions[i++] = c;
			}
			levels[level.id] = level;
		}

		achievements = new Dictionary<int, Achievement> (); // load achievement config
		XmlNodeList achievementDocs = doc.SelectSingleNode ("AngryShot").SelectSingleNode ("Achievements").SelectNodes("Achievement");
		foreach (XmlNode achievementDoc in achievementDocs) 
		{
			XmlElement _achievement = (XmlElement) achievementDoc;
			Achievement achievement = new Achievement();
			achievement.id = int.Parse(_achievement.GetAttribute("id"));
			string key = achievementDoc.SelectSingleNode("txt").InnerText;
			achievement.txt = UITextScript.GetTextByKey(key);
			achievements[achievement.id] = achievement;
		}
	}
}

public class Level 
{
	public int id; // level id
	public string name; // player name
	public int quality; // level quality color
	public Condition[] conditions; // level reach condition, id=num
}

public class Condition 
{
	public int id; // achievement id
	public int num; // achievement num
}

public class Achievement 
{
	public int id; // achievement id
	public string txt; // achievement txt

	public string GetNumText(int num) 
	{
		return txt.Replace("{0}", num.ToString());
	}
}