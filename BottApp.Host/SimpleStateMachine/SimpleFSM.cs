namespace BottApp.Host.SimpleStateMachine;

public class SimpleFSM
{
    private State state = State.Auth;
    
    public void SetState(State value)
    {
        state = value;
    }
    
    public State GetCurrentState()
    {
        return state;
    }
}