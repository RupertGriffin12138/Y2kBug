using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{

    public bool isBusy { get; private set; }

    [Header("Move Info")]
    public float moveSpeed = 12f;
    public float frontSpeed = 0;
    public float belowSpeed = 0;
    public float jumpForce;


    #region States
    public PlayerStateMachine stateMachine { get; private set; }

    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerFrontState frontState { get; private set; }
    public PlayerBelowState belowState { get; private set; }


    #endregion


    //Awake 是一个生命周期方法，在脚本实例被加载时调用，且仅调用一次。这个方法通常用于初始化一些变量或者设置对象的状态。
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


    //coroutine 协程 并行程序流
    //僵直
    public IEnumerator BusyFor(float _seconds)
    {
        isBusy = true;

        yield return new WaitForSeconds(_seconds);

        isBusy = false;
    }

    public void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();
    
}
