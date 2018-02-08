using System.Collections;
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

    public ENUM_ActorState m_curActorState = ENUM_ActorState.None;

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
        EffectMgr.Instance.CreateEffect(eEffectType.Boom, null, 1f, transform.localPosition);

        LevelManager.Instance.RemovePlayer(PlayerNumber);
    }

    public void SetState(ENUM_ActorState state)
    {
        //Debug.Log("state:" + state);

        if (state == m_curActorState)
            return;

        m_animator.SetTrigger(state.ToString());

        m_lastState = m_curActorState;
        m_curActorState = state;

    }
}
