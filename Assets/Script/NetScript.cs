using UnityEngine;
using System.Collections;

public class NetScript : MonoBehaviour {
    
    // 相对速度对网子的冲量因子
    public float XFactor;
    public float YFactor; 

	public AudioClip BeatAudio; // 擦网音效

	// 左侧或者右侧的边网
	public bool left;
	public bool right;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag != "Ball") return; // 球触网有反应
        var speed1 = coll.gameObject.GetComponent<Rigidbody2D>().velocity; // 球速度
        var speed2 = GetComponent<Rigidbody2D>().velocity; // 网速度
		var ballX = coll.gameObject.transform.position.x;
		var netX = this.gameObject.transform.position.x;
		float speedX = speed1.x - speed2.x;
		if ((left && ballX < netX) || (right && ballX > netX)) // 允许击穿边网，减小反弹力，避免像钢板一样硬
		{
			if (Mathf.Abs (speedX) > 1f) 
			{
				speedX = speedX > 0 ? 1f : -1f;
			}
		}
		float speedY = speed1.y - speed2.y;
        var relativelocity = new Vector2(speedX * XFactor, speedY * YFactor); // 相对速度
        GetComponent<Rigidbody2D>().AddForce(relativelocity, ForceMode2D.Impulse);
		coll.gameObject.GetComponent<Rigidbody2D>().AddForce(relativelocity * -0.3f, ForceMode2D.Impulse); // 对球进行反弹
		if (BeatAudio != null) // 音效
		{
			AudioSource.PlayClipAtPoint(BeatAudio, new Vector2(0, 0));
		}
    }

}
