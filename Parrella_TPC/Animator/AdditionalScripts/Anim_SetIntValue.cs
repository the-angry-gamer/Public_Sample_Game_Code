using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anim_SetIntValue : StateMachineBehaviour
{

    [Tooltip("The Name of the parameter to update")]

    public string ParameterName = "";

    [Tooltip("The value to set for the parameter")]
    public int setValue = 0;

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetInteger(ParameterName, setValue);
    }
}
