using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class PlayerAttackState : PlayerBaseState
{
    public PlayerAttackState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base (currentContext, playerStateFactory) 
    {
        _isRootState = true;
    }
    public override void EnterState()
    {
        _ctx.canAttack = false;
        _ctx.targetSpeed = 0;
        _ctx.targetTilt = 0;
        _ctx.currentMovement = Vector3.zero;

        OnAttack();
    }
    public override void UpdateState()
    {
    }
    public override void ExitState(){}
    public override void CheckSwitchStates(){}
    public override void InitializeSubState(){}
    public override void Trigger(string trigger)
    {
        if(trigger == "endDash")
        {
            SwitchState(_factory.Grounded());
        }
        if(trigger == "attackEnd")
        {
            _ctx.canAttack = true;
        }
        if(trigger == "attack" && _ctx.canAttack)
        {
            if (_ctx.currentAirCombo < _ctx.airComboMax)
                EnterState();
        }
        if(trigger == "doDamage")
        {
            DoDamage();
        }
    }
    public override void Trigger(int damage, Vector3 position){}
    private void OnAttack()
    {
        //If combo ended, restart
        if(_ctx.currentCombo >= _ctx.comboMax)
            _ctx.currentCombo = 0;

        //Seek best enemy target
        GameObject targetEnemy = SeekEnemy();

        if (!_ctx.isFloorBelow)
        {
            _ctx.animator.SetInteger("airCombo", _ctx.currentAirCombo);
            _ctx.currentAirCombo++;

            _ctx.ResetCombo();

        } else if (_ctx.isFloorBelow)
        {
            _ctx.animator.SetInteger("combo", _ctx.currentCombo);
            _ctx.currentCombo++;

            _ctx.ResetCombo();
        }

        //Dash
        if (targetEnemy == null)
        {
            CombatDash();
            _ctx.animator.SetTrigger("attack");
        } else
        {
            MoveToEnemy(targetEnemy.transform.position);
        }
}
    
    //DoTween to enemy
    void MoveToEnemy(Vector3 enemyPos)
    {
        //Animation based on distance
        float dis = Vector3.Distance(_ctx.characterCenter, enemyPos);

        if (dis >= _ctx.longAttackDistance && _ctx.isFloorBelow)
        {
            _ctx.animator.SetTrigger("longAttack");
        } else
        {
            _ctx.animator.SetTrigger("attack");
        }

        //Freeze camera
        if (dis >= 4 )
            _ctx.lookTarget.Freeze(.3f);
        else
            _ctx.lookTarget.Freeze(.2f);
        

        _ctx.transform.DOLookAt(enemyPos, .1f, AxisConstraint.Y);
        _ctx.attackTween = _ctx.transform.DOMove(TargetOffest(enemyPos), .1f).SetEase(Ease.OutCirc).OnComplete(() => {_ctx.attackTween = null;});

        _ctx.enemyTarget.playerTarget();

        if (_ctx.characterController.isGrounded) {
            _ctx.ResetMovement();
        } else
        {
            _ctx.ResetMovement();
        }
    }

    private void CheckEnemyCollision()
    {
        //TODO: CLEAN THIS
        Vector3 direction = new Vector3(_ctx.currentMovementInput.x, 0, _ctx.currentMovementInput.y);

        if (Physics.OverlapBox(_ctx.characterCenter + direction * 0.5f, Vector3.one * 0.5f, _ctx.transform.rotation, _ctx.enemyLayerMask).
            Contains(_ctx.enemyTarget.GetComponent<Collider>()))
        {
            _ctx.attackTween.Kill();
            _ctx.attackTween = null;
        }
    }

    public void DoDamage(bool up = false)
    {
        if (_ctx.enemyTarget != null)
        {
            //Camera Shake
            //TODO: Expose values
            _ctx.actionCamera.Shake(6, .18f, .1f);
            //Camera Distortion
            _ctx.actionCamera.Distortion(.4f, .7f);
            //Combat Zoom
            _ctx.actionCamera.PlayerJustHit();

            //Hit particle
            _ctx.particleHelper.GenerateFX(_ctx.hitFX, 1);

            _ctx.enemyTarget.OnHit(1, _ctx.transform.position, up);

            //Push other enemies
            Vector3 finalPosition = _ctx.characterCenter + _ctx.transform.forward * _ctx.enemyBoundsOffset;
            var nearEnemies = Physics.OverlapBox(finalPosition, _ctx.enemyTargetBoxExtents / 2,
                Quaternion.LookRotation(_ctx.transform.forward), _ctx.enemyLayerMask);

            foreach (var enemy in nearEnemies)
            {
                if(enemy.gameObject == _ctx.enemyTarget.gameObject) continue;
                enemy.GetComponent<EnemyBase>().KnockBack(_ctx.transform.position);
            }
        }

        //Fist particle

        Vector3 pos;
        Vector3 rot;

        if (_ctx.currentAirCombo != 0)
        {
            pos = _ctx.airComboParticles[_ctx.currentAirCombo - 1].pos;
            rot = _ctx.airComboParticles[_ctx.currentAirCombo - 1].rot;
        } else
        {
            pos = _ctx.comboParticles[_ctx.currentCombo - 1].pos;
            rot = _ctx.comboParticles[_ctx.currentCombo - 1].rot;
        }

        var fx = new ParticleFX(_ctx.fistFX, pos, rot);
        _ctx.particleHelper.GenerateFX(fx, .8f);
    }

    public void DoRadiusDamage(float radius, bool up = false)
    {
        var nearEnemies = Physics.OverlapSphere(_ctx.transform.position, radius, _ctx.enemyLayerMask);

        foreach (var enemy in nearEnemies)
        {
            var eb = enemy.GetComponent<EnemyBase>();
            eb.playerTarget();
            eb.OnHit(1, _ctx.transform.position, up);
        }
    }

    //Get the position where the player should move regarding
    Vector3 TargetOffest(Vector3 enemyPos)
    {
        //Get direction of the enemy
        Vector3 enemyDir = enemyPos - _ctx.transform.position;
        RaycastHit hit;
        if (Physics.Raycast(_ctx.transform.position, enemyDir, out hit, 100, _ctx.enemyLayerMask))
        {
            return hit.point - enemyDir.normalized * _ctx.enemyTargetOffset;
        } else
        {
            return _ctx.transform.position + _ctx.transform.forward;
        }
    }

    GameObject SeekEnemy()
    {
        //Overlap Box Detect In-Range Enemies
        Vector3 direction = _ctx.InputDirection();
        Vector3 finalPosition = _ctx.characterCenter + direction * _ctx.enemyBoundsOffset;

        Collider[] nearbyEnemies = Physics.OverlapBox(finalPosition, _ctx.enemyTargetBoxExtents / 2,
            direction == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(direction), _ctx.enemyLayerMask);

        float closestDist = 1000;
        GameObject closestEnemy = null;

        var distancePoint = _ctx.characterCenter + (direction * _ctx.enemyBoundsOffset * 1.8f);

        foreach (Collider enemyCol in nearbyEnemies)
        {
            //Check distance
            float dist = Vector3.SqrMagnitude(enemyCol.transform.position - distancePoint);
            if (dist < closestDist)
            {
                //Get direction of the enemy
                Vector3 enemyDir = enemyCol.transform.position + Vector3.up * 1.04f - _ctx.characterCenter;

                //Check if something in between
                RaycastHit hit;
                //TODO: CHECK SPHERE RADIUS
                if(Physics.SphereCast(_ctx.characterCenter - _ctx.transform.forward, .6f, enemyDir, out hit, 30, _ctx.collisionLayer))
                {
                    if (hit.collider.gameObject == enemyCol.gameObject)
                    {
                        closestEnemy = enemyCol.gameObject;
                        closestDist = dist;
                    }
                }
            }
        }

        _ctx.enemyTarget = closestEnemy == null ? null : closestEnemy.GetComponent<EnemyBase>();

        return closestEnemy;
    }

    //Regular combat dash
    public void CombatDash()
    {
        _ctx.lookTarget.Freeze(.18f);
        if (!_ctx.isFloorBelow)
        {
            _ctx.ExecuteDash(_ctx.airComboDashes[_ctx.currentAirCombo-1], false);
        } else
        {
            _ctx.ExecuteDash(_ctx.comboDashes[_ctx.currentCombo-1], false);
        }
    }
}
