﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CreativeSpore.SmartColliders;

public class Player : Actor
{
    public int PlayerNumber = 0;

    SmartPlatformController m_SmartPlatformController;
    PlayerControl m_PlayerControl;

    const string KeyFire1 = "Fire1";

    string m_Fire1 = "Fire1";

    Animator m_animator;

    public ENUM_ActorState CurActorState = ENUM_ActorState.Idle;


    public ENUM_ActorState m_lastState = ENUM_ActorState.None;

    // Use this for initialization
    void Start ()
    {
        m_animator = GetComponent<Animator>();
    }

    public void Init(int p)
    {
        PlayerNumber = p;

        m_SmartPlatformController = GetComponent<SmartPlatformController>();
        m_SmartPlatformController.Init(p);

        m_PlayerControl = GetComponent<PlayerControl>();
        m_PlayerControl.Init(p);
    }

    public void OnDie()
    {
        SetState(ENUM_ActorState.Die);

        //EffectMgr.Instance.CreateEffect(eEffectType.Boom, null, 1f, transform.localPosition);

    }

    public void OnDieAnimationEnd()
    {
        LevelManager.Instance.RemovePlayer(PlayerNumber);
    }

    [SerializeField]
    GameObject m_bomb;

    public void OnAttackByAnimation()
    {
        Instantiate(m_bomb, transform.position, transform.rotation, transform);
    }

    public void OnAttackEndByAnimation()
    {
        //Instantiate(m_bomb, transform.position, transform.rotation, transform);
        //if (CurActorState != ENUM_ActorState.Run)
           // SetState(ENUM_ActorState.Idle);
    }

    public void SetStateToIdle()
    {
        if(CurActorState != ENUM_ActorState.Run)
        SetState(ENUM_ActorState.Idle);
    }

    public void SetState(ENUM_ActorState state)
    {
        //Debug.Log("state:" + state);

        AnimatorStateInfo stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);

        if (state == CurActorState)
            return;

        if(m_lastState!= ENUM_ActorState.None && m_lastState!= CurActorState)
        m_animator.ResetTrigger(m_lastState.ToString());

        m_animator.SetTrigger(state.ToString());

        //m_lastState = m_curActorState;
        //m_curActorState = state;

    }

    public bool IsEnableControl
    {
        get
        {
            if(CurActorState == ENUM_ActorState.Die)
            {
                return false;
            }
            return true;
        }
    }
}
