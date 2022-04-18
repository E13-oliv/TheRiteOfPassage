using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepaIsRunning_Behaviour : StateMachineBehaviour
{
    private GameObject depa;

    private float tiltAngle = 30f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        depa = animator.gameObject;
        depa.transform.Rotate(Vector3.right, tiltAngle);
    }

    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{

    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        depa.transform.Rotate(Vector3.left, tiltAngle);
    }
}
