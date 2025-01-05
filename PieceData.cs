namespace Tetris
{
    internal partial class Piece
    {
        private struct PieceData(E_Type type, E_Orientation orientation)
        {
            public readonly E_Type PieceType => type;
            public readonly E_Orientation Orientation => orientation;
            public readonly Position[] Placeholders => AllPlaceholders(type)[(int)orientation];

            public void TwiceRotate()
            {
                orientation = (E_Orientation)(((int)orientation + 2) % E_OrientationCount);
            }

            public void RightRotate()
            {
                orientation = (E_Orientation)(((int)orientation + 1) % E_OrientationCount);
            }

            public void LeftRotate()
            {
                orientation = (E_Orientation)(((int)orientation + E_OrientationCount - 1) % E_OrientationCount);
            }

            public readonly Position RotateCenter => type switch
            {
                E_Type.I or E_Type.O => new(2, 2),
                E_Type.T or E_Type.J or E_Type.L or E_Type.S or E_Type.Z => new(1.5f, 2.5f),
                _ => throw new ArgumentOutOfRangeException(),
            };

            public readonly Position[][] AllWallKickTable => type switch
            {
                // UR, RR, DR, LR, UL, RL, DL, LL
                E_Type.O => [
                    /* UR */ [new(0, 0)],
                    /* RR */ [new(0, 0)],
                    /* DR */ [new(0, 0)],
                    /* LR */ [new(0, 0)],
                    /* UL */ [new(0, 0)],
                    /* RL */ [new(0, 0)],
                    /* DL */ [new(0, 0)],
                    /* LL */ [new(0, 0)],
                ],
                E_Type.I => [
                    /* UR */ [new(0, 0), new(-2, 0), new(+1, 0), new(-2, -1), new(+1, +2)],
                    /* RR */ [new(0, 0), new(-1, 0), new(+2, 0), new(-1, +2), new(+2, -1)],
                    /* DR */ [new(0, 0), new(+2, 0), new(-1, 0), new(+2, +1), new(-1, -2)],
                    /* LR */ [new(0, 0), new(+1, 0), new(-2, 0), new(+1, -2), new(-2, +1)],
                    /* UL */ [new(0, 0), new(-1, 0), new(+2, 0), new(-1, +2), new(+2, -1)],
                    /* RL */ [new(0, 0), new(+2, 0), new(-1, 0), new(+2, +1), new(-1, -2)],
                    /* DL */ [new(0, 0), new(+1, 0), new(-2, 0), new(+1, -2), new(-2, +1)],
                    /* LL */ [new(0, 0), new(-2, 0), new(+1, 0), new(-2, -1), new(+1, +2)],
                ],
                E_Type.T or E_Type.J or E_Type.L or E_Type.S or E_Type.Z =>
                [
                    /* UR */ [new(0, 0), new(-1, 0), new(-1, +1), new(0, -2), new(-1, -2)],
                    /* RR */ [new(0, 0), new(+1, 0), new(+1, -1), new(0, +2), new(+1, +2)],
                    /* DR */ [new(0, 0), new(+1, 0), new(+1, +1), new(0, -2), new(+1, -2)],
                    /* LR */ [new(0, 0), new(-1, 0), new(-1, -1), new(0, +2), new(-1, +2)],
                    /* UL */ [new(0, 0), new(+1, 0), new(+1, +1), new(0, -2), new(+1, -2)],
                    /* RL */ [new(0, 0), new(+1, 0), new(+1, -1), new(0, +2), new(+1, +2)],
                    /* DL */ [new(0, 0), new(-1, 0), new(-1, +1), new(0, -2), new(-1, -2)],
                    /* LL */ [new(0, 0), new(-1, 0), new(-1, -1), new(0, +2), new(-1, +2)],
                ],
                _ => throw new ArgumentOutOfRangeException(),
            };

            public static Position[][] AllPlaceholders(E_Type type) => type switch
            {
                E_Type.I => [
                    [new(0, 1), new(1, 1), new(2, 1), new(3, 1)], // -------- | ----[]-- | -------- | --[]----
                    [new(2, 0), new(2, 1), new(2, 2), new(2, 3)], // [][][][] | ----[]-- | -------- | --[]----
                    [new(0, 2), new(1, 2), new(2, 2), new(3, 2)], // -------- | ----[]-- | [][][][] | --[]----
                    [new(1, 0), new(1, 1), new(1, 2), new(1, 3)], // -------- | ----[]-- | -------- | --[]----
                ],
                E_Type.O => [
                    [new(1, 1), new(2, 1), new(1, 2), new(2, 2)], // -------- | -------- | -------- | --------
                    [new(1, 1), new(2, 1), new(1, 2), new(2, 2)], // --[][]-- | --[][]-- | --[][]-- | --[][]--
                    [new(1, 1), new(2, 1), new(1, 2), new(2, 2)], // --[][]-- | --[][]-- | --[][]-- | --[][]--
                    [new(1, 1), new(2, 1), new(1, 2), new(2, 2)], // -------- | -------- | -------- | --------
                ],
                E_Type.T => [
                    [new(1, 1), new(0, 2), new(1, 2), new(2, 2)], // -------- | -------- | -------- | --------
                    [new(2, 2), new(1, 1), new(1, 2), new(1, 3)], // --[]---- | --[]---- | -------- | --[]----
                    [new(1, 3), new(2, 2), new(1, 2), new(0, 2)], // []()[]-- | --()[]-- | []()[]-- | []()----
                    [new(0, 2), new(1, 3), new(1, 2), new(1, 1)], // -------- | --[]---- | --[]---- | --[]----
                ],
                E_Type.J => [
                    [new(0, 1), new(0, 2), new(1, 2), new(2, 2)], // -------- | -------- | -------- | --------
                    [new(2, 1), new(1, 1), new(1, 2), new(1, 3)], // []------ | --[][]-- | -------- | --[]----
                    [new(2, 3), new(2, 2), new(1, 2), new(0, 2)], // []()[]-- | --()---- | []()[]-- | --()----
                    [new(0, 3), new(1, 3), new(1, 2), new(1, 1)], // -------- | --[]---- | ----[]-- | [][]----
                ],
                E_Type.L => [
                    [new(2, 1), new(0, 2), new(1, 2), new(2, 2)], // -------- | -------- | -------- | --------
                    [new(2, 3), new(1, 1), new(1, 2), new(1, 3)], // ----[]-- | --[]---- | -------- | --[]----
                    [new(0, 3), new(2, 2), new(1, 2), new(0, 2)], // []()[]-- | --()---- | []()[]-- | --()----
                    [new(0, 1), new(1, 3), new(1, 2), new(1, 1)], // -------- | --[][]-- | []------ | --[][]--
                ],
                E_Type.S => [
                    [new(2, 1), new(1, 1), new(1, 2), new(0, 2)], // -------- | -------- | -------- | --------
                    [new(2, 3), new(2, 2), new(1, 2), new(1, 1)], // --[][]-- | --[]---- | -------- | []------
                    [new(0, 3), new(1, 3), new(1, 2), new(2, 2)], // []()---- | --()[]-- | --()[]-- | []()----
                    [new(0, 1), new(0, 2), new(1, 2), new(1, 3)], // -------- | ----[]-- | [][]---- | --[]----
                ],
                E_Type.Z => [
                    [new(0, 1), new(1, 1), new(1, 2), new(2, 2)], // -------- | -------- | -------- | --------
                    [new(2, 1), new(2, 2), new(1, 2), new(1, 3)], // [][]---- | ----[]-- | -------- | --[]----
                    [new(2, 3), new(1, 3), new(1, 2), new(0, 2)], // --()[]-- | --()[]-- | []()---- | []()----
                    [new(0, 3), new(0, 2), new(1, 2), new(1, 1)], // -------- | --[]---- | --[][]-- | []------
                ],
                _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };
        }
    }
}
