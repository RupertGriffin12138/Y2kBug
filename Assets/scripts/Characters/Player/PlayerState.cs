using Mobile;
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

#if UNITY_ANDROID || UNITY_IOS
            // 移动端：从 Joystick 获取输入
            if (GlobalInput.joystick)
            {
                xInput = GlobalInput.joystick.Horizontal;
                yInput = GlobalInput.joystick.Vertical;

                // 死区过滤，防止轻微漂移
                if (Mathf.Abs(xInput) < 0.1f) xInput = 0f;
                if (Mathf.Abs(yInput) < 0.1f) yInput = 0f;
            }
            else
            {
                xInput = 0;
                yInput = 0;
            }
#else
            // PC / 编辑器：键盘输入
            xInput = Input.GetAxisRaw("Horizontal");
            yInput = Input.GetAxisRaw("Vertical");
#endif

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
