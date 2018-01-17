using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieBehaviour : EnemyBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        m_self.CurrentState = ENUM_ActorState.Die;
        m_self.OnDie();
    }

}
