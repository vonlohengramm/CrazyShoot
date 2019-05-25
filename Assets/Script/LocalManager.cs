using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class LocalManager : MonoBehaviour {

	public GameObject ScoreUsCanvas; // 评价提示
	public GameObject EndCanvas; // 结束画面

	[DllImport("__Internal")]
	private static extern void ShowShare(string s);//显示分享弹窗
	[DllImport("__Internal")]
	private static extern void CloseShare();//关闭分享弹窗
	[DllImport("__Internal")] 
	private static extern void shareToWeiXin(string s); // 分享到微信
	[DllImport("__Internal")] 
	private static extern void initWeiXin(); // 初始化微信

	void Start ()
	{
		initWeiXin ();
	}

	//  粉丝页
	public void LikeUS() 
	{
		TouchHandler.PlayClickAudio (); // 按钮点击音效
		Application.OpenURL ("https://www.facebook.com/blstudio1st");
	}

	// 联系我们
	public void ContactUS() 
	{
		TouchHandler.PlayClickAudio (); // 按钮点击音效
		Application.OpenURL ("mailto://ball_lightning@qq.com");
	}
	
	// 评星
	public void ScoreUS()
	{
		TouchHandler.PlayClickAudio (); // 按钮点击音效
		this.HideScoreUsCanvas (false);
		#if UNITY_IPHONE || UNITY_EDITOR
		var url = "http://itunes.apple.com/WebObjects/MZStore.woa/wa/viewContentsUserReviews?id=1147038717&pageNumber=0&sortOrdering=2&type=Purple+Software&mt=8";
		Application.OpenURL(url);
		#endif
	}

	public void ShowRank() 
	{
		TouchHandler.PlayClickAudio (); // 播放按钮声音

		#if (UNITY_IPHONE || UNITY_ANDROID) && !UNITY_EDITOR
		if (Social.Active == null) // 没有排行实现
			return;
		Social.ShowLeaderboardUI ();
		#endif
	}

	//本地推送
	public static void NotificationMessage(string message, int hour, bool isRepeatDay)
	{
		int year = System.DateTime.Now.Year;
		int month = System.DateTime.Now.Month;
		int day = System.DateTime.Now.Day;
		System.DateTime newDate = new System.DateTime(year, month, day, hour, 0, 0);
		NotificationMessage(message,newDate,isRepeatDay);
	}

	//本地推送 你可以传入一个固定的推送时间
	public static void NotificationMessage(string message, System.DateTime newDate, bool isRepeatDay)
	{
//		//推送时间需要大于当前时间
//		if (newDate > System.DateTime.Now)
//		{
		UnityEngine.iOS.LocalNotification localNotification = new UnityEngine.iOS.LocalNotification ();
		localNotification.fireDate = newDate;	
		localNotification.alertBody = message;
		localNotification.applicationIconBadgeNumber = 1;
		localNotification.hasAction = true;
		if (isRepeatDay)
		{
			//是否每天定期循环
			localNotification.repeatInterval = UnityEngine.iOS.CalendarUnit.Day;
		}
		localNotification.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
		UnityEngine.iOS.NotificationServices.ScheduleLocalNotification (localNotification);
//		} 
//		else if (isRepeatDay) // 每天的话晚一天
//		{
//			NotificationMessage(message, newDate.AddDays(1), isRepeatDay);
//		}
	}
	
	void OnApplicationPause(bool paused)
	{
#if UNITY_IPHONE || UNITY_ANDROID
		//程序进入后台时
		if(paused)
		{
			//每天12, 19点推送
			NotificationMessage(UITextScript.GetTextByKey("push_day"), 12, true);
			NotificationMessage(UITextScript.GetTextByKey("push_night"), 19, true);
		}
		else
		{
			//程序从后台进入前台时
			CleanNotification();
		}
		CloseShare ();
#endif
	}

	// 中断了应用
	void OnApplicationFocus(){		
		#if UNITY_IPHONE || UNITY_ANDROID
		CloseShare ();
		#endif
	}
	
	//清空所有本地消息
	void CleanNotification()
	{
		UnityEngine.iOS.LocalNotification l = new UnityEngine.iOS.LocalNotification (); 
		l.applicationIconBadgeNumber = -1; 
		UnityEngine.iOS.NotificationServices.PresentLocalNotificationNow (l); 
		UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications (); 
		UnityEngine.iOS.NotificationServices.ClearLocalNotifications (); 
	}

	// Awake function from Unity's MonoBehavior
	void Awake ()
	{
		//第一次进入游戏的时候清空，有可能用户自己把游戏冲后台杀死，这里强制清空
		CleanNotification();

//		if (!FB.IsInitialized) {
//			// Initialize the Facebook SDK
//			FB.Init(InitCallback);
//		} else {
//			// Already initialized, signal an app activation App Event
//			FB.ActivateApp();
//		}

		string lastlogin = PlayerPrefs.GetString (GameConsts.LOGIN_TODAY_LOCAL, "01/01/1970"); // 最后登录时间
		string lastversion = PlayerPrefs.GetString (GameConsts.LOGIN_VERSION_LOCAL, ""); // 最后适用版本
		int logindays = PlayerPrefs.GetInt (GameConsts.LOGIN_DAYS_LOCAL, 1); // 第几天登陆
		var now = DateTime.Now;
		var lastDay = DateTime.Parse (lastlogin);
		if (!Application.version.Equals(lastversion)) // 变了一个版本
		{
			logindays = 1; // 恢复到第一天登陆
			PlayerPrefs.SetInt(GameConsts.LOGIN_DAYS_LOCAL, logindays);
			PlayerPrefs.SetString(GameConsts.LOGIN_VERSION_LOCAL, Application.version);
		}
		else if (!now.Date.Equals(lastDay.Date)) // 到了新的一天
		{
			logindays++;
			PlayerPrefs.SetInt(GameConsts.LOGIN_DAYS_LOCAL, logindays);
			if (logindays == 2) // 第二天结束弹评价
			{
				PlayerPrefs.SetInt(GameConsts.NEED_SHOW_SCORE_LOCAL, 1);
			}
		} 
		PlayerPrefs.SetString(GameConsts.LOGIN_TODAY_LOCAL, now.ToShortDateString());
		PlayerPrefs.Save();
	}
	
//	private void InitCallback ()
//	{
//		if (FB.IsInitialized) {
//			// Signal an app activation App Event
//			FB.ActivateApp();
//			// Continue with Facebook SDK
//			// ...
//		} else {
//			Debug.Log("Failed to Initialize the Facebook SDK");
//		}
//	}

	public void FBShare() 
	{
		TouchHandler.PlayClickAudio (); // 播放按钮声音
		EndCanvas.transform.Find ("UIFreeButton").gameObject.SetActive (false);
		EndCanvas.transform.Find ("UIShareButton").gameObject.SetActive (false);
		EndCanvas.transform.Find ("UIHomeButton").gameObject.SetActive (false);
		EndCanvas.transform.Find ("UIStartButton").gameObject.SetActive (false);
		EndCanvas.transform.Find ("UIShopButton").gameObject.SetActive (false);
		Application.CaptureScreenshot("Screenshot.png",0); // 截取当前图

		Invoke ("PopUpShare", 0.5f);

//		if (FB.IsInitialized)
//		{
//			FB.LogInWithPublishPermissions(new List<string>(){"publish_actions"}, LogInCallback);
//		}
	}

	private void PopUpShare() 
	{
		EndCanvas.transform.Find ("UIFreeButton").gameObject.SetActive (true);
		EndCanvas.transform.Find ("UIShareButton").gameObject.SetActive (true);
		EndCanvas.transform.Find ("UIHomeButton").gameObject.SetActive (true);
		EndCanvas.transform.Find ("UIStartButton").gameObject.SetActive (true);
		EndCanvas.transform.Find ("UIShopButton").gameObject.SetActive (true);

		string imagePath = ""; // 截图文件的路径
		#if UNITY_IPHONE
		imagePath = Application.persistentDataPath;	
		#elif UNITY_ANDROID
		imagePath = Application.persistentDataPath;		
		#elif UNITY_EDITOR
		imagePath = Application.dataPath;  				
		imagePath = imagePath.Replace("/Assets",null);  	
		#endif
		imagePath = imagePath + "/Screenshot.png";
		
		string shareText = UITextScript.GetTextByKey ("feed");

		#if UNITY_IPHONE
		TouchScreenKeyboard.hideInput = false; 		
		ShowShare (imagePath + "\t" + shareText);
		#endif
	}

	public void WeiXinShare() 
	{
		TouchHandler.PlayClickAudio (); // 播放按钮声音

		string shareText = UITextScript.GetTextByKey ("wechat_txt");
		shareText = shareText.Replace ("{0}", GetComponent<ScoreController> ().CurScore.ToString());

		#if UNITY_IPHONE
		TouchScreenKeyboard.hideInput = false; 		
		shareToWeiXin (shareText);
		#endif
	}

//	private void LogInCallback(ILoginResult result) 
//	{
//		if (FB.IsLoggedIn)
//		{
//			var wwwForm = new WWWForm();
//			byte[] screenBytes = this.GetImage();
//			wwwForm.AddBinaryData("image", screenBytes, "Screenshot.png");
//			wwwForm.AddField("caption", UITextScript.GetTextByKey("feed"));
//			
//			FB.API("me/photos", HttpMethod.POST, ImageCallback, wwwForm);
//		}
//	}

	private byte[] GetImage() 
	{
		string imagePath = ""; // 截图文件的路径
		#if UNITY_IPHONE
			imagePath = Application.persistentDataPath;	
		#elif UNITY_ANDROID
			imagePath = Application.persistentDataPath;		
		#elif UNITY_EDITOR
			imagePath = Application.dataPath;  				
			imagePath = imagePath.Replace("/Assets",null);  
		#endif
		imagePath = imagePath + "/Screenshot.png";
		//创建文件读取流
		FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
		fileStream.Seek(0, SeekOrigin.Begin);
		//创建文件长度缓冲区
		byte[] bytes = new byte[fileStream.Length]; 
		//读取文件
		fileStream.Read(bytes, 0, (int)fileStream.Length);
		//释放文件读取流
		fileStream.Close();
		fileStream.Dispose();
		fileStream = null;
		return bytes;
	}

//	private void ImageCallback(IGraphResult result)
//	{
//		if (FB.IsLoggedIn)
//		{
//			string picId = result.ResultDictionary["id"].ToString();
//			const string APP_ID = "1044752639";
//			var url = new Uri(string.Format(
//				"http://itunes.apple.com/app/id{0}",
//				APP_ID));
//			FB.ShareLink(url, "Angry Shot", "I begin to work hard for shining on basketball court, come on and join me, bro!", new Uri("https://www.facebook.com/photo?fbid=" + picId), ShareCallback);
//		}
//	}

	// 评价弹窗
	public void ShowScoreUsCanvas() 
	{
		TouchHandler.PlayClickAudio (); // 按钮点击音效
		ScoreUsCanvas.SetActive (true);
	}

	// 关闭评价弹窗
	public void HideScoreUsCanvas(bool audio) 
	{
		if (audio)
		{
			TouchHandler.PlayClickAudio (); // 按钮点击音效
		}
		ScoreUsCanvas.SetActive (false);
	}

}
