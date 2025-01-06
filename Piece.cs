using NAudio.Gui;

namespace Tetris
{
    internal struct Position(float x, float y)
    {
        public float X { get; set; } = x;
        public float Y { get; set; } = y;

        public Position(float all) : this(all, all) { }

        public readonly PointF ToPointF(float unitSize = 1.0f) => new(X * unitSize, Y * unitSize);

        public static Position operator *(Position a, float b) => new(a.X * b, a.Y * b);
        public static Position operator /(Position a, float b) => new(a.X / b, a.Y / b);
        public static Position operator +(Position a, Position b) => new(a.X + b.X, a.Y + b.Y);
        public static Position operator -(Position a, Position b) => new(a.X - b.X, a.Y - b.Y);
        public static bool operator ==(Position a, Position b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Position a, Position b) => !(a == b);

        public override readonly bool Equals(object? obj)
        {
            if (obj == null) { return false; }
            if (obj is Position other) { return this == other; }
            return false;
        }

        public override readonly int GetHashCode() => HashCode.Combine(X, Y);
    }

    internal partial class Piece(Piece.E_Type type, Piece.E_Orientation orientation, Position pos) : IEquatable<Piece>
    {
        public static readonly Position InitialPos = new(3, -3);

        public static Position[] InitialPlaceholders(E_Type type) => PieceData.AllPlaceholders(type)[0];

        public bool Equals(Piece? other)
        {
            if (other == null) { return false; }
            return this.Type == other.Type && this.Orientation == other.Orientation && this.Pos == other.Pos;
        }
        public override bool Equals(object? obj) => Equals(obj as Piece);
        public override int GetHashCode() => HashCode.Combine(Type, Orientation, Pos);

        public Piece(E_Type type) : this(type, default, InitialPos) { }
        public Piece(Piece other) : this(other.Type, other.Orientation, other.Pos) { }

        public enum E_Type { I = 0, O, T, J, L, S, Z, }
        public static int E_TypeCount => Enum.GetValues(typeof(E_Type)).Length;

        public enum E_Rotation { Right = 0, Left, }
        public static int E_RotationCount => Enum.GetValues(typeof(E_Rotation)).Length;

        public enum E_Orientation { Up = 0, Right, Down, Left, }
        public static int E_OrientationCount => Enum.GetValues(typeof(E_Orientation)).Length;

        private PieceData _blockData = new(type, orientation);
        private Position _pos = pos;

        public Position Pos => _pos;
        public E_Type Type => _blockData.PieceType;
        public E_Orientation Orientation => _blockData.Orientation;
        public Position RotateCenter => _blockData.RotateCenter;
        public Position[] Placeholders => _blockData.Placeholders;

        public Position[] GetPrevWallKickTable(E_Rotation rotation)
        {
            if (rotation != E_Rotation.Left && rotation != E_Rotation.Right)
            {
                throw new ArgumentOutOfRangeException(nameof(rotation));
            }

            var prevI = (int)_blockData.Orientation;
            prevI += rotation == E_Rotation.Left ? 1 : E_OrientationCount - 1;
            prevI %= E_OrientationCount;
            return _blockData.AllWallKickTable[((int)rotation * E_OrientationCount) + prevI];
        }
        public Position[] GetCurrWallKickTable(E_Rotation rotation)
        {
            if (rotation != E_Rotation.Left && rotation != E_Rotation.Right)
            {
                throw new ArgumentOutOfRangeException(nameof(rotation));
            }
            return _blockData.AllWallKickTable[((int)rotation * E_OrientationCount) + (int)_blockData.Orientation];
        }

        public void RotateTwice() { _blockData.RotateTwice(); }
        public void RotateRight() { _blockData.RotateRight(); }
        public void RotateLeft() { _blockData.RotateLeft(); }

        public void MoveLeftOrRight(float x) { _pos.X += x; }
        public void MoveUpOrDown(float y) { _pos.Y += y; }
    }
}
