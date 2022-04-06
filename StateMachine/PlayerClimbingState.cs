using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerClimbingState : PlayerBaseState
{
    public PlayerClimbingState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory)
    {
        _isRootState = true;
    }
    public override void EnterState()
    {
        _ctx.currentMovement = Vector3.zero;
        _ctx.currentSpeed = 0;
        _ctx.targetTilt = 0;
        Climb();
    }
    public override void UpdateState(){}
    public override void ExitState(){}
    public override void CheckSwitchStates(){}
    public override void InitializeSubState(){}
    public override void Trigger(string trigger){}
    public override void Trigger(int damage, Vector3 position){}

    private void Climb()
    {
        Vector3 hangPos = _ctx.climbPos;
        hangPos.y += _ctx.hangPosOffset.y;
        hangPos += _ctx.transform.forward * _ctx.hangPosOffset.z;
        _ctx.transform.DOMove(hangPos, 0.1f).OnComplete(
            () => _ctx.transform.DOMove(_ctx.climbPos, .25f).SetEase(Ease.Linear).OnComplete(
                () => _ctx.transform.DOMove(_ctx.climbPos, .3f).SetEase(Ease.Linear).OnComplete(
                    () => {
                        SwitchState(_factory.Grounded());
                    })));
        _ctx.animator.SetTrigger("climb");
    }
}
