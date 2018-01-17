using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollBehaviour : EnemyBehaviour
{

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        m_self.CurrentState = ENUM_ActorState.Scroll;
        m_self.DamageCount = 4;

    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        m_Rigidbody2D.velocity = Vector3.zero;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if(m_self.m_frontHasWall && m_self.IsGround)
        {
            m_self.Flip();
        }

        if (m_self.IsGround)
        {
            m_Rigidbody2D.velocity = new Vector2(m_self.transform.localScale.x * m_self.moveSpeed * 5, m_Rigidbody2D.velocity.y);
        }
        else
        {
            m_Rigidbody2D.velocity = new Vector2(m_self.transform.localScale.x , m_Rigidbody2D.velocity.y *2f);
        }

    }
}
