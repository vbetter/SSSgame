using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreativeSpore.SmartColliders;

public enum ENUM_ActorState
{
    None,
    Snow,
    Run,
    Die,
    Idle,
    Scroll,
    Jump,
    Push,
    Attack,
}

public class Enemy : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer m_sp;

    [SerializeField]
    SpriteRenderer m_snow;

    [SerializeField]
    Sprite[] m_snow_array;

    [SerializeField]
    ENUM_ActorState m_ActorState = ENUM_ActorState.None;

    public ENUM_ActorState m_lastState = ENUM_ActorState.None;

    int m_damageCount = 0;//被攻击计数
    public int DamageCount
    {
        set
        {
            
            m_damageCount = value;

            UpdateSnowState();
        }
        get
        {
            return m_damageCount;
        }
    }

    public float m_intervalTimer = 0;

    public float m_interval = 1f;

    public float moveSpeed = 2f;
    public float maxSpeed = 5f;

    Transform frontCheck;

    Rigidbody2D m_Rigidbody2D;

    Transform groundCheck;
    bool grounded = false;

    Animator m_animator;

    SmartPlatformController m_SmartPlatformController;

    //检查目标点内是否有对象
    Transform Check_front;
    Transform Check_back;
    Transform Check_top;

    public bool m_headHasPlatform= false;//头上有跳板
    public bool m_frontHasWall = false;//前面是否有墙

    private void Awake()
    {
        groundCheck = transform.Find("groundCheck");
        frontCheck = transform.Find("frontCheck").transform;
        Check_top = transform.Find("topCheck").transform;
        Check_back = transform.Find("backCheck").transform;

        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        m_SmartPlatformController = GetComponent<SmartPlatformController>();
    }
    // Use this for initialization
    void Start () {

        //UpdateSnowState();

        SetState(ENUM_ActorState.Idle);
    }
	
	// Update is called once per frame
	void Update () {
        //监测是否碰到地面
        grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Platform"));
        //监测头上有跳板
        m_headHasPlatform = Physics2D.Linecast(transform.position, Check_top.position, 1 << LayerMask.NameToLayer("Platform"));
        //监测前面是否有路
        m_frontHasWall = Physics2D.Linecast(transform.position, frontCheck.position, 1 << LayerMask.NameToLayer("Wall"));
       
    }

    public void DisableCurrentState()
    {
        m_animator.SetBool(CurrentState.ToString(), false);
    }

    public void SetState(ENUM_ActorState state)
    {
        //Debug.Log("state:" + state);

        m_animator.SetTrigger(state.ToString());

        m_lastState = CurrentState;
        CurrentState = state;

    }

    public void OnHit(BulletMsg msg)
    {
        if (CurrentState == ENUM_ActorState.Die || CurrentState == ENUM_ActorState.Scroll)
            return;

        m_intervalTimer = 0;

        if (DamageCount >= 4)
        {
            if(CurrentState == ENUM_ActorState.Snow)
            SetState(ENUM_ActorState.Scroll);
        }
        else
        {
            DamageCount += msg.damage;

            if (CurrentState!= ENUM_ActorState.Snow)
            {
                SetState(ENUM_ActorState.Snow);
            }
        }

        //print("OnHit:"+msg.damage);
    }

    public void UpdateSnowState()
    {

        if (m_damageCount == 0)
        {
            m_sp.enabled = true;
            m_snow.enabled = false;
        }
        else if (m_damageCount == 1)
        {
            m_sp.enabled = true;
            m_snow.enabled = true;
            m_snow.sprite = m_snow_array[0];
        }
        else if (m_damageCount == 2)
        {
            m_sp.enabled = true;
            m_snow.enabled = true;
            m_snow.sprite = m_snow_array[1];
        }
        else if (m_damageCount == 3)
        {
            m_sp.enabled = true;
            m_snow.enabled = true;
            m_snow.sprite = m_snow_array[2];
        }
        else if (m_damageCount == 4)
        {
            m_snow.enabled = true;
            m_sp.enabled = false;
            m_snow.sprite = m_snow_array[3];
        }
    }

    public void Flip()
    {
        // Multiply the x component of localScale by -1.
        Vector3 enemyScale = transform.localScale;
        enemyScale.x *= -1;
        transform.localScale = enemyScale;

        ENUM_ActorState state =CurrentState;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject go = collision.gameObject;
        string log = string.Format("go : {0} , tag : {1}",go.name ,go.tag);
        //Debug.LogError(log);
        if (go.tag == "Trigger_ScrollDie")
        {
            if (CurrentState == ENUM_ActorState.Scroll)
            {
                Debug.Log("Die");
                CurrentState = ENUM_ActorState.Die;
                SetState(ENUM_ActorState.Die);
            }
        }
        else if (go.tag == "Wall")
        {
            if (CurrentState != ENUM_ActorState.Die)
            {
                //Flip();
            }
        }
        else if (go.tag == "Player")
        {
            if (CurrentState == ENUM_ActorState.Snow)
            {
                SetState(ENUM_ActorState.Scroll);
            }
            else if (CurrentState == ENUM_ActorState.Scroll)
            {

            }
            else if (CurrentState == ENUM_ActorState.Idle || CurrentState == ENUM_ActorState.Run)
            {
                //SetState(ENUM_ActorState.Die);
                Debug.LogError("YouDie:" + go.name);
                Player player = go.GetComponent<Player>();
                player.OnDie();
            }
            else
            {



            }
        }
    }


   
    public ENUM_ActorState CurrentState
    {
        set
        {
            m_ActorState = value;
        }
        get
        {
            return m_ActorState;
        }
    }

    public void OnJump()
    {
        SetState(ENUM_ActorState.Jump);
    }

    public void OnDie()
    {
        EffectMgr.Instance.CreateEffect(eEffectType.Birth, null, 1f, transform.localPosition);
        Destroy(gameObject);
    }

    public bool IsGround
    {
        get
        {
            return grounded;
        }
    }
}
