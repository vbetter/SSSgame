using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBaseBehaviour : BaseBehaviour
{
    protected Player m_player;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_player = animator.GetComponent<Player>();
        base.OnStateEnter(animator, stateInfo, layerIndex);

    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        m_player.m_lastState = m_player.CurActorState;
        m_player.CurActorState = ENUM_ActorState.None;
    }
}
