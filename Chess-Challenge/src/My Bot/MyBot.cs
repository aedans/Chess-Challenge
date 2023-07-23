using ChessChallenge.API;
using System;
using System.Linq;
using System.Collections.Generic;

public class MyBot : IChessBot
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
    return ScoreMove(board, 4, -99999, 99999).Item2;
  }

  public (int, Move) ScoreMove(Board board, int depth, int alpha, int beta)
  {
    var moves = board.GetLegalMoves();

    if (depth == 0 || moves.Count() == 0)
    {
      return (Score(board), Move.NullMove);
    }

    var bestMove = moves[0];
    var value = -99999;
    foreach (var move in moves)
    {
      board.MakeMove(move);
      var (eval, _) = ScoreMove(board, depth - 1, -beta, -alpha);
      board.UndoMove(move);

      if (-eval > value)
      {
        value = -eval;
        bestMove = move;
      }

      alpha = Math.Max(alpha, value);
      if (alpha >= beta)
      {
        break;
      }
    }

    return (value, bestMove);
  }

  public int Score(Board board)
  {
    if (board.IsDraw())
    {
      return 0;
    }

    if (board.IsInCheckmate())
    {
      return -99999;
    }

    return PieceScores(board, board.IsWhiteToMove) - PieceScores(board, !board.IsWhiteToMove);
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