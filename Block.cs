namespace Tetris
{
    internal struct Position(float x, float y)
    {
        public float x = x;
        public float y = y;

        public static Position operator *(Position a, float b) => new(a.x * b, a.y * b);

        public static Position operator /(Position a, float b) => new(a.x / b, a.y / b);

        public static Position operator +(Position a, Position b) => new(a.x + b.x, a.y + b.y);

        public static Position operator -(Position a, Position b) => new(a.x - b.x, a.y - b.y);
    }

    internal partial class Block(Block.E_BlockType type, Block.E_FacingDirection facing, Position pos)
    {
        static public readonly Position DefaultPos = new(3, -3);

        public Block(E_BlockType type) : this(type, E_FacingDirection.Up, DefaultPos) { }

        public enum E_BlockType
        {
            I = 0, O, T, J, L, S, Z, Count,
        }

        public enum E_RotateDirection
        {
            Right = 0, Left, Count,
        }

        public enum E_FacingDirection
        {
            Up = 0, Right, Down, Left, Count,
        }

        private BlockData _blockData = new(type, facing);
        private Position _pos = pos;

        public Position Pos => _pos;
        public E_BlockType BlockType => _blockData.BlockType;
        public E_FacingDirection FacingDirection => _blockData.FacingDirection;
        public Position RotateCenter => _blockData.RotateCenter;
        public Position[] Placeholders => _blockData.Placeholders;

        public Position[] GetPrevKickWallTable(E_RotateDirection direction)
        {
            if (direction != E_RotateDirection.Left && direction != E_RotateDirection.Right)
            {
                throw new ArgumentException("Invalid direction");
            }

            var prevI = (int)_blockData.FacingDirection;
            prevI += direction == E_RotateDirection.Left ? 1 : (int)E_FacingDirection.Count - 1;
            prevI %= (int)E_FacingDirection.Count;
            return _blockData.AllKickWallTable[((int)direction * (int)E_FacingDirection.Count) + prevI];
        }
        public Position[] GetCurrKickWallTable(E_RotateDirection direction)
        {
            if (direction != E_RotateDirection.Left && direction != E_RotateDirection.Right)
            {
                throw new ArgumentException("Invalid direction");
            }
            return _blockData.AllKickWallTable[((int)direction * (int)E_FacingDirection.Count) + (int)_blockData.FacingDirection];
        }

        public void RightRotate() { _blockData.RightRotate(); }
        public void LeftRotate() { _blockData.LeftRotate(); }

        public void MoveLeftOrRight(float x) { _pos.x += x; }
        public void MoveUpOrDown(float y) { _pos.y += y; }
    }
}
