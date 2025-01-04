namespace Tetris
{
    internal partial class GameControl
    {
        [Flags]
        private enum E_PieceBehavior : uint
        {
            None = 0,

            // Piece behaviors
            I_Behavior = 0x01,
            O_Behavior = 0x02,
            T_Behavior = 0x04,
            J_Behavior = 0x08,
            L_Behavior = 0x10,
            S_Behavior = 0x20,
            Z_Behavior = 0x40,

            // Base behaviors
            Rotate = 0x0100,
            Move = 0x0200,
            Drop = 0x0400,
            Lock = 0x0800,
            Hold = 0x1000,

            Left = 0x2000,
            Right = 0x4000,

            // Merge behaviors ...

            // Kick behaviors
            Kick = 0x010000 | Rotate,
            WallKick = 0x020000 | Kick,
            FloorKick = 0x040000 | Kick,

            // ClearLine behaviors
            ClearL = 0x080000 | Lock,
            Single = 0x100000 | ClearL,
            Double = 0x200000 | ClearL,
            Triple = 0x400000 | ClearL,
            Tetris = 0x800000 | ClearL,

            // Drop behaviors
            AutoDrop = 0x01000000 | Drop,
            SoftDrop = 0x02000000 | Drop,
            FirmDrop = 0x04000000 | Drop,
            HardDrop = 0x08000000 | FirmDrop | Lock,

            // Combined behaviors

            // Spin behaviors
            Spin = Kick | Lock,
            Spin_Single = Spin | Single,
            Spin_Double = Spin | Double,
            Spin_Triple = Spin | Triple,
            Spin_Tetris = Spin | Tetris,
        }

    }
}
