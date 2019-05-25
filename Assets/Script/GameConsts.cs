using UnityEngine;
using System.Collections;

public class GameConsts : MonoBehaviour {

	public static string LEVEL_CONFIG_URL = Application.streamingAssetsPath + "/Config/Level.xml";  
	public static string ITEM_CONFIG_URL = Application.streamingAssetsPath + "/Config/Items.xml";  
	public static string LANG_CONFIG_URL = Application.streamingAssetsPath + "/Config/Lang.xml"; 

	public const string HISTORY_SCORE_LOCAL = "L1";
	public const string GOLD_LOCAL = "L2";
	public const string LEVEL_LOCAL = "L3";
	public const string ACHIEVEMENT_LOCAL_PREFIX = "L4";
	public const string PROP_BASKET_LOCAL = "L5";
	public const string PROP_MINI_BALL_LOCAL = "L6";
	public const string PROP_WIND_SHILED_LOCAL = "L7";
	public const string PROP_LONGER_LINE_LOCAL = "L8";
	public const string RANK_UPLOAD_LOCAL = "L9";
	public const string GAME_INIT_LOCAL = "L10";
	public const string AUDIO_PAUSE_LOCAL = "L12";
	public const string LOGIN_TODAY_LOCAL = "L13";
	public const string LOGIN_VERSION_LOCAL = "L14";
	public const string LOGIN_DAYS_LOCAL = "L15";
	public const string NEED_SHOW_SCORE_LOCAL = "L16";
	public const string GAME_TIMES = "L17";
	public const string GUIDE_LOCAL = "L18";
}
