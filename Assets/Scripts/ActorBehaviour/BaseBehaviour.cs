using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBehaviour : StateMachineBehaviour
{
    protected float m_stayTime = 0;//停留当前状态的时间

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_stayTime = 0;
        
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_stayTime += Time.deltaTime;

        base.OnStateUpdate(animator, stateInfo, layerIndex);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
    }
}
