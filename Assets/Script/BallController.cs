using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour {

    // 场景控制器
    public GameObject SceneController;

	// 超越上边框
	public bool OverCeiling;

    // 属于最后一投
    public bool LastBall;

	// Hit basket audio
	public AudioClip HitBasketAudio;
	// Hit floor audio
	public AudioClip HitFloorAudio;

	// 总共加时间
	public int TotalAddTime;

    // 空心球的记录
    private Dictionary<string, bool> _hallowDic;

    // 得分记录
    private Dictionary<string, int> _scoreDic;

    // 已经关闭等待销毁
    private bool _isDestroying;

	// 超框已处理
	private bool _ceilingDealed;

	// 最大边界
	private float _maxEdge;

	// rebound
	private bool _rebound;
	public bool Rebound 
	{
		get 
		{
			return _rebound;
		}
	}

	// Use this for initialization
	void Start () {
        if (SceneController == null) // 初始化场景控制器
        {
            SceneController = GameObject.FindGameObjectWithTag("SceneController");
        }

        _hallowDic = new Dictionary<string, bool>();
        _scoreDic = new Dictionary<string, int>();
        GameObject[] boards = GameObject.FindGameObjectsWithTag("Board"); // 根据篮板初始化数据
	    foreach (var board in boards)
	    {
	        _hallowDic[board.name] = true; // 初始空心值
	        _scoreDic[board.name] = 0; // 初始分数值
	    }

	    _isDestroying = false; // 初始化变量
		_ceilingDealed = false;
		OverCeiling = false;
		_rebound = false;
		var c = GameObject.FindGameObjectWithTag("MainCamera") as GameObject;
		_maxEdge = c.GetComponent<Camera> ().orthographicSize / 5.68f * 3.2f;
		HitBasketAudio = Resources.Load<AudioClip> ("Audio/HitBasket");
		HitFloorAudio = Resources.Load<AudioClip> ("Audio/HitFloor");
	}

	private void ShakeScreen(bool shake) 
	{
		var camera = GameObject.FindGameObjectWithTag("MainCamera") as GameObject;
		camera.GetComponent<ShakeScreen>().IsShaking = shake;
	}
	
	// Update is called once per frame
	void Update () {
	}

	void FixedUpdate() 
	{
		if (OverCeiling && !_ceilingDealed) // 超框还未处理
		{
			var velocity = GetComponent<Rigidbody2D>().velocity;
			if (velocity.y <= 0) // 已经开始下落，震屏
			{
				this.ShakeScreen(true);
				_ceilingDealed = true;
				SceneController.GetComponent<AnimatorScript>().AddFallingStar(this.gameObject, 0.3f);
			}
		}
		if (Mathf.Abs (transform.position.x) > _maxEdge + 0.4f) // 超了左右边界
		{
			this.FinishAndDestroy(0);
		}
        float fanForce = SceneController.GetComponent<WindController>().FanForce * GetComponent<Rigidbody2D>().mass;
        if (Mathf.Abs(fanForce) > 0) // 风力
	    {
            GetComponent<Rigidbody2D>().AddForce(new Vector2(fanForce, 0), ForceMode2D.Force);
	    }
	}

    void OnTriggerExit2D(Collider2D coll)
    {
        // 如果碰上边界则消失
        if (coll.gameObject.name == "Background")
        {
            this.FinishAndDestroy(0);
        }
		// 如果碰上上边框
		if (coll.gameObject.name == "Ceiling") 
		{
			var velocity = GetComponent<Rigidbody2D>().velocity;
			if (velocity.y > 0 && !OverCeiling) { // 上升中，可以震屏
				OverCeiling = true;
                SceneController.GetComponent<TextController>().ShowCrazyShoot(); // 超屏文字
				this.GetComponent<Rigidbody2D>().AddTorque(-0.6f, ForceMode2D.Impulse); // 逆向冲量抵消旋转
			}
		}
    }

    // 碰撞触发
    void OnCollisionEnter2D(Collision2D obj)
    {
        if (obj.gameObject.name == "Floor") // 撞上地板了
        {
			// play hit floor audio
			GetComponent<AudioSource>().clip = HitFloorAudio;
			GetComponent<AudioSource>().Play();
			if (GetComponent<Rigidbody2D>().velocity.y < 7.5) { // 没有力气可以反弹了
				this.FinishAndDestroy();
			}
			_rebound = true;
        }
        if (obj.gameObject.tag == "Basket") // 撞上筐了
        {
            string boardName = obj.gameObject.GetComponent<BasketController>().Board.name;
            if (_hallowDic[boardName] && _scoreDic[boardName] == 0) // 如果没碰过，要缩减篮筐耐久
            {
                obj.gameObject.GetComponent<BasketController>().DecDurability();
				_hallowDic[boardName] = false;
				// Play hit audio
				GetComponent<AudioSource>().clip = HitBasketAudio;
				GetComponent<AudioSource>().Play();
            }
        }
    }

    // 处理消失前事件
    public void FinishAndDestroy(float destroyTime = 0.5f)
    {
        if (_isDestroying) return; // 关闭过了
        _isDestroying = true;
        int scoreNum = this.ScoreNum();
        if (scoreNum > 0) 
        {
			if (scoreNum > 1) // 穿越了一个以上的筐
			{
	            int extraScore = this.TotalScore() * (scoreNum - 1); // 额外的倍数分数
	            SceneController.GetComponent<ScoreController>().AddScore(extraScore);
	            SceneController.GetComponent<TextController>().ShowMultiScore(scoreNum);
				// cat wow
				SceneController.GetComponent<CatController>().MakeAllWow();
				SceneController.GetComponent<HumanController>().MakeAllHumanReward();
			}
			// achievement
			if (scoreNum == 3) 
			{
				SceneController.GetComponent<AchievementController>().AddTripleNum();
			}
			int hollowNum = 0;
			foreach (var i in _hallowDic) 
			{
				if (_scoreDic[i.Key] > 0 && i.Value) // 有空心分数
				{
					hollowNum++;
				}
			}
			SceneController.GetComponent<AchievementController>().AddHallowNum(hollowNum);
			if (Rebound) // rebound achievement
			{
				SceneController.GetComponent<AchievementController>().AddReboundNum(scoreNum);
				SceneController.GetComponent<AchievementController>().AddReboundHallowNum(hollowNum);
			}

			// time achievement
			SceneController.GetComponent<AchievementController> ().AddTimeNum (TotalAddTime);

			if (hollowNum == 3 && scoreNum == 3) // triple hallow achievement
			{
				SceneController.GetComponent<AchievementController>().AddTripleHallowNum();
			}
			// angry shot
			if (OverCeiling) 
			{
				// num
				SceneController.GetComponent<AchievementController>().AddAngryShotNum(scoreNum);
				SceneController.GetComponent<AchievementController>().AddAngryShotHallowNum(hollowNum);
				if (scoreNum == 3) 
				{
					SceneController.GetComponent<AchievementController>().AddTripleAngryShotNum();
				}
				// triple angry hallow shot achievement
				if (hollowNum == 3) 
				{
					SceneController.GetComponent<AchievementController>().AddTripleHallowAngryShotNum();
				}
			}
			
			SceneController.GetComponent<AchievementController>().NewHighestScore(SceneController.GetComponent<ScoreController>().CurScore);
			SceneController.GetComponent<AchievementController>().CheckLevel(); // 最后再检查等级，保证之前的进度能清空
        }
        else if (scoreNum == 0) // 完全没进
        {
            SceneController.GetComponent<TextController>().ShowMiss();
			// hide cat
			SceneController.GetComponent<CatController>().HideAllCat();
			SceneController.GetComponent<HumanController>().HideAllHuman();
        }
		if (OverCeiling) // 震屏结束
		{
			this.ShakeScreen(false);
			OverCeiling = false;
			SceneController.GetComponent<AnimatorScript>().RemoveFallingStar(this.gameObject);
		}

        foreach (var score in _scoreDic)
        {
            if (score.Value == 0) // 没得分的话
            {
                var board = GameObject.Find(score.Key) as GameObject;
                if (board.GetComponent<BoardScript>().Basket != null)
                {
                    board.GetComponent<BoardScript>().Basket.GetComponent<BasketController>().ClearHollow(); // 清除掉空心记录   
                }
            }
        }

        var sc = SceneController.GetComponent<SceneController>();
        if (!LastBall || sc.HasGameTime()) // 不是最后一个球，或者又有了时间就继续出
        {
            sc.RandomNext();
        }
        else
        {
            sc.EndGame(); // 游戏结束
        }

        // 清除本身
        Destroy(this.gameObject, destroyTime);
    }

    // 空心
    public bool Hollow(string boardName)
    {
        return _hallowDic[boardName];
    }

    // 计分
    public void SetScore(string boardName, int score)
    {
         _scoreDic[boardName] = score;
    }

    // 是否已经有分数
    public bool HasScore(string boardName)
    {
        return _scoreDic[boardName] > 0;
    }

    // 有分数的个数
    public int ScoreNum()
    {
        return _scoreDic.Values.Count(score => score > 0);
    }

    // 计算总分
    public int TotalScore()
    {
        return _scoreDic.Values.Sum();
    }

}
