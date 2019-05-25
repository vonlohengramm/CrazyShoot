using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Umeng;

public class GoldController : MonoBehaviour {

	public GameObject GameCanvas;
	public GameObject ShopCanvas;

	// player gold
	private int[] _gold;
	public int Gold
	{
		get 
		{
			if (_gold == null) 
			{
				this.init();
			}
			return Utils.DecodeNum(_gold);
		}
		set 
		{
			if (_gold !=null && Gold == value) return;
			_gold = Utils.EncodeNum(value);
			PlayerPrefs.SetInt(GameConsts.GOLD_LOCAL, value); // save to local
			PlayerPrefs.Save();
			GameCanvas.transform.Find("UIGoldText").GetComponent<Text>().text = value.ToString(); // show ui
			ShopCanvas.transform.Find("UIGoldText").GetComponent<Text>().text = value.ToString();
		}
	}

	// prop basket
	private int[] _basket;
	public int Basket
	{
		get 
		{
			return Utils.DecodeNum(_basket);
		}
		set 
		{
			if (_basket != null && Basket == value) return;
			_basket = Utils.EncodeNum(value);
			PlayerPrefs.SetInt(GameConsts.PROP_BASKET_LOCAL, value); // save to local
			PlayerPrefs.Save();
			var text = ShopCanvas.transform.Find("Basket/Icon/Text").gameObject; // change ui
			text.GetComponent<Text>().text = value.ToString();
			var gameText = GameCanvas.transform.Find("UIBasketButton/Text").gameObject;
			gameText.GetComponent<Text>().text = value.ToString();
			var numBack = GameCanvas.transform.Find ("UIBasketButton/Image"); // 自动适配底框宽度
			numBack.GetComponent<RectTransform>().sizeDelta = new Vector2(value.ToString().Length * 10 + 30, 40);
			this.RefreshButtonStatus();
		}
	}

	// prop longer line
	private int[] _longerline;
	public int LongerLine
	{
		get 
		{
			return Utils.DecodeNum(_longerline);
		}
		set 
		{
			if (_longerline != null && LongerLine == value) return;
			_longerline = Utils.EncodeNum(value);
			PlayerPrefs.SetInt(GameConsts.PROP_LONGER_LINE_LOCAL, value);
			PlayerPrefs.Save();
			var text = ShopCanvas.transform.Find("LongerLine/Icon/Text").gameObject; // change ui
			text.GetComponent<Text>().text = value.ToString();
			var gameText = GameCanvas.transform.Find("UILongerLineButton/Text").gameObject;
			gameText.GetComponent<Text>().text = value.ToString();
			var numBack = GameCanvas.transform.Find ("UILongerLineButton/Image"); // 自动适配底框宽度
			numBack.GetComponent<RectTransform>().sizeDelta = new Vector2(value.ToString().Length * 10 + 30, 40);
			this.RefreshButtonStatus();
		}
	}

	// prop wind shield
	private int _windshield = -1;
	public int WindShield
	{
		get 
		{
			return _windshield;
		}
		set 
		{
			if (_windshield == value) return;
			_windshield = value;
			PlayerPrefs.SetInt(GameConsts.PROP_WIND_SHILED_LOCAL, value);
			PlayerPrefs.Save();
			var text = ShopCanvas.transform.Find("WindShield/Icon/Text").gameObject; // change ui
			text.GetComponent<Text>().text = _windshield.ToString();
			var gameText = GameCanvas.transform.Find("UIWindSheildButton/Text").gameObject;
			gameText.GetComponent<Text>().text = _windshield.ToString();
			var numBack = GameCanvas.transform.Find ("UIWindSheildButton/Image"); // 自动适配底框宽度
			numBack.GetComponent<RectTransform>().sizeDelta = new Vector2(_windshield.ToString().Length * 10 + 30, 40);
			this.RefreshButtonStatus();
		}
	}

	void Awake() 
	{
		this.init ();
	}

	private void init() 
	{
		int initSign = PlayerPrefs.GetInt (GameConsts.GAME_INIT_LOCAL, 0); // 初始化标记
		if (initSign <= 0) // 需要进行初始化
		{
			Gold = 200; // 初始200金币
			Basket = 5; // 初始5个道具
			LongerLine = 5;
			WindShield = 5;

			GetComponent<SceneController>().AudioPause = false;
			
			PlayerPrefs.SetInt(GameConsts.GAME_INIT_LOCAL, 1); // 标记初始化完成
			PlayerPrefs.Save();
		} 
		else
		{
			Gold = PlayerPrefs.GetInt(GameConsts.GOLD_LOCAL);
			Basket = PlayerPrefs.GetInt(GameConsts.PROP_BASKET_LOCAL);
			LongerLine = PlayerPrefs.GetInt(GameConsts.PROP_LONGER_LINE_LOCAL);
			WindShield = PlayerPrefs.GetInt(GameConsts.PROP_WIND_SHILED_LOCAL);
		}
	}

	public void ShowShopCanvas() 
	{
		// basket
		Item basketConfig = ItemConfig.GetItem (2001);
		var basket = ShopCanvas.transform.Find ("Basket").gameObject;
		var titleText = basket.transform.Find("TitleText").gameObject; // name
		titleText.GetComponent<Text> ().text = basketConfig.name;
		var descText = basket.transform.Find ("DescText").gameObject; // desc
		descText.GetComponent<Text> ().text = basketConfig.desc;

		// longer line
		Item longerLineConfig = ItemConfig.GetItem (2003);
		var longerLine = ShopCanvas.transform.Find ("LongerLine").gameObject;
		titleText = longerLine.transform.Find("TitleText").gameObject; // name
		titleText.GetComponent<Text> ().text = longerLineConfig.name;
		descText = longerLine.transform.Find ("DescText").gameObject; // desc
		descText.GetComponent<Text> ().text = longerLineConfig.desc;

		// wind shield
		Item windShieldConfig = ItemConfig.GetItem (2004);
		var windShield = ShopCanvas.transform.Find ("WindShield").gameObject;
		titleText = windShield.transform.Find("TitleText").gameObject; // name
		titleText.GetComponent<Text> ().text = windShieldConfig.name;
		descText = windShield.transform.Find ("DescText").gameObject; // desc
		descText.GetComponent<Text> ().text = windShieldConfig.desc;

		this.RefreshButtonStatus ();

		GetComponent<SceneController> ().ShowOneUI ("Shop");

		TouchHandler.PlayClickAudio (); // 按钮点击音效
	}

	// judge button enabled or false by gold enough
	private void RefreshButtonStatus() 
	{
		// basket 
		var basketButton = ShopCanvas.transform.Find ("Basket/BuyButton").gameObject;
		basketButton.GetComponent<Button> ().interactable = ItemConfig.GetItem (2001).price <= Gold;
		var basketButton10 = ShopCanvas.transform.Find ("Basket/BuyButton10").gameObject;
		basketButton10.GetComponent<Button> ().interactable = ItemConfig.GetItem (2001).price * 10 <= Gold;
		var basketNumBack = ShopCanvas.transform.Find ("Basket/Icon/Image"); // 自动适配底框宽度
		basketNumBack.GetComponent<RectTransform>().sizeDelta = new Vector2(Basket.ToString().Length * 10 + 30, 40);
		// longer line
		var longerLineButton = ShopCanvas.transform.Find ("LongerLine/BuyButton").gameObject;
		longerLineButton.GetComponent<Button> ().interactable = ItemConfig.GetItem (2003).price <= Gold;
		var longerLineButton10 = ShopCanvas.transform.Find ("LongerLine/BuyButton10").gameObject;
		longerLineButton10.GetComponent<Button> ().interactable = ItemConfig.GetItem (2003).price * 10 <= Gold;
		var longerLineNumBack = ShopCanvas.transform.Find ("LongerLine/Icon/Image"); // 自动适配底框宽度
		longerLineNumBack.GetComponent<RectTransform>().sizeDelta = new Vector2(LongerLine.ToString().Length * 10 + 30, 40);
		// wind shield
		var windShieldButton = ShopCanvas.transform.Find ("WindShield/BuyButton").gameObject;
		windShieldButton.GetComponent<Button> ().interactable = ItemConfig.GetItem (2004).price <= Gold;
		var windShieldButton10 = ShopCanvas.transform.Find ("WindShield/BuyButton10").gameObject;
		windShieldButton10.GetComponent<Button> ().interactable = ItemConfig.GetItem (2004).price * 10 <= Gold;
		var windShieldNumBack = ShopCanvas.transform.Find ("WindShield/Icon/Image"); // 自动适配底框宽度
		windShieldNumBack.GetComponent<RectTransform>().sizeDelta = new Vector2(WindShield.ToString().Length * 10 + 30, 40);
	}

	public void BuyBasket(int num) 
	{
		int price = ItemConfig.GetItem (2001).price * num;
		if (Gold < price) // gold not enough
			return;
		Basket += num;
		Gold -= price;

		GA.Buy ("Basket", num, price);

		TouchHandler.PlayClickAudio (); // 按钮点击音效
	}

	public void BuyLongerLine(int num) 
	{
		int price = ItemConfig.GetItem (2003).price * num;
		if (Gold < price) // gold not enough
			return;
		LongerLine += num;
		Gold -= price;

		GA.Buy ("LongerLine", num, price);

		TouchHandler.PlayClickAudio (); // 按钮点击音效
	}

	public void BuyWindShield(int num) 
	{
		int price = ItemConfig.GetItem (2004).price * num;
		if (Gold < price) // gold not enough
			return;
		WindShield += num;
		Gold -= price;

		GA.Use ("WindShield", num, price);

		TouchHandler.PlayClickAudio (); // 按钮点击音效
	}
	
}
