using UnityEngine;

namespace Characters.Player
{
    public class PlayerState
    {
        protected readonly PlayerStateMachine stateMachine;
        protected readonly Player player;

        protected Rigidbody2D rb;

        protected float xInput;
        protected float yInput;
        private readonly string animBoolName;

        protected float stateTimer;
        protected bool triggerCalled;

        //_��ʾ˽�У�Լ��
        public PlayerState(PlayerStateMachine _stateMachine, Player _player, string _animBoolName)
        {
            this.stateMachine = _stateMachine;
            this.player = _player;
            this.animBoolName = _animBoolName;


        }

        public virtual void Enter()
        {
            player.anim.SetBool(animBoolName, true);
            rb = player.rb;
            triggerCalled = false;
        }

        public virtual void Update()
        {
            stateTimer -= Time.deltaTime;

            xInput = Input.GetAxisRaw("Horizontal");
            yInput = Input.GetAxisRaw("Vertical");

        }

        public virtual void Exit()
        {

            player.anim.SetBool(animBoolName, false);
        }

        public virtual void AnimationFinishTrigger()
        {
            triggerCalled = true;
        }
    }
}
