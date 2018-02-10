﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunBehaviour : PlayerBaseBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        m_player.CurActorState = ENUM_ActorState.Run;
    }

}
