using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Template.MonoSingleton<GameManager> {

    public int SelectPlayerCount = 1;//选择玩家人数

    bool m_isPause = false;
    public bool IsPause
    {
        get
        {
            return m_isPause;
        }
        set
        {
            if(value)
            {
                Time.timeScale = 1;
            }
            else
            {
                Time.timeScale = 0;
            }
            m_isPause = value;
        }
    }

	// Use this for initialization
	void Start ()
    {
        
        StartCoroutine(EffectMgr.Instance.LoadData());
    }
	

}
