using UnityEngine;
using System.Collections;

public class HumanScript : MonoBehaviour {

	// whether the cat is walking
	private bool _isWalking;
	// walk direction
	private int _direction;
	// target position X
	private float _targetPos;
	// initial position X
	private float _initPos;
	// change sprite countdown
	private float _cdTime;
	// current sprite
	private int _curSpr;
	// callback after walk to target position
	private System.Action<GameObject> _callback;
	// move speed
	private float _speed;
	// SceneController
	private GameObject _sceneController;
	
	public GameObject BangPrefab;
	public GameObject GoldPrefab;
	
	// move step per frame
	public float Step;
	// sprite change cd time
	public float SpriteChangeCD;
	// 人物名称前缀
	public string HumanPrefix;
	
	// Use this for initialization
	void Start () {
		_sceneController = GameObject.Find ("SceneController");
		_initPos = transform.position.x;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (_isWalking) 
		{
			var curPos = transform.position; // current position
			var realStep = _speed * _direction; // step with direction
			curPos.x = curPos.x + realStep;
			transform.position = curPos; // move cat
			if (_direction > 0) // need move to right
			{
				if (curPos.x >= _targetPos) // stop walking
				{
					this.StopWalk();
				}
			}
			else if (_direction < 0) // need move to right
			{
				if (curPos.x <= _targetPos) // stop walking
				{
					this.StopWalk();
				}
			}
			else // don't need to move
			{
				this.StopWalk();
			}
			// deal sprite
			_cdTime = _cdTime + Time.fixedDeltaTime;
			if (_cdTime >= SpriteChangeCD) { // need change another sprite
				_cdTime = 0;
				_curSpr++;
				if (_curSpr > 3) // 总共3张图
				{
					_curSpr = 1;
				}
				TextController.ReplaceSprite(HumanPrefix + _curSpr, this.gameObject);
			}
		}
	}
	
	// cat walk in
	public void WalkIn() 
	{
		int dir = Random.Range (-1, 1); // 人是随机从左右走进来
		if (dir == 0)
		{
			dir = 1;
		}
		float startX = dir * 3.2f; // begin from edge, end in init position
		float endX = _initPos;
		this.StartWalk (startX, endX, Step);
	}
	
	// cat walk out
	public void WalkOut()
	{
		int dir;
		if (_initPos < 0) 
		{
			dir = -1;
		}
		else 
		{
			dir = 1;
		}
		float startX = transform.position.x; // begin from middle, end in edge
		float endX = dir * 3.2f;
		this.StartWalk (startX, endX, Step * 1.5f, new System.Action<GameObject>((go) => GetComponent<Renderer>().enabled = false));
	}
	
	// start walking
	private void StartWalk(float startX, float targetX, float speed, System.Action<GameObject> callback = null) 
	{
		_curSpr = 1;
		TextController.ReplaceSprite(HumanPrefix + _curSpr, this.gameObject); // walk sprite
		_targetPos = targetX; // cur position is target position
		if (_targetPos < startX) // judge direction
		{
			_direction = -1;
		}
		else  
		{
			_direction = 1;
		}
		transform.localScale = new Vector3(_direction, 1, 1); // change scale
		transform.position = new Vector2 (startX, transform.position.y); // change begin position
		// show cat
		GetComponent<Renderer> ().enabled = true;
		_isWalking = true;
		_speed = speed;
		_callback = callback;
	}
	
	// Make a new reward wow
	public void MakeBang(GameObject prefab = null)
	{
		if (prefab == null) 
		{
			prefab = BangPrefab;
		} 
		if (this.GetComponent<Renderer>().enabled == false) 
		{
			return; // if has been hide
		}
		// show wow animation
		TextController.ReplaceSprite(HumanPrefix + "fire", this.gameObject); // 变成开枪样式
		var wow = GameObject.Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
		wow.transform.parent = this.gameObject.transform;
		wow.transform.localPosition = new Vector2 (0, 0.5f);
		_sceneController.GetComponent<AnimatorScript> ().AddRising (wow, 0.5f, 1, new AnimatorDelegate (this.WowCallback));
	}
	
	// Make a new reward wow
	public void MakeGold()
	{
		this.MakeBang (GoldPrefab);
	}
	
	private void WowCallback(GameObject go) 
	{	
		TextController.ReplaceSprite(HumanPrefix + "stop", this.gameObject); // 还原开枪样式
		Destroy (go);
	}
	
	// stop walking
	public void StopWalk() 
	{
		_isWalking = false; // sign
		TextController.ReplaceSprite(HumanPrefix + "stop", this.gameObject); // sit
		if (_callback != null) // callback
		{
			_callback(this.gameObject);
		}
	}

}
