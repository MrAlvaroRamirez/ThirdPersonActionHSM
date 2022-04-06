using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory)
    {
        _isRootState = true;
    }
    public override void EnterState()
    {
        _ctx.currentMovement.y = _ctx.groundedGravity;
        InitializeSubState();

        _ctx.canDoubleJump = true;

        _ctx.animator.SetBool("isFalling", false);

        Debug.Log("Grounded State");
    }
    public override void UpdateState()
    {
        //Handle Movement
        if(_ctx.isMovementPressed)
        {
            _ctx.currentMovement.x = _ctx.currentMovementInput.x * _ctx.currentSpeed;
            _ctx.currentMovement.z = _ctx.currentMovementInput.y * _ctx.currentSpeed;
        } else 
        {
            _ctx.currentMovement.x = _ctx.transform.forward.x * _ctx.currentSpeed;
            _ctx.currentMovement.z = _ctx.transform.forward.z * _ctx.currentSpeed;
        }
        
        CheckSlope();

        HandleAnimation();

        CheckSwitchStates();
    }
    public override void ExitState()
    {
        _ctx.animator.SetBool("isBraking", false);
    }

    public override void CheckSwitchStates()
    {
        if(_ctx.isJumpPressed && !_ctx.requireNewJump)
        {
            SwitchState(_factory.Jump());
        }

        if(!_ctx.isFloorBelow)
        {
            SwitchState(_factory.Fall());
        }
    }
    public override void InitializeSubState()
    {
        if(!_ctx.isMovementPressed)
            SetSubState(_factory.Idle());
        else if(_ctx.isMovementPressed && !_ctx.isRunPressed)
            SetSubState(_factory.Walk());
        else if(_ctx.isMovementPressed && _ctx.isRunPressed)
            SetSubState(_factory.Run());
    }
    public override void Trigger(string trigger)
    {
        if(trigger == "dash")
        {
            SwitchState(_factory.Dash());
            return;
        }

        if(trigger == "attack")
        {
            if (_ctx.currentAirCombo < _ctx.airComboMax)
                SwitchState(_factory.Attack());
        }
    }
    public override void Trigger(int damage, Vector3 position)
    {
        SwitchState(_factory.Damage(damage, position));
    }

    void HandleAnimation()
    {
        if(_ctx.characterController.isGrounded && _ctx.currentSpeed >= 13)
        {
            _ctx.animator.SetBool("isBraking", true);
            return;
        } else {
            _ctx.animator.SetBool("isBraking", false);
        }

        //Change blendtree's vlaue
        if (_ctx.isMovementPressed)
        {
            if (_ctx.isRunPressed)
            {
                _ctx.blendSmooth = Mathf.SmoothDamp(_ctx.blendSmooth, 1f, ref _ctx.velocity2, 0.1f);
            } else
            {
                _ctx.blendSmooth = Mathf.SmoothDamp(_ctx.blendSmooth, 0.3f, ref _ctx.velocity2, 0.1f);
            }
        } else
        {
            _ctx.blendSmooth = Mathf.SmoothDamp(_ctx.blendSmooth, 0f, ref _ctx.velocity2, 0.1f);
        }

        _ctx.animator.SetFloat("movementSpeed", _ctx.blendSmooth);
    }

    private void CheckSlope()
    {
        if(OnSteepSlope())
        {
            SwitchState(_factory.Slide());
        }
    }

    private bool OnSteepSlope()
    {
        if(!_ctx.characterController.isGrounded || _ctx.currentMovement.y > 0) return false;

        return _ctx.hitPointAngle > _ctx.characterController.slopeLimit;
    }
}
