using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunBehaviour : EnemyBehaviour
{
    float m_randomTime = 0;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        m_Rigidbody2D = animator.GetComponent<Rigidbody2D>();
        m_self.CurrentState = ENUM_ActorState.Run;

        m_randomTime = Random.Range(2, 7);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        if (m_Rigidbody2D!=null) m_Rigidbody2D.velocity = Vector3.zero;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if (m_self.m_frontHasWall && m_self.IsGround)
        {
            m_self.Flip();
        }

        if (m_self.IsGround)
        {
            float x = 0;
            if (m_Rigidbody2D.velocity.x > m_self.maxSpeed)
            {
                x = Mathf.Sign(m_Rigidbody2D.velocity.x) * m_self.maxSpeed;
            }
            else
            {
                x = m_self.transform.localScale.x * m_self.moveSpeed;
            }
            m_Rigidbody2D.velocity = new Vector2(x, m_Rigidbody2D.velocity.y);
        }
        else
        {
            m_Rigidbody2D.velocity = new Vector2(m_self.transform.localScale.x, m_Rigidbody2D.velocity.y);
        }

        if(m_stayTime > m_randomTime)
        {
            m_self.SetState(ENUM_ActorState.Idle);
        }
    }
}
