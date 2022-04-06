using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory)
    {
        _isRootState = true;
    }
    public override void EnterState()
    {
        _ctx.targetTilt = 0;
        _ctx.targetSpeed = 0;
        HandleJump();

        _ctx.animator.SetTrigger("jump");
        Debug.Log("Jump State");

        SwitchState(_factory.Fall());
    }
    public override void UpdateState()
    {
        HandleGravity(); 
    }
    public override void ExitState()
    {
        if(_ctx.isJumpPressed)
        {
            _ctx.requireNewJump = true;    
        }
    }

    public override void CheckSwitchStates()
    {
    }

    public override void InitializeSubState(){}
    public override void Trigger(string trigger){}
    public override void Trigger(int damage, Vector3 position){}

    void HandleJump()
    {
        //Stop wallrun and start cooldown
        //WallRunCooldown(isWallRunning ? wallRunCooldown * 10 : wallRunCooldown);
        //isWallRunning = false;

        //_ctx.isJumping = true;
        _ctx.currentMovement.y = _ctx.initialJumpVelocity;
        //isJumping = false;
        //canDoubleJump = true;
    }

    void HandleGravity()
    {
        bool isFalling = _ctx.currentMovement.y <= 0.0f || !_ctx.isJumpPressed;
        float fallMultiplier = 1.4f;

        if (isFalling) {
            _ctx.currentMovement.y += _ctx.gravity * Time.deltaTime * fallMultiplier;
        } else {
            _ctx.currentMovement.y += _ctx.gravity * Time.deltaTime;
        }
    }
}
