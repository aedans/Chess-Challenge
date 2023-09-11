using ChessChallenge.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MyBot7 : IChessBot
{
  int[] pieceValues = { 0, 100, 320, 330, 500, 900 };
  ulong[][] pieceEvalboards = new ulong[][]{
    new ulong[] { 0x888888889aa44aa9, 0x97688679888cc888, 0x99adda99aaceecaa, 0xffffffffffffffff, },
    new ulong[] { 0x0022220004899840, 0x29abba9228bccb82, 0x29bccb9228abba82, 0x0488884000222200, },
    new ulong[] { 0x4666666469888896, 0x6aaaaaa668aaaa86, 0x699aa996689aa986, 0x6888888646666664, },
    new ulong[] { 0x8889988878888887, 0x7888888778888887, 0x7888888778888887, 0x9aaaaaa988888888, },
    new ulong[] { 0x4667766468888986, 0x6899999678999988, 0x7899998768999986, 0x6888888646677664, },
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

      legalMoves.InsertRange(0, moves.Where(move => legalMoves.Contains(move)));
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
    var index = piece.IsWhite ? piece.Square.Index : 63 - piece.Square.Index;
    var offset = 60 - index % 16 * 4;
    var value = 5 * ((int) ((pieceEvalboards[(int)piece.PieceType - 1][index / 16] & (0xful << offset)) >> offset) - 8);
    return (int)(pieceValues[(int)piece.PieceType] + value);
  }
}