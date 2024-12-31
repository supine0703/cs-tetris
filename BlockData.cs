namespace Tetris
{
    internal partial class Block
    {
        private struct BlockData(E_BlockType type, E_Orientation orientation)
        {
            public readonly E_BlockType BlockType => type;
            public readonly E_Orientation Orientation => orientation;
            public readonly Position[] Placeholders => AllPlaceholders[(int)orientation];

            public void RightRotate()
            {
                orientation = (E_Orientation)(((int)orientation + 1) % (int)E_Orientation.Count);
            }

            public void LeftRotate()
            {
                orientation = (E_Orientation)(((int)orientation + (int)E_Orientation.Count - 1) % (int)E_Orientation.Count);
            }

            public readonly Position RotateCenter => type switch
            {
                E_BlockType.I or E_BlockType.O => new(2, 2),
                E_BlockType.T or E_BlockType.J or E_BlockType.L or E_BlockType.S or E_BlockType.Z => new(1.5f, 2.5f),
                _ => throw new ArgumentOutOfRangeException(),
            };

            public readonly Position[][] AllWallKickTable => type switch
            {
                // UR, RR, DR, LR, UL, RL, DL, LL
                E_BlockType.O => [
                    /* UR */ [new(0, 0)],
                    /* RR */ [new(0, 0)],
                    /* DR */ [new(0, 0)],
                    /* LR */ [new(0, 0)],
                    /* UL */ [new(0, 0)],
                    /* RL */ [new(0, 0)],
                    /* DL */ [new(0, 0)],
                    /* LL */ [new(0, 0)],
                ],
                E_BlockType.I => [
                    /* UR */ [new(0, 0), new(-2, 0), new(+1, 0), new(-2, -1), new(+1, +2)],
                    /* RR */ [new(0, 0), new(-1, 0), new(+2, 0), new(-1, +2), new(+2, -1)],
                    /* DR */ [new(0, 0), new(+2, 0), new(-1, 0), new(+2, +1), new(-1, -2)],
                    /* LR */ [new(0, 0), new(+1, 0), new(-2, 0), new(+1, -2), new(-2, +1)],
                    /* UL */ [new(0, 0), new(-1, 0), new(+2, 0), new(-1, +2), new(+2, -1)],
                    /* RL */ [new(0, 0), new(+2, 0), new(-1, 0), new(+2, +1), new(-1, -2)],
                    /* DL */ [new(0, 0), new(+1, 0), new(-2, 0), new(+1, -2), new(-2, +1)],
                    /* LL */ [new(0, 0), new(-2, 0), new(+1, 0), new(-2, -1), new(+1, +2)],
                ],
                E_BlockType.T or E_BlockType.J or E_BlockType.L or E_BlockType.S or E_BlockType.Z =>
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

            private readonly Position[][] AllPlaceholders => type switch
            {
                E_BlockType.I => [
                    [new(0, 1), new(1, 1), new(2, 1), new(3, 1)],
                    [new(2, 0), new(2, 1), new(2, 2), new(2, 3)],
                    [new(0, 2), new(1, 2), new(2, 2), new(3, 2)],
                    [new(1, 0), new(1, 1), new(1, 2), new(1, 3)],
                ],
                E_BlockType.O => [
                    [new(1, 1), new(2, 1), new(1, 2), new(2, 2)],
                    [new(1, 1), new(2, 1), new(1, 2), new(2, 2)],
                    [new(1, 1), new(2, 1), new(1, 2), new(2, 2)],
                    [new(1, 1), new(2, 1), new(1, 2), new(2, 2)],
                ],
                E_BlockType.T => [
                    [new(1, 1), new(0, 2), new(1, 2), new(2, 2)],
                    [new(2, 2), new(1, 1), new(1, 2), new(1, 3)],
                    [new(1, 3), new(2, 2), new(1, 2), new(0, 2)],
                    [new(0, 2), new(1, 3), new(1, 2), new(1, 1)],
                ],
                E_BlockType.J => [
                    [new(0, 1), new(0, 2), new(1, 2), new(2, 2)],
                    [new(2, 1), new(1, 1), new(1, 2), new(1, 3)],
                    [new(2, 3), new(2, 2), new(1, 2), new(0, 2)],
                    [new(0, 3), new(1, 3), new(1, 2), new(1, 1)],
                ],
                E_BlockType.L => [
                    [new(2, 1), new(0, 2), new(1, 2), new(2, 2)],
                    [new(2, 3), new(1, 1), new(1, 2), new(1, 3)],
                    [new(0, 3), new(2, 2), new(1, 2), new(0, 2)],
                    [new(0, 1), new(1, 3), new(1, 2), new(1, 1)],
                ],
                E_BlockType.S => [
                    [new(2, 1), new(1, 1), new(1, 2), new(0, 2)],
                    [new(2, 3), new(2, 2), new(1, 2), new(1, 1)],
                    [new(0, 3), new(1, 3), new(1, 2), new(2, 2)],
                    [new(0, 1), new(0, 2), new(1, 2), new(1, 3)],
                ],
                E_BlockType.Z => [
                    [new(0, 1), new(1, 1), new(1, 2), new(2, 2)],
                    [new(2, 1), new(2, 2), new(1, 2), new(1, 3)],
                    [new(2, 3), new(1, 3), new(1, 2), new(0, 2)],
                    [new(0, 3), new(0, 2), new(1, 2), new(1, 1)],
                ],
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }
}
