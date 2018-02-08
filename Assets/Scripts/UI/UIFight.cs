using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFight : MonoBehaviour {

    [SerializeField]
    GameObject m_pauseBtn;

    [SerializeField]
    UIPause m_UIPause;

    [SerializeField]
    UILabel m_levelLabel;

    [SerializeField]
    UILevelTips m_UILevelTips;

    // Use this for initialization
    void Start ()
    {

        UIEventListener.Get(m_pauseBtn).onClick = OnClickPauseBtn;

        m_UILevelTips.Hide();

        UpdateUI();
    }

    public void UpdateUI()
    {
        m_levelLabel.text = string.Format("第{0}关", LevelManager.Instance.CurLevel);

        m_UIPause.Hide();

        AudioManager.Instance.PlayBackgroundMusic(Sound.BG_Fight);

        StartCoroutine(PlayLevelTips());
    }

    IEnumerator PlayLevelTips()
    {
        LevelTipsStruct item = new LevelTipsStruct();
        item.TipsType = eTipsType.Level;
        item.LevelCount = LevelManager.Instance.CurLevel;
        m_UILevelTips.Show(item);
        yield return new WaitForSeconds(2f);
        m_UILevelTips.Hide();
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
