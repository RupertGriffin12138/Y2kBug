using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerGroundState
{
    public PlayerIdleState(PlayerStateMachine _stateMachine, Player _player, string _animBoolName) : base(_stateMachine, _player, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (xInput == player.facingDir && player.IsWallDetected())
            return;

        if (xInput != 0 && yInput == 0 && !player.isBusy)
            stateMachine.ChangeState(player.moveState);

        if(yInput !=0 && xInput == 0 && !player.isBusy)
        {
            if(yInput>0)
                stateMachine.ChangeState(player.frontState);
            else
                stateMachine.ChangeState(player.belowState);
        }
    }
}
