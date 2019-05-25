using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BoardScript : MonoBehaviour {

    // 场景控制器
    public GameObject SceneController;

    // 多倍点亮显示
    public GameObject[] AirBound;

    // 框
    public GameObject Basket;

    // 挂筐线
    public GameObject Line;

    // 显示了火焰
    private bool _flameShowed;

    // 挂筐线闪烁
    private bool _flashLine;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if (_flashLine)
        {
            var visible = ((int)(Time.time * 6) % 2) == 0; // 闪烁
            var color = Line.GetComponent<SpriteRenderer>().color;
            color.a = visible ? 1 : 0.5f;
            Line.GetComponent<SpriteRenderer>().color = color;
        }
	}

    // 进行多次空心的亮框显示
    public void ShowAirBound(int num)
    {
        for (int i = 0; i < 3; i++)
        {
            AirBound[i].GetComponent<Renderer>().enabled = i < num;
        }
        this.ShowFlame(num >= 3);
    }

    // 显示火焰效果
    private void ShowFlame(bool show)
    {
        if (show == _flameShowed) return; // 不变不处理
        if (show)
        {
            // 篮板火焰
            SceneController.GetComponent<AnimatorScript>().AddFlame(this.name + "Flame", this.gameObject, 2.3f, new Vector2(0, -0.6f));
        }
        else
        {
            // 移除火焰
            SceneController.GetComponent<AnimatorScript>().RemoveFlame(this.name + "Flame");
        }
        _flameShowed = show;
    }
    
    // 弹增加的分数
    public void ShowAddScore(int score)
    {
        var text = transform.Find("Text").gameObject; // 获取文字
        text.GetComponent<Renderer>().enabled = true;
		TextController.ReplaceSprite("Number/AddScore/" + score, text); // 调整显示文字
        SceneController.GetComponent<AnimatorScript>().AddRising(text, 0.5f, 1, new AnimatorDelegate(this.ScoreCallback));
    }

    // 回调函数
    private void ScoreCallback(GameObject go)
    {
        go.transform.localPosition = new Vector3(0, 0.6f, 0); // 还原位置
        go.GetComponent<Renderer>().enabled = false; // 重新隐藏
    }

    // 闪烁挂筐
    public void FlashLine(bool flash)
    {
        flash = flash && Basket == null;
        if (_flashLine == flash) return; // 无变化
        _flashLine = flash;
        Line.GetComponent<Renderer>().enabled = flash;
    }

}
