using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anim_SetBoolValue : StateMachineBehaviour
{
    [Tooltip("The Name of the parameter to update")]

    public string   ParameterName   = "";

    [Tooltip("The value to set for the parameter")]
    public bool     setValue        = true;

    [Tooltip("Determines whether to have it continously update on animator state update")]
    public bool Continous           = false;


    [Tooltip("Determines whether to have it update on state leave")]
    public bool OnStateLeave        = true;

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (OnStateLeave)
        {
            DoTheWork(animator);
        }
    }

    ///OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Continous)
        {
            DoTheWork(animator);
        }
    }

    private void DoTheWork(Animator animator)
    {
        animator.SetBool(ParameterName, setValue);
    }
}
 