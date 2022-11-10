namespace StateMachine.Bot;

public class MyBot
{
    public enum State
    {
        Auth,
        MainMenu,
        Votes,
        Help,
        OnVotesDB,
    }

    public enum Action
    {
        GetAuth,
        GetMainMenu,
        GetVotes,
        GetHelp,
        GetVotesDB,
    }

    //private State _state = State.Auth;
    //public State CurState { get {return _state;} }
}