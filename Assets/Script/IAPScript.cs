using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class IAPScript : MonoBehaviour {

	public GameObject RechargeCanvas;
	public AudioClip CoinAudio;
	
	[DllImport("__Internal")]
	private static extern void TestMsg();//测试信息发送
	
	[DllImport("__Internal")]
	private static extern void TestSendString(string s);//测试发送字符串
	
	[DllImport("__Internal")]
	private static extern void TestGetString();//测试接收字符串
	
	[DllImport("__Internal")]
	private static extern void InitIAPManager();//初始化
	
	[DllImport("__Internal")]
	private static extern bool IsProductAvailable();//判断是否可以购买
	
	[DllImport("__Internal")]
	private static extern void RequstProductInfo(string s);//获取商品信息
	
	[DllImport("__Internal")]
	private static extern void BuyProduct(string s);//购买商品

	
	//测试从xcode接收到的字符串
	void IOSToU(string s){
		Debug.Log ("[MsgFrom ios]"+s);
		string[] p = s.Split ('\t');
	}

	// 本土化价格
	void ChangeLocalPrice(string s)
	{
		Debug.Log ("[ChangeLocalPrice:]"+s);
		string[] p = s.Split ('\t');
		switch (p[2])
		{
		case "as_gold2_1":
			RechargeCanvas.transform.Find("UI1/Button/UImoneyText").GetComponent<Text>().text = p[1];
			break;
		case "as_gold2_10":
			RechargeCanvas.transform.Find("UI10/Button/UImoneyText").GetComponent<Text>().text = p[1];
			break;
		case "as_gold2_100":
			RechargeCanvas.transform.Find("UI100/Button/UImoneyText").GetComponent<Text>().text = p[1];
			break;
		}
	}
	
	//获取商品回执
	void ProvideContent(string s){
		Debug.Log ("[MsgFrom ios]proivideContent : "+s);
		switch (s)
		{
		case "as_gold2_1":
			GetComponent<GoldController>().Gold += 50;
			break;
		case "as_gold2_10":
			GetComponent<GoldController>().Gold += 600;
			break;
		case "as_gold2_100":
			GetComponent<GoldController>().Gold += 10000;
			break;
		}
		var goldText = RechargeCanvas.transform.Find("UIGoldText");
		goldText.GetComponent<Text>().text = GetComponent<GoldController>().Gold.ToString();
		AudioSource.PlayClipAtPoint(CoinAudio, new Vector2(0, 0));
		GetComponent<SceneController> ().HideNoticeCanvas ();
	}

	// 内购取消
	void CancelIap()
	{
		GetComponent<SceneController> ().HideNoticeCanvas ();
	}
	
	
	// Use this for initialization
	void Start () {
		InitIAPManager();
		if (IsProductAvailable ())
		{
			RequstProductInfo ("as_gold2_1\tas_gold2_10\tas_gold2_100");
		}
	}

	// 购买金币
	public void BuyGold(string id) 
	{
		TouchHandler.PlayClickAudio (); // 播放按钮声音
		// 显示提示框
		GetComponent<SceneController>().ShowNoitceCanvas(UITextScript.GetTextByKey("window_waiting"), 30);
		if (IsProductAvailable ())
		{
			BuyProduct (id);
		}
	}

}
