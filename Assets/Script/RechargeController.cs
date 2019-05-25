using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RechargeController : MonoBehaviour {

	public GameObject RechargeCanvas; // 充值界面

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ShowRechargeCanvas() 
	{
		GetComponent<SceneController> ().ShowOneUI ("Recharge");
		TouchHandler.PlayClickAudio (); // 播放按钮声音
	}
}
