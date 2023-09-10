using ChessChallenge.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MyBot4 : IChessBot
{
  int[] pieceValues = { 0, 100, 300, 300, 500, 900 };
  ulong[][] pieceScoreboards = new ulong[][]{
    new ulong[] { 0x0000000000000000, 0x1223322123344332, 0x3445544345566554, 0x56677665ffffffff },
    new ulong[] { 0x0123321012344321, 0x2345543234566543, 0x2345543234566543, 0x0123321012344321 },
    new ulong[] { 0x0123321012344321, 0x2345543234566543, 0x2345543234566543, 0x0123321012344321 },
    new ulong[] { 0x0123321012344321, 0x2345543234566543, 0x2345543234566543, 0x0123321012344321 },
    new ulong[] { 0x0123321012344321, 0x2345543234566543, 0x2345543234566543, 0x0123321012344321 },
  };

  public Move Think(Board board, Timer timer)
  {
    var depth = 0;

    while (true)
    {
      var score = ScoreMove(board, ++depth, -99999, 99999, out Move move);

      if (timer.MillisecondsElapsedThisTurn > 10 || Math.Abs(score) == 99999) 
      {
        return move;
      }
    }
  }

  public int ScoreMove(Board board, int depth, int alpha, int beta, out Move bestMove)
  {
    bestMove = Move.NullMove;

    if (board.IsInCheckmate())
    {
      return -99999;
    }

    if (board.IsDraw())
    {
      return 0;
    }

    if (depth == 0)
    {
      return PieceScores(board, board.IsWhiteToMove) - PieceScores(board, !board.IsWhiteToMove);
    }

    var legalMoves = board.GetLegalMoves().ToList();
    bestMove = legalMoves[0];

    var bestEval = -99999;
    foreach (var move in legalMoves)
    {
      if (alpha >= beta)
      {
        continue;
      }

      board.MakeMove(move);

      var eval = -ScoreMove(board, depth - 1, -beta, -alpha, out Move _);

      board.UndoMove(move);

      if (eval > bestEval)
      {
        bestEval = eval;
        bestMove = move;
      }

      alpha = Math.Max(alpha, bestEval);
    }

    return bestEval;
  }

  public int PieceScores(Board board, bool white)
  {
    return new PieceType[] { PieceType.Pawn, PieceType.Knight, PieceType.Bishop, PieceType.Rook, PieceType.Queen }
      .Sum(type => board.GetPieceList(type, white).Sum(piece => GetPieceScore(piece)));
  }

  public int GetPieceScore(Piece piece)
  {
    var index = piece.Square.Index;
    if (!piece.IsWhite)
    {
      index = 63 - index;
    }

    var offset = 60 - (index % 16) * 4;
    var value = (pieceScoreboards[(int)piece.PieceType - 1][index / 16] & (0xful << offset)) >> offset;
    return (int)(pieceValues[(int)piece.PieceType] * (1 + value * .1));
  }
}