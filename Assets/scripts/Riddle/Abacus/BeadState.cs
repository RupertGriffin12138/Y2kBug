using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeadState
{
    protected BeadStateMachine stateMachine;
    protected Bead bead;

    protected float xInput;
    protected float yInput;
    private string animBoolName;

    public BeadState(BeadStateMachine _stateMachine, Bead _bead, string _animBoolName)
    {
        this.stateMachine = _stateMachine;
        this.bead = _bead;
        this.animBoolName = _animBoolName;


    }

    public virtual void Enter()
    {
        bead.anim.SetBool(animBoolName, true);

    }

    public virtual void Update()
    {

        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

    }

    public virtual void Exit()
    {

        bead.anim.SetBool(animBoolName, false);
    }

}
