using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BulletMsg
{
    public GameObject sender;
    public int damage;
}

public class Bullet : MonoBehaviour {


    [SerializeField]
    float m_destroyTime = 1f;

	// Use this for initialization
	void Start () {
        Init();
    }

    public void Init()
    {
        
    }

    public void OnAnimationEnd()
    {
        RemoveSelf();
    }

    void RemoveSelf()
    {
        Destroy(transform.parent.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Enemy")
        {
            BulletMsg msg = new BulletMsg();
            msg.damage = 1;
            collision.gameObject.SendMessage("OnHit", msg);
        }

        RemoveSelf();
    }
}
