// Plan (pseudocode):
// 1. Provide a public static DFT class with a Transform<T> method that operates in-place on an array.
// 2. Transform<T>(T[] items, Func<T,double> valueSelector):
//    - Validate inputs.
//    - Build an array of Complex numbers from valueSelector(items[i]) (imag = 0).
//    - Compute the forward DFT (O(N^2) naive implementation).
//    - Compute the inverse DFT of the spectrum to obtain a "transformed" real sequence.
//    - Pair original items with the real part of the inverse result.
//    - Sort the items in-place according to the transformed real values.
// 3. Provide a helper DftCompute(Complex[] input, bool forward) that runs the naive DFT/IDFT.
// 4. Keep implementation simple and self-contained (no external FFT dependency).
// 5. Ensure correctness for small-to-moderate array sizes (e.g., 100x100 = 10k elements).

using System;
using System.Numerics;
using System.Drawing;

namespace ConsoleApp1
{
    public static class DFT
    {
        // In-place transform: compute DFT -> inverse DFT -> reorder items by inverse real part
        public static void Transform<T>(T[] items, Func<T, double> valueSelector)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));
            if (valueSelector is null) throw new ArgumentNullException(nameof(valueSelector));
            int n = items.Length;
            if (n <= 1) return;

            // Build complex input from selected scalar values
            var input = new Complex[n];
            for (int i = 0; i < n; i++)
            {
                input[i] = new Complex(valueSelector(items[i]), 0.0);
            }

            // Compute forward DFT (spectrum)
            var spectrum = DftCompute(input, forward: true);

            // Optionally, one could modify 'spectrum' here (filtering, etc.)

            // Compute inverse DFT to get transformed sequence
            var inverse = DftCompute(spectrum, forward: false);

            // Pair original items with transformed real values and sort by the real part
            var pairs = new (T Item, double Value)[n];
            for (int i = 0; i < n; i++)
            {
                pairs[i] = (items[i], inverse[i].Real);
            }

            Array.Sort(pairs, (a, b) => a.Value.CompareTo(b.Value));

            // Write sorted items back into the original array (in-place)
            for (int i = 0; i < n; i++)
            {
                items[i] = pairs[i].Item;
            }
        }

        // Naive DFT / IDFT (O(N^2))
        // forward == true -> computes forward DFT: X[k] = sum_{n=0}^{N-1} x[n] * exp(-i*2*pi*k*n/N)
        // forward == false -> computes inverse DFT: x[n] = (1/N) * sum_{k=0}^{N-1} X[k] * exp(+i*2*pi*k*n/N)
        private static Complex[] DftCompute(Complex[] x, bool forward)
        {
            int N = x.Length;
            var result = new Complex[N];
            if (N == 0) return result;

            // Precompute 2*pi/N
            double twoPiOverN = 2.0 * Math.PI / N;

            // sign: forward uses -1 for exp(-i angle), inverse uses +1
            double sign = forward ? -1.0 : 1.0;

            for (int k = 0; k < N; k++)
            {
                Complex sum = Complex.Zero;
                for (int n = 0; n < N; n++)
                {
                    double angle = twoPiOverN * k * n;
                    double cos = Math.Cos(angle);
                    double sin = Math.Sin(angle);
                    // exp(i * sign * angle) = cos(angle) + i * sign * sin(angle)
                    var twiddle = new Complex(cos, sign * sin);
                    sum += x[n] * twiddle;
                }

                if (!forward)
                {
                    // inverse: divide by N
                    sum /= N;
                }

                result[k] = sum;
            }

            return result;
        }
    }
}