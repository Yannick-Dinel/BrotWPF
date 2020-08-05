using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace HRDCCalendar.Extras.Mandelbrot
{
    public enum MandelFunction
    {
        Mandelbrot,
        PerpendicularMandelbrot,
        BurningShip,
        PerpendicularBurningShip,
        Tricorn,
        Heart,
        CelticHeart,
        CelticMandelbrot,
        CelticMandelbar,
        PerpendicularCelticMandelbrot,
        Buffalo,
        PerpendicularBuffalo,
        Simonbrot,
        InvertedSimonbrot
    }

    public class Mandelbrot
    {
        private Dictionary<MandelFunction, Func<Complex, Complex, Complex, Complex>> _iterationFunctions =
            new Dictionary<MandelFunction, Func<Complex, Complex, Complex, Complex>>
            {
                { MandelFunction.Mandelbrot,
                    new Func<Complex, Complex, Complex, Complex>((z, c, p) =>
                        ComplexPower(z, p) + c
                    )
                },
                { MandelFunction.PerpendicularMandelbrot,
                    new Func<Complex, Complex, Complex, Complex>((z, c, p) =>
                        ComplexPower(Complex.Conjugate(ComplexAbsR(z)), p) + c
                    )
                },
                { MandelFunction.BurningShip,
                    new Func<Complex, Complex, Complex, Complex>((z, c, p) =>
                        ComplexPower(ComplexAbs(z), p) + c
                    )
                },
                { MandelFunction.PerpendicularBurningShip,
                    new Func<Complex, Complex, Complex, Complex>((z, c, p) =>
                        ComplexPower(ComplexAbsI(z), p) + c
                    )
                },
                { MandelFunction.Tricorn,
                    new Func<Complex, Complex, Complex, Complex>((z, c, p) =>
                        Complex.Conjugate(ComplexPower(z, p)) + c
                    )
                },
                { MandelFunction.Heart,
                    new Func<Complex, Complex, Complex, Complex>((z, c, p) =>
                        ComplexPower(ComplexAbsR(z), p) + c
                    )
                },
                { MandelFunction.CelticHeart,
                    new Func<Complex, Complex, Complex, Complex>((z, c, p) =>
                        ComplexAbsR(ComplexPower(ComplexAbsR(z), p)) + c
                    )
                },
                { MandelFunction.CelticMandelbrot,
                    new Func<Complex, Complex, Complex, Complex>((z, c, p) =>
                        ComplexAbsR(ComplexPower(z, p)) + c
                    )
                },
                { MandelFunction.CelticMandelbar,
                    new Func<Complex, Complex, Complex, Complex>((z, c, p) =>
                        Complex.Conjugate(ComplexAbsR(ComplexPower(z, p))) + c
                    )
                },
                { MandelFunction.PerpendicularCelticMandelbrot,
                    new Func<Complex, Complex, Complex, Complex>((z, c, p) =>
                        Complex.Conjugate(ComplexAbsR(ComplexPower(ComplexAbsR(z), p))) + c
                    )
                },
                { MandelFunction.Buffalo,
                    new Func<Complex, Complex, Complex, Complex>((z, c, p) =>
                        ComplexAbs(ComplexPower(z, p)) + c
                    )
                },
                { MandelFunction.PerpendicularBuffalo,
                    new Func<Complex, Complex, Complex, Complex>((z, c, p) =>
                        ComplexAbsR(ComplexPower(ComplexAbsI(z), p)) + c
                    )
                },
                { MandelFunction.Simonbrot,
                    new Func<Complex, Complex, Complex, Complex>((z, c, p) =>
                        ComplexPower(z, p) * ComplexPower(ComplexAbs(z), p) + c
                    )
                },
                { MandelFunction.InvertedSimonbrot,
                    new Func<Complex, Complex, Complex, Complex>((z, c, p) =>
                        ComplexPower(z, p) * ComplexAbs(ComplexPower(z, p)) + c
                    )
                }
            };

        public struct MandelbrotCellInfo
        {
            public Complex Z;
            public Complex C;
            public int Iterations;
            public bool IsEscaped;
        }

        public MandelFunction MandelFunctionType { get; private set; }

        public int SizeX { get; private set; }
        public int SizeY { get; private set; }

        public double EscapeRadius { get; private set; }
        public double EscapeRadius2 { get; private set; }

        public Complex Center { get; private set; }
        public double Range { get; private set; }
        public Complex MinValue { get; private set; }
        public Complex MaxValue { get; private set; }
        public Complex Power { get; private set; }
        public Complex ZOffset { get; private set; }

        private MandelbrotCellInfo[,] _cells;

        public int TotalIterations { get; private set; }

        private Func<Complex, Complex, Complex, Complex> _iteractionFunction;

        public Tuple<int, int> MinMaxIterations
        {
            get
            {
                int min = int.MaxValue;
                int max = int.MinValue;
                for (int x = 0; x < SizeX; x++)
                    for (int y = 0; y < SizeY; y++)
                    {
                        var iter = _cells[x, y].Iterations;
                        if (iter < min) min = iter;
                        if (iter > max) max = iter;
                    }
                return Tuple.Create(min, max);
            }
        }


        public MandelbrotCellInfo this[int x, int y] => _cells[x, y];

        public Mandelbrot(
            MandelFunction mandelFunctionType = MandelFunction.Mandelbrot,
            int sizeX = 1000,
            int sizeY = 1000,
            double powerReal = 2,
            double powerImag = 0,
            double escapeRadius = 2,
            double centerReal = 0,
            double centerImag = 0,
            double range = 2,
            double zRealOffset = 0,
            double zImagOffset = 0)
            : this(mandelFunctionType, sizeX, sizeY,
                  new Complex(powerReal, powerImag), escapeRadius,
                  new Complex(centerReal, centerImag), range,
                  new Complex(zRealOffset, zImagOffset))
        {
        }

        private Mandelbrot(
            MandelFunction mandelFunctionType,
            int sizeX,
            int sizeY,
            Complex power,
            double escapeRadius,
            Complex center,
            double range,
            Complex zOffset)
        {
            ReinitializeMandelbrot(
                mandelFunctionType,
                sizeX, sizeY, power,
                escapeRadius, center, range,
                zOffset);
        }

        public void ReinitializeMandelbrot(
            MandelFunction mandelFunctionType = MandelFunction.Mandelbrot,
            int sizeX = 1000,
            int sizeY = 1000,
            double powerReal = 2,
            double powerImag = 0,
            double escapeRadius = 2,
            double centerReal = 0,
            double centerImag = 0,
            double range = 2,
            double zRealOffset = 0,
            double zImagOffset = 0)
        {
            ReinitializeMandelbrot(mandelFunctionType, sizeX, sizeY,
                new Complex(powerReal, powerImag), escapeRadius,
                new Complex(centerReal, centerImag), range,
                new Complex(zRealOffset, zImagOffset));
        }

        private void ReinitializeMandelbrot(
            MandelFunction mandelFunctionType,
            int sizeX,
            int sizeY,
            Complex power,
            double escapeRadius,
            Complex center,
            double range,
            Complex zOffset)
        {
            TotalIterations = 0;

            bool rebuildArray = _cells == null || SizeX != sizeX || SizeY != sizeY;
            SizeX = sizeX;
            SizeY = sizeY;
            EscapeRadius = escapeRadius;
            EscapeRadius2 = escapeRadius * escapeRadius;

            if (rebuildArray)
                _cells = new MandelbrotCellInfo[SizeX, SizeY];

            Center = center;
            Range = range;
            Power = power;
            ZOffset = zOffset;

            double minReal, minImag, rRange, iRange, inc, cr, ci;
            if (SizeX > SizeY)
            {
                rRange = Range;
                iRange = Range * SizeY / SizeX;
                minReal = Center.Real - rRange;
                minImag = Center.Imaginary - iRange;
                inc = 2 * Range / SizeX;
            }
            else
            {
                rRange = Range * SizeX / SizeY;
                iRange = Range;
                minReal = Center.Real - rRange;
                minImag = Center.Imaginary - iRange;
                inc = 2 * Range / SizeY;
            }

            MinValue = new Complex(minReal, minImag);
            MaxValue = new Complex(minReal + 2 * rRange - inc, minImag + 2 * iRange - inc);

            for (int x = 0; x < SizeX; x++)
            {
                cr = minReal + x * inc;
                for (int y = 0; y < SizeY; y++)
                {
                    ci = minImag + y * inc;
                    _cells[x, y] = new MandelbrotCellInfo
                    {
                        Z = Complex.Zero,
                        C = new Complex(cr, ci),
                        Iterations = 0,
                        IsEscaped = false
                    };
                }
            }

            MandelFunctionType = mandelFunctionType;
            if (!_iterationFunctions.TryGetValue(mandelFunctionType, out _iteractionFunction))
                _iteractionFunction = _iterationFunctions[MandelFunction.Mandelbrot];
        }

        protected bool CheckIsEscaped(Complex c) => CheckIsEscaped(c.Real, c.Imaginary);
        protected bool CheckIsEscaped(double r, double i) => r * r + i * i > EscapeRadius2;

        public Complex IterateValue(Complex z, Complex c, int numIters)
        {
            int iter = 0;
            while (iter < numIters)
            {
                z = _iteractionFunction(z + ZOffset, c, Power);
                iter++;
            }
            return z;
        }

        public void Iterate(int numIters, IProgress<double> progress = null)
        {
            if (numIters < 1) return;

            int numElem = _cells.GetLength(0);
            int interval = numElem / 100;
            int done = 0;
            for (int x = 0; x < SizeX; x++)
            {
                // Parallelize every vertical line
                Parallel.For(0, SizeY, y =>
                {
                    var cell = _cells[x, y];
                    if (!cell.IsEscaped)
                    {
                        int iter = 0;
                        var z = cell.Z;
                        var c = cell.C;
                        bool escaped = false;
                        while (iter < numIters)
                        {
                            z = _iteractionFunction(z + ZOffset, c, Power);
                            iter++;
                            if (CheckIsEscaped(z))
                            {
                                escaped = true;
                                break;
                            }
                        }
                        _cells[x, y] = new MandelbrotCellInfo
                        {
                            Z = z,
                            C = c,
                            Iterations = cell.Iterations + iter,
                            IsEscaped = escaped
                        };
                    }
                });
                if (progress != null)
                    if (++done % interval == 0)
                        progress.Report(done / interval);
            }

            TotalIterations += numIters;
        }

        // Faster power calculation if power is real only and integer.
        protected static Complex ComplexPower(Complex val, Complex pow)
        {
            if (pow == Complex.Zero) return Complex.One;

            int intPow = (int)pow.Real;
            if (pow.Imaginary != 0 || pow.Real - intPow != 0) return Complex.Pow(val, pow);

            bool inverse = false;
            if (intPow < 0) { intPow = -intPow; inverse = true; };

            --intPow;
            var result = val;
            while (intPow > 0)
            {
                if ((intPow & 1) == 0)
                {
                    val *= val;
                    intPow >>= 1;
                }
                else
                {
                    result *= val;
                    --intPow;
                }
            }

            return inverse ? Complex.Reciprocal(result) : result;
        }

        protected static Complex ComplexAbs(Complex c) => new Complex(Math.Abs(c.Real), Math.Abs(c.Imaginary));
        protected static Complex ComplexAbsR(Complex c) => new Complex(Math.Abs(c.Real), c.Imaginary);
        protected static Complex ComplexAbsI(Complex c) => new Complex(c.Real, Math.Abs(c.Imaginary));
        protected static Complex ComplexSwap(Complex c) => new Complex(c.Imaginary, c.Real);
    }
}
