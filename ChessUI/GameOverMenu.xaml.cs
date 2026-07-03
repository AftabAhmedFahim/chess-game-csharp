using ChessLogic;
using System.Windows;
using System.Windows.Controls;

namespace ChessUI
{
    /// <summary>
    /// Interaction logic for GameOverMenu.xaml
    /// </summary>
    public partial class GameOverMenu : UserControl
    {
        public event Action<Option> OptionSelected;

        public GameOverMenu(GameState gameState)
        {
            InitializeComponent();

            Result result = gameState.Result;
            WinnerText.Text = GetWinnerText(result.Winner);
            ReasonText.Text = GetReasonText(result.Reason, gameState.CurrentPlayer);
        }

        private static string GetWinnerText(Player winner)
        {
            return winner switch
            {
                Player.White => "White wins!",
                Player.Black => "Black wins!",
                _ => "It's a draw!"
            };
        }

        private static string PlayerString(Player player)
        {
            return player switch
            {
                Player.White => "White",
                Player.Black => "Black!",
                _ => ""
            };
        }

        private static string GetReasonText(EndReason reason, Player currentPlayer)
        {
            return reason switch
            {
                EndReason.Stalemate => $"Stalemate! {PlayerString(currentPlayer)} has no legal moves.",
                EndReason.Checkmate => $"Checkmate! {PlayerString(currentPlayer)} is in check and has no legal moves.",
                EndReason.FiftyMoveRule => "Draw! Fifty moves have been made without a pawn move or a capture.",
                EndReason.InsufficientMaterial => "Draw! Neither player has enough material to checkmate the other.",
                EndReason.ThreefoldRepetition => "Draw! The same position has occurred three times.",
                _ => ""
            };
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Restart);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Exit);
        }
    }
}
