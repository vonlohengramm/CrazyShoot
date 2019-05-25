using System;
using UnityEngine;
using System.Collections;

public class LineCreator : MonoBehaviour {

    // 球的初始位置，用于绘制抛物线
    public float BallInitX; 
    public float BallInitY;

	// 抛物线截止点
	public float LineCutOffX;
    public float LineCutOffY;

    // 屏幕控制器
    public GameObject SceneController;

    // 点的prefab
    public GameObject DotPrefab;
	public GameObject DotLongerPrefab;

    // 绘制中，根据点击状态来确定
    public bool IsPainting;

	// have a longer line prop
	public bool LongerLine;

    // 上次的鼠标位置
    private Vector2 lastPosition;

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void FixedUpdate()
    {
        if (IsPainting) // 需要绘制预测线
        {
            Vector3 mousePosition = Input.mousePosition;
            if (Math.Abs(mousePosition.x - lastPosition.x) < 3 && Math.Abs(mousePosition.y - lastPosition.y) < 3) // 如果跟上次完全一样，就不用重新计算了
            {
                return;
            }
            this.ClearLine(); // 清理旧的线
			this.CreateLine(mousePosition.x / Screen.width * 640 - BallInitX, mousePosition.y / Screen.height * 1136 - BallInitY); // 重新绘制线
        }
    }

    // 开始绘制抛物线
    public void BeginPaint() {
        var controller = SceneController.GetComponent<SceneController>();
        GameObject curThrowObject = controller.CurThrowObject; // 取得当前抛出的物体
        if (curThrowObject == null) // 空的不开始
        {
            return;
        }
        IsPainting = true;
    }

    // 停止绘制抛物线
    public void EndPaint() {
        var controller = SceneController.GetComponent<SceneController>();
        GameObject curThrowObject = controller.CurThrowObject; // 取得当前抛出的物体
        if (curThrowObject == null) // 空的不开始
        {
            return;
        }
        IsPainting = false;
        this.ClearLine(); // 清除线
		LongerLine = false;
    }

    // 绘制完毕，清除所有的绘制线
    public void ClearLine() {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("LinePoint");
        foreach (GameObject obj in objs)
        {
            Destroy(obj);
        }
    }

    // 绘制新的预测线
    public void CreateLine(float mouseX, float mouseY) {
        var controller = SceneController.GetComponent<SceneController>();
        GameObject curThrowObject = controller.CurThrowObject; // 取得当前抛出的物体
        if (curThrowObject == null) // 空的不开始
        {
            return;
        }
        float mass = curThrowObject.GetComponent<Rigidbody2D>().mass;
        float factor = controller.ForceFactor * mass; // 预测的力量因子，根据质量进行加成，保持统一的曲线
        float fanForce = SceneController.GetComponent<WindController>().FanForce * mass; // 风力
        float x0 = curThrowObject.GetComponent<Transform>().position.x; // 初始位置
        float y0 = curThrowObject.GetComponent<Transform>().position.y;
	
        float forceX = mouseX * factor * 0.5f; // 取得初始推力
        float forceY = mouseY * factor;
		float timeCell = Time.fixedDeltaTime / 100; // 时间间隔
		float windSpeed = fanForce / mass; // 风力加速度
		float g = -9.8f; // 重力加速度
        float v0x = forceX / mass + Time.fixedDeltaTime * windSpeed; // 计算初始速度
        float v0y = forceY / mass + Time.fixedDeltaTime * g;

        float lastX = x0;
        float lastY = y0;
        double s = 0;
		
		GameObject prefab = LongerLine ? DotLongerPrefab : DotPrefab;
		float cutx = LongerLine ? LineCutOffX * 10 : LineCutOffX;
		float cuty = LongerLine ? -LineCutOffY * 5 : LineCutOffY;
		
        for (float time = 0; time < 10; time += timeCell) // 最长10秒内预测
        {
            var x = x0 + v0x * time + windSpeed * time * time / 2; // v0t + 1/2at^2
            var y = y0 + v0y * time + g * time * time / 2;
            var disX = Mathf.Abs(x - lastX);
            var disY = Mathf.Abs(y - lastY);
            lastX = x;
            lastY = y;
            s = s + Mathf.Pow(disX * disX + disY * disY, 0.5f);
            if (s > 0.3)
            {
				var dot = Instantiate(prefab, new Vector3(x, y, -0.3f), Quaternion.identity) as GameObject;
                s = 0;
            }
			if (v0y + g * time < cuty) // 到一定程度抛物线就可以停了
            {
                break;
            }
			if (Math.Abs(windSpeed * time) > cutx) // 到一定程度抛物线就可以停了
            {
                break;
            }
        }
    }
}
