namespace GameMods.Subnautica.VRCheats;

internal static class MenuTexture
{
    private static readonly IReadOnlyDictionary<char, string[]> Glyphs =
        new Dictionary<char, string[]>
        {
            ['A'] = ["01110","10001","10001","11111","10001","10001","10001"],
            ['B'] = ["11110","10001","10001","11110","10001","10001","11110"],
            ['C'] = ["01111","10000","10000","10000","10000","10000","01111"],
            ['E'] = ["11111","10000","10000","11110","10000","10000","11111"],
            ['G'] = ["01111","10000","10000","10111","10001","10001","01111"],
            ['H'] = ["10001","10001","10001","11111","10001","10001","10001"],
            ['I'] = ["11111","00100","00100","00100","00100","00100","11111"],
            ['K'] = ["10001","10010","10100","11000","10100","10010","10001"],
            ['L'] = ["10000","10000","10000","10000","10000","10000","11111"],
            ['N'] = ["10001","11001","10101","10101","10011","10001","10001"],
            ['O'] = ["01110","10001","10001","10001","10001","10001","01110"],
            ['P'] = ["11110","10001","10001","11110","10000","10000","10000"],
            ['R'] = ["11110","10001","10001","11110","10100","10010","10001"],
            ['S'] = ["01111","10000","10000","01110","00001","00001","11110"],
            ['T'] = ["11111","00100","00100","00100","00100","00100","00100"],
            ['U'] = ["10001","10001","10001","10001","10001","10001","01110"],
            ['V'] = ["10001","10001","10001","10001","10001","01010","00100"],
            ['X'] = ["10001","10001","01010","00100","01010","10001","10001"],
            ['Y'] = ["10001","10001","01010","00100","00100","00100","00100"]
        };

    public static byte[] Create(int width, int height)
    {
        var canvas = new Canvas(width, height);

        canvas.Fill(8, 18, 28, 255);
        canvas.FillRect(25, 25, width - 50, height - 50, 12, 35, 52, 255);

        canvas.DrawTextCentered("SUBNAUTICA VR CHEATS", 65, 6, 225, 240, 255, 255);

        canvas.FillRect(55, 155, 570, 250, 11, 119, 138, 255);
        canvas.DrawTextCentered("OXYGEN", 245, 9, 255, 255, 255, 255);

        canvas.FillRect(660, 155, 185, 250, 122, 43, 55, 255);
        canvas.DrawTextCentered("CLOSE", 250, 6, 255, 255, 255, 255);

        canvas.DrawTextCentered("BOTH GRIPS  R STICK", 430, 3, 150, 185, 205, 255);

        return canvas.Pixels;
    }

    private sealed class Canvas
    {
        private readonly int _width;
        private readonly int _height;

        public byte[] Pixels { get; }

        public Canvas(int width, int height)
        {
            _width = width;
            _height = height;
            Pixels = new byte[width * height * 4];
        }

        public void Fill(byte r, byte g, byte b, byte a) =>
            FillRect(0, 0, _width, _height, r, g, b, a);

        public void FillRect(int x, int y, int width, int height, byte r, byte g, byte b, byte a)
        {
            var x0 = Math.Max(0, x);
            var y0 = Math.Max(0, y);
            var x1 = Math.Min(_width, x + width);
            var y1 = Math.Min(_height, y + height);

            for (var py = y0; py < y1; py++)
            {
                for (var px = x0; px < x1; px++)
                    SetPixel(px, py, r, g, b, a);
            }
        }

        public void DrawTextCentered(string text, int y, int scale, byte r, byte g, byte b, byte a)
        {
            var width = MeasureText(text, scale);
            DrawText(text, Math.Max(0, (_width - width) / 2), y, scale, r, g, b, a);
        }

        private void DrawText(string text, int x, int y, int scale, byte r, byte g, byte b, byte a)
        {
            var cursor = x;

            foreach (var raw in text)
            {
                var ch = char.ToUpperInvariant(raw);

                if (ch == ' ')
                {
                    cursor += 4 * scale;
                    continue;
                }

                if (!Glyphs.TryGetValue(ch, out var rows))
                {
                    cursor += 6 * scale;
                    continue;
                }

                for (var row = 0; row < rows.Length; row++)
                {
                    for (var col = 0; col < rows[row].Length; col++)
                    {
                        if (rows[row][col] != '1')
                            continue;

                        FillRect(
                            cursor + col * scale,
                            y + row * scale,
                            scale,
                            scale,
                            r,
                            g,
                            b,
                            a);
                    }
                }

                cursor += 6 * scale;
            }
        }

        private static int MeasureText(string text, int scale)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            var units = 0;

            foreach (var ch in text)
                units += ch == ' ' ? 4 : 6;

            return Math.Max(0, (units - 1) * scale);
        }

        private void SetPixel(int x, int y, byte r, byte g, byte b, byte a)
        {
            if ((uint)x >= _width || (uint)y >= _height)
                return;

            var index = (y * _width + x) * 4;
            Pixels[index] = r;
            Pixels[index + 1] = g;
            Pixels[index + 2] = b;
            Pixels[index + 3] = a;
        }
    }
}
