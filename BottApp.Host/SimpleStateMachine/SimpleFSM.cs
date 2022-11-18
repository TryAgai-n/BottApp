namespace BottApp.Host.SimpleStateMachine;

public class SimpleFSM
{
    private UserState _userState = UserState.Auth;
    
    public void SetState(UserState value)
    {
        _userState = value;
    }
    
    public UserState GetCurrentState()
    {
        return _userState;
    }
}