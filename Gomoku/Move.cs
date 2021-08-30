
namespace Gomoku
{
    //This class is used to handle/store a single move with its piece type.
    public class Move
    {
        public bool isCross { get; set; }
        public int position { get; set; }

        //Default Constructor
        public Move(bool isCross, int x) 
        {
            this.isCross = isCross;
            this.position = x;
        }
    }
}
