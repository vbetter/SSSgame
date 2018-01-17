using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleBehaviour : EnemyBehaviour
{
    bool m_isChoose = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_isChoose = false;

        base.OnStateEnter(animator, stateInfo, layerIndex);

        m_self.CurrentState = ENUM_ActorState.Idle;

        if (m_self.DamageCount > 0)
        {
            m_self.SetState(ENUM_ActorState.Snow);
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if(m_stayTime>2f && !m_isChoose)
        {
            m_isChoose = true;

            int random = 1;// Random.Range(0, 3);
            if (m_self.m_headHasPlatform && random ==1)
            {
                m_self.OnJump();
            }
            else
            {
                m_self.SetState(ENUM_ActorState.Run);
            }
        }
    }

    bool isJump()
    {

        if(m_self.m_lastState == ENUM_ActorState.Run )
        {
            m_self.OnJump();
            /*
            if (m_self.m_headHasPlatform)
            {
                m_self.OnJump();
            }
            */
        }
        return false;
    }
}
