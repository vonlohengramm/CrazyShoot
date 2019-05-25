#if UNITY_ANDRIOD && !UNITY_EDITOR
#define ANDROID
#endif

#if UNITY_IPHONE && !UNITY_EDITOR
#define IPHONE
#endif

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class TouchHandler : MonoBehaviour {

    // 屏幕控制器
    public GameObject SceneController;

	// 按钮点击音效
	private static AudioClip ClickButtonAudio;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0)) { // 按下按钮
#if IPHONE || ANDRIOD
			if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) // if touch ui, do nothing
#else
			if (EventSystem.current.IsPointerOverGameObject()) // if touch ui, do nothing
#endif
			{
				return;
			}
            this.OnMouseDown();
        }
		if (Input.GetMouseButtonUp (0)) { // 抬起按钮
#if IPHONE || ANDRIOD
			if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId) && !GetComponent<LineCreator>().IsPainting) // if touch ui, do nothing
#else
			if (EventSystem.current.IsPointerOverGameObject () && !GetComponent<LineCreator> ().IsPainting) // if touch ui, do nothing
#endif 
			{
				int guide = PlayerPrefs.GetInt (GameConsts.GUIDE_LOCAL, 0); // 需要做新手引导
				if (guide == 0 && SceneController.GetComponent<SceneController>().NoticeCanvas.activeSelf)
				{
					SceneController.GetComponent<SceneController> ().HideNoticeCanvas ();
				}
				return;
			} 
			this.OnMouseUp ();
		}
	}
        
    void OnMouseDown() {      
        // 进入预测线绘制状态
        var creator = GetComponent<LineCreator>();
        var controller = SceneController.GetComponent<SceneController>();
        GameObject curThrowObject = controller.CurThrowObject; // 取得当前抛出的物体
        if (curThrowObject == null) return; // 还没物体先不管
        // 调整小人和物体位置
		TextController.ReplaceSprite("Role/2_" + curThrowObject.tag, SceneController.GetComponent<SceneController>().Role);
        if (curThrowObject.tag == "Basket")
        {
            curThrowObject.transform.position = new Vector3(curThrowObject.transform.position.x - 0.07f, curThrowObject.transform.position.y + 0.16f);
        }
        else
        {
            curThrowObject.transform.position = new Vector3(curThrowObject.transform.position.x - 0.31f, curThrowObject.transform.position.y + 0.12f);
        }
        creator.BeginPaint(); // 开始绘制抛物线
    }

    private void RoleCallback(GameObject go)
    {
        var role = SceneController.GetComponent<SceneController>().Role;
		TextController.ReplaceSprite("Role/3", role);
        SceneController.GetComponent<AnimatorScript>().AddRising(role, 0.3f, -0.5f);
    }

    void OnMouseUp() {
        if (!GetComponent<LineCreator>().IsPainting) // 还没画就不用处理了
        {
            return;
        }

        // 结束预测线绘制状态
        var creator = GetComponent<LineCreator>();
        creator.EndPaint();

        // 调整小人，播放动画
        var role = SceneController.GetComponent<SceneController>().Role;
		TextController.ReplaceSprite("Role/3", role);
        SceneController.GetComponent<AnimatorScript>().AddRising(role, 0.3f, 0.5f, new AnimatorDelegate(this.RoleCallback));

        // 抛出球
        var controller = SceneController.GetComponent<SceneController>();
        controller.ThrowCurrent();           
    }

	// 播放按钮点击音效
	public static void PlayClickAudio() 
	{
		if (ClickButtonAudio == null) 
		{
			ClickButtonAudio = Resources.Load<AudioClip> ("Audio/Button");
		}
		AudioSource.PlayClipAtPoint (ClickButtonAudio, new Vector2 (0, 0));
	}

}
