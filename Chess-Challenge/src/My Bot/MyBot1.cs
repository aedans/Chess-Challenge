using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot1 : IChessBot
{
  int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

  public Move Think(Board board, Timer timer)
  {
    return board.GetLegalMoves().MaxBy(move => -ScoreMove(board, move, 2, -99999, 99999));
  }

  public int ScoreMove(Board board, Move move, int depth, int alpha, int beta)
  {
    board.MakeMove(move);

    if (depth == 0 || board.GetLegalMoves().Count() == 0)
    {
      alpha = Score(board);
      board.UndoMove(move);
      return alpha;
    }

    foreach (var nextMove in board.GetLegalMoves())
    {
      var eval = -ScoreMove(board, nextMove, depth - 1, -beta, -alpha);
      if (eval >= beta)
      {
        alpha = beta;
        break;
      }

      alpha = Math.Max(eval, alpha);
    }

    board.UndoMove(move);

    return alpha;
  }

  public int Score(Board board)
  {
    if (board.IsInCheckmate())
    {
      return -9999;
    }

    if (board.IsDraw())
    {
      return 0;
    }

    return PieceScores(board, board.IsWhiteToMove) - PieceScores(board, !board.IsWhiteToMove);
  }

  public int PieceScores(Board board, bool white)
  {
    return new PieceType[] { PieceType.Pawn, PieceType.Knight, PieceType.Bishop, PieceType.Rook, PieceType.Queen }
      .Sum(type => board.GetPieceList(type, white).Count * pieceValues[(int)type]);
  }
}