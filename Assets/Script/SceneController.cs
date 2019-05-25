using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using Umeng;

public class SceneController : MonoBehaviour {

    // 球的力量因子，初始位置和鼠标位置的差值乘上这个
    public float ForceFactor;

    // 当前需要抛出去的物体
    public GameObject CurThrowObject;

    // 抛球的小人
    public GameObject Role;

    // 背景对象
    public GameObject Background;

    // 网的比重，0~1
    public float BasketWeight;

    // 空心分数
    public int HallowScore;

    // 连续空心分数
    public int ContinuityHallowScore;
    
    // 普通分数
    public int NormalScore;

    // 单局最大时间
    public int GameMaxTime;

    // 球的prefab
    public GameObject BallPrefab;

    // 筐的prefab
    public GameObject BasketPrefab;

	// start canvas
	public GameObject StartCanvas;
	// game canvas
	public GameObject GameCanvas;
	// end canvas
	public GameObject EndCanvas;
	// setting canvas
	public GameObject SettingCanvas;	
	// 通知界面

	public GameObject NoticeCanvas;
	// 加时界面
	public GameObject ExtraTimeCanvas;

	// 引导文字
	public GameObject GuideTxt;

	// throw current audio
	public AudioClip ThrowAudio;
	// 破纪录音效
	public AudioClip RecordBreakingAudio;
	// 结算音效
	public AudioClip EndAudio;
	// 晚间背景音乐
	public AudioClip NightBgm;

	// 用户是否已经登录
	public bool UserLogined;

    // 单局计时器
    private float _gameTime;

	// 有额外计时器机会
	private bool _haveExtraTime;

	// cat wow
	private int[] _wowScore;
	public int WowScore
	{
		set 
		{
			_wowScore = Utils.EncodeNum(value);
			GameCanvas.transform.Find("UIWowText").GetComponent<Text>().text = value.ToString(); // show ui
		} 
		get 
		{
			return Utils.DecodeNum(_wowScore);
		}
	}

	// bullet
	private int[] _bulletScore;
	public int BulletScore
	{
		set 
		{
			_bulletScore = Utils.EncodeNum(value);
			GameCanvas.transform.Find("UIWowText").GetComponent<Text>().text = value.ToString(); // show ui
		} 
		get 
		{
			return Utils.DecodeNum(_bulletScore);
		}
	}

	// 白天和夜晚
	private bool _isNight;
	public bool IsNight
	{
		set 
		{
			_isNight = value;
		} 
		get 
		{
			return _isNight;
		}
	}

	// 音乐暂停
	public bool AudioPause
	{
		set 
		{
			PlayerPrefs.SetInt(GameConsts.AUDIO_PAUSE_LOCAL, value ? 0 : 1);
			PlayerPrefs.Save();
			AudioListener.pause = value;
		}
		get
		{
			int l = PlayerPrefs.GetInt(GameConsts.AUDIO_PAUSE_LOCAL);
			return l == 0;
		}
	}
	
	// Use this for initialization
	void Start ()
	{

		// umeng
		GA.StartWithAppKeyAndChannelId ("5704d486e0f55ac69b0002d5", "jw004");

		AudioPause = AudioPause; // 重置声音

		PlayerPrefs.SetInt(GameConsts.GAME_TIMES, 0); // 重置游戏局数
		PlayerPrefs.Save();

		this.ReturnHome (false);
#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		Social.localUser.Authenticate (ProcessAuthentication); // 进行gamecenter登录
#endif
		this.JudgeNight (false);
	}

	// 登录返回
	void ProcessAuthentication (bool success) 
	{
		UserLogined = success;
		int upload = PlayerPrefs.GetInt (GameConsts.RANK_UPLOAD_LOCAL); // 是否没能上传最高分
		if (success && upload > 0) // 需要重新上传分数
		{
			int score = this.GetComponent<ScoreController>().RecordScore;
			Social.ReportScore(score, "as_4", ReportScoreHandler);
		} 
	}

	// 上传分数结果
	public void ReportScoreHandler(bool success)
	{
		if (success) 
		{
			PlayerPrefs.DeleteKey (GameConsts.RANK_UPLOAD_LOCAL); // 清理掉本地的标记，下次不用再上传了
		} 
		else 
		{
			PlayerPrefs.SetInt (GameConsts.RANK_UPLOAD_LOCAL, 1); // 本地标记，下次再上传
		}
		PlayerPrefs.Save();
	}

	// 显示某一个ui或者隐藏所有ui
	public void ShowOneUI(string showName = null) 
	{
		var canvas = GameObject.Find("Canvas"); // hide all ui
		int childCount = canvas.transform.childCount;
		for (int i=0; i<childCount; i++)
		{
			var c = canvas.transform.GetChild(i).gameObject;
			c.SetActive(showName == c.name);
		}
		this.UpdateCanvasGold ();
	}

	// 更新各个ui上的金币显示
	public void UpdateCanvasGold() 
	{
		var canvas = GameObject.Find("Canvas"); 
		int childCount = canvas.transform.childCount;
		for (int i=0; i<childCount; i++)
		{
			var c = canvas.transform.GetChild(i).gameObject;
			var wowText = c.transform.Find ("UIWowText"); // wow score
			if (wowText != null)
			{
				if (IsNight) 
				{
					wowText.GetComponent<Text>().text = BulletScore.ToString();
				}
				else 
				{
					wowText.GetComponent<Text> ().text = WowScore.ToString (); 
				}
			}
			var goldText = c.transform.Find ("UIGoldText"); // gold 
			if (goldText != null)
			{
				var gold = GetComponent<GoldController> ().Gold;
				goldText.GetComponent<Text> ().text = gold.ToString ();
			}
		}
	}

	public void RestartNewGame() 
	{
		_gameTime = GameMaxTime; // 初始大一点，之后开始计时
		GetComponent<TimeNumController>().ShowNumber((int)_gameTime);

		_haveExtraTime = true; // 有加时机会
		
		WowScore = 0; // wow score 0
		BulletScore = 0; // bullet score 0

		// 清除之前的球
		var balls = GameObject.FindGameObjectsWithTag ("Ball");
		foreach (var ball in balls)
		{
			if (ball && ball.GetComponent<BallController>()) 
			{
				Destroy(ball);
			}
		}
		// 清除之前的筐
		var baskets = GameObject.FindGameObjectsWithTag ("Basket");
		foreach (var basket in baskets)
		{
			basket.GetComponent<BasketController>().DestoryNow();
		}
		GetComponent<AchievementController> ().GameAddGold = 0;
		GetComponent<AchievementController> ().RestartRecord (); // achievement

		GetComponent<ScoreController> ().EndGame = false; // score 0
		GetComponent<ScoreController> ().SetScore (0);

		GetComponent<AchievementController>().EndGameAfterPlay = false; // 结束游戏

		GetComponent<WindController> ().RestartGame (); // 风力

		if (this.CurThrowObject != null) // rethrow
		{
			Destroy(this.CurThrowObject);
			this.CurThrowObject = null;
		}

		// 初始给三个筐
		var boards = GameObject.FindGameObjectsWithTag ("Board");
		foreach (var board in boards)
		{
			this.GenerateBasket(2, false);
			CurThrowObject.transform.position = new Vector2 (2f, board.transform.position.y);
			CurThrowObject.GetComponent<Rigidbody2D> ().isKinematic = true; // 把速度变为零，黏住      
			board.GetComponent<BoardScript>().Basket = CurThrowObject;
			CurThrowObject.GetComponent<BasketController> ().BeginScore (board, false);
		}

		GetComponent<TimeNumController> ().TimeDecreasing = false;

		this.JudgeNight (); // 判断白天和夜景
	
		this.RandomNext (); // begin next

		this.ResumeGame (); // hide all ui

		GetComponent<CatController> ().HideAllCat (false); // hide cat 
		GetComponent<HumanController> ().HideAllHuman (false); // hide human

		TouchHandler.PlayClickAudio (); // 播放按钮声音

		var camera = GameObject.FindGameObjectWithTag("MainCamera") as GameObject; // 停止震动
		camera.GetComponent<ShakeScreen>().IsShaking = false;

		GetComponent<AdvertisementShower> ().LoadAdmob (); // 提前加载广告

		// 游戏次数
		int gameTimes = PlayerPrefs.GetInt(GameConsts.GAME_TIMES, 0);
		PlayerPrefs.SetInt(GameConsts.GAME_TIMES, gameTimes + 1);
		PlayerPrefs.Save();

		// umeng
		GA.StartLevel ("1");
	}

	// 判断当前是白天还是黑夜
	public void JudgeNight(bool playBgm = true) 
	{
		var now = System.DateTime.Now;
		IsNight = (now.Hour < 7 || now.Hour >= 19); // 以7点和19点为界
		string backgroundName = "background"; // 切换背景
		if (IsNight)
		{
			backgroundName += "_night";
		}
		TextController.ReplaceSprite (backgroundName, Background);
		// 切换一般ui上wow和bang
		string iconName = IsNight ? "Human/icon_bang" : "Cat/icon_wow"; // 确定图像
		var sprite = Resources.Load(iconName, typeof(Sprite)) as Sprite;
		var canvas = GameObject.Find("Canvas"); 
		int childCount = canvas.transform.childCount;
		for (int i=0; i<childCount; i++)
		{
			var c = canvas.transform.GetChild(i).gameObject;
			var wowIcon = c.transform.Find("UIWowIcon");
			if (wowIcon != null) 
			{
				wowIcon.GetComponent<Image>().sprite = sprite;
			}
		}
		// 切换gamecanvas上的wow和bang
		var gameCanvasIcon = GameCanvas.transform.Find("UIWowIcon");
		gameCanvasIcon.GetComponent<Image>().sprite = sprite;
		// 替换栏杆
		var frontground = GameObject.Find ("Frontground");
		string frontname = IsNight ? "picture_002" : "picture_001";
		TextController.ReplaceSprite (frontname, frontground.gameObject);
		// 背景音乐
		var floor = GameObject.Find ("Floor");
		var audioSource = floor.GetComponent<AudioSource> ();
		if (IsNight && playBgm)
		{
			if (!audioSource.isPlaying)
			{
				audioSource.volume = 0.15f;
				audioSource.clip = NightBgm;
				audioSource.loop = true;
				audioSource.Play ();
			}
		} 
		else
		{
			if (audioSource.isPlaying) 
			{
				audioSource.Stop();
			}
		}
	}

	// 返回，用于有可能直接回返游戏的场景
	public void ReturnBackToGame(bool toShop = false) 
	{
		if (!GetComponent<ScoreController> ().EndGame) // 一局还没结束，先继续
		{
			if (toShop) 
			{
				GetComponent<GoldController>().ShowShopCanvas();
			}
			else
			{
				this.ShowOneUI ();
				this.ResumeGame ();
			}
			return;
		} 
		else
		{
			this.ReturnHome();
		}
	}

	public void ReturnHome(bool audio = true) 
	{
		GetComponent<ScoreController> ().EndGame = true;

		Level levelConfig = GetComponent<LevelController> ().CurLevelConfig;
		// 替换等级名称
		var levelText = StartCanvas.transform.Find ("UILevelButton/Text").gameObject;
		levelText.GetComponent<Text> ().text = levelConfig.name;
		// 替换等级图片
		var sprite = Resources.Load("UI/level_" + levelConfig.quality, typeof(Sprite)) as Sprite; // replace sprite color
		StartCanvas.transform.Find("UILevelButton").GetComponent<Image>().sprite = sprite;
		// 显示ui
		this.ShowOneUI ("Start");
		if (audio)
		{
			TouchHandler.PlayClickAudio (); // 播放按钮声音
		}
		// 版本号
		var versionText = StartCanvas.transform.Find ("UIVersion").GetComponent<Text> ();
		versionText.text = Application.version;
		// 停止bgm
		this.StopBgm ();
	}

	public void PauseGame(bool showCanvas = false) 
	{
		this.StopAllReplayAudio ();
		TouchHandler.PlayClickAudio (); // 播放按钮声音
		if (showCanvas)
		{
			GetComponent<LevelController> ().ShowPauseCanvas ();
			AdvertisementShower ad = GetComponent<AdvertisementShower>();
			ad.ShowAdmob("admob_active");
		}
		Time.timeScale = 0;
	}

	public void PauseGameWithoutCanvas() 
	{
		this.PauseGame ();
	}

	// 把场景上所有循环播放的音效停掉
	private void StopAllReplayAudio() 
	{
		var audios = new AudioSource[]{ Camera.main.GetComponent<AudioSource>() };
		foreach (var audio in audios) 
		{
			if (audio.loop && audio.isPlaying) 
			{
				audio.Stop();
			}
		}
	}

	public void ResumeGame() 
	{
		this.ShowOneUI ();
		Time.timeScale = 1;
		TouchHandler.PlayClickAudio (); // 播放按钮声音
		GetComponent<AdvertisementShower> ().LoadAdmob (); // 提前加载广告
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private void FixedUpdate()
    {
		if (_gameTime > 0 && GetComponent<TimeNumController>().TimeDecreasing) // 已经在计时
        {
            _gameTime = _gameTime - Time.fixedDeltaTime; // 设定时间
            GetComponent<TimeNumController>().ShowNumber((int) _gameTime);
        }
        else if (_gameTime <= 0)
        {
			_gameTime = 0; 
			GetComponent<TimeNumController>().TimeDecreasing = false; // 暂时停止计时
            this.LastBall(); // 进入最后一球
        }
    }

    // 扔出当前的球或者筐
    public void ThrowCurrent() {
        if (CurThrowObject != null) {
            if (!GetComponent<TimeNumController>().TimeDecreasing) // 开始倒计时
            {
				GetComponent<TimeNumController>().TimeDecreasing = true;
            }

            CurThrowObject.GetComponent<Rigidbody2D>().isKinematic = false; // 让其开始处理物理事件

            float mass = CurThrowObject.GetComponent<Rigidbody2D>().mass;
            float factor = ForceFactor * mass; // 预测的力量因子，根据质量进行加成，保持统一的曲线
            float ballInitX = Background.GetComponent<LineCreator>().BallInitX; // 计算力量的球初始位置
            float ballInitY = Background.GetComponent<LineCreator>().BallInitY;
			float forceX = (Input.mousePosition.x / Screen.width * 640 - ballInitX) * factor * 0.5f; // 取得初始推力
            float forceY = (Input.mousePosition.y / Screen.height * 1136 - ballInitY) * factor;
            var r = CurThrowObject.GetComponent<Rigidbody2D>(); // 推动物体
            r.AddForce(new Vector2(forceX, forceY), ForceMode2D.Impulse);
            if (CurThrowObject.tag == "Ball")
            {
				r.AddTorque(0.3f, ForceMode2D.Impulse); // 做一个抛出时的旋转
            }

			// Play audio
			GetComponent<AudioSource>().clip = ThrowAudio;
			GetComponent<AudioSource>().Play();

            CurThrowObject = null;
        }
    }

    // 随机下一个
    public void RandomNext()
    {
		GetComponent<WindController> ().ChangeWindMask (false); // destroy wind shield
		var x = Random.Range (-2.5f, -0.5f); // 位置随机
        
		GetComponent<WindController> ().NextWind (); // 随机风力

		// Set current BallInitX
		Background.GetComponent<LineCreator> ().BallInitX = (x + 3.2f) * 100;

        int basketNum = this.CurBasketNum(); // 看现在有几个筐
        if (basketNum == 0) // 没有筐了就生成一个新筐
        {
            this.GenerateBasket(x);
            return;
        }
        if (basketNum == 3) // 筐满了就只生成球
        {
            this.GenerateBall(x);
            return;
        }
        if (Random.value <= BasketWeight) // 按照随机率随机一个新物体
        {
			this.GenerateBasket (x);
			int guide = PlayerPrefs.GetInt (GameConsts.GUIDE_LOCAL, 0); // 需要做新手引导
			if (guide == 0)
			{
				this.ShowGuideTxt (true);
			}
        }
        else
        {
            this.GenerateBall(x);
        }
    }

    // 生成一个新筐
	private void GenerateBasket(float x, bool changeRole = true)
    {
        // 创建球
        CurThrowObject = Instantiate(BasketPrefab, new Vector3(x, -4.67f, -1f), Quaternion.identity) as GameObject;
        // 标记篮板状态
        GameObject[] boards = GameObject.FindGameObjectsWithTag("Board");
		// 调整人物
		if (changeRole) 
		{
	        foreach (var board in boards)
	        {
	            board.GetComponent<BoardScript>().FlashLine(true);
			}        
			TextController.ReplaceSprite("Role/1", Role);
	        Role.transform.position = new Vector3(x - 0.5f, -4.42f);
		}
    }

    // 生成一个新的球
    private void GenerateBall(float x)
    {
		// 出球了就隐藏掉
		this.ShowGuideTxt (false);
		
        CurThrowObject = Instantiate(BallPrefab, new Vector3(x, -4.67f, -0.5f), Quaternion.identity) as GameObject; 
        GameObject[] boards = GameObject.FindGameObjectsWithTag("Board");
        foreach (var board in boards)
        {
            board.GetComponent<BoardScript>().FlashLine(false);
        }
        // 调整人物
		TextController.ReplaceSprite("Role/1", Role);
        Role.transform.position = new Vector3(x - 0.48f, -4.3f);
    }

    // 当前篮板上总共放着几个筐
    private int CurBasketNum()
    {
        GameObject[] boards = GameObject.FindGameObjectsWithTag("Board");
        int sum = 0;
        foreach (var board in boards)
        {
            if (board.GetComponent<BoardScript>() != null && board.GetComponent<BoardScript>().Basket != null)
            {
                sum++;
            }
        }
        return sum;
    }
    
    // 进入最后一球状态
    public void LastBall()
    {
        if (CurThrowObject != null && CurThrowObject.gameObject.tag == "Ball") // 球的话设定最后一球
        {
            if (!CurThrowObject.GetComponent<BallController>().LastBall)
            {
                CurThrowObject.GetComponent<BallController>().LastBall = true;
                if (!GetComponent<ScoreController>().EndGame) // 显示最后一球的信息
                {
                    GetComponent<TextController>().ShowLastBall();
                }
				GetComponent<AchievementController>().AddFinalShotNum(); // achievement
            }            
        }        
    }

    // 结束一局
    public void EndGame()
    {
		if (GetComponent<AchievementController> ().IsPlaying) // 还在播放动画，先等等
		{
			this.StopAllReplayAudio ();
			GetComponent<AchievementController>().EndGameAfterPlay = true;
			return;
		}
		if (_haveExtraTime) // 询问用户是否加时
		{
			_haveExtraTime = false;
			if (GetComponent<AdvertisementShower>().IsUnityAdsReady())
			{
				GetComponent<AdvertisementShower> ().NeedTime = true;
				this.ShowExtraTime();
				return;
			}
		}
		var scoreController = GetComponent<ScoreController> ();
		scoreController.EndGame = true; // end score
		EndCanvas.transform.Find ("UIFreeButton").gameObject. // 免费获得金币按钮
			SetActive (GetComponent<AdvertisementShower> ().IsUnityAdsReady ());
		var recordText = EndCanvas.transform.Find ("UIRecordText").gameObject; // record score
		var record = scoreController.RecordScore;
		recordText.GetComponent<Text> ().text = record.ToString ();
		// new record image
		var newRecord = EndCanvas.transform.Find ("UINewRecord").gameObject;
		if (scoreController.RecordBreaking) 
		{
			newRecord.SetActive(true);
			AudioSource.PlayClipAtPoint(RecordBreakingAudio, new Vector2(0, 0));
		}
		else 
		{
			newRecord.SetActive(false);
			AudioSource.PlayClipAtPoint(EndAudio, new Vector2(0, 0));
		}		
		// 替换等级名称
		Level levelConfig = GetComponent<LevelController> ().CurLevelConfig;
		var levelText = EndCanvas.transform.Find ("UILevelButton/Text").gameObject;
		levelText.GetComponent<Text> ().text = levelConfig.name;
		// 替换等级图片
		var sprite = Resources.Load("UI/level_" + levelConfig.quality, typeof(Sprite)) as Sprite; // replace sprite color
		EndCanvas.transform.Find("UILevelButton").GetComponent<Image>().sprite = sprite;
		// 本局金币
		var recordGoldText = EndCanvas.transform.Find ("UIRecordGoldText");
		recordGoldText.GetComponent<Text> ().text = "+" + GetComponent<AchievementController> ().GameAddGold;
		// 本局分数
		var scoreText = EndCanvas.transform.Find ("UINormalScoreText").gameObject;
		scoreText.GetComponent<Text>().text = scoreController.CurScore.ToString();	
		if (scoreController.RecordBreaking)
		{
			scoreText.GetComponent<Text> ().color = newRecord.GetComponent<Text> ().color; // 破纪录分数颜色
		} 
		else
		{
			scoreText.GetComponent<Text>().color = recordText.GetComponent<Text>().color; // 普通分数颜色
		}
		// 显示ui
		this.ShowOneUI ("End");
		// 停止bgm
		this.StopBgm ();
		// 显示评价
		int needScore = PlayerPrefs.GetInt(GameConsts.NEED_SHOW_SCORE_LOCAL, 0);
		if (needScore > 0)
		{
			if (needScore == 10) 
			{
				GetComponent<LocalManager> ().ShowScoreUsCanvas ();
				PlayerPrefs.SetInt(GameConsts.NEED_SHOW_SCORE_LOCAL, 0);
				PlayerPrefs.Save();
			}
			else 
			{
				PlayerPrefs.SetInt(GameConsts.NEED_SHOW_SCORE_LOCAL, needScore + 1);
				PlayerPrefs.Save();
			}
		}
		// 游戏次数
		int gameTimes = PlayerPrefs.GetInt(GameConsts.GAME_TIMES, 0);
		if (gameTimes >= 3) // 第3局结束后播放广告
		{
			GetComponent<AdvertisementShower>().ShowAdmob("admob_active2");
		}

		// umeng
		GA.FinishLevel ("1");
		GA.Event ("EndScore", GetComponent<ScoreController> ().CurScore.ToString ());
	}

	// 停止bgm
	public void StopBgm() 
	{
		// stop bgm
		var floor = GameObject.Find ("Floor");
		var audioSource = floor.GetComponent<AudioSource> ();
		if (IsNight && audioSource.isPlaying && audioSource.clip == NightBgm)
		{
			audioSource.Stop();
		} 
	}

	// 回调函数
	private void PropsCallback(GameObject go)
	{
		go.transform.localPosition = new Vector3(0, 0f, 0); // 还原位置
		go.SetActive(false); // 重新隐藏
	}

	// use a basket prop
	public void UseBasket() 
	{
		if (CurThrowObject == null || CurThrowObject.tag == "Basket") // have a basket, do nothing
			return;
		if (GetComponent<GoldController> ().Basket <= 0) // 道具不够，显示商店，让玩家购买
		{
			
			GetComponent<GoldController>().ShowShopCanvas();
			this.PauseGame();
			return;
		}
		Destroy (CurThrowObject); // destroy current ball 
		this.GenerateBasket (Role.transform.position.x + 0.48f); // generate a new basket
		GetComponent<GoldController> ().Basket--; // change prop num
		var ani = GameCanvas.transform.Find ("UIBasketButton/-1").gameObject;
		ani.SetActive (true);
		GetComponent<AnimatorScript>().AddRising(ani, 0.5f, 1, new AnimatorDelegate(this.PropsCallback));
		TouchHandler.PlayClickAudio (); // 播放按钮声音

		int guide = PlayerPrefs.GetInt (GameConsts.GUIDE_LOCAL, 0); // 需要做新手引导
		if (guide == 0)
		{
			this.ShowGuideTxt (true);
		}

		// umeng
		GA.Use ("Basket", 1, 15);
	}
//
//	// use a mini ball prop
//	public void UseMiniBall() 
//	{
//		if (GetComponent<GoldController> ().MiniBall <= 0) // prop not enough, do nothing
//			return;
//		if (CurThrowObject == null) // throwing, do nothing
//			return;
//		if (CurThrowObject.tag == "Basket") 
//		{
//			Destroy (CurThrowObject); // destroy current ball
//			this.GenerateBall(Role.transform.position.x + 0.5f); // generate a new ball
//		}
//		CurThrowObject.transform.localScale = new Vector3 (0.5f, 0.5f, 1); // half the ball
//		CurThrowObject.GetComponent<CircleCollider2D> ().radius /= 2;
//		GetComponent<GoldController> ().MiniBall--; // change prop num
//		TouchHandler.PlayClickAudio (); // 播放按钮声音
//
//		// umeng
//		GA.Use ("MiniBall", 1, 15);
//	}

	// use a longer line prop
	public void UseLongerLine() 
	{
		if (Background.GetComponent<LineCreator> ().LongerLine) // used one, do nothing
			return;
		if (GetComponent<GoldController> ().LongerLine <= 0) // 道具不够，显示商店，让玩家购买
		{
			GetComponent<GoldController>().ShowShopCanvas();
			this.PauseGame();
			return;
		}
		Background.GetComponent<LineCreator> ().LongerLine = true; // next line is longer
		GetComponent<GoldController> ().LongerLine--; // change prop 
		var ani = GameCanvas.transform.Find ("UILongerLineButton/-1").gameObject;
		ani.SetActive (true);
		GetComponent<AnimatorScript>().AddRising(ani, 0.5f, 1, new AnimatorDelegate(this.PropsCallback));
		TouchHandler.PlayClickAudio (); // 播放按钮声音

		// umeng
		GA.Use ("LongerLine", 1, 15);
	}

	// use a wind shield prop
	public void UseWindShield() 
	{
		if (GetComponent<WindController> ().IsWindMask()) // used one, do nothing
			return;
		if (CurThrowObject == null) // throwing, do nothing
			return;
		if (GetComponent<GoldController> ().WindShield <= 0) // 道具不够，显示商店，让玩家购买
		{
			GetComponent<GoldController>().ShowShopCanvas();
			this.PauseGame();
			return;
		}
		GetComponent<WindController> ().ChangeWindMask (true); // wind mask
		GetComponent<GoldController> ().WindShield--; // change prop num
		var ani = GameCanvas.transform.Find ("UIWindSheildButton/-1").gameObject;
		ani.SetActive (true);
		GetComponent<AnimatorScript>().AddRising(ani, 0.5f, 1, new AnimatorDelegate(this.PropsCallback));
		TouchHandler.PlayClickAudio (); // 播放按钮声音

		// umeng
		GA.Use ("WindShield", 1, 15);
	}

    // 剩余时间
    public bool HasGameTime()
    {
        return _gameTime > 0;
    }

    // 增加时间
    public void AddGameTime(float addTime)
    {
        _gameTime += addTime;
		if (_gameTime > 0) // 恢复倒计时
		{
			GetComponent<TimeNumController>().TimeDecreasing = true;
		}
    }

	// 中断了应用
	void OnApplicationFocus(){		
		AudioPause = AudioPause; // 重设音量暂停
#if (UNITY_IPHONE || UNITY_ANDROID)
		if (!GetComponent<ScoreController>().EndGame && Time.timeScale > 0) 
		{
			this.PauseGame (true); // 把游戏暂停掉
		}
#endif
	}

	// 显示设置界面
	public void ShowSettingCanvas() 
	{
		this.ShowOneUI ("Setting");
		TouchHandler.PlayClickAudio (); // 按钮点击音效
		this.UpdateSoundIcon ();
	}

	// 设置声音暂停
	public void SwitchSound() 
	{
		AudioPause = !AudioPause;
		TouchHandler.PlayClickAudio (); // 按钮点击音效
		this.UpdateSoundIcon ();
	}

	private void UpdateSoundIcon() 
	{
		string soundIcon = AudioListener.pause ? "UI/sound_off" : "UI/sound_on";
		var sprite = Resources.Load(soundIcon, typeof(Sprite)) as Sprite; // replace sprite 
		SettingCanvas.transform.Find("UISoundButton/Image").GetComponent<Image>().sprite = sprite;
	}

	public void ShowExtraTime() 
	{
		Button buyButton = ExtraTimeCanvas.transform.Find ("BuyButton").GetComponent<Button> ();
		buyButton.interactable = GetComponent<GoldController> ().Gold >= 50;
		ExtraTimeCanvas.SetActive (true);
		ExtraTimeCanvas.transform.Find ("FreeButton/Image").GetComponent<ShakeScreen> ().IsShaking = true;
//		ExtraTimeCanvas.transform.Find ("FreeButton/Image").GetComponent<ShakeScreen> ().InvokeUpdate ();
		this.PauseGameWithoutCanvas ();
	}

	public void HideExtraTime() 
	{
		if (Time.timeScale < 1 && !GetComponent<ScoreController>().EndGame) // 还没结束游戏就继续
		{
			this.ResumeGame();
			if (!this.HasGameTime())
			{
				this.EndGame();
			}
		}
		ExtraTimeCanvas.SetActive (false);
		ExtraTimeCanvas.transform.Find ("FreeButton/Image").GetComponent<ShakeScreen> ().IsShaking = false;
	}

	// 看广告免费加时
	public void FreeExtra() 
	{
		GetComponent<AdvertisementShower> ().ShowAds ("unityads_active2");
	}

	// 花金币加时
	public void BuyExtra() 
	{
		if (GetComponent<GoldController> ().Gold >= 50)
		{
			GetComponent<GoldController>().Gold -= 50;
			AddGameTime(11);

			GA.Buy ("ExtraTime", 1, 50);

			this.HideExtraTime();
			this.RandomNext();
		}
	}

	// 显示引导文字
	public void ShowGuideTxt(bool show)
	{
		GuideTxt.SetActive (show);
	}

	// 显示通知框
	public void ShowNoitceCanvas(string text, float sec) 
	{
		if (text != null) // 替换文字
		{
			NoticeCanvas.transform.Find("UIText").GetComponent<Text>().text = text;
		}
		if (sec > 0)
		{
			Invoke("HideNoticeCanvas", sec);
		}
		NoticeCanvas.SetActive (true);
	}

	// 隐藏通知框
	public void HideNoticeCanvas() 
	{
		NoticeCanvas.SetActive (false);
	}

}
