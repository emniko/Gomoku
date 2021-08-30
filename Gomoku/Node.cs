using System.Collections.Generic;

namespace Gomoku
{
    class Node
    {
        public int move;
        public List<int> untriedMoves;
        public double wins = 0;
        public int games = 0;
        public Node parent;
        public List<Node> nodes = new List<Node>();

        public Node(int move, Node parent, Gameboard state, int searchRange = 1)
        {
            this.move = move;
            this.parent = parent;
            this.untriedMoves = new List<int>(state.GetBestMoves(searchRange));
        }

        public int GetRandomMove()
        {
            int next = AI.rand.Next(untriedMoves.Count);
            int move = untriedMoves[next];
            untriedMoves.RemoveAt(next);
            return move;
        }

        public Node AddChild(int move, Gameboard state)
        {
            Node child = new Node(move, this, state);
            nodes.Add(child);
            return child;
        }
    }
}