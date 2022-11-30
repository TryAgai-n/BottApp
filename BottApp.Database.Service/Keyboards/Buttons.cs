namespace BottApp.Database.Service.Keyboards;

public enum MenuButton
{
    ToVotes,
    ToHelp,
    Hi,
}

public enum MainVoteButton
{
    Back,
    GiveAVote,
    AddCandidate,
    ToMainMenu,
    ToChooseNomination,
    ToHelp,
}

public enum VotesButton
{
    Right,
    Left,
    Back,
    Like,
    ToVotes,
    ToHelp,
}
public enum NominationButton
{
  ToFirstNomination,
  ToSecondNomination,
  ToThirdNomination,
}

public enum UploadCandidateButton
{
    Right,
    Left,
    Back,
}

public enum AdminButton
{
    Approve,
    Decline,
}