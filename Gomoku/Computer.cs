using System;
using System.Collections.Generic;
using System.Threading;

namespace Gomoku
{
    //This class is used to handle the movement of computer.
    class Computer
    {
        //Declaring neccessary variables
        public static Random random = new Random();
        private object lockerObj = new();
        private List<KeyValuePair<int, int>> outcomes = new List<KeyValuePair<int, int>>();

        //Default Constructor
        public Computer()
        {
        }

        //This function uses the AlphaBetaPruning Algorithm to handle the computer AI.
        public int AlphaBeta(int alpha, int beta, Gameboard board, int depth, int color)
        {
            Gameboard boardClone = board;
            if (depth == 0)
            {
                return AssessGameboard(boardClone) * color;
            }
            int[] moves = boardClone.GetBestMoves();
            for (int i = 0; i < moves.Length; i++)
            {
                boardClone = new Gameboard(board);
                boardClone.SwitchSquare(moves[i]);
                color = boardClone.crossTurn ? -1 : 1;
                int value = AssessGameboard(boardClone) * color;
                if (value > 9000)
                {
                    return -value * depth;
                }
                else if (value < -9000)
                {
                    return -value * depth;
                }
                boardClone.SwitchTurn();
                int score = -AlphaBeta(-beta, -alpha, boardClone, depth - 1, color);
                if (beta != -2147483648 && score >= beta) return beta;
                if (score > alpha) alpha = score;
            }
            return alpha;
        }

        //This is the optimized version of AlphaBetaPruning Algorithm
        public int AlphaBetaOptimized(int alpha, int beta, Gameboard board, int depth, int color, List<int> lastMoves)
        {
            Gameboard boardClone = board;
            if (depth == 0)
            {
                return AssessGameboard(boardClone) * color;
            }
            List<int> moves;
            if (lastMoves != null)
            {
                moves = lastMoves;
            }
            else
            {
                moves = new List<int>(boardClone.GetBestMoves());
            }
            for (int i = 0; i < moves.Count; i++)
            {
                boardClone = new Gameboard(board);
                if (boardClone.board[moves[i]] != 0)
                {
                    continue;
                }
                boardClone.SwitchSquare(moves[i]);
                color = boardClone.crossTurn ? -1 : 1;
                int value = AssessGameboard(boardClone) * color;
                if (value > 9000)
                {
                    return -value * depth;
                }
                else if (value < -9000)
                {
                    return -value * depth;
                }
                boardClone.SwitchTurn();
                List<int> movesCopy = new List<int>(moves);
                int[] movesToAdd = FindNewMoves(boardClone, boardClone.previousMove);
                for (int j = 0; j < movesToAdd.Length; j++)
                {
                    if (!movesCopy.Contains(movesToAdd[j]))
                    {
                        movesCopy.Add(movesToAdd[j]);
                    }
                }
                int score = -AlphaBetaOptimized(-beta, -alpha, boardClone, depth - 1, color, movesCopy);
                if (beta != -2147483648 && score >= beta) return beta;
                if (score > alpha) alpha = score;
            }
            return alpha;
        }

        public void AlphaBetaFromMoves(Gameboard board, int depth, int[] moves)
        {
            if (moves.Length == 0)
            {
                lock (lockerObj)
                {
                    outcomes.Add(new KeyValuePair<int, int>(-1, -1));
                }
                return;
            }
            int alpha = int.MinValue;
            int beta = int.MaxValue;
            Gameboard bCopy;
            int bestId = 0;
            int color = board.crossTurn ? 1 : -1;
            for (int i = 0; i < moves.Length; i++)
            {
                bCopy = new Gameboard(board);
                bCopy.SwitchSquare(moves[i]);
                bCopy.SwitchTurn();
                int score;
                if (depth > 5)
                {
                    score = -AlphaBetaOptimized(-beta, -alpha, bCopy, depth - 1, color, null);
                }
                else
                {
                    score = -AlphaBeta(-beta, -alpha, bCopy, depth - 1, color);
                }
                if (score > alpha)
                {
                    alpha = score;
                    bestId = i;
                }
            }
            lock (lockerObj)
            {
                outcomes.Add(new KeyValuePair<int, int>(moves[bestId], alpha));
            }
        }

        //This function is used to take turn by computer.
        public int ComputerSelectSquare(Gameboard board, int depth, int maxThreadNum)
        {
            Queue<int> moves;
            if (depth > 4)
            {
                moves = new Queue<int>(SearchMoves(board, 3));
            }
            else
            {
                moves = new Queue<int>(board.GetBestMoves());
            }
            int moveNum = moves.Count;
            if (moves.Count == 0)
            {
                return (board.size * board.size) / 2;
            }
            else if (moves.Count == 1)
            {
                return moves.Dequeue();
            }
            int bestMove = 0;
            int alpha = int.MinValue;
            int color = board.crossTurn ? 1 : -1;
            int threadNum = 4;
            if (moves.Count <= 4)
            {
                threadNum = moves.Count;
            }
            else if (moves.Count >= 32)
            {
                threadNum = maxThreadNum - 2 > 4 ? maxThreadNum - 2 : 4;
            }
            else
            {
                for (int i = maxThreadNum; i >= 4; i--)
                {
                    if (moves.Count % i == 0)
                    {
                        threadNum = i;
                        break;
                    }
                }
            }
            Thread[] threads = new Thread[threadNum];
            outcomes.Clear();
            for (int i = 0; i < threadNum; i++)
            {
                int[] threadMoves;
                if (moves.Count > moveNum / threadNum)
                {
                    threadMoves = new int[moveNum / threadNum];
                }
                else
                {
                    threadMoves = new int[moves.Count];
                }
                for (int j = 0; j < threadMoves.Length; j++)
                {
                    threadMoves[j] = moves.Dequeue();
                }
                threads[i] = new Thread(() => { AlphaBetaFromMoves(board, depth, threadMoves); });
                threads[i].Start();
            }
            for (int i = 0; i < threadNum; i++)
            {
                threads[i].Join();
            }
            for (int i = 0; i < threadNum; i++)
            {
                if (outcomes[i].Value > alpha && outcomes[i].Key != -1)
                {
                    bestMove = outcomes[i].Key;
                    alpha = outcomes[i].Value;
                }
            }

            return bestMove;
        }

        //This function is used to read the full gameboard and get its value.
        public int AssessGameboard(Gameboard state)
        {
            int value = 0;
            for (int i = 0; i < state.board.Length; i++)
            {
                if (state.board[i] != 0)
                {
                    int color = state.board[i] == 1 ? 1 : -1;
                    int row = InRow(state, i, 1, 0);
                    value += (row) * color;
                    row = InRow(state, i, 0, 1);
                    value += (row) * color;
                    row = InRow(state, i, 1, 1);
                    value += (row) * color;
                    row = InRow(state, i, -1, 1);
                    value += (row) * color;
                }
            }
            return value;
        }

        //This function is used to search the best moves
        public int[] SearchMoves(Gameboard state, int additionalDepth)
        {
            List<int> moves = new List<int>(state.GetBestMoves());
            //Go through the moves and remove the real bad ones
            for (int i = moves.Count - 1; i >= 0; i--)
            {
                Gameboard copy = new Gameboard(state);
                int alpha = int.MinValue;
                int beta = int.MaxValue;
                copy.SwitchSquare(moves[i]);
                copy.SwitchTurn();
                int win = copy.EvaluateWin(copy.previousMove);
                if ((win == 1 && state.crossTurn) || (win == 2 && !state.crossTurn))
                {
                    return new int[] { moves[i] };
                }
                int color = copy.crossTurn ? 1 : -1;
                int value = AlphaBeta(alpha, beta, copy, additionalDepth, color);
                if (value > 10000)
                {
                    moves.RemoveAt(i);
                }
                else if (value < -10000)
                {
                    return new int[] { moves[i] };
                }
            }
            if (moves.Count == 0)
            {
                return state.GetBestMoves();
            }
            return moves.ToArray();
        }

        //This function is used to find new moves
        public int[] FindNewMoves(Gameboard state, int lastMove)
        {
            List<int> moves = new List<int>();
            int[] rowMoves = FindMovesInRow(state, lastMove, 1, 0);
            for (int i = 0; i < rowMoves.Length; i++)
            {
                moves.Add(rowMoves[i]);
            }
            rowMoves = FindMovesInRow(state, lastMove, 0, 1);
            for (int i = 0; i < rowMoves.Length; i++)
            {
                moves.Add(rowMoves[i]);
            }
            rowMoves = FindMovesInRow(state, lastMove, 1, 1);
            for (int i = 0; i < rowMoves.Length; i++)
            {
                moves.Add(rowMoves[i]);
            }
            rowMoves = FindMovesInRow(state, lastMove, -1, 1);
            for (int i = 0; i < rowMoves.Length; i++)
            {
                moves.Add(rowMoves[i]);
            }
            return moves.ToArray();
        }

        //This function is used to find moves in row
        public int[] FindMovesInRow(Gameboard state, int pos, int dirX, int dirY)
        {
            int inRow = 1;
            List<int> moves = new List<int>();
            int c = state.board[pos];
            while (state.ColFromPos(pos) - dirX >= 0 && state.ColFromPos(pos) - dirX < state.size
                     && state.RowFromPos(pos) - dirY >= 0 && state.RowFromPos(pos) - dirY < state.size
                     && state.board[pos - dirX - (dirY * state.size)] == c)
            {
                pos = pos - dirX - (dirY * state.size);
            }
            if (state.ColFromPos(pos) - dirX >= 0 && state.ColFromPos(pos) - dirX < state.size
                     && state.RowFromPos(pos) - dirY >= 0 && state.RowFromPos(pos) - dirY < state.size
                     && state.board[pos - dirX - (dirY * state.size)] == 0)
            {
                moves.Add(pos - dirX - (dirY * state.size));
            }
            while (state.ColFromPos(pos) + dirX >= 0 && state.ColFromPos(pos) + dirX < state.size
                     && state.RowFromPos(pos) + dirY >= 0 && state.RowFromPos(pos) + dirY < state.size
                     && state.board[pos + dirX + (dirY * state.size)] == c)
            {
                pos = pos + dirX + (dirY * state.size);
                inRow++;
            }
            if (state.ColFromPos(pos) + dirX >= 0 && state.ColFromPos(pos) + dirX < state.size
                     && state.RowFromPos(pos) + dirY >= 0 && state.RowFromPos(pos) + dirY < state.size
                     && state.board[pos + dirX + (dirY * state.size)] == 0)
            {
                moves.Add(pos + dirX + (dirY * state.size));
            }
            if (inRow >= 3)
            {
                return moves.ToArray();
            }
            else
            {
                return new int[0];
            }
        }

        public int InRow(Gameboard state, int pos, int dirX, int dirY)
        {
            int inRow = 1;
            int blocks = 0;
            int c = state.board[pos];
            while (state.ColFromPos(pos) - dirX >= 0 && state.ColFromPos(pos) - dirX < state.size
                     && state.RowFromPos(pos) - dirY >= 0 && state.RowFromPos(pos) - dirY < state.size
                     && state.board[pos - dirX - (dirY * state.size)] == c)
            {
                pos = pos - dirX - (dirY * state.size);
            }
            if (state.ColFromPos(pos) - dirX < 0 || state.ColFromPos(pos) - dirX >= state.size
                     || state.RowFromPos(pos) - dirY < 0 || state.RowFromPos(pos) - dirY >= state.size
                     || (state.board[pos - dirX - (dirY * state.size)] != c && state.board[pos - dirX - (dirY * state.size)] != 0))
            {
                blocks++;
            }
            while (state.ColFromPos(pos) + dirX >= 0 && state.ColFromPos(pos) + dirX < state.size
                     && state.RowFromPos(pos) + dirY >= 0 && state.RowFromPos(pos) + dirY < state.size
                     && state.board[pos + dirX + (dirY * state.size)] == c)
            {
                pos = pos + dirX + (dirY * state.size);
                inRow++;
            }
            if (state.ColFromPos(pos) + dirX < 0 || state.ColFromPos(pos) + dirX >= state.size
                     || state.RowFromPos(pos) + dirY < 0 || state.RowFromPos(pos) + dirY >= state.size
                     || (state.board[pos + dirX + (dirY * state.size)] != c && state.board[pos + dirX + (dirY * state.size)] != 0))
            {
                blocks++;
            }
            if (inRow >= 5)
            {
                return 10000;
            }
            else
            {
                if (inRow == 3)
                {
                    inRow = 7;
                }
                else if (inRow == 4)
                {
                    inRow = 12;
                }
                else if (inRow == 1)
                {
                    return 0;
                }
                if (blocks == 1)
                {
                    return inRow / 2;
                }
                else if (blocks == 2)
                {
                    return 0;
                }
                else
                {
                    return inRow;
                }
            }
        }
    }
}