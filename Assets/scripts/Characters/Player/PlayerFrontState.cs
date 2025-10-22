public class PlayerFrontState : PlayerGroundState
{
    public PlayerFrontState(PlayerStateMachine _stateMachine, Player _player, string _animBoolName) : base(_stateMachine, _player, _animBoolName)
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

        player.SetVelocity(0, player.frontSpeed);

        if ((xInput == 0 && yInput == 0) || player.IsWallDetected())
            stateMachine.ChangeState(player.idleState);
        if (xInput != 0 && yInput == 0 && !player.isBusy)
            stateMachine.ChangeState(player.moveState);
        if (yInput < 0 && xInput == 0 && !player.isBusy)
            stateMachine.ChangeState(player.belowState);
    }
}
