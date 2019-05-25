using System;
using UnityEngine;
using System.Collections;

public class TimeNumController : MonoBehaviour
{
    // 三个数字显示
    public GameObject num1;
    public GameObject num2;
    public GameObject num3;

    // 旧数字
    private int _oldNum;

	// 本局已经开始计时
	public bool TimeDecreasing;

	// Use this for initialization
	void Start () {
	}

    // 显示数字
    public void ShowNumber(int num)
    {
        if (num == _oldNum) // 完全相同，不更新
        {
            return;
        }
		if (num > _oldNum) { // add time, need play animation
			_oldNum = num;
			GetComponent<AnimatorScript>().AddScale(num1, 0.3f, 1.3f, 1.3f, new AnimatorDelegate(
				(GameObject go) => 
				{
					GetComponent<AnimatorScript>().AddScale(num1, 0.1f, 1, 1);
				}
			));
			GetComponent<AnimatorScript>().AddScale(num2, 0.3f, 1.3f, 1.3f, new AnimatorDelegate(
				(GameObject go) => 
				{
					GetComponent<AnimatorScript>().AddScale(num2, 0.1f, 1, 1);
				}
			));
			GetComponent<AnimatorScript>().AddScale(num3, 0.3f, 1.3f, 1.3f, new AnimatorDelegate(
				(GameObject go) => 
				{
					ChangeNumber(num);
					GetComponent<AnimatorScript>().AddScale(num3, 0.1f, 1, 1);
				}
			));
			return;
		}
		ChangeNumber(num);
    }

	private void ChangeNumber(int num) 
	{
		_oldNum = num;
		if (num > 999) // 超过三位数显示三位数
		{
			num = 999;
		}
		string colorStr = "Green";
		if (num <= 10) // 10以内的数字显示红色
		{
			colorStr = "Red";
		}
		int first = num/100; // 百位
		int second = (num - first*100)/10; // 十位
		int third = num - first*100 - second*10; // 个位
		this.ShowOneNumber(num1, colorStr, first, num > 99); // 分别显示数字
		this.ShowOneNumber(num2, colorStr, second, num > 9);
		this.ShowOneNumber(num3, colorStr, third, true);
	}

    private void ShowOneNumber(GameObject go, string colorStr, int num, bool show)
    {
        go.GetComponent<Renderer>().enabled = show;
		if (!show) // 需不需要显示
			return;
        SpriteRenderer spr = go.GetComponent<SpriteRenderer>();
        Texture2D texture2d = (Texture2D)Resources.Load("Number/" + colorStr + "/" + num);
        Sprite sp = Sprite.Create(texture2d, spr.sprite.textureRect, new Vector2(0.5f, 0.5f)); //注意居中显示采用0.5f值
        spr.sprite = sp;
    }
}
