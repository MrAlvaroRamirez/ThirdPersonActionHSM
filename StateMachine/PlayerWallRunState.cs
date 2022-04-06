using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallRunState : PlayerBaseState
{
    public PlayerWallRunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory)
    {
        _isRootState = true;
    }
    public override void EnterState()
    {
        _ctx.targetSpeed = _ctx.walkSpeed * _ctx.runMultiplier * _ctx.wallRunSpeedMultiplier;
        _ctx.currentMovement.y = 0;
        _ctx.targetTilt = 0;
    }
    public override void UpdateState()
    {
        HandleGravity();
        CheckWall();
        WallRun();
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        _ctx.rightWallColliding = false;
        _ctx.leftWallColliding = false;
        _ctx.animator.SetBool("isWallRunningL", false);
        _ctx.animator.SetBool("isWallRunningR", false);
    }
    public override void CheckSwitchStates()
    {
        if(_ctx.isJumpPressed && !_ctx.requireNewJump)
        {
            SwitchState(_factory.Jump());
            return;
        }
    }
    public override void InitializeSubState(){}
    public override void Trigger(string trigger){}
    public override void Trigger(int damage, Vector3 position){}

    private void CheckWall()
    {
        if(Physics.Raycast(_ctx.characterCenter,-_ctx.transform.right,out RaycastHit hit, _ctx.wallRunMinDistance,_ctx.groundLayer))
        {
            _ctx.leftWallColliding = true;
            _ctx.animator.SetBool("isWallRunningL", true);
        } else if (Physics.Raycast(_ctx.characterCenter,_ctx.transform.right,out RaycastHit hit2, _ctx.wallRunMinDistance,_ctx.groundLayer))
        {
            _ctx.rightWallColliding = true;
            _ctx.animator.SetBool("isWallRunningR", true);
        }
    }

    private void WallRun()
    {
        Quaternion surfaceRotation = Quaternion.LookRotation(_ctx.transform.forward);

        //Handle requirements
        if(_ctx.isRunPressed && _ctx.rawMovementInput.y != 0)
        {
            //Get normal of surface, and apply the rotation
            if(Physics.Raycast(_ctx.characterCenter,(_ctx.leftWallColliding ? -1 : 1) * _ctx.transform.right,out RaycastHit hit, _ctx.wallRunMinDistance,_ctx.groundLayer))
            {
                surfaceRotation = Quaternion.FromToRotation ((_ctx.leftWallColliding ? 1 : -1) * _ctx.transform.right, hit.normal) * _ctx.transform.rotation;
            } else 
            {
                SwitchState(_factory.Fall());
            }
        } else 
        {
            SwitchState(_factory.Fall());
        }
        
        _ctx.transform.rotation = surfaceRotation;

        var previousYSpeed = _ctx.currentMovement.y;
        //Override speed
        _ctx.currentMovement = _ctx.transform.forward * _ctx.currentSpeed;
        _ctx.currentMovement.y = previousYSpeed;
    }

    void HandleGravity()
    {
        _ctx.currentMovement.y += _ctx.gravity * Time.deltaTime * _ctx.wallRunGravityMultiplier;
    }
}
