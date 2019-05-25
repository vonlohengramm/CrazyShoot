using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class UITextScript : MonoBehaviour {

	public Font ZhFont; // 中文字体
	public Font Jpfont; // 日文字体
	public Font EnFont; // 英文字体
	public Font KoFont; // 韩文字体
	public float ZhLineSpacing; 
	public float JpLineSpacing;
	public float EnLineSpacing;
	public float KoLineSpacing;
	public int ZhFontSize;
	public int JpFontSize;
	public int EnFontSize;
	public int KoFontSize;
	public string key; // 文字的多语言key

	private static Dictionary<string, string> LangDic;

	public static string CurLang
	{
		get
		{
#if UNITY_EDITOR
			string defaultLang = GameObject.Find("SceneController").GetComponent<ScoreController>().DefaultLang;
			if (defaultLang != null && defaultLang.Length > 0) 
			{
				return defaultLang;
			}
#endif
			return Application.systemLanguage.ToString();
		}
	}

	// Use this for initialization
	void Start () {
		if (LangDic == null) // 需要初始化语言配置
		{
			LoadConfig();
		}

		// 如果有原定内容就调整字体和内容
		Text textComponent = GetComponent<Text> ();
		if (textComponent != null)
		{
			if (key != null && key.Length > 0) 
			{
				textComponent.text = GetTextByKey(key);
			}
			if (EnFont != null) 
			{
				textComponent.font = EnFont;
			}
			if (EnLineSpacing > 0) 
			{
				textComponent.lineSpacing = EnLineSpacing;
			}
			if (EnFontSize > 0) 
			{
				textComponent.resizeTextMaxSize = EnFontSize;
			}
			switch (CurLang) 
			{
			case "ChineseSimplified":
			case "Chinese":
				if (ZhFont != null) 
				{
					textComponent.font = ZhFont;
				}
				if (ZhLineSpacing > 0) 
				{
					textComponent.lineSpacing = ZhLineSpacing;
				}
				if (ZhFontSize > 0) 
				{
					textComponent.resizeTextMaxSize = ZhFontSize;
				}
				break;
			case "Japanese":
				if (Jpfont != null) 
				{
					textComponent.font = Jpfont;
				}
				if (JpLineSpacing > 0) 
				{
					textComponent.lineSpacing = JpLineSpacing;
				}
				if (JpFontSize > 0) 
				{
					textComponent.resizeTextMaxSize = JpFontSize;
				}
				break;
			case "Korean":
				if (KoFont != null) 
				{
					textComponent.font = KoFont;
				}
				if (KoLineSpacing > 0) 
				{
					textComponent.lineSpacing = KoLineSpacing;
				}
				if (KoFontSize > 0) 
				{
					textComponent.resizeTextMaxSize = KoFontSize;
				}
				break;
			}
		}
	}

	public static string GetTextByKey(string key) 
	{
		if (LangDic == null) // 需要初始化语言配置
		{
			LoadConfig();
		}

		return LangDic.ContainsKey (key) ? LangDic [key] : key;
	}

	public static string GetLangSuffix() 
	{
		switch (CurLang)
		{
		case "ChineseSimplified":
		case "Chinese":
			return "_c";
		case "Japanese":
			return "_j";
		case "Korean":
			return "_k";
		}
		return "";
	}

	private static void LoadConfig()
	{
		LangDic = new Dictionary<string, string> ();
		
		XmlDocument doc = new XmlDocument(); // load xml file
		doc.Load(GameConsts.LANG_CONFIG_URL); 
		XmlNodeList itemDocs = doc.SelectSingleNode ("Language").SelectNodes("item");
		foreach (XmlNode itemDoc in itemDocs) 
		{
			string key = itemDoc.SelectSingleNode("Key").InnerText;
			string text = "";
			switch (CurLang) 
			{
			case "ChineseSimplified":
			case "Chinese":
				XmlNode node = itemDoc.SelectSingleNode("Zh");
				if (node == null) 
				{
					goto default;
				}
				else
				{
					text = node.InnerText;
				}
				break;
			case "Japanese":
				node = itemDoc.SelectSingleNode("Jp");
				if (node == null) 
				{
					goto default;
				}
				else
				{
					text = node.InnerText;
				}
				break;
			case "Korean":
				node = itemDoc.SelectSingleNode("Ko");
				if (node == null) 
				{
					goto default;
				}
				else
				{
					text = node.InnerText;
				}
				break;
			default:
				text = itemDoc.SelectSingleNode("En").InnerText;
				break;
			}
			LangDic[key] = text;
		}
	}

}
