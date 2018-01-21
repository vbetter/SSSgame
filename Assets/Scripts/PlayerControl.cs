using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour {

    float m_interval = 0;

    [SerializeField]
    float m_interval_Max = 0.5f;

    const string KeyFire1 = "Fire1";

    string m_Fire1 = "Fire1";

    int m_PlayerNumber = 0;

    [SerializeField]
    GameObject bomb;

    // Use this for initialization
    void Start () {
		
	}

    public void Init(int p)
    {
        m_PlayerNumber = p;

        string pStr = string.Format("_p{0}", p);
        m_Fire1 = m_Fire1 + pStr;
    }

    // Update is called once per frame
    void Update ()
    {
        if(m_interval<= m_interval_Max)
        {
            m_interval += Time.deltaTime;
        }

        if ((Input.GetKey(Utils.GetKeyCodeByPlayer(KeyFire1, m_PlayerNumber))) && m_interval>= m_interval_Max)
        {
            m_interval = 0;
            //Debug.Log(m_fireName);
            PlaySkill();
        }

    }

    void PlaySkill()
    {
        Instantiate(bomb, transform.position, transform.rotation,transform);
    }
}
