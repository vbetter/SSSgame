using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFight : MonoBehaviour {

    [SerializeField]
    GameObject m_pauseBtn;

    [SerializeField]
    UIPause m_UIPause;

    // Use this for initialization
    void Start ()
    {

        m_UIPause.Hide();

        UIEventListener.Get(m_pauseBtn).onClick = OnClickPauseBtn;

        AudioManager.Instance.PlayBackgroundMusic(Sound.BG_Fight);
    }
	
	void OnClickPauseBtn(GameObject go)
    {
        Debug.Log(go.name);

        m_UIPause.Show();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Joystick1Button9) )
        {
            bool isPause = GameManager.Instance.IsPause;
            if (isPause)
            {
                m_UIPause.Show();
            }
            else
            {
                m_UIPause.Hide();
            }

            GameManager.Instance.IsPause = !isPause;
        }
    }
}
