using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour {

    float m_interval = 0;

    [SerializeField]
    float m_interval_Max = 0.5f;

    [SerializeField]
    string m_fireName = "Fire1";

    [SerializeField]
    GameObject bomb;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(m_interval<= m_interval_Max)
        {
            m_interval += Time.deltaTime;
        }

        if (Input.GetButtonDown(m_fireName) && m_interval>= m_interval_Max)
        {
            m_interval_Max = 0;
            //Debug.Log(m_fireName);
            PlaySkill();
        }

    }

    void PlaySkill()
    {
        GameObject go = Instantiate(bomb, transform.position, transform.rotation,transform);
    }
}
