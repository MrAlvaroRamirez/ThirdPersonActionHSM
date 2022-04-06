using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    public PlayerDashState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) 
    {
        _isRootState = true;
    }
    public override void EnterState()
    {
        _ctx.animator.SetTrigger("roll");
        _ctx.lookTarget.Freeze(.26f);
        //QuickRotation(new Vector3(lastMovementInput.x, 0, lastMovementInput.y));
        _ctx.ExecuteDash(_ctx.sprintDash, false);
        _ctx.particleHelper.GenerateFX(_ctx.DashParticle, 2);
    }
    public override void UpdateState()
    {
        //Handle Gravity
        float Yvelocity = _ctx.currentMovement.y;

        //if (Yvelocity < 0)
            Yvelocity = 0;

        _ctx.currentMovement.y = Yvelocity;
    }
    public override void ExitState(){}
    public override void CheckSwitchStates(){}
    public override void InitializeSubState(){}
    public override void Trigger(string trigger)
    {
        if(trigger == "endDash")
        {
            SwitchState(_factory.Grounded());
            //Exit State and switch
            //If !isgrounded / wallcolliding to flow between wallrun
        }
    }
    public override void Trigger(int damage, Vector3 position){}
}
