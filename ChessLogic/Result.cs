namespace ChessLogic
{
    public class Result
    {
        public Player Winner { get; }
        public EndReason Reason { get; }

        public Result(Player winner, EndReason reason)
        {
            Winner = winner;
            Reason = reason;
        }

        // takes winning player and returns a result containing that player & checkmate reason
        public static Result Win(Player winner)
        {
            return new Result(winner, EndReason.Checkmate);
        }

        // creates draw result with a reason
        public static Result Draw(EndReason reason)
        {
            return new Result(Player.None, reason);
        }
    }
}
