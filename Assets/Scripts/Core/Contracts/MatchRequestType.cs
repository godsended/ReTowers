namespace Core.Contracts
{
    public enum MatchRequestType
    {
        FindingMatch,
        FindingBotMatch,
        CancelFindingMatch,
        ExitMatch,
        WinMatch,
        LoseMatch,
        DrawMatch,
        StartTurn,
        EndTurn
    }
}