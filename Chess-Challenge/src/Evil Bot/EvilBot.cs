using ChessChallenge.API;

namespace ChessChallenge.Example
{
  // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
  // Plays randomly otherwise.
  public class EvilBot : IChessBot
  {
    public Move Think(Board board, Timer timer)
    {
      return new MyBot3().Think(board, timer);
    }
  }
}