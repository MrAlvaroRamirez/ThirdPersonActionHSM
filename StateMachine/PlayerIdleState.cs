using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {}
    public override void EnterState()
    {
        _ctx.targetSpeed = 0;
        _ctx.targetTilt = 0;
        Debug.Log("Idle State");
    }
    public override void UpdateState()
    {
        CheckSwitchStates();
    }
    public override void ExitState(){}
    public override void CheckSwitchStates()
    {
        if(_ctx.isMovementPressed && _ctx.isRunPressed)
            SwitchState(_factory.Run());
        else if(_ctx.isMovementPressed && !_ctx.isRunPressed)
        {
            SwitchState(_factory.Walk());
        }
    }
    public override void InitializeSubState(){}
    public override void Trigger(string trigger){}
    public override void Trigger(int damage, Vector3 position){}
}
