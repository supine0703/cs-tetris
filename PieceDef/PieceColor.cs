namespace Tetris
{
    internal partial class Piece
    {
        public static Color[] GetColors(E_Type type) => type switch
        {
            E_Type.I => [Color.FromArgb(121, 184, 169), Color.FromArgb(184, 231, 223)], // 青色
            E_Type.O => [Color.FromArgb(212, 177, 72),  Color.FromArgb(240, 230, 140)], // 黄色
            E_Type.T => [Color.FromArgb(172, 69, 165),  Color.FromArgb(200, 150, 180)], // 紫红色
            E_Type.J => [Color.FromArgb(81, 120, 220),  Color.FromArgb(100, 150, 220)], // 蓝色
            E_Type.L => [Color.FromArgb(220, 130, 70),  Color.FromArgb(220, 130, 70)],  // 橙红色
            E_Type.S => [Color.FromArgb(52, 139, 43),   Color.FromArgb(130, 170, 110)], // 绿色
            E_Type.Z => [Color.FromArgb(201, 70, 90),   Color.FromArgb(230, 100, 100)], // 红色
            _ => [],
        };

        public Color[] Colors => GetColors(Type);
    }
}
