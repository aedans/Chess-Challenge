using ChessChallenge.API;

public class MyBot0 : IChessBot
{
  public Move Think(Board board, Timer timer)
  {
    var moves = board.GetLegalMoves();
    return moves[0];
  }
}