# 📖 Chess Rules & Engine Technical Reference

This document maps official FIDE laws of chess directly to their mathematical implementations within the `ChessLogic` pure C# rule engine.

---

## 1. Standard Piece Moves

**The Chess Rule:**
Each piece has a distinct movement pattern. Sliding pieces (Rook, Bishop, Queen) move continuously along their vector until they hit an obstacle or board edge. Jumping pieces (Knights) leap directly to their destination. Kings move one square in any direction. Pawns move forward but capture diagonally.
*Verification Source: [FIDE Laws of Chess - Article 3: The moves of the pieces](https://handbook.fide.com/chapter/E012023)*

**Codebase Implementation:**
- **Namespace:** `ChessLogic`
- **Location:** `ChessLogic/Pieces/`
- **Classes:** `Pawn`, `Knight`, `Bishop`, `Rook`, `Queen`, `King` (Derived from base `Piece` class)
- **Method Name:** `public override IEnumerable<Move> GetMoves(Position from, Board board)`
- **Structural Approach:** 
  - Each piece overrides `GetMoves` to yield an `IEnumerable<Move>`.
  - Direction vectors (`Direction.cs`) are used to calculate iterative position steps.
  - The `Board.IsInside()` boundary constraint prevents out-of-bounds traversal, and `Board.IsEmpty()` detects obstructions or valid capture targets via checking `Color != Color`.
  - See `Pawn.cs` for unique `ForwardMoves` and `DiagonalMoves` logic separation.

---

## 2. Check & Checkmate Math

**The Chess Rule:**
The king is in "check" if it is attacked by one or more opponent's pieces. No piece can be moved that will either expose the king of the same colour to check or leave that king in check. Checkmate occurs when a player's king is in check and no legal move can remove the threat.
*Verification Source: [FIDE Laws of Chess - Article 3.9 & 5.1](https://handbook.fide.com/chapter/E012023)*

**Codebase Implementation:**
- **Namespace:** `ChessLogic`
- **Location:** `ChessLogic/Board.cs` & `ChessLogic/GameState.cs`
- **Classes:** `Board`, `GameState`, `Move`
- **Method Name:** `public bool IsInCheck(Player player)` & `private void CheckForGameOver()`
- **Structural Approach:** 
  - The engine determines check by evaluating if any opponent's pseudo-legal move intersects with the King's square (`Board.IsInCheck` maps over `Piece.CanCaptureOpponentKing`).
  - To prevent illegal self-checks, the abstract `Move.IsLegal()` simulates the move on a cloned board (`board.Copy()`) and verifies that `!boardCopy.IsInCheck(player)` remains true.
  - Checkmate is evaluated in `GameState.CheckForGameOver()`: If `!AllLegalMovesFor(CurrentPlayer).Any()` is true *and* the board `IsInCheck(CurrentPlayer)`, the game ends in Checkmate.

---

## 3. Castling Rights & Conditions

**The Chess Rule:**
A move of the king and either rook of the same colour along the player's first rank. It is illegal if: the king has already moved, the rook has already moved, the king is currently in check, or the king passes through/ends up on a square under attack. All squares between the king and rook must be empty.
*Verification Source: [FIDE Laws of Chess - Article 3.8.b](https://handbook.fide.com/chapter/E012023)*

**Codebase Implementation:**
- **Namespace:** `ChessLogic`
- **Location:** `ChessLogic/Pieces/King.cs` & `ChessLogic/Moves/Castle.cs`
- **Classes:** `King`, `Castle`
- **Method Name:** `CanCastleKingSide(Position from, Board board)`, `CanCastleQueenSide(...)`, `public override bool IsLegal(Board board)`
- **Structural Approach:**
  - `King.cs` ensures `HasMoved` is false, targets `IsUnmovedRook`, and verifies intermediate squares using `AllEmpty()`.
  - The `Castle` move object simulates the king taking exactly two steps. In `Castle.IsLegal()`, a loop iteratively executes a single step on a copied board and evaluates `copy.IsInCheck(player)` to explicitly ensure the King does not pass through an attacked square.

---

## 4. En Passant Capture Vector

**The Chess Rule:**
A pawn attacking a square crossed by an opponent's pawn which has advanced two squares in one move from its original square may capture this opponent's pawn as though the latter had been moved only one square. This capture is only legal on the move following this advance.
*Verification Source: [FIDE Laws of Chess - Article 3.7.d](https://handbook.fide.com/chapter/E012023)*

**Codebase Implementation:**
- **Namespace:** `ChessLogic`
- **Location:** `ChessLogic/Pieces/Pawn.cs`, `ChessLogic/Moves/DoublePawn.cs`, `ChessLogic/Moves/EnPassant.cs`
- **Classes:** `Pawn`, `DoublePawn`, `EnPassant`
- **Method Name:** `public override bool Execute(Board board)`
- **Structural Approach:**
  - When `DoublePawn` executes, it registers the bypassed square in `Board.SetPawnSkipPosition(CurrentPlayer, skippedPos)`.
  - The opponent's `Pawn.DiagonalMoves()` checks if the target coordinate matches `board.GetPawnSkipPosition()`.
  - The `EnPassant.Execute()` method executes a `NormalMove` for the attacking pawn and explicitly nullifies the capturing pawn's original rank vector `board[capturePos] = null;`.
  - The skip position is reset on every standard turn in `GameState.MakeMove()`.

---

## 5. Pawn Promotion Branching

**The Chess Rule:**
When a pawn reaches the rank furthest from its starting position, it must be exchanged as part of the same move for a new queen, rook, bishop or knight of the same colour on the intended square of arrival.
*Verification Source: [FIDE Laws of Chess - Article 3.7.e](https://handbook.fide.com/chapter/E012023)*

**Codebase Implementation:**
- **Namespace:** `ChessLogic` & `ChessUI`
- **Location:** `ChessLogic/Pieces/Pawn.cs`, `ChessLogic/Moves/PawnPromotion.cs`, `ChessUI/MainWindow.xaml.cs`
- **Classes:** `Pawn`, `PawnPromotion`, `MainWindow`
- **Method Name:** `private static IEnumerable<Move> PromotionMoves(...)`, `HandlePromotion(Position from, Position to)`
- **Structural Approach:**
  - In `Pawn.cs`, if `oneMovePos.Row == 0 || oneMovePos.Row == 7`, the engine branches move generation yielding 4 distinct `PawnPromotion` move objects (Queen, Rook, Bishop, Knight).
  - The `ChessUI` detects a `MoveType.PawnPromotion` and interrupts execution, loading the asynchronous `PromotionMenu`.
  - `PawnPromotion.Execute()` destroys the pawn at `FromPos`, creates the new piece type, instantly sets its `HasMoved = true`, and places it on `ToPos`.

---

## 6. Draw/Endgame Criteria

**The Chess Rule:**
A game can be drawn via Stalemate (no legal moves, not in check), Insufficient Material (impossible to checkmate), the 50-Move Rule (no pawn moves or captures in the last 50 moves), or Threefold Repetition (identical board state, rights, and turn player occurs 3 times).
*Verification Source: [FIDE Laws of Chess - Article 5.2 & Article 9](https://handbook.fide.com/chapter/E012023)*

**Codebase Implementation:**
- **Namespace:** `ChessLogic`
- **Location:** `ChessLogic/GameState.cs`, `ChessLogic/Board.cs`, `ChessLogic/StateString.cs`
- **Classes:** `GameState`, `Board`, `Counting`, `StateString`
- **Method Name:** `private void CheckForGameOver()`, `public bool InsufficientMaterial()`, `UpdateStateString()`
- **Structural Approach:**
  - **Stalemate:** `!AllLegalMovesFor(CurrentPlayer).Any()` combined with `!Board.IsInCheck()`.
  - **Insufficient Material:** `Board.CountPieces()` analyzes absolute piece totals and exact compositions (e.g., `IsKingBishopVKingBishop` checks if both bishops share the same square color modulo).
  - **50-Move Rule:** `GameState.noCaptureOrPawnMoves` increments on every non-capture/non-pawn move, halving the counter to check if `fullMoves == 50`. Counter is zeroed on capture or pawn advance.
  - **Threefold Repetition:** `StateString` encodes the board layout, current player, castling rights, and en passant square. This string hashes into a dictionary `stateHistory`. If the count reaches 3, it declares a draw.
