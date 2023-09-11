using ChessChallenge.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MyBot6 : IChessBot
{
  int[] pieceValues = { 0, 100, 300, 300, 500, 900 };
  ulong[][] pieceEvalboards = new ulong[][]{
    new ulong[] { 0x0000000000000000, 0x1223322123344332, 0x3445544345566554, 0x56677665ffffffff },
    new ulong[] { 0x0123321012344321, 0x2345543234566543, 0x2345543234566543, 0x0123321012344321 },
    new ulong[] { 0x0123321012344321, 0x2345543234566543, 0x2345543234566543, 0x0123321012344321 },
    new ulong[] { 0x0123321012344321, 0x2345543234566543, 0x2345543234566543, 0x0123321012344321 },
    new ulong[] { 0x0123321012344321, 0x2345543234566543, 0x2345543234566543, 0x0123321012344321 },
  };

  Dictionary<ulong, (int, int, List<Move>)> evaluations = new();

  public Move Think(Board board, Timer timer)
  {
    var depth = 0;
    var bestEval = 0;
    Move bestMove = Move.NullMove;

    while (true)
    {
      var eval = EvalMove(timer, board, ++depth, -99999, 99999, out Move move);

      if (eval == 99999)
      {
        bestMove = move;
        bestEval = eval;
        move = Move.NullMove;
      }

      if (move.IsNull)
      {
        return bestMove;
      }

      bestEval = eval;
      bestMove = move;
    }
  }

  public int EvalMove(Timer timer, Board board, int depth, int alpha, int beta, out Move bestMove)
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
      return PieceEvals(board, board.IsWhiteToMove) - PieceEvals(board, !board.IsWhiteToMove);
    }

    var legalMoves = board.GetLegalMoves().ToList();

    if (evaluations.ContainsKey(board.ZobristKey))
    {
      var (evalDepth, eval, moves) = evaluations[board.ZobristKey];
      if (evalDepth >= depth && moves.Count > 0)
      {
        bestMove = moves.First();
        return eval;
      }

      legalMoves.InsertRange(0, moves);
    }

    bestMove = legalMoves[0];

    var bestMoves = new List<Move>() { legalMoves[0] };
    var bestEval = -99999;
    foreach (var move in legalMoves)
    {
      if (timer.MillisecondsElapsedThisTurn > 100)
      {
        bestMove = Move.NullMove;
        return 0;
      }

      if (alpha >= beta)
      {
        continue;
      }

      board.MakeMove(move);

      var eval = -EvalMove(timer, board, depth - 1, -beta, -alpha, out Move _);

      board.UndoMove(move);

      if (eval > bestEval)
      {
        bestEval = eval;
        bestMove = move;
        bestMoves.Insert(0, move);
      }

      alpha = Math.Max(alpha, bestEval);
    }

    evaluations[board.ZobristKey] = (depth, bestEval, bestMoves);

    return bestEval;
  }

  public int PieceEvals(Board board, bool white)
  {
    return new PieceType[] { PieceType.Pawn, PieceType.Knight, PieceType.Bishop, PieceType.Rook, PieceType.Queen }
      .Sum(type => board.GetPieceList(type, white).Sum(piece => GetPieceEval(piece)));
  }

  public int GetPieceEval(Piece piece)
  {
    var index = piece.Square.Index;
    if (!piece.IsWhite)
    {
      index = 63 - index;
    }

    var offset = 60 - (index % 16) * 4;
    var value = (pieceEvalboards[(int)piece.PieceType - 1][index / 16] & (0xful << offset)) >> offset;
    return (int)(pieceValues[(int)piece.PieceType] * (1 + value * .1));
  }
}