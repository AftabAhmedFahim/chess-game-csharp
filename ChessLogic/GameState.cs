namespace ChessLogic
{
    public class GameState
    {
        public Board Board { get; }
        public Player CurrentPlayer { get; private set; }

        // makes easier to test out certain scenarios
        public GameState(Player player, Board board)
        {
            CurrentPlayer = player;
            Board = board;
        }
    }
}
