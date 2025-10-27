namespace Characters.Player
{
    public class PlayerMoveState : PlayerGroundState
    {
        public PlayerMoveState(PlayerStateMachine _stateMachine, Player _player, string _animBoolName) : base(_stateMachine, _player, _animBoolName)
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

            player.SetVelocity(xInput * player.moveSpeed, rb.velocity.y);

            if ((xInput == 0 && yInput == 0) || player.IsWallDetected())
                stateMachine.ChangeState(player.idleState);
        }
    }
}
