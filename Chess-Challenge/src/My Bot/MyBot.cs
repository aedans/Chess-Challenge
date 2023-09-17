using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
  int[] pieceValues = { 0, 100, 320, 330, 500, 900 };
  ulong[][] pieceEvalboards = new ulong[][]{
    new ulong[] { 0x888888889aa44aa9, 0x97688679888cc888, 0x99adda99aaceecaa, 0xffffffffffffffff, },
    new ulong[] { 0x0022220004899840, 0x29abba9228bccb82, 0x29bccb9228abba82, 0x0488884000222200, },
    new ulong[] { 0x4666666469888896, 0x6aaaaaa668aaaa86, 0x699aa996689aa986, 0x6888888646666664, },
    new ulong[] { 0x8889988878888887, 0x7888888778888887, 0x7888888778888887, 0x9aaaaaa988888888, },
    new ulong[] { 0x4667766468888986, 0x6899999678999988, 0x7899998768999986, 0x6888888646677664, },
  };

  Dictionary<ulong, (int, int, List<Move>, List<Move>)> evaluations = new();

  public Move Think(Board board, Timer timer)
  {
    var depth = 1;
    var bestEval = 0;
    Move bestMove = Move.NullMove;
    var alpha = -99999;
    var beta = 99999;
    var isTime = false;

    while (!isTime)
    {
      var eval = EvalMove(depth == 1 ? null : timer, board, depth, alpha, beta, new List<Move>(), ref isTime, out Move move);

      if (move == Move.NullMove)
      {
        // if (eval <= alpha) 
        // {
        //   alpha = -99999;
        // }
        // else if (eval >= beta)
        // {
        //   beta = 99999;
        // }
      }
      else
      {
        bestEval = eval;
        bestMove = move;
        // alpha = eval - 50;
        // beta = eval + 50;
        depth++;
      }
    }

    Console.WriteLine("Depth: " + depth + " Eval: " + bestEval + " " + bestMove);
    return bestMove;
  }

  public int EvalMove(Timer? timer, Board board, int depth, int alpha, int beta, List<Move> parentKillers, ref bool isTime, out Move bestMove)
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

    var legalMoves = new Span<Move>(new Move[256]);
    board.GetLegalMovesNonAlloc(ref legalMoves);

    var hasCaptures = false;
    foreach (var move in legalMoves)
    {
      if (move.IsCapture && move.CapturePieceType != PieceType.Pawn)
      {
        hasCaptures = true;
        break;
      }
    }

    if ((depth <= 0 && !hasCaptures && !board.IsInCheck()) || depth <= -2)
    {
      return PieceEvals(board, board.IsWhiteToMove) - PieceEvals(board, !board.IsWhiteToMove);
    }

    var allMoves = new List<Move>();
    var childKillers = new List<Move>();

    if (evaluations.ContainsKey(board.ZobristKey))
    {
      var (evalDepth, eval, moves, killers) = evaluations[board.ZobristKey];
      if (evalDepth >= depth && moves.Count > 0)
      {
        bestMove = moves.First();
        return eval;
      }

      childKillers.AddRange(killers);

      if (depth > 0) 
      {
        foreach (var move in moves)
        {
          if (legalMoves.Contains(move))
          {
            allMoves.Add(move);
          }
        }
      }
    }

    if (depth > 0) 
    {
      foreach (var move in parentKillers)
      {
        if (legalMoves.Contains(move))
        {
          allMoves.Add(move);
        }
      }
    }

    foreach (var move in legalMoves)
    {
      if (move.IsPromotion || move.IsCapture)
      {
        allMoves.Add(move);
      }
    }

    if (depth > 0 || allMoves.Count == 0)
    {
      foreach (var move in legalMoves)
      {
        allMoves.Add(move);
      }
    }

    bestMove = Move.NullMove;
    isTime = timer != null && timer.MillisecondsElapsedThisTurn > (timer.MillisecondsRemaining / 50) + timer.IncrementMilliseconds;

    var analyzedMoves = new HashSet<Move>();
    var bestMoves = new List<Move>() { allMoves[0] };
    foreach (var move in allMoves)
    {
      if (isTime) 
      {
        bestMove = Move.NullMove;
        return 0;
      }

      if (alpha >= beta)
      {
        break;
      }

      if (analyzedMoves.Contains(move))
      {
        continue;
      }
      else
      {
        analyzedMoves.Add(move);
      }

      board.MakeMove(move);

      var eval = -EvalMove(timer, board, depth - 1, -beta, -alpha, childKillers, ref isTime, out Move _);

      board.UndoMove(move);

      if (eval > alpha)
      {
        bestMove = move;
        bestMoves.Insert(0, move);
        alpha = eval;
      }
    }

    evaluations[board.ZobristKey] = (depth, alpha, bestMoves, childKillers);

    return alpha;
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
    var value = 5 * ((int)((pieceEvalboards[(int)piece.PieceType - 1][index / 16] & (0xful << offset)) >> offset) - 8);
    return pieceValues[(int)piece.PieceType] + value;
  }
}