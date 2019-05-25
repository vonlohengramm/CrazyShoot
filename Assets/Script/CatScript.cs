using UnityEngine;
using System.Collections;

public delegate void CatWalkCallback(GameObject go);

public class CatScript : MonoBehaviour {

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
	private CatWalkCallback _callback;
	// move speed
	private float _speed;
	// SceneController
	private GameObject _sceneController;

	public GameObject WowPrefab;
	public GameObject GoldPrefab;

	// move step per frame
	public float Step;
	// sprite change cd time
	public float SpriteChangeCD;

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
				_curSpr = 3 - _curSpr; // change spr
				TextController.ReplaceSprite("Cat/walk_" + _curSpr, this.gameObject);
			}
		}
	}

	// cat walk in
	public void WalkIn() 
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
		this.StartWalk (startX, endX, Step * 1.5f, new CatWalkCallback((go) => GetComponent<Renderer>().enabled = false));
	}

	// start walking
	private void StartWalk(float startX, float targetX, float speed, CatWalkCallback callback = null) 
	{
		_curSpr = 1;
		TextController.ReplaceSprite("Cat/walk_" + _curSpr, this.gameObject); // walk sprite
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
	public void MakeWOW(GameObject prefab = null)
	{
		if (prefab == null) 
		{
			prefab = WowPrefab;
		}
		if (this.GetComponent<Renderer>().enabled == false) 
		{
			return; // if has been hide
		}
		// show wow animation
		var wow = GameObject.Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
		wow.transform.parent = this.gameObject.transform;
		wow.transform.localPosition = new Vector2 (0, 0);
		_sceneController.GetComponent<AnimatorScript> ().AddRising (wow, 0.5f, 1, new AnimatorDelegate (this.WowCallback));
	}

	// Make a new reward wow
	public void MakeGold()
	{
		this.MakeWOW (GoldPrefab);
	}

	private void WowCallback(GameObject go) 
	{	
		Destroy (go);
	}

	// stop walking
	public void StopWalk() 
	{
		_isWalking = false; // sign
		TextController.ReplaceSprite("Cat/sit", this.gameObject); // sit
		if (_callback != null) // callback
		{
			_callback(this.gameObject);
		}
	}

}
