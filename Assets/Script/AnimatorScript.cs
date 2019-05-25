using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void AnimatorDelegate(GameObject go);

public class AnimatorScript : MonoBehaviour {

	// scale set
	private Dictionary<GameObject, ScaleInfo> _scaleSet = new Dictionary<GameObject, ScaleInfo> ();
    // 上升的集合，位置和集合
    private Dictionary<GameObject, RisingInfo> _risingSet;
    // 需要删除的上升
    private HashSet<GameObject> _needDelete;
    // 火焰集合
    private Dictionary<string, FlameInfo> _flameSet;
	// 流星集合
	private Dictionary<GameObject, FallingStarInfo> _fallingStarSet;
    // 火焰prefab
    public GameObject FlamePrefab;
    // 火焰初始颜色
    public Color FlameStartColor;
    // 火焰终止颜色
    public Color FlameEndColor;
	// 流星所使用的素材
	public GameObject[] FallingStar;

	// Use this for initialization
	void Start () {
        _risingSet = new Dictionary<GameObject, RisingInfo>();
        _needDelete = new HashSet<GameObject>();
        _flameSet = new Dictionary<string, FlameInfo>();
		_fallingStarSet = new Dictionary<GameObject, FallingStarInfo> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (Time.timeScale == 0) // pause game
		{
			return;
		}
	    foreach (var r in _risingSet) {
            var go = r.Key; // 移动位置
            if (go == null)
            {
                continue;
            }
            go.transform.position = new Vector3(go.transform.position.x, go.transform.position.y + r.Value.GetStep(), go.transform.position.z);
            if (!r.Value.AddCurTime(Time.fixedDeltaTime)) // 已经超过了时间，待删除
            {
                _needDelete.Add(go);
            }
        }
        foreach (var d in _needDelete) // 删除所有已完成的
        {
            var info = _risingSet[d];
            _risingSet.Remove(d);
            if (info != null && info.GetCallback() != null) // 需要进行回调操作
            {
                info.GetCallback()(d);
            }             
        }
        _needDelete.Clear();

        foreach (var f in _flameSet) // 火焰特效
        {
            var info = f.Value;
            if (info.GO.Count > 0)
            {
                foreach (var g in info.GO) // 每个火焰变色
                {
                    if (g.Key == null)
                    {
                        continue;
                    }
                    float timeDistance = Time.time - g.Value;
                    g.Key.GetComponent<SpriteRenderer>().color = Color.Lerp(FlameStartColor, FlameEndColor, timeDistance / 2);
                }         
            }               
            // 制造新的火焰
            if ((int)(Time.time * 10) % 3 == 0)
            {
                var flame = info.AddGO(FlamePrefab, FlameStartColor);
                this.AddRising(flame, 1f, 1.2f, new AnimatorDelegate(this.FlameCallback)); // 上升动画
            }            
        }


		foreach (var v in _scaleSet.Values) 
		{
			v.CurTime += Time.deltaTime; // update time
			var curScale = v.Go.transform.localScale; // current scale 
			if (Mathf.Abs(v.ScaleStepX) > 1e-7) // need scale X
			{
				curScale.x = curScale.x + v.ScaleStepX;
			}	
			if (Mathf.Abs(v.ScaleStepY) > 1e-7) // need scale Y
			{
				curScale.y = curScale.y + v.ScaleStepY;
			}
			if (v.CurTime >= v.TotalTime) // the last, change to final
			{
				curScale.x = v.TargetScaleX;
				curScale.y = v.TargetScaleY;
				_needDelete.Add(v.Go);
			}
			v.Go.transform.localScale = curScale; 
		}
		foreach (var d in _needDelete) // 删除所有已完成的
		{
			var info = _scaleSet[d];
			_scaleSet.Remove(d);
			if (info != null && info.ScaleCallback != null) // 需要进行回调操作
			{
				info.ScaleCallback(d);
			}             
		}
		_needDelete.Clear ();

		// 流星效果
		foreach (var v in _fallingStarSet.Values)
		{
			foreach (var o in v.Stars.Keys) // 超时的删除
			{
				if (Mathf.Abs(Time.time - v.Stars[o]) >= v.StarDuration) 
				{
					_needDelete.Add(o);
				}
			}
			foreach (var d in _needDelete) 
			{
				v.Stars.Remove(d);
				Destroy(d);
			}
			_needDelete.Clear();
			// 生成新的星星
			v.NewTime = v.NewTime + Time.fixedDeltaTime;
			if (v.NewTime > 0.01f) {
				v.NewTime = 0;
				var goPos = v.Go.transform.position;
				var prefab = FallingStar[Random.Range(0, FallingStar.Length)]; // 选定星星
				Vector2 pos = Random.insideUnitCircle * 0.21f;
				var star = GameObject.Instantiate(prefab, new Vector2(goPos.x + pos.x, goPos.y + pos.y), Quaternion.identity) as GameObject; // 生成新图
				v.Stars[star] = Time.time;
			}
		}
	}

	public void AddScale(GameObject go, float time, float targetScaleX, float targetScaleY, AnimatorDelegate callback = null) 
	{
		var curScale = go.transform.localScale;
		float scaleStepX = 0;
		float scaleStepY = 0;
		if (Mathf.Abs(targetScaleX - scaleStepX) > 1e-7) {
			scaleStepX = (targetScaleX - curScale.x) / (time / Time.deltaTime);
		} 
		if (Mathf.Abs(targetScaleY - scaleStepY) > 1e-7) {
			scaleStepY = (targetScaleY - curScale.y) / (time / Time.deltaTime);
		} 
		var info = new ScaleInfo (go, time, scaleStepX, scaleStepY, targetScaleX, targetScaleY, callback);
		_scaleSet [go] = info;
	}

    private void FlameCallback(GameObject go)
    {
        if (go != null) 
        {
            Destroy(go); // 销毁
        }        
    }

    // 增加一个新的动画
    public void AddRising(GameObject go, float time, float distance, AnimatorDelegate callback = null)
    {
        RisingInfo info = new RisingInfo(time, distance, callback);
        _risingSet[go] = info;
    }

    // 删除一个动画
    public void RemoveRising(GameObject go)
    {
        _risingSet.Remove(go);
    }

    // 火焰上升效果
    public void AddFlame(string signName, GameObject back, float width, Vector2 initPos)
    {
        FlameInfo info = new FlameInfo(back, width, initPos);
        _flameSet[signName] = info;
    }

    // 删除一个火焰效果
    public void RemoveFlame(string signName)
    {
        var info = _flameSet[signName];
        _flameSet.Remove(signName);
        if (info != null)  // 需要清除已设置好的火焰效果
        {
            foreach (var g in info.GO.Keys) {
                Destroy(g);
            }
        }
    }

	// 流星效果
	public void AddFallingStar(GameObject go, float durationTime) 
	{
		var info = new FallingStarInfo (go, durationTime);
		_fallingStarSet[go] = info;
	}

	public void RemoveFallingStar(GameObject go) 
	{
		if (go == null || !_fallingStarSet.ContainsKey (go))
			return;
		var info = _fallingStarSet [go];
		_fallingStarSet.Remove (go);
		if (info != null) // 清除已存在的火星
		{
			foreach (var star in info.Stars.Keys) 
			{
				Destroy(star);
			}
		}
	}

}

// 流星效果
class FallingStarInfo 
{
	public GameObject Go; // 需要增加流星的目标	
	public float StarDuration; // 星星持续时间
	public Dictionary<GameObject, float> Stars; // 还存在的星星
	public float NewTime; // 新星星间隔时间

	public FallingStarInfo(GameObject go, float starDuration) 
	{
		Go = go;
		StarDuration = starDuration;
		Stars = new Dictionary<GameObject, float> ();
		NewTime = 0;
	}
}

// scale info
class ScaleInfo {
	public GameObject Go; // scale object
	public float TotalTime; // scale animation time
	public float ScaleStepX; // scale X change per frame
	public float ScaleStepY; // scale Y change per frame
	public float TargetScaleX; // target scale size
	public float TargetScaleY; // target scale size
	public AnimatorDelegate ScaleCallback; // callback
	public float CurTime; // time after scale begin

	public ScaleInfo(GameObject go, float totalTime, float scaleStepX, float scaleStepY, float targetScaleX, float targetScaleY, AnimatorDelegate scaleCallback) 
	{
		Go = go;
		TotalTime = totalTime;
		TargetScaleX = targetScaleX;
		TargetScaleY = targetScaleY;
		ScaleCallback = scaleCallback;
		ScaleStepX = scaleStepX;
		ScaleStepY = scaleStepY;
		CurTime = 0;
	}
}

// 火焰信息
class FlameInfo {

    public Dictionary<GameObject, float> GO; // 上升中的火焰物体
    public GameObject Back; // 父节点
    public float Width; // 火焰宽度
    public Vector2 InitPos; // 初始位置（下中）

    public FlameInfo(GameObject back, float width, Vector2 initPos) {
        Back = back;
        Width = width;
        InitPos = initPos;
        GO = new Dictionary<GameObject, float>();
    }

    // 增加一个新的火焰点
    public GameObject AddGO(GameObject flamePrefab, Color beginColor) 
    {
        var g = GameObject.Instantiate(flamePrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject; // 生成新图
        GO[g] = Time.time; // 记录
        g.transform.parent = Back.transform; // 调整层级
        var x = Random.Range(-Width / 2, Width / 2) + InitPos.x; // 水平位置随机
        g.transform.localPosition = new Vector3(x, InitPos.y); // 调整位置
        var scale = Random.Range(1, 2.5f); // 大小随机
        g.transform.localScale = new Vector3(scale, scale);
        g.GetComponent<SpriteRenderer>().color = beginColor; // 初始颜色
        return g;
    }

}

// 上升的信息
class RisingInfo
{

    private float _curTime; // 当前时间
    private float _time; // 总时间
    private float _distance; // 移动总距离
    private float _step; // 距离步长
    private AnimatorDelegate _callback; // 回调

    public RisingInfo(float time, float distance, AnimatorDelegate callback)
    {
        _time = time;
        _distance = distance;
        _step = distance / (time / Time.deltaTime);
        _callback = callback;
    }

    public float GetCurTime()
    {
        return _curTime;
    }

    public bool AddCurTime(float t)
    {
        _curTime += t;
        return _curTime <= _time;
    }

    public float GetTime()
    {
        return _time;
    }

    public float GetDistance()
    {
        return _distance;
    }

    public float GetStep()
    {
        return _step;
    }

    public AnimatorDelegate GetCallback()
    {
        return _callback;
    }

}