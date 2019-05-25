using UnityEngine;
using System.Collections;

public class ShakeScreen : MonoBehaviour {

	private bool _isShaking;

	// 震动屏幕
	public bool IsShaking
	{
		set
		{
			if (_isShaking != value) 
			{
				_isShaking = value;
				_time = 0;
				// Shaking Audio
				if (GetComponent<AudioSource>() == null) 
				{
					return;
				}
				if (_isShaking) 
				{
					GetComponent<AudioSource>().Play();
				}
				else
				{
					GetComponent<AudioSource>().Stop();
				}
			}
		}
	}
	// 震动调整
	private Vector3 deltaPos = Vector3.zero;

	public float Factor = 0.1f;

	public float Interval = 0f;

	private float _time = 0;
	
	// Use this for initialization
	void Start () {
	
	}

	public void InvokeUpdate() 
	{
		Invoke ("FixedUpdate", 0.02f);
	}
	
	// Update is called once per frame
	void Update () {
		if (Interval > 0) // 需要做出间隔效果的情况
		{
			_time += Time.fixedDeltaTime;
			if (_time > Interval)
			{
				IsShaking = !_isShaking;
				_time = 0;
			}
		}

		if (!_isShaking) return;

		transform.localPosition -= deltaPos; // 在圆周范围内震动
		deltaPos = Random.insideUnitSphere * Factor;
		transform.localPosition += deltaPos;
	}

}
