using System.Collections;
using UnityEngine;

namespace Characters.Player
{
    public class Player : Entity
    { 
        public bool isBusy  { get; set; }

        [Header("Move Info")]
        public float moveSpeed = 12f;
        public float frontSpeed = 0;
        public float belowSpeed = 0;

        [Header("Dash Info")]
        [SerializeField] private float dashCooldown;
        private float dashUsageTimer;

        #region States
        public PlayerStateMachine stateMachine { get; private set; }

        public PlayerIdleState idleState { get; private set; }
        public PlayerMoveState moveState { get; private set; }
        public PlayerFrontState frontState { get; private set; }
        public PlayerBelowState belowState { get; private set; }


        #endregion
        
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
            // �������æµ״̬��������ĳЩ�Ի��У���ֱ�Ӳ�����״̬ ͬʱҲ��ֹ�ƶ�
            if (isBusy) return;
            stateMachine.currentState.Update();
        }

        public void LockControl()
        {
            isBusy = true;

            // ����ǰ״̬���� idle����ǿ���л�
            if (stateMachine.currentState != idleState)
            {
                stateMachine.ChangeState(idleState);
            }
            // === ����������ͣ�������ٶ� ===
            if (rb != null)
                rb.velocity = Vector2.zero;

        }
        public void UnlockControl() => isBusy = false;
        
        public IEnumerator BusyFor(float _seconds)
        {
            isBusy = true;

            yield return new WaitForSeconds(_seconds);

            isBusy = false;
        }

        public void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();
    
    }
}
