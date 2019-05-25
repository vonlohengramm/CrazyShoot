using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class ItemConfig : MonoBehaviour {

	private static Dictionary<int, Item> items;

	public static Item GetItem(int id) 
	{
		if (items == null) 
		{
			LoadConfig();
		}
		return items [id];
	}

	private static void LoadConfig()
	{
		items = new Dictionary<int, Item> ();

		XmlDocument doc = new XmlDocument(); // load xml file
		doc.Load(GameConsts.ITEM_CONFIG_URL); 
		XmlNodeList itemDocs = doc.SelectSingleNode ("AngryShot").SelectSingleNode ("items").SelectNodes("item");
		foreach (XmlNode itemDoc in itemDocs) 
		{
			XmlElement _item = (XmlElement) itemDoc;
			Item item = new Item();
			item.id = int.Parse(_item.GetAttribute("id"));
			item.name = UITextScript.GetTextByKey(itemDoc.SelectSingleNode("name").InnerText);
			item.desc = UITextScript.GetTextByKey(itemDoc.SelectSingleNode("desc").InnerText);
			item.price = int.Parse(itemDoc.SelectSingleNode("price").InnerText);
			items[item.id] = item;
		}
	}
}

public class Item 
{
	public int id;
	public string name;
	public string desc;
	public int price;
}
