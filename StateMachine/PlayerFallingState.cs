using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallingState : PlayerBaseState
{
    public PlayerFallingState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory)
    {
        _isRootState = true;
    }
    public override void EnterState()
    {
        _ctx.targetTilt = 0;
        _ctx.targetSpeed = 0;

        if(_ctx.currentMovement.y <= 0)
            _ctx.currentMovement.y = 0;

        Debug.Log("Falling State");
    }
    public override void UpdateState()
    {
        HandleGravity();

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

        CheckSwitchStates();
    }
    public override void ExitState()
    {
    }

    public override void CheckSwitchStates()
    {
        if(_ctx.isJumpPressed)
        {
            if(CheckForLedge())
                SwitchState(_factory.Climb()); 
        }

        if(CheckWallRun())
        {
            SwitchState(_factory.Wallrun());    
        }

        if(_ctx.characterController.isGrounded)
        {
            SwitchState(_factory.Grounded());
        }
    }

    public override void InitializeSubState(){}
    public override void Trigger(string trigger)
    {
        if(trigger == "boost")
        {
            SwitchState(_factory.Airboost());
        }

        if(trigger == "dash")
        {
            SwitchState(_factory.Dash());
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

    private bool CheckWallRun()
    {
        if(_ctx.currentMovement.y > 4 || _ctx.currentSpeed < 8 || !_ctx.canWallRun) return false;
        
        if(_ctx.isRunPressed && _ctx.rawMovementInput.y != 0)
        {
            if(Physics.Raycast(_ctx.characterCenter,-_ctx.transform.right,out RaycastHit hit, _ctx.wallRunMinDistance,_ctx.groundLayer))
            {
                _ctx.characterController.Move((-_ctx.transform.right * hit.distance * 0.9f));
            } else if (Physics.Raycast(_ctx.characterCenter,_ctx.transform.right,out RaycastHit hit2, _ctx.wallRunMinDistance,_ctx.groundLayer))
            {
                _ctx.characterController.Move((_ctx.transform.right * hit2.distance * 0.9f));
            } else 
            {
                return false;
            }
            return true;

        } else {
            return false;
        };
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

    private bool CheckForLedge()
    {
        //Check for wall
        Vector3 boxPos = new Vector3(_ctx.transform.forward.x * _ctx.wallDetectionBoxPosition.z, _ctx.wallDetectionBoxPosition.y, _ctx.transform.forward.z * _ctx.wallDetectionBoxPosition.z);
        _ctx.isCollidingWall = Physics.CheckBox(_ctx.transform.position + boxPos, _ctx.wallDetectionBoxExtents/2, _ctx.transform.rotation, _ctx.ledgeLayer);

        if(_ctx.isCollidingWall)
        {
            //Check for surface
            Vector3 rayPos = new Vector3(_ctx.transform.forward.x * _ctx.ledgeStartHeight.z, _ctx.ledgeStartHeight.y, _ctx.transform.forward.z * _ctx.ledgeStartHeight.z);
            Vector3 startPos = _ctx.transform.position + rayPos - (_ctx.transform.right * _ctx.ledgeSpread);

            Collider objectHit = null;
            float surfaceDist = -1;

            _ctx.climbPos = Vector3.zero;

            for (int i = 0; i < 3; i++)
            {
                Vector3 linePos = startPos + (_ctx.transform.right * _ctx.ledgeSpread * i);
                RaycastHit hit;
                if(Physics.Raycast(linePos, Vector3.down, out hit, _ctx.ledgeMaxHeight, _ctx.ledgeLayer))
                {
                    //TODO: CHECK NORMAL
                    
                    //Check if surface is big enough
                    if(objectHit == null)
                    {
                        objectHit = hit.collider;
                    } else if (hit.collider != objectHit)
                    {
                        Debug.Log("Surface not big enough");
                        return false;
                    }

                    //Check if surface is even
                    if(surfaceDist <= 0)
                    {
                        surfaceDist = hit.distance;
                    } else if (Mathf.Abs(surfaceDist - hit.distance) >= _ctx.maxSurfaceTilt)
                    {
                        Debug.Log("Surface not even enough");
                        return false;
                    }

                    if(i == 1)
                        _ctx.climbPos = hit.point;
                } else {
                    Debug.Log("No surface found");
                    return false;
                }
            }
            return true;
        }
        return false;
    }
}
