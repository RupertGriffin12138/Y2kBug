using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.Player
{
    public class Player : Entity
    {
        [Header("Attack details")]
        public Vector2[] attackMovement;

        public bool isBusy { get; private set; }

        [Header("Move Info")]
        public float moveSpeed = 12f;
        public float frontSpeed = 0;
        public float belowSpeed = 0;
        public float jumpForce;

        [FormerlySerializedAs("dashCooldown")]
        [Header("Dash Info")]
        [SerializeField] private float dashCooldown;
        private float dashUsageTimer;
        public float dashSpeed;
        public float dashDuration;
        public float dashDir { get; private set; }

        #region States
        public PlayerStateMachine stateMachine { get; private set; }

        public PlayerIdleState idleState { get; private set; }
        public PlayerMoveState moveState { get; private set; }
        public PlayerFrontState frontState { get; private set; }
        public PlayerBelowState belowState { get; private set; }


        #endregion


        //Awake ��һ���������ڷ������ڽű�ʵ��������ʱ���ã��ҽ�����һ�Ρ��������ͨ�����ڳ�ʼ��һЩ�����������ö����״̬��
        protected override void Awake()
        {
            base.Awake();

            stateMachine = new PlayerStateMachine();

            idleState = new PlayerIdleState(stateMachine, this, "Idle");
            moveState = new PlayerMoveState(stateMachine, this, "Move");
            frontState = new PlayerFrontState(stateMachine, this, "Front");
            belowState = new PlayerBelowState(stateMachine, this, "Below");

        }

        protected override void Start()
        {
            base.Start();
            stateMachine.Initialize(idleState);
        }


        protected override void Update()
        {
            base.Update();
            stateMachine.currentState.Update();

        

        }

        //coroutine Э�� ���г�����
        //��ֱ
        public IEnumerator BusyFor(float _seconds)
        {
            isBusy = true;

            yield return new WaitForSeconds(_seconds);

            isBusy = false;
        }

        public void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();
    
    }
}
