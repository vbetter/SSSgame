using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPause : MonoBehaviour {

    [SerializeField]
    GameObject m_backBtn,m_resumeBtn,m_resetBtn;


	// Use this for initialization
	void Start () {
        UIEventListener.Get(m_backBtn).onClick = OnBackBtn;
        UIEventListener.Get(m_resumeBtn).onClick = OnResumeBtn;
        UIEventListener.Get(m_resetBtn).onClick = OnResetBtn;
    }

    public void Show()
    {
        NGUITools.SetActive(gameObject, true);

        GameManager.Instance.IsPause = true;
    }

    public void Hide()
    {
        NGUITools.SetActive(gameObject, false);

    }

    void OnBackBtn(GameObject go)
    {
        Hide();

        GameManager.Instance.IsPause = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene("login");
    }

    void OnResumeBtn(GameObject go)
    {
        Hide();
        GameManager.Instance.IsPause = false;
    }

    void OnResetBtn(GameObject go)
    {
        Hide();
        GameManager.Instance.IsPause = false;
    }
}
