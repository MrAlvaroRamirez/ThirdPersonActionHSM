using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageState : PlayerBaseState
{
    public PlayerDamageState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory, int damage, Vector3 position)
    : base (currentContext, playerStateFactory) 
    {  
        _isRootState = true;
        ReceiveDamage(damage, position);
    }
    public override void EnterState()
    {
        _ctx.targetSpeed = 0;
        _ctx.targetTilt = 0;
        _ctx.currentMovement = Vector3.zero;
    }
    public override void UpdateState(){}
    public override void ExitState(){}
    public override void CheckSwitchStates(){}
    public override void InitializeSubState(){}
    public override void Trigger(string trigger)
    {
        if(trigger == "endDash")
        {
            SwitchState(_factory.Grounded());
        }
    }
    public override void Trigger(int damage, Vector3 position){}

    private void ReceiveDamage(int damage, Vector3 enemyPos)
    {
        _ctx.animator.SetTrigger("damage");

        //Cameda Feedback
        _ctx.actionCamera.Shake(30, .4f, .1f);
        _ctx.actionCamera.Distortion(.4f, 1f);
        
        //Knockback in direction
        Vector3 dir = _ctx.transform.position - enemyPos;
        dir.y = 0;
        dir = dir.normalized;

        //justHit = true;

        //character.isAttacking = false;

        _ctx.ExecuteDash(_ctx.kncockbackDash, false, dir);

        _ctx.QuickRotation(-dir);

        //FX
        _ctx.particleHelper.GenerateFX(_ctx.damageFX, .8f);


        Debug.Log("Player Damage");
    }
}
