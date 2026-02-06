using System;
using System.Drawing;

namespace ConsoleApp1
{
    public static class Fill
    {
        public static Color[,] FillCircle(Color[] flatColors, int size)
        {
            Color[,] sorted2DColors = new Color[size, size];

            // Fill the 2D array in a spiral (swirl) order from the sorted flatColors
            int top = 0;
            int bottom = size - 1;
            int left = 0;
            int right = size - 1;
            int idx = 0;

            while (top <= bottom && left <= right)
            {
                // left -> right on top row
                for (int col = left; col <= right && idx < flatColors.Length; col++)
                {
                    sorted2DColors[top, col] = flatColors[idx++];
                }
                top++;

                // top -> bottom on right column
                for (int row = top; row <= bottom && idx < flatColors.Length; row++)
                {
                    sorted2DColors[row, right] = flatColors[idx++];
                }
                right--;

                // right -> left on bottom row
                if (top <= bottom)
                {
                    for (int col = right; col >= left && idx < flatColors.Length; col--)
                    {
                        sorted2DColors[bottom, col] = flatColors[idx++];
                    }
                    bottom--;
                }

                // bottom -> top on left column
                if (left <= right)
                {
                    for (int row = bottom; row >= top && idx < flatColors.Length; row--)
                    {
                        sorted2DColors[row, left] = flatColors[idx++];
                    }
                    left++;
                }
            }

            return sorted2DColors;
        }

        /*
        Pseudocode / Plan for `FillDiagonal`:
        - Create a 2D Color array `sorted2DColors` with dimensions [size, size].
        - Initialize `idx = 0`.
        - For `sum = 0` to `2*(size - 1)` (each anti-diagonal index):
            - For `row = 0` to `size - 1`:
                - Compute `col = sum - row`.
                - If `col` is out of range (col < 0 or col >= size), continue to next row.
                - If `idx >= color.Length`, stop filling (use a flag to break out).
                - Assign `sorted2DColors[row, col] = color[idx++]`.
        - Return `sorted2DColors`.
        - This visits diagonals from the top-left toward the bottom-right.
          Within each diagonal it visits from the topmost element downward:
          (0,sum), (1,sum-1), (2,sum-2), ...
        */

        public static Color[,] FillDiagonal(Color[] color, int size)
        {
            Color[,] sorted2DColors = new Color[size, size];
            int idx = 0;
            bool done = false;

            for (int sum = 0; sum <= 2 * (size - 1) && !done; sum++)
            {
                for (int row = 0; row < size; row++)
                {
                    int col = sum - row;
                    if (col < 0 || col >= size) continue;

                    if (idx >= color.Length)
                    {
                        done = true;
                        break;
                    }

                    sorted2DColors[row, col] = color[idx++];
                }
            }

            return sorted2DColors;
        }

        public static Color[,] FillHorizontal(Color[] color, int size)
        {
            Color[,] sorted2DColors = new Color[size, size];
            int idx = 0;
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (idx >= color.Length) break;
                    sorted2DColors[row, col] = color[idx++];
                }
            }
            return sorted2DColors;
        }

        /*
        Detailed Pseudocode & Plan for improved `FillVoronoi`:
        Goal: make Voronoi cells larger / change seeds less often and produce more interesting organic patterns.
        Approach:
        1. Use fewer seeds than colors (so regions are larger). Compute `seedCount = clamp(colors.Length / 3, min 4, max colors.Length)`.
        2. Place seeds on a jittered grid instead of uniformly random. This reduces tiny islands and creates larger, evenly distributed regions.
           - Compute gridCols = ceil(sqrt(seedCount)), gridRows = ceil(seedCount / gridCols).
           - For each seed slot, compute a grid cell center and add random jitter up to a fraction of the cell size.
        3. Give each seed a random "weight" (influence scale). When computing which seed owns a pixel, divide distance by weight so some seeds make bigger blobs.
        4. Map seed -> color by cycling through provided colors (if seeds < colors.Length).
        5. For each pixel compute the closest seed using weighted distance (squared distances for speed). Assign that seed's color.
        This yields fewer, larger Voronoi regions with varied shapes and reduced rapid color switching.
        */

        public static Color[,] FillVoronoi(Color[] colors, int size)
        {
            Color[,] sorted2DColors = new Color[size, size];
            int n = Math.Max(1, colors.Length);
            // Use fewer seeds than colors for larger regions. At least 4 seeds.
            int seedCount = Math.Min(n, Math.Max(4, n / 20));

            Random rand = new();
            // Arrange seeds on a jittered grid for more even, larger regions
            int gridCols = (int)Math.Ceiling(Math.Sqrt(seedCount));
            int gridRows = (int)Math.Ceiling(seedCount / (double)gridCols);
            double cellW = (double)size / gridCols;
            double cellH = (double)size / gridRows;

            var seeds = new (double Row, double Col, double Weight, Color Color)[seedCount];
            for (int i = 0; i < seedCount; i++)
            {
                int gx = i % gridCols;
                int gy = i / gridCols;
                // base center of the grid cell
                double baseX = gx * cellW + cellW * 0.5;
                double baseY = gy * cellH + cellH * 0.5;
                // jitter up to ~60% of half cell to create organic offsets but keep seeds well-separated
                double jitterX = (rand.NextDouble() - 0.5) * cellW * 0.6;
                double jitterY = (rand.NextDouble() - 0.5) * cellH * 0.6;
                double seedCol = Math.Clamp(baseX + jitterX, 0, size - 1);
                double seedRow = Math.Clamp(baseY + jitterY, 0, size - 1);
                // weight controls influence size; vary between 0.6 and 1.4
                double weight = 0.6 + rand.NextDouble() * 0.8;
                // Map seed to a color (cycle if there are more colors than seeds)
                Color seedColor = colors[i % n];
                seeds[i] = (seedRow, seedCol, weight, seedColor);
            }

            // Assign each pixel to the nearest weighted seed
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    double bestVal = double.MaxValue;
                    Color bestColor = Color.Black;
                    for (int s = 0; s < seedCount; s++)
                    {
                        double dr = row - seeds[s].Row;
                        double dc = col - seeds[s].Col;
                        // use squared distance and divide by weight^2 to scale influence
                        double val = (dr * dr + dc * dc) / (seeds[s].Weight * seeds[s].Weight);
                        if (val < bestVal)
                        {
                            bestVal = val;
                            bestColor = seeds[s].Color;
                        }
                    }

                    sorted2DColors[row, col] = bestColor;
                }
            }

            return sorted2DColors;
        }
    }
}
