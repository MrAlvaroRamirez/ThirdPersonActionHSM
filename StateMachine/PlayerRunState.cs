using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    public PlayerRunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) {}
    public override void EnterState()
    {
        _ctx.targetSpeed = _ctx.walkSpeed * _ctx.runMultiplier;
        Debug.Log("Run State");
    }
    public override void UpdateState()
    {
        CheckSwitchStates();
        HandleCharacterTilt();
    }
    public override void ExitState(){}
    public override void CheckSwitchStates()
    {
        if(!_ctx.isMovementPressed)
            SwitchState(_factory.Idle());
        else if(!_ctx.isRunPressed)
            SwitchState(_factory.Walk());
    }
    public override void InitializeSubState(){}
    public override void Trigger(string trigger){}
    public override void Trigger(int damage, Vector3 position){}

    void HandleCharacterTilt()
    {
        //Obtain Y axis rotation
        float angleA = _ctx.targetRotation.eulerAngles.y;
        float angleB = _ctx.currentRotation.eulerAngles.y;
        //Get angle difference
        float diff = Mathf.DeltaAngle(angleA, angleB);
        //Min and max difference
        diff = Mathf.Clamp(diff, -20, 20);

        //Normalize
        float diffNormalized = Mathf.InverseLerp(-20, 20, diff);

        _ctx.targetTilt = Mathf.Lerp(_ctx.targetTilt, Mathf.Lerp(-20, 20, diffNormalized), Time.deltaTime * _ctx.tiltSpeed);
    }
}
