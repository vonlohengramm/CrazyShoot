using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using System;
using System.Collections;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using Umeng;

public class AdvertisementShower : MonoBehaviour {

	// 充值界面
	public GameObject RechargeCanvas;

	// 给时间还是给金币
	public bool NeedTime = false;

	private InterstitialAd _interstitial;
	private string _adUnitId;

	// Use this for initialization
	void Start () {
		Advertisement.debugLevel = Advertisement.DebugLevel.Info;
		if (Advertisement.isSupported)
		{
			Advertisement.Initialize ("1056080", false);
		}

		#if UNITY_EDITOR
		_adUnitId = "unused";
		#elif UNITY_ANDROID
		_adUnitId = "INSERT_ANDROID_INTERSTITIAL_AD_UNIT_ID_HERE";
		#elif UNITY_IPHONE
		_adUnitId = "ca-app-pub-1055788184715466/3748303835";
		#else
		_adUnitId = "unexpected_platform";
		#endif
	}
	
	// Returns an ad request with custom ad targeting.
	private AdRequest createAdRequest()
	{
		return new AdRequest.Builder ().Build ();
//		return new AdRequest.Builder()
//			.AddTestDevice(AdRequest.TestDeviceSimulator)
//				.AddTestDevice("0123456789ABCDEF0123456789ABCDEF")
//				.AddKeyword("game")
//				.SetGender(Gender.Male)
//				.SetBirthday(new DateTime(1985, 1, 1))
//				.TagForChildDirectedTreatment(false)
//				.AddExtra("color_bg", "9B30FF")
//				.Build();
//		
	}

	public bool IsUnityAdsReady() 
	{
		return Advertisement.IsReady ();
	}

	public void ShowAds(string source = "") 
	{
		if (Advertisement.IsReady ())
		{ 
			Advertisement.Show (null, new ShowOptions {
				resultCallback = result => { // 在回调中奖励，更新ui
					if (result == ShowResult.Finished) 
					{
						if (NeedTime) // 游戏中看视频加10秒
						{
							GetComponent<SceneController>().AddGameTime(11);
							GetComponent<SceneController>().RandomNext();
						}
						else // 游戏外加10金币
						{
							GetComponent<GoldController>().Gold += 10;
							GetComponent<SceneController>().UpdateCanvasGold();
							var goldText = RechargeCanvas.transform.Find("UIGoldText");
							goldText.GetComponent<Text>().text = GetComponent<GoldController>().Gold.ToString();
						}
					}
					GetComponent<SceneController>().HideExtraTime(); // 如果是加时赛，恢复游戏
					NeedTime = false;
				}
			});
		} 
		else
		{
			GetComponent<SceneController>().ShowNoitceCanvas(UITextScript.GetTextByKey("ui_ad"), 2f);
		}

		TouchHandler.PlayClickAudio (); // 按钮点击音效

		GA.Event (source);
	}

	public void LoadAdmob() 
	{
		// Initialize an InterstitialAd.
		_interstitial = new InterstitialAd(_adUnitId);
		// Load the interstitial with the request.
		_interstitial.LoadAd(createAdRequest());
	}

	public void ShowAdmob(string source)
	{
		Invoke ("ShowImm", 0.5f);	
		GA.Event (source);
	}

	public bool IsAdmobLoaded() 
	{
		return _interstitial.IsLoaded ();
	}

	public void ShowImm() {
		Debug.Log ("Show admob:" + _interstitial.IsLoaded());
		if (_interstitial.IsLoaded()) {
			_interstitial.Show();
			this.LoadAdmob ();
		}
	}

}
