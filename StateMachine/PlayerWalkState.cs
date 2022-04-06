using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    public PlayerWalkState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {}
    public override void EnterState()
    {
        _ctx.targetSpeed = _ctx.walkSpeed;
        _ctx.targetTilt = 0;

        Debug.Log("Walk State");
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }
    public override void ExitState(){}
    public override void CheckSwitchStates()
    {
        if(!_ctx.isMovementPressed)
            SwitchState(_factory.Idle());
        else if(_ctx.isRunPressed)
            SwitchState(_factory.Run());
    }
    public override void InitializeSubState(){}
    public override void Trigger(string trigger){}
    public override void Trigger(int damage, Vector3 position){}
}
