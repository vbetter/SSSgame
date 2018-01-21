using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILogin : MonoBehaviour {

    [SerializeField]
    GameObject m_startBtn;

    [SerializeField]
    UIToggle m_toggleP1, m_toggleP2, m_toggleP3;

    // Use this for initialization
    void Start () {
        UIEventListener.Get(m_startBtn).onClick = (go) => 
        {
            StartGame();
        };

        AudioManager.Instance.PlayBackgroundMusic(Sound.Login);
	}

    void StartGame()
    {
        int count = GameManager.Instance.SelectPlayerCount;
        if (count <= 0 || count >3)
        {
            Debug.LogError("Error SelectPlayerCount:" + count);
            return;
        }

        SaveData();

        UnityEngine.SceneManagement.SceneManager.LoadScene("game");
    }
	
    public void OnToggleSelectPlayer(GameObject go)
    {
        int select = 0;
        switch (go.name)
        {
            case "p1":
                select = 1;
                break;
            case "p2":
                select = 2;
                break;
            case "p3":
                select = 3;
                break;
            default:
                break;
        }

        GameManager.Instance.SelectPlayerCount = select;
    }
    
    void SetToggle(bool value)
    {
        UIToggle lastToggle = UIToggle.GetActiveToggle(1);
        int index = getIndexByToggle(lastToggle.name);
        if(value)
        {
            index++;
        }
        else
        {
            index--;
        }
        index = index <= 0 ? 1 : index;
        index = index >= 4 ? 3 : index;

        lastToggle.Set(false);
        UIToggle curToggle = getToggleByIndex(index);
        curToggle.Set(true);
        GameManager.Instance.SelectPlayerCount = index;
    }

    UIToggle getToggleByIndex(int index)
    {
        UIToggle toggle = m_toggleP1;
        switch (index)
        {
            case 1:
                toggle = m_toggleP1;
                break;
            case 2:
                toggle = m_toggleP2;
                break;
            case 3:
                toggle = m_toggleP3;
                break;
            default:
                break;
        }
        return toggle;
    }

    int getIndexByToggle(string name)
    {
        int select = 0;
        switch (name)
        {
            case "p1":
                select = 1;
                break;
            case "p2":
                select = 2;
                break;
            case "p3":
                select = 3;
                break;
            default:
                break;
        }

        return select;
    }

    void SaveData()
    {

    }

    float m_durectionTimer = 0;
    float m_durectionMax = 0.5f; 
    private void Update()
    {
        if(m_durectionTimer<m_durectionMax)
        {
            m_durectionTimer += Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button12)
            || Input.GetKeyDown(KeyCode.Joystick2Button12)
            || Input.GetKeyDown(KeyCode.Joystick3Button12)
            )
        {
            StartGame();
        }
        else if (Input.GetAxis("RightAndLeft")>0 && m_durectionTimer>=m_durectionMax)
        {
            m_durectionTimer = 0;
            print("Right");
            SetToggle(true);
        }
        else if (Input.GetAxis("RightAndLeft") < 0 && m_durectionTimer >= m_durectionMax)
        {
            m_durectionTimer = 0;

            print("Left");
            SetToggle(false);
        }
        else if ((Input.GetAxis("Horizontal_p1") > 0.5f 
            || Input.GetAxis("Horizontal_p2") > 0.5f
            || Input.GetAxis("Horizontal_p3") > 0.5f
            ) && m_durectionTimer >= m_durectionMax)
        {
            m_durectionTimer = 0;
            print("Right");
            SetToggle(true);
        }
        else if ((Input.GetAxis("Horizontal_p1") < -0.5f
            || Input.GetAxis("Horizontal_p2") < -0.5f
            || Input.GetAxis("Horizontal_p3") < -0.5f
            ) && m_durectionTimer >= m_durectionMax)
        {
            m_durectionTimer = 0;

            print("Left");
            SetToggle(false);
        }


    }
}
