using UnityEngine;
using System.Collections;

public class Utils : MonoBehaviour {

	// 把一个数字拆解成多个
	public static int[] EncodeNum(int num) 
	{
		int r = Random.Range (2, 3);
		int[] pass = new int[r];
		int last = num;
		for (int i=0; i<r-1; i++)
		{
			pass[i] = Random.Range(1, last);
			last -= pass[i];
		}
		pass [r - 1] = last;
		return pass;
	}

	// 把多个数字合成一个
	public static int DecodeNum(int[] pass) 
	{
		if (pass == null)
			return 0;
		int num = 0;
		foreach (int a in pass)
		{
			num += a;
		}
		return num;
	}

}
