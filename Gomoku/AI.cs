using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Gomoku
{
    class AI
    {
        public static Random rand = new Random();
        private object locker = new();
        private List<KeyValuePair<int, int>> results = new List<KeyValuePair<int, int>>();

        public AI()
        {
        }

        public int GetRandomMove(Gameboard b, int searchRange = 1)
        {
            int[] moves = b.GetBestMoves(searchRange);
            if (moves.Length == 0 && b.board[0] == 0)
            {
                return (b.size * b.size) / 2;
            }
            else if (moves.Length == 0)
            {
                return -1;
            }
            return moves[rand.Next(moves.Length)];
        }

        public int GetHighestValueMove(Gameboard b, int searchRange = 1)
        {
            int[] moves = b.GetBestMoves(searchRange);
            if (moves.Length == 0)
            {
                return (b.size * b.size) / 2;
            }
            int maxVal = int.MinValue;
            int bestI = 0;
            for (int i = 0; i < moves.Length; i++)
            {
                Gameboard copy = new Gameboard(b);
                copy.SwitchSquare(moves[i]);
                int val;
                if (b.crossTurn)
                {
                    val = AssessGameboard(copy);
                }
                else
                {
                    val = -AssessGameboard(copy);
                }
                if (val > maxVal)
                {
                    maxVal = val;
                    bestI = i;
                }
            }
            return moves[bestI];
        }

        public int MonteCarloSearch(Gameboard rootState, double searchTimer)
        {
            //Check if we can find a winning move in one move
            string deb = "";
            if (true)
            {
                int winCheck = GetHighestValueMove(rootState, 1);
                if (winCheck == (rootState.size * rootState.size) / 2)
                {
                    return (rootState.size * rootState.size) / 2;
                }
                Gameboard copy = new Gameboard(rootState);
                copy.SwitchSquare(winCheck);
                int value = rootState.crossTurn ? AssessGameboard(copy) : -AssessGameboard(copy);
                deb += "Highest 1 ply value move is: " + winCheck.ToString() + ", value " + value.ToString() + ".\n  ";
                if (value > 9999)
                {
                    rootState.DebugWrite(deb);
                    return winCheck;
                }
            }
            Node root = new Node(rootState.lastSquare, null, rootState, 1);
            Stopwatch s = new Stopwatch();
            s.Start();
            while (s.Elapsed.Seconds < searchTimer)
            {
                Node node = root;
                Gameboard state = new Gameboard(rootState);
                //Select
                while (node.untriedMoves.Count == 0 && node.nodes.Count != 0)
                {
                    node = NodeSelection(node);
                    state.SwitchSquare(node.move);
                    state.SwitchTurn();
                }
                //Expand
                if (node.untriedMoves.Count != 0)
                {
                    int move = node.GetRandomMove();
                    state.SwitchSquare(move);
                    state.SwitchTurn();
                    node = node.AddChild(move, state);
                }
                //Rollout
                int lastMove = node.move;
                while (AssessGameboard(state) == 0)
                {
                    //int move = RootAlphaBeta(int.MinValue, int.MaxValue, state, 2);
                    int move = GetHighestValueMove(state, 1);
                    state.SwitchSquare(move);
                    state.SwitchTurn();
                    lastMove = move;
                }
                //Backpropagate
                int result = state.EvaluateWin(lastMove);
                while (node != null)
                {
                    node.games++;
                    if (result == -1)
                    {
                        node.wins += 0.4;
                    }
                    else if ((result == 1 && rootState.crossTurn) || (result == 2 && !rootState.crossTurn))
                    {
                        node.wins += 1;
                    }
                    else
                    {
                        node.wins += 0;
                    }
                    node = node.parent;
                }
            }
            int bestI = 0;
            double bestValue = double.MinValue;
            for (int i = 0; i < root.nodes.Count; i++)
            {
                double val = root.nodes[i].games * (root.nodes[i].wins / root.nodes[i].games);
                if (val > bestValue)
                {
                    deb += "Move: " + root.nodes[i].move.ToString() + " has Wins " + root.nodes[i].wins.ToString("0.0") + ", games " + root.nodes[i].games.ToString() + ", = " + (root.nodes[i].wins / root.nodes[i].games).ToString("0.000") + ".\n  ";
                    bestI = i;
                    bestValue = val;
                }
            }
            rootState.DebugWrite(deb);
            //rootState.DebugWrite("Done finding move from " + root.nodes.Count.ToString() + " available.");
            return root.nodes[bestI].move;
        }

        public Node NodeSelection(Node n)
        {
            int bestN = 0;
            double bestV = double.MinValue;
            for (int i = 0; i < n.nodes.Count; i++)
            {
                //double value = Math.Sqrt(2*Math.Log(n.games)/n.nodes[i].games);
                double value = (double)n.nodes[i].wins / (double)n.nodes[i].games + 2.0 * Math.Sqrt(2 * Math.Log(n.games) / n.nodes[i].games);
                if (value > bestV)
                {
                    bestN = i;
                    bestV = value;
                }
            }
            return n.nodes[bestN];
        }

        public int AlphaBeta(int alpha, int beta, Gameboard b, int depth, int color)
        {
            Gameboard bCopy = b;
            if (depth == 0)
            {
                return AssessGameboard(bCopy) * color;
            }
            int[] moves = bCopy.GetBestMoves();
            for (int i = 0; i < moves.Length; i++)
            {
                bCopy = new Gameboard(b);
                bCopy.SwitchSquare(moves[i]);
                color = bCopy.crossTurn ? -1 : 1;
                int value = AssessGameboard(bCopy) * color;
                if (value > 9000)
                {
                    return -value * depth;
                }
                else if (value < -9000)
                {
                    return -value * depth;
                }
                bCopy.SwitchTurn();
                int score = -AlphaBeta(-beta, -alpha, bCopy, depth - 1, color);
                if (beta != -2147483648 && score >= beta) return beta;
                if (score > alpha) alpha = score;
            }
            return alpha;
        }

        public int RootAlphaBeta(int alpha, int beta, Gameboard board, int depth)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            Gameboard bCopy = board;
            int[] moves = bCopy.GetBestMoves();
            if (moves.Length == 0)
            {
                return (board.size * board.size) / 2;
            }
            int bestId = 0;
            int color = board.crossTurn ? 1 : -1;
            string deb = "";
            for (int i = 0; i < moves.Length; i++)
            {
                bCopy = new Gameboard(board);
                bCopy.SwitchSquare(moves[i]);
                bCopy.SwitchTurn();
                int score = -AlphaBeta(-beta, -alpha, bCopy, depth - 1, color);
                if (score > alpha)
                {
                    alpha = score;
                    bestId = i;
                    deb += "Move: " + moves[i].ToString() + " has value " + score.ToString() + ".\n  ";
                }
            }
            s.Stop();
            deb += "\n\n  Execution took " + s.ElapsedMilliseconds.ToString() + " miliseconds.";
            //board.DebugWrite(deb);
            return moves[bestId];
        }

        public int AlphaBetaOptimized(int alpha, int beta, Gameboard b, int depth, int color, List<int> movesLast)
        {
            Gameboard bCopy = b;
            if (depth == 0)
            {
                return AssessGameboard(bCopy) * color;
            }
            List<int> moves;
            if (movesLast != null)
            {
                moves = movesLast;
            }
            else
            {
                moves = new List<int>(bCopy.GetBestMoves());
            }
            for (int i = 0; i < moves.Count; i++)
            {
                bCopy = new Gameboard(b);
                if (bCopy.board[moves[i]] != 0)
                {
                    continue;
                }
                bCopy.SwitchSquare(moves[i]);
                color = bCopy.crossTurn ? -1 : 1;
                int value = AssessGameboard(bCopy) * color;
                if (value > 9000)
                {
                    return -value * depth;
                }
                else if (value < -9000)
                {
                    return -value * depth;
                }
                bCopy.SwitchTurn();
                List<int> movesCopy = new List<int>(moves);
                int[] movesToAdd = FindNewMoves(bCopy, bCopy.previousMove);
                for (int j = 0; j < movesToAdd.Length; j++)
                {
                    if (!movesCopy.Contains(movesToAdd[j]))
                    {
                        movesCopy.Add(movesToAdd[j]);
                    }
                }
                int score = -AlphaBetaOptimized(-beta, -alpha, bCopy, depth - 1, color, movesCopy);
                if (beta != -2147483648 && score >= beta) return beta;
                if (score > alpha) alpha = score;
            }
            return alpha;
        }

        public void AlphaBetaFromMoves(Gameboard board, int depth, int[] moves)
        {
            if (moves.Length == 0)
            {
                lock (locker)
                {
                    results.Add(new KeyValuePair<int, int>(-1, -1));
                }
                return;
            }
            int alpha = int.MinValue;
            int beta = int.MaxValue;
            Gameboard bCopy = board;
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
            lock (locker)
            {
                results.Add(new KeyValuePair<int, int>(moves[bestId], alpha));
            }
        }

        public int MultiThreadedRAlphaBeta(Gameboard board, int depth, int maxThreadNum)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            Queue<int> moves;
            if (depth > 4)
            {
                moves = new Queue<int>(FindMoves(board, 3));
            }
            else
            {
                moves = new Queue<int>(board.GetBestMoves());
            }
            //Queue<int> moves = new Queue<int>(board.GetInterestingMoves());
            int moveNum = moves.Count;
            if (moves.Count == 0)
            {
                return (board.size * board.size) / 2;
            }
            else if (moves.Count == 1)
            {
                board.DebugWrite("Execution took " + s.ElapsedMilliseconds.ToString() + " miliseconds for " + moveNum + " move. (defmoves) = " + board.GetBestMoves().Length);
                return moves.Dequeue();
            }
            int bestMove = 0;
            int alpha = int.MinValue;
            int color = board.crossTurn ? 1 : -1;
            string deb = "";
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
            results.Clear();
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
                deb += "Move: " + results[i].Key.ToString() + " has value " + results[i].Value.ToString() + ".\n  ";
                if (results[i].Value > alpha && results[i].Key != -1)
                {
                    bestMove = results[i].Key;
                    alpha = results[i].Value;
                }
            }
            s.Stop();
            deb += "\n\n  Execution took " + s.ElapsedMilliseconds.ToString() + " miliseconds for " + moveNum + " moves. (defmoves) = " + board.GetBestMoves().Length;
            //deb += "\n\n After last move FindNewMoves found " + FindNewMoves(board, board.lastMove).Length + " new moves.";
            //board.DebugWrite(deb);
            return bestMove;
        }

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
            //state.DebugWrite("Board value: " + value.ToString());
            return value;
        }

        public int[] FindMoves(Gameboard state, int additionalDepth)
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
                //Console.WriteLine("move " + moves[i] + " val " + value);
                //Console.Read();
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