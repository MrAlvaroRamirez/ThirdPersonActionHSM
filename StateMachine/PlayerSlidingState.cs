using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlidingState : PlayerBaseState
{
    public PlayerSlidingState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory)
    {
        _isRootState = true;
    }
    public override void EnterState()
    {
        _ctx.animator.SetBool("isSliding", true);

        _ctx.isSliding = true;

        Debug.Log("Slide State");

        SlopeRotation();
    }
    public override void UpdateState()
    {
        //Handle Speed
        //TODO: Check this
        _ctx.targetSpeed += _ctx.slopeSpeed * Time.deltaTime;
        _ctx.targetSpeed = Mathf.Min(_ctx.targetSpeed, _ctx.maxSlopeSpeed);

        HandleSlope();

        CheckSwitchStates();
    }

    public override void ExitState()
    {
        _ctx.animator.SetBool("isSliding", false);
        _ctx.isSliding = false;
    }
    public override void CheckSwitchStates()
    {
        if(!OnSteepSlope())
        {
            SwitchState(_factory.Grounded());
        }

        if(_ctx.isJumpPressed && !_ctx.requireNewJump)
        {
            SwitchState(_factory.Jump());
            return;
        }

        if(!_ctx.characterController.isGrounded)
        {
            SwitchState(_factory.Fall());
        }
    }

    public override void InitializeSubState(){}
    public override void Trigger(string trigger){}
    public override void Trigger(int damage, Vector3 position){}

    private bool OnSteepSlope()
    {
        if(!_ctx.characterController.isGrounded || _ctx.currentMovement.y > 0) return false;

        return _ctx.hitPointAngle > _ctx.characterController.slopeLimit;
    }

    void HandleSlope()
    {
        _ctx.slopeDirection = Vector3.Cross(Vector3.Cross(_ctx.hitPointNormal, Vector3.up), _ctx.hitPointNormal);

        //_ctx.smoothXaxis = Mathf.SmoothDamp(_ctx.smoothXaxis, 45 * _ctx.currentMovementInput.x, ref _ctx.xVelocity, .4f);
        _ctx.smoothXaxis = Mathf.SmoothDamp(_ctx.smoothXaxis, 45 * 0, ref _ctx.xVelocity, .4f);
        _ctx.slopeDirection = Quaternion.AngleAxis(_ctx.smoothXaxis, _ctx.hitPointNormal) * _ctx.slopeDirection;

        Vector3 dir = -_ctx.slopeDirection;
        dir.y = 0;

        _ctx.currentMovement = dir.normalized * _ctx.currentSpeed;
        _ctx.currentMovement.y = _ctx.gravity*10;

        _ctx.playerModel.rotation = Quaternion.LookRotation(-_ctx.slopeDirection, _ctx.hitPointNormal);
    }

    void SlopeRotation()
    {
        Vector3 dir = -_ctx.slopeDirection;
        dir.y = 0;
        _ctx.currentSpeed *= Mathf.Clamp(Vector3.Dot(dir.normalized, _ctx.transform.forward),-.12f,1);
        _ctx.smoothXaxis = _ctx.currentMovementInput.x * 45;
        _ctx.transform.rotation = Quaternion.LookRotation(dir,Vector3.up);
    }
}
