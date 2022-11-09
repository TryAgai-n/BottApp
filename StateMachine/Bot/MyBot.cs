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
        GetMainMenu,
        GetVotes,
        GetHelp,
        GetVotesDB,
    }

  //  private State _state = State.MainMenu;

    //public State CurState { get {return _state;} }
}