using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirBoostState : PlayerBaseState
{
    public PlayerAirBoostState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory)
    {
        _isRootState = true;
    }
    public override void EnterState()
    {
        _ctx.animator.SetTrigger("doubleJump");
        _ctx.canDoubleJump = false;

        //Slow
        _ctx.currentMovement *= .4f;

        //TODO: Gravity
    }
    public override void UpdateState(){}
    public override void ExitState(){}
    public override void CheckSwitchStates(){}
    public override void InitializeSubState(){}
    public override void Trigger(string trigger)
    {
        if(trigger == "executeBoost")
        {
            BoostDash();
        }
    }
    public override void Trigger(int damage, Vector3 position){}

    void BoostDash()
    { 
        //_ctx.lookTarget.Freeze(.26f);
        _ctx.QuickRotation(new Vector3(_ctx.lastMovementInput.x, 0, _ctx.lastMovementInput.y));

        if(_ctx.isMovementPressed)
            _ctx.currentSpeed += _ctx.doubleJumpDash.dashForce;

        //Jump
        _ctx.currentMovement.y = _ctx.doubleJumpDash.upForce;

        //Particles
        var fx = new ParticleFX(_ctx.airBoostFx, _ctx.airBoostPositionOffset, Vector3.zero);
        _ctx.particleHelper.GenerateFX(fx, 2);

        SwitchState(_factory.Fall());
    }
}
