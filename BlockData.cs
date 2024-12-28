using System.Diagnostics;

namespace Tetris
{
    partial class Block
    {
        private struct BlockData(E_BlockType type, E_FacingDirection facing)
        {
            public readonly E_BlockType BlockType => type;
            public readonly E_FacingDirection FacingDirection => facing;
            public readonly Position[] Placeholders => AllPlaceholders[(int)facing];

            public void RightRotate()
            {
                facing = (E_FacingDirection)(((int)facing + 1) % (int)E_FacingDirection.Count);
            }

            public void LeftRotate()
            {
                facing = (E_FacingDirection)(((int)facing + (int)E_FacingDirection.Count - 1) % (int)E_FacingDirection.Count);
            }

            public readonly Position RotateCenter => type switch
            {
                E_BlockType.I or E_BlockType.O => new(2, 2),
                E_BlockType.T or E_BlockType.J or E_BlockType.L or E_BlockType.S or E_BlockType.Z => new(1.5f, 2.5f),
                _ => throw new ArgumentOutOfRangeException(),
            };

            public readonly Position[][] AllKickWallTable => type switch
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
