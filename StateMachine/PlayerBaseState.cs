public abstract class PlayerBaseState
{
    protected bool _isRootState = false;
    protected PlayerStateMachine _ctx;
    protected PlayerStateFactory _factory;
    protected PlayerBaseState _currentSubState;
    protected PlayerBaseState _currentSuperState;

    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    {
        _ctx = currentContext;
        _factory = playerStateFactory;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchStates();
    public abstract void InitializeSubState();
    public abstract void Trigger(string trigger);
    public abstract void Trigger(int damage, UnityEngine.Vector3 position);
    public void UpdateStates()
    {
        UpdateState();
        if(_currentSubState != null)
        {
            _currentSubState.UpdateState();
        }
    }
    protected void SwitchState(PlayerBaseState newState)
    {
        ExitState();

        if(_isRootState)
        {
            //Switch current state of the context
            _ctx.CurrentState = newState;
            newState.EnterState();
        } else if (_currentSuperState != null)
        {
            _currentSuperState.SetSubState(newState);
        }
    }
    protected void SetSuperState(PlayerBaseState newSuperState)
    {
        _currentSuperState = newSuperState;
    }
    protected void SetSubState(PlayerBaseState newSubState)
    {
        _currentSubState = newSubState;
        newSubState.EnterState();
        newSubState.SetSuperState(this);
    }
}
