using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour {

	public string DefaultLang;

    // 当前分数
    public int[] _curScore;
	public int CurScore
	{
		get
		{
			return Utils.DecodeNum(_curScore);
		}
		set
		{
			_curScore = Utils.EncodeNum(value);
		}
	}

	// 游戏场景
	public GameObject ScoreText;

	// new record
	private bool _recordBreaking;
	public bool RecordBreaking 
	{
		get 
		{
			return _recordBreaking;
		}
	}

	// end game
	private bool _endGame = true;
	public bool EndGame 
	{
		set 
		{
			_endGame = value;
			if (_endGame && _recordBreaking) // end game, save the record
			{
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
				if (GetComponent<SceneController>().UserLogined) // 登陆的上传分数
				{
				Social.ReportScore(CurScore, "as_4", GetComponent<SceneController>().ReportScoreHandler);
				}
				else 
				{
					GetComponent<SceneController>().ReportScoreHandler(false); // 失败则下次登陆时候上传
				}
#endif
				PlayerPrefs.SetInt(GameConsts.HISTORY_SCORE_LOCAL, CurScore);
				PlayerPrefs.Save();
			}
		}
		get
		{
			return _endGame;
		}
	}

	public int RecordScore 
	{
		get
		{
			int historyScore = PlayerPrefs.GetInt (GameConsts.HISTORY_SCORE_LOCAL);
			return historyScore;
		}
	}

	// Use this for initialization
	void Start () {
		this.SetScore (0);
	}
	
	// Update is called once per frame
	void Update () {

	}

    public void SetScore(int score)
    {
        CurScore = score;

		ScoreText.GetComponent<Text> ().text = CurScore.ToString ();

		// history record
		_recordBreaking = CurScore > RecordScore; 
	}

    private void ShowOne(GameObject go, int number, bool show)
    {
        go.GetComponent<Renderer>().enabled = show;
        if (show)
        {
            TextController.ReplaceSprite("Number/Score/" + number, go);
        }
    }

    public void AddScore(int score)
    {
        CurScore += score;
        this.SetScore(CurScore); // ui
    }
	
}
