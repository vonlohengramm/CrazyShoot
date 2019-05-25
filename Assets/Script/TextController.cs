using UnityEngine;
using System.Collections;

public class TextController : MonoBehaviour
{

    // 提示背景
    public GameObject PromptBackground;
    // 提示文字
    public GameObject PromptText;
    // 变更提示文字
    public GameObject ChangeText;
    // miss背景
    public GameObject WarningBackground;
    // miss文字
    public GameObject WarningText;

    // 消失时间
    public float MaxShowTime;

    // 消失倒计时
    private float _curShowTime;

	// Use this for initialization
	void Start () {
	    this.ShowPrompt(false);
        this.ShowWarning(false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void FixedUpdate()
    {
        if (_curShowTime > 0)
        {
            _curShowTime -= Time.fixedDeltaTime;
            if (_curShowTime <= 0) // 结束显示
            {
                this.ShowPrompt(false);
                this.ShowWarning(false);
            }
        }
    }

    // 替换文字
    public static void ReplaceSprite(string path, GameObject txt)
    {
        SpriteRenderer spr = txt.GetComponent<SpriteRenderer>();
        Texture2D texture2d = (Texture2D)Resources.Load(path);
        if (texture2d == null) return; // 不存在则放弃
		Rect newRect = new Rect (0, 0, 0, 0);
		newRect.width = texture2d.width;
		newRect.height = texture2d.height;
        Sprite sp = Sprite.Create(texture2d, newRect, new Vector2(0.5f, 0.5f)); //注意居中显示采用0.5f值
        spr.sprite = sp;
    }

    // 提示罚丢
    public void ShowMiss()
    {
        // miss文字
		string str = "Text/word_miss" + UITextScript.GetLangSuffix ();
        TextController.ReplaceSprite(str, WarningText);

        // 显示
        this.ShowWarning(true);
        this.ShowPrompt(false);

        // 倒计时
        _curShowTime = MaxShowTime;
    }

    // 提示空心
    public void ShowAirball(int time)
    {
        // airball文字
		string str = "Text/word_cleanshot" + UITextScript.GetLangSuffix ();
		TextController.ReplaceSprite(str, PromptText);
        // 具体时间
		TextController.ReplaceSprite("Number/AddTime/" + time, ChangeText);

        // 显示
        this.ShowPrompt(true);
        this.ShowWarning(false);

        // 倒计时
        _curShowTime = MaxShowTime;
    }

    // 提示超屏球
    public void ShowCrazyShoot()
    {
        // 超屏文字
		string str = "Text/word_angryshot" + UITextScript.GetLangSuffix ();
		TextController.ReplaceSprite(str, PromptText);
        // 三倍文字
		TextController.ReplaceSprite("Text/word_x3", ChangeText);

        // 显示
        this.ShowPrompt(true);
        this.ShowWarning(false);

        // 倒计时
        _curShowTime = MaxShowTime;
    }

    // 提示最后一球
    public void ShowLastBall()
    {
        // 最后一球文字
		string str = "Text/word_lastball" + UITextScript.GetLangSuffix ();
		TextController.ReplaceSprite(str, WarningText);

        // 显示
        this.ShowPrompt(false);
        this.ShowWarning(true);

        // 倒计时
        _curShowTime = MaxShowTime;
    }

    // 显示双倍和三倍
    public void ShowMultiScore(int time)
    {
        // 多倍的文字
		string str = "Text/word_" + time + UITextScript.GetLangSuffix ();
		TextController.ReplaceSprite(str, PromptText);
		TextController.ReplaceSprite("Text/word_x" + time, ChangeText);

        // 显示
        this.ShowPrompt(true);
        this.ShowWarning(false);

        // 倒计时
        _curShowTime = MaxShowTime;
    }

    // 显隐提示文字和背景
    private void ShowPrompt(bool show = true)
    {
        PromptBackground.GetComponent<Renderer>().enabled = show;
        PromptText.GetComponent<Renderer>().enabled = show;
        ChangeText.GetComponent<Renderer>().enabled = show;
    }

    // 显隐时间文字和背景
    private void ShowWarning(bool show = true)
    {
        WarningBackground.GetComponent<Renderer>().enabled = show;
        WarningText.GetComponent<Renderer>().enabled = show;
    }
    
}
