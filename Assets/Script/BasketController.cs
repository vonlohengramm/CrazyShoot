using UnityEngine;
using System.Collections;

	public class BasketController : MonoBehaviour {

    // 场景控制器
    public GameObject SceneController;
    // 对应的板子
    public GameObject Board;
    // 悬挂点
    public GameObject HangPrefab;
	// hollow audio
	public AudioClip HallowAudio;
	// hang audio
	public AudioClip HangAudio;

    // 最大耐久度
    public int MaxDurability;

    // 当前耐久度
    private int _curDurability;

    // 正在计分中
    private bool _isScoring;

    // 当前进行了几次空心
    private int _curHollow;

	// 最大边界
	private float _maxEdge;

    // 篮板原先的位置
    private Vector2 _oldBoardPos;
    
    // 摧毁过程中倒计时
    private float _destroyTime;
    
    // 几个支架
    private GameObject _leftPoint;
    private GameObject _rightPoint;
    private GameObject _sidePoint;

	// Use this for initialization
	void Start () {
        if (SceneController == null)
        {
            SceneController = GameObject.FindGameObjectWithTag("SceneController");
        }
	    _curDurability = MaxDurability;
		var camera = GameObject.FindGameObjectWithTag("MainCamera") as GameObject;
		_maxEdge = camera.GetComponent<Camera> ().orthographicSize / 5.68f * 3.2f;
	}

	void FixedUpdate() 
	{
		if (Mathf.Abs (transform.position.x) > _maxEdge || transform.position.y < -5) // 超了左右边界或下边界
		{
            this.FinishAndDestroy();
		}
        float fanForce = SceneController.GetComponent<WindController>().FanForce * GetComponent<Rigidbody2D>().mass;
        if (Mathf.Abs(fanForce) > 0) // 风力
        {
            GetComponent<Rigidbody2D>().AddForce(new Vector2(fanForce, 0), ForceMode2D.Force);
        }
	    if (_destroyTime > 0) // 在摧毁中，需要处理
	    {
	        _destroyTime -= Time.fixedDeltaTime; // 继续倒计时
	        if (_destroyTime <= 0) // 已经完成倒计时，需要立即摧毁
	        {
	            this.DestoryNow();
	            return;
	        }
            var visible = ((int)(_destroyTime * 6) % 2) == 0; // 闪烁
	        transform.GetComponent<Renderer>().enabled = visible; 
	        foreach (var r in GetComponentsInChildren<Renderer>())
	        {
	            if (r.gameObject.tag != "Board" && r.gameObject.tag != "AirBoard")
	            {
                    r.enabled = visible;
	            }
	        }
	    }
	}

    // 开始计分
    public void BeginScore(GameObject b, bool needGuide = true) {
        _isScoring = true;
        _curHollow = 0;
        Board = b; // 记录板子
        // 停止所有闪烁
        GameObject[] boards = GameObject.FindGameObjectsWithTag("Board");
        foreach (var board in boards)
        {
            board.GetComponent<BoardScript>().FlashLine(false);
        }
        var colls = GetComponents<BoxCollider2D>(); // 把之前没打开的碰撞器打开
        foreach (BoxCollider2D coll in colls)
        {
            if (!coll.enabled)
            {
                coll.enabled = true;
            }
        }
		var edgeColl = GetComponent<EdgeCollider2D>(); // 把之前没打开的碰撞器打开
		if (!edgeColl.enabled) 
		{
			edgeColl.enabled = true;
		}
        _oldBoardPos = Board.transform.position;
        Board.transform.parent = this.transform; // 调整父子关系，保持两者共同运动
		Board.transform.localPosition = new Vector2 (Board.transform.localPosition.x, 0.28f); // 调整最后挂筐位置
        this.putHang(); // 摆放支架
        this.unlockNet(); // 解锁球网

		int guide = PlayerPrefs.GetInt (GameConsts.GUIDE_LOCAL, 0); // 需要做新手引导
		if (guide == 0 && needGuide)
		{
			SceneController.GetComponent<SceneController>().ShowGuideTxt (false);
			PlayerPrefs.SetInt (GameConsts.GUIDE_LOCAL, 1); // 挂筐成功，新手引导结束
			PlayerPrefs.Save ();
		}
    }

    private void unlockNet() // 解锁球网
    {
        var netBodys = this.transform.GetComponentsInChildren<Rigidbody2D>();
        foreach (var r in netBodys)
        {
            r.isKinematic = false; // 受到重力影响
            var edge = r.GetComponent<EdgeCollider2D>(); // 边缘碰撞
            if (edge != null)
            {
                edge.enabled = true;
            }
            var hinge = r.GetComponent<HingeJoint2D>(); // 链条连接
            if (hinge != null)
            {
                hinge.enabled = true;
            }
        }
    } 

    // 摆放篮筐支架
    private void putHang()
    {
        // 摆放左支架
        var basketPos = transform.position;
        _leftPoint = Instantiate(HangPrefab, new Vector3(basketPos.x, basketPos.y, basketPos.z), Quaternion.identity) as GameObject;
        var lineDistance = _leftPoint.GetComponent<SpringJoint2D>().distance; // 弹簧长度
        var connectedX = _leftPoint.GetComponent<SpringJoint2D>().connectedAnchor.x; // 被连接位置
        var connectedY = _leftPoint.GetComponent<SpringJoint2D>().connectedAnchor.y;
        _leftPoint.transform.position = new Vector3(basketPos.x + connectedX, basketPos.y + lineDistance + connectedY, basketPos.z); // 重新设定位置
        _leftPoint.GetComponent<SpringJoint2D>().connectedBody = GetComponent<Rigidbody2D>(); // 设定与自身的链接
        // 摆放右支架
        _rightPoint = Instantiate(HangPrefab, new Vector3(basketPos.x - connectedX, basketPos.y + connectedY + lineDistance, basketPos.z), Quaternion.identity) as GameObject;
        _rightPoint.GetComponent<SpringJoint2D>().connectedAnchor = new Vector2(-connectedX, connectedY); // 右侧的，x倒置
        _rightPoint.GetComponent<SpringJoint2D>().connectedBody = GetComponent<Rigidbody2D>(); // 设定与自身的链接
        // 摆放侧边支架，位置在斜上方四十五度
        _sidePoint = Instantiate(HangPrefab, new Vector3(basketPos.x - connectedX + lineDistance, basketPos.y + connectedY, basketPos.z), Quaternion.identity) as GameObject;
        _sidePoint.GetComponent<SpringJoint2D>().connectedAnchor = new Vector2(-connectedX, connectedY); // 右侧的，x倒置
        _sidePoint.GetComponent<SpringJoint2D>().connectedBody = GetComponent<Rigidbody2D>(); // 设定与自身的链接
        GetComponent<Rigidbody2D>().isKinematic = false; // 释放篮筐
    }

    // 碰撞触发
    private void OnCollisionEnter2D(Collision2D obj)
    {
        if (obj.gameObject.name == "Floor") // 撞上地板了
        {
           this.FinishAndDestroy();
        }
    }

    // 清除
    private void FinishAndDestroy(bool hasNext = true)
    {
        if (_destroyTime > 0) // 已经确定摧毁了
        {
            return;
        }
        // 下一个
        if (hasNext)
        {
            SceneController.GetComponent<SceneController>().RandomNext();
        }
        // 标记
        _destroyTime = 1.5f;
        if (Board != null) // 关闭筐存在，可以继续出筐
        {
            Board.GetComponent<BoardScript>().Basket = null;
        }
    }

    public void DestoryNow()
    {
        if (Board != null)
        {
            Board.GetComponent<BoardScript>().ShowAirBound(0); // 板子空心显示
			Board.GetComponent<BoardScript>().Basket = null;
			// 还原父子关系
			Board.transform.parent = null; // 调整父子关系，解除共同运动关系    
			Board.transform.position = _oldBoardPos;
			Board.transform.rotation = new Quaternion(0, 0, 0, 0);
        }
        
        // 清除连接线
        if (_leftPoint != null)
        {
            Destroy(_leftPoint);
        }
        if (_rightPoint != null)
        {
            Destroy(_rightPoint);
        }
        if (_sidePoint != null)
        {
            Destroy(_sidePoint);
        }
        // 清除本身
        Destroy(this.gameObject);
    }

    void OnDestroy()
    {
    }

    // 扣减耐久度
    public void DecDurability()
    {
        _curDurability--;
        if (_curDurability <= 0) // 没有耐久了
        {
            // 清除本身
            this.FinishAndDestroy(false);
            // 允许挂新筐
            Board.GetComponent<BoardScript>().Basket = null;
        }
        else
        {
            TextController.ReplaceSprite("BasketLine/" + _curDurability, this.gameObject); // 替换篮筐颜色
        }
    }

	void OnTriggerEnter2D(Collider2D coll) 
	{
		if (coll.gameObject.tag == "Board") // 只对篮板需要做这个
		{
			var boardScript = coll.GetComponent<BoardScript> ();
			if (boardScript.Basket != null) { // 已经有筐了，直接放过去
				return;
			}
			if (this.GetComponent<Rigidbody2D> ().velocity.y > 0)
				return; // 上升中挂不上
			this.GetComponent<Rigidbody2D> ().isKinematic = true; // 把速度变为零，黏住      
			boardScript.Basket = this.gameObject;
			this.BeginScore (coll.gameObject);
			SceneController.GetComponent<SceneController> ().RandomNext (); // 随机下一个
			// play hang audio
			this.GetComponent<AudioSource> ().clip = HangAudio;
			this.GetComponent<AudioSource> ().Play ();
		}
		// 如果碰上边界则消失
		if (coll.gameObject.name == "Background")
		{
			this.FinishAndDestroy();
		}
		// 只在计分时有反应
		if (!_isScoring) return; 
		var collObj = coll.gameObject;
		// 只对球有反应
		if (collObj.tag != "Ball") return;
		// 位置计算
		var ballPos = collObj.transform.position;
		var basketPos = this.transform.position;
		// 从上方离开的不算
		if (ballPos.y >= basketPos.y || collObj.GetComponent<Rigidbody2D>().velocity.y >= 0) return; 
		// 左右穿过的不算
		if (ballPos.x < basketPos.x - 0.35 || ballPos.x > basketPos.x + 0.35) return; 
		// 如果已经得分了不算
		if (collObj.GetComponent<BallController>().HasScore(Board.name)) return;
		
		bool overCeiling = collObj.GetComponent<BallController>().OverCeiling; // 超屏
		bool hollow = collObj.GetComponent<BallController>().Hollow(Board.name); // 空心
		int score = 0;
		int addTime = 0;
		if (hollow) // 空心处理
		{
			_curHollow++;
			if (_curHollow >= 3) { // 三次连续空心
				score = SceneController.GetComponent<SceneController>().ContinuityHallowScore;
				addTime += score;
				// add wow
				SceneController.GetComponent<CatController>().MakeAllWow();
				SceneController.GetComponent<HumanController>().MakeAllHumanReward();
			}                        
			else // 普通空心
			{
				score = SceneController.GetComponent<SceneController>().HallowScore;
				addTime += score;
			}
			SceneController.GetComponent<TextController>().ShowAirball(overCeiling ? (3 * addTime) : addTime); // 空心文字
			// Play AirBall Audio
			GetComponent<AudioSource>().clip = HallowAudio;
			GetComponent<AudioSource>().Play();
		}
		else // 没有空心
		{
			_curHollow = 0;
			score = SceneController.GetComponent<SceneController>().NormalScore;
		}
		Board.GetComponent<BoardScript>().ShowAirBound(_curHollow); // 板子空心显示		
		if (overCeiling) // 超屏处理
		{
			score = score * 3;
			if (hollow) // 空心时间也翻倍
			{
				addTime = addTime * 3;
			}
			// make all cat wow
			SceneController.GetComponent<CatController>().MakeAllWow();
			SceneController.GetComponent<HumanController>().MakeAllHumanReward();
		}
		SceneController.GetComponent<SceneController>().AddGameTime(addTime); // 增加时间
		SceneController.GetComponent<ScoreController>().AddScore(score); // 计分加分
		collObj.GetComponent<BallController>().SetScore(Board.name, score); // 球上计分（用做多个穿过)
		if (Board != null) // 板上计分动画
		{
			Board.GetComponent<BoardScript>().ShowAddScore(score);
		}
		// Add a wow cat
		if (SceneController.GetComponent<SceneController> ().IsNight) // 夜晚出小人，白天出猫咪
		{
			SceneController.GetComponent<HumanController>().AddHuman();
		} 
		else
		{
			SceneController.GetComponent<CatController> ().AddCat ();
		}
		collObj.GetComponent<BallController> ().TotalAddTime += addTime;;
	}

    // 触发
    void OnTriggerExit2D(Collider2D coll)
    {
		this.OnTriggerEnter2D (coll); // 多检测一次,避免出现问题
    }

    // 没进的取消掉当前连分
    public void ClearHollow()
    {
        _curHollow = 0;
        Board.GetComponent<BoardScript>().ShowAirBound(_curHollow); // 板子空心显示
    }

}
