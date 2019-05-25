using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WindController : MonoBehaviour
{
    // 风力向左
    public GameObject[] LeftWind;
    // 风力向右
    public GameObject[] RightWind;
    // 风力背景板
    public GameObject WindBoard;
	// Wind Mask
	public GameObject WindMask;

	// 风力概率
	public float[] FanForceWeight;

	// 风力
	private float _fanForce;
	public float FanForce 
	{
		set 
		{
			_fanForce = value;
		}
		get
		{
			if (this.IsWindMask()) // wind shield, return 0;
			{
				return 0;
			}
			return _fanForce;
		}
	}

	// 本局风力次数
	private int _windTimes;

	// Use this for initialization
	void Start () {
		_windTimes = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// 重启游戏
	public void RestartGame() 
	{
		_windTimes = 0;
	}

	// 出一次风力，返回风力值
	public void NextWind() 
	{
		this.ChangeWindMask (false); // destroy wind shield
		_windTimes++; 
		_fanForce = 0;
		int forceNum = 0;
		int fanSign = 1;
		if (_windTimes > 5) // 前五次没有风
		{
			var forceRandom = Random.value; // 风力随机
			float weight = GetComponent<ScoreController> ().CurScore;
			if (weight > 50) 
			{
				weight = 50;
			}
			weight /= 100f; // 换算成百分比
			if (forceRandom < weight) 
			{
				forceNum = Random.Range(1, 4); // 随机一种风
			}
			fanSign = Random.Range(-1, 1); // 风力的方向
			if (fanSign == 0)
			{
				fanSign = 1;
			}
			_fanForce = forceNum * 0.4f * fanSign;
		}
		this.ChangeWind(forceNum, fanSign);
	}

	// Mask Wind or false
	public void ChangeWindMask(bool mask) 
	{
		WindMask.GetComponent<Renderer> ().enabled = mask;
	}

	// is wind mask or false
	public bool IsWindMask() 
	{
		return WindMask.GetComponent<Renderer> ().enabled;
	}

    // 更改风力
    public void ChangeWind(int level, int sign)
    {
        WindBoard.GetComponent<Renderer>().enabled = level > 0; // 无风时背景隐藏
        for (var i = 0; i < 3; i++)
        {
            var left = LeftWind[i]; // 调整左风
            if (sign < 0 && level > 0) // 左风时
            {
                left.GetComponent<Renderer>().enabled = true;
                if (3 - i > level) // 浅色部分
                {
					TextController.ReplaceSprite("Wind/left2", left);
                }
                else // 深色部分
                {
					TextController.ReplaceSprite("Wind/left1", left);
                }
            }
            else
            {
                left.GetComponent<Renderer>().enabled = false; // 需要隐藏
            }

            var right = RightWind[i];
            if (sign > 0 && level > 0) // 左风时
            {
                right.GetComponent<Renderer>().enabled = true;
                if (i + 1 > level) // 浅色部分
                {
					TextController.ReplaceSprite("Wind/right2", right);
                }
                else // 深色部分
                {
					TextController.ReplaceSprite("Wind/right1", right);
                }
            }
            else
            {
                right.GetComponent<Renderer>().enabled = false; // 需要隐藏
            }
        }
    }
}
