using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Template.MonoSingleton<LevelManager> {

    public int CurLevel = 1;

    [SerializeField]
    GameObject m_ClonePlayer;

    List<Player> m_playerList = new List<Player>();

	// Use this for initialization
	void Start () {
        SceneManager.activeSceneChanged += OnSceneChanged;
        
        InitLevel();

    
    }
	
    void OnSceneChanged(Scene lastScene ,Scene curScene)
    {
        if(curScene.name == "game")
        {
            InitLevel();
            print("game");
        }
        else if (curScene.name == "login")
        {
            print("login");
        }

    }

    /// <summary>
    /// 初始化关卡数据
    /// </summary>
    public void InitLevel()
    {
        initPlayers();
    }

    public void Clear()
    {
        if(m_playerList.Count>0)
        {
            for (int i = 0; i < m_playerList.Count; i++)
            {
                Player player = m_playerList[i];
                player.RemoveSelf();
                m_playerList.Remove(player);
                Destroy(player.gameObject);
            }
        }
    }

    public void RemovePlayer(int index)
    {
        for (int i = 0; i < m_playerList.Count; i++)
        {
            Player player = m_playerList[i];
            if (player.PlayerNumber == index)
            {
                player.RemoveSelf();
                m_playerList.Remove(player);
                Destroy(player.gameObject);
                break;
            }
        }
    }

    /// <summary>
    /// 初始化玩家
    /// </summary>
    void initPlayers()
    {
        Clear();

        int count = GameManager.Instance.SelectPlayerCount;

        GameObject[] posArray = GetPlayerBirthPosition();
        if(posArray==null || posArray.Length<count)
        {
            Debug.LogError("BirthPosition init Error");
            return;
        }

        if (count > 0 && count < 4)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject go = CreatePlayer();
                if(go!=null)
                {
                    go.transform.localPosition = posArray[i].transform.localPosition;
                    Player player= go.GetComponent<Player>();
                    player.Init(i+1);

                    EffectMgr.Instance.CreateEffect(eEffectType.Birth, null, 1f, player.transform.localPosition);
                }
                else
                {
                    Debug.LogError("CreatePlayer is null!");
                }
            }
        }
    }

    GameObject CreatePlayer()
    {
        if (m_ClonePlayer == null) return null;

        GameObject go = Instantiate(m_ClonePlayer);
        return go;
    }


    GameObject[] GetPlayerBirthPosition()
    {
        GameObject[] childs = GameObject.FindGameObjectsWithTag("BirthPosition");
        return childs;

    }
}
