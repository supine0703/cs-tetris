namespace Tetris
{
    internal struct Position(float x, float y)
    {
        public float X { get; set; } = x;
        public float Y { get; set; } = y;

        public static Position operator *(Position a, float b) => new(a.X * b, a.Y * b);

        public static Position operator /(Position a, float b) => new(a.X / b, a.Y / b);

        public static Position operator +(Position a, Position b) => new(a.X + b.X, a.Y + b.Y);

        public static Position operator -(Position a, Position b) => new(a.X - b.X, a.Y - b.Y);
    }

    internal partial class Block(Block.E_Type type, Block.E_Orientation orientation, Position pos)
    {
        static public readonly Position DefaultPos = new(3, -3);

        public Block(E_Type type) : this(type, E_Orientation.Up, DefaultPos) { }

        public enum E_Type
        {
            I = 0, O, T, J, L, S, Z, Count,
        }

        public enum E_Rotation
        {
            Right = 0, Left, Count,
        }

        public enum E_Orientation
        {
            Up = 0, Right, Down, Left, Count,
        }

        private BlockData _blockData = new(type, orientation);
        private Position _pos = pos;

        public Position Pos => _pos;
        public E_Type Type => _blockData.BlockType;
        public E_Orientation Orientation => _blockData.Orientation;
        public Position RotateCenter => _blockData.RotateCenter;
        public Position[] Placeholders => _blockData.Placeholders;

        public Position[] GetPrevWallKickTable(E_Rotation rotation)
        {
            if (rotation != E_Rotation.Left && rotation != E_Rotation.Right)
            {
                throw new ArgumentException("Invalid direction");
            }

            var prevI = (int)_blockData.Orientation;
            prevI += rotation == E_Rotation.Left ? 1 : (int)E_Orientation.Count - 1;
            prevI %= (int)E_Orientation.Count;
            return _blockData.AllWallKickTable[((int)rotation * (int)E_Orientation.Count) + prevI];
        }
        public Position[] GetCurrWallKickTable(E_Rotation rotation)
        {
            if (rotation != E_Rotation.Left && rotation != E_Rotation.Right)
            {
                throw new ArgumentException("Invalid direction");
            }
            return _blockData.AllWallKickTable[((int)rotation * (int)E_Orientation.Count) + (int)_blockData.Orientation];
        }

        public void TwiceRptate() { _blockData.TwiceRotate(); }
        public void RightRotate() { _blockData.RightRotate(); }
        public void LeftRotate() { _blockData.LeftRotate(); }

        public void MoveLeftOrRight(float x) { _pos.X += x; }
        public void MoveUpOrDown(float y) { _pos.Y += y; }
    }
}
