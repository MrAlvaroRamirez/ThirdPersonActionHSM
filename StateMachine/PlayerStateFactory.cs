using System;
using System.Collections.Generic;

public class PlayerStateFactory
{
    PlayerStateMachine _context;
    Dictionary<string, PlayerBaseState> states = new Dictionary<string, PlayerBaseState>();

    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;

        states.Add("idle", new PlayerIdleState(_context, this));
        states.Add("airboost", new PlayerAirBoostState(_context, this));
        states.Add("climb", new PlayerClimbingState(_context, this));
        //states.Add("damage", new PlayerDamageState(_context, this));
        states.Add("dash", new PlayerDashState(_context, this));
        states.Add("grounded", new PlayerGroundedState(_context, this));
        states.Add("jump", new PlayerJumpState(_context, this));
        states.Add("run", new PlayerRunState(_context, this));
        states.Add("skill", new PlayerSkillState(_context, this));
        states.Add("slide", new PlayerSlidingState(_context, this));
        states.Add("walk", new PlayerWalkState(_context, this));
        states.Add("wallrun", new PlayerWallRunState(_context, this));
        states.Add("fall", new PlayerFallingState(_context, this));
        states.Add("attack", new PlayerAttackState(_context, this));
    }

    public PlayerBaseState Idle()
    {
        return states["idle"];
    }
    public PlayerBaseState Walk()
    {
        return states["walk"];
    }
    public PlayerBaseState Run()
    {
        return states["run"];
    }
    public PlayerBaseState Jump()
    {
        return states["jump"];
    }
    public PlayerBaseState Grounded()
    {
        return states["grounded"];
    }
    public PlayerBaseState Attack()
    {
        return states["attack"];
    }
    public PlayerBaseState Damage(int damage, UnityEngine.Vector3 position)
    {
        return new PlayerDamageState(_context, this, damage, position);
    }
    public PlayerBaseState Wallrun()
    {
        return states["wallrun"];
    }
    public PlayerBaseState Skill()
    {
        return states["skill"];
    }
    public PlayerBaseState Airboost()
    {
        return states["airboost"];
    }
    public PlayerBaseState Slide()
    {
        return states["slide"];
    }
    public PlayerBaseState Climb()
    {
        return states["climb"];
    }
    public PlayerBaseState Fall()
    {
        return states["fall"];
    }

    public PlayerBaseState Dash()
    {
        return states["dash"];
    }
}
