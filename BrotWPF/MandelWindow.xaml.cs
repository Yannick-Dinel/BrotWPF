using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Numerics;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Windows.Controls.Primitives;

namespace HRDCCalendar.Extras.Mandelbrot
{
    /// <summary>
    /// Interaction logic for MandelWindow.xaml
    /// </summary>
    public partial class MandelWindow : Window, INotifyPropertyChanged
    {
        public class DescriptionTipHolder : INotifyPropertyChanged
        {
            private string _desc;
            private string _tip;

            public string Description { get { return _desc; } set { if (value != _desc) { _desc = value; NotifyPropertyChanged(nameof(Description)); } } }
            public string Tip { get { return _tip; } set { if (value != _tip) { _tip = value; NotifyPropertyChanged(nameof(Tip)); } } }

            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public enum MandelColorPalette
        {
            BlueWhiteOrange,
            BlueGreenRed,
            GreenWhiteMagenta,
            RedYellowGreen,
            IndigoOrangeWhite,
            GrayScale,
            RedScale,
            BlueScale,
            GreenScale,
            YellowScale,
            CyanScale,
            MagentaScale,
            PinkScale,
            WhitePinkScale,
            JojoScale
        }

        public enum MandelColorFunction
        {
            Linear,
            SquareRoot,
            Sine,
            AutoLogarithmic,
            Logarithmic,
            Gamma,
            HyperbolicTangent
        }

        private enum MandelPopupEnum
        {
            Display,
            Function,
            Position,
            SaveLoad
        }

        public struct LastMandelFunctionSettings
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public MandelFunction Function;
            public int InitialIterations;
            public double PowerReal;
            public double PowerImag;
            public double EscapeRadius;
            public double ZRealOffset;
            public double ZImagOffset;
        }

        public struct LastMandelPositionSettings
        {
            public double CenterReal;
            public double CenterImag;
            public double Range;
        }

        public struct LastMandelDisplaySettings
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public MandelColorPalette ColorPalette;
            [JsonConverter(typeof(StringEnumConverter))]
            public MandelColorFunction ColorFunction;
            public double NValue;
            public double PaletteOffset;
            public bool UseSmoothColoring;
            public int SmoothingIterations;
            public bool UseSineColorInterpolation;
            public double InterpolationPeriod;
        }

        public struct MandelJsonStruct
        {
            public LastMandelFunctionSettings Function;
            public LastMandelPositionSettings Position;
            public LastMandelDisplaySettings Display;
            public int Iterations;
        }

        public Dictionary<MandelFunction, DescriptionTipHolder> MandelFunctions { get; set; }
        public MandelFunction SelectedMandelFunction { get; set; }

        public Dictionary<MandelColorPalette, DescriptionTipHolder> MandelColorPalettes { get; set; }
        public Dictionary<MandelColorFunction, DescriptionTipHolder> MandelColorFunctions { get; set; }
        public MandelColorPalette SelectedMandelColorPalette { get; set; }
        public MandelColorFunction SelectedMandelColorFunction { get; set; }

        private readonly static Color SET_COLOR = Colors.Black;

        private Mandelbrot _mandelbrot;
        private WriteableBitmap _wtb;
        private byte _paletteColors;
        private bool _isIterating = false;
        private int _sizeX, _sizeY;

        private LastMandelFunctionSettings _lastFunctionSettings;
        private LastMandelPositionSettings _lastPositionSettings;
        private LastMandelDisplaySettings _lastDisplaySettings;
        public double ZoomFactor { get; set; }

        public MandelWindow() : this(1000, 1000) { }

        public MandelWindow(int sizeX = 1000, int sizeY = 1000)
        {
            MandelFunctions = new Dictionary<MandelFunction, DescriptionTipHolder>
            {
                { MandelFunction.Mandelbrot,
                    new DescriptionTipHolder
                    {
                        Description = "Mandelbrot",
                        Tip = "z <= z^p + c"
                    }
                },
                { MandelFunction.PerpendicularMandelbrot,
                    new DescriptionTipHolder
                    {
                        Description = "Perpendicular Mandelbrot",
                        Tip = "z <= conj(rabs(z))^p + c"
                    }
                },
                { MandelFunction.BurningShip,
                    new DescriptionTipHolder
                    {
                        Description = "Burning Ship",
                        Tip = "z <= abs(z)^p + c"
                    }
                },
                { MandelFunction.PerpendicularBurningShip,
                    new DescriptionTipHolder
                    {
                        Description = "Perpendicular Burning Ship",
                        Tip = "z <= conj(iabs(z))^p + c"
                    }
                },
                { MandelFunction.Mandelbar,
                    new DescriptionTipHolder
                    {
                        Description = "Tricorn/Mandelbar",
                        Tip = "z = conj(z^p) + c"
                    }
                },
                { MandelFunction.Heart,
                    new DescriptionTipHolder
                    {
                        Description = "Heart Mandelbrot",
                        Tip = "z <= rabs(z)^p + c"
                    }
                },
                { MandelFunction.CelticHeart,
                    new DescriptionTipHolder
                    {
                        Description = "Celtic Heart Mandelbrot",
                        Tip = "z <= rabs(rabs(z)^p) + c"
                    }
                },
                { MandelFunction.CelticMandelbrot,
                    new DescriptionTipHolder
                    {
                        Description = "Celtic Mandelbrot",
                        Tip = "z <= rabs(z^p) + c"
                    }
                },
                { MandelFunction.CelticMandelbar,
                    new DescriptionTipHolder
                    {
                        Description = "Celtic Mandelbar",
                        Tip = "z <= conj(rabs(z^p)) + c"
                    }
                },
                { MandelFunction.PerpendicularCelticMandelbrot,
                    new DescriptionTipHolder
                    {
                        Description = "Perpendicular Celtic Mandelbrot",
                        Tip = "z <= conj(rabs(rabs(z)^p)) + c"
                    }
                },
                { MandelFunction.Buffalo,
                    new DescriptionTipHolder
                    {
                        Description = "Buffalo",
                        Tip = "z <= abs(z^p) + c"
                    }
                },
                { MandelFunction.PerpendicularBuffalo,
                    new DescriptionTipHolder
                    {
                        Description = "Perpendicular Buffalo",
                        Tip = "z <= rabs(iabs(z)^p) + c"
                    }
                },
                { MandelFunction.Simonbrot,
                    new DescriptionTipHolder
                    {
                        Description = "Simonbrot",
                        Tip = "z <= z^p * abs(z)^2 + c"
                    }
                },
                { MandelFunction.InvertedSimonbrot,
                    new DescriptionTipHolder
                    {
                        Description = "Inverted Simonbrot",
                        Tip = "z <= z^p * abs(z^2) + c"
                    }
                },
                { MandelFunction.HeartSimonbrot,
                    new DescriptionTipHolder
                    {
                        Description = "Heart Simonbrot",
                        Tip = "z <= z^p * rabs(z)^2 + c"
                    }
                },
                { MandelFunction.CelticSimonbrot,
                    new DescriptionTipHolder
                    {
                        Description = "Celtic Simonbrot",
                        Tip = "z <= z^p * rabs(z^2) + c"
                    }
                }
            };

            MandelColorPalettes = new Dictionary<MandelColorPalette, DescriptionTipHolder>
            {
                { MandelColorPalette.BlueWhiteOrange,
                    new DescriptionTipHolder
                    {
                        Description = "Blue - White - Orange",
                        Tip = "Linear interpolated gradient from blue to white then orange."
                    }
                },
                { MandelColorPalette.BlueGreenRed,
                    new DescriptionTipHolder
                    {
                        Description = "Blue - Green - Red",
                        Tip = "Linear interpolated gradient from blue to green then red."
                    }
                },
                { MandelColorPalette.GreenWhiteMagenta,
                    new DescriptionTipHolder
                    {
                        Description = "Green - White - Magenta",
                        Tip = "Linear interpolated gradient from green to white then magenta."
                    }
                },
                { MandelColorPalette.RedYellowGreen,
                    new DescriptionTipHolder
                    {
                        Description = "Red - Yellow - Green",
                        Tip = "Linear interpolated gradient from red to yellow then green."
                    }
                },
                { MandelColorPalette.IndigoOrangeWhite,
                    new DescriptionTipHolder
                    {
                        Description = "Indigo - Orange - White",
                        Tip = "Linear interpolated gradient from indigo to orange then white."
                    }
                },
                { MandelColorPalette.GrayScale,
                    new DescriptionTipHolder
                    {
                        Description = "Monochromatic Gray-Scale",
                        Tip = "Linear monochromatic scale from black to white."
                    }
                },
                { MandelColorPalette.RedScale,
                    new DescriptionTipHolder
                    {
                        Description = "Monochromatic Red-Scale",
                        Tip = "Linear monochromatic scale from black to red."
                    }
                },
                { MandelColorPalette.GreenScale,
                    new DescriptionTipHolder
                    {
                        Description = "Monochromatic Green-Scale",
                        Tip = "Linear monochromatic scale from black to green."
                    }
                },
                { MandelColorPalette.BlueScale,
                    new DescriptionTipHolder
                    {
                        Description = "Monochromatic Blue-Scale",
                        Tip = "Linear monochromatic scale from black to blue."
                    }
                },
                { MandelColorPalette.YellowScale,
                    new DescriptionTipHolder
                    {
                        Description = "Monochromatic Yellow-Scale",
                        Tip = "Linear monochromatic scale from black to yellow."
                    }
                },
                { MandelColorPalette.CyanScale,
                    new DescriptionTipHolder
                    {
                        Description = "Monochromatic Cyan-Scale",
                        Tip = "Linear monochromatic scale from black to cyan."
                    }
                },
                { MandelColorPalette.MagentaScale,
                    new DescriptionTipHolder
                    {
                        Description = "Monochromatic Magenta-Scale",
                        Tip = "Linear monochromatic scale from black to magenta."
                    }
                },
                { MandelColorPalette.PinkScale,
                    new DescriptionTipHolder
                    {
                        Description = "Monochromatic Pink-Scale",
                        Tip = "Linear monochromatic scale from black to pink."
                    }
                },
                { MandelColorPalette.WhitePinkScale,
                    new DescriptionTipHolder
                    {
                        Description = "Monochromatic Pink-Scale (White)",
                        Tip = "Linear monochromatic scale from white to deep pink with a white set color."
                    }
                },
                { MandelColorPalette.JojoScale,
                    new DescriptionTipHolder
                    {
                        Description = "Monochromatic Jojo-Scale",
                        Tip = "Linear monochromatic scale from deep pink to light pink with a white set color. Jojo's favorite!"
                    }
                }
            };

            MandelColorFunctions = new Dictionary<MandelColorFunction, DescriptionTipHolder>
            {
                { MandelColorFunction.Linear,
                    new DescriptionTipHolder
                    {
                        Description = "Linear",
                        Tip = "color = x"
                    }
                },
                { MandelColorFunction.SquareRoot,
                    new DescriptionTipHolder
                    {
                        Description = "Square Root",
                        Tip = "color = sqrt(x)"
                    }
                },
                { MandelColorFunction.Sine,
                    new DescriptionTipHolder
                    {
                        Description = "Sine",
                        Tip = "color = sin(x * pi / 2)"
                    }
                },
                { MandelColorFunction.AutoLogarithmic,
                    new DescriptionTipHolder
                    {
                        Description = "Auto Logarithmic",
                        Tip = "color = (log(iter) - log(min_iter)) / (log(max_iter) - log(min_iter))"
                    }
                },
                { MandelColorFunction.Logarithmic,
                    new DescriptionTipHolder
                    {
                        Description = "Logarithmic",
                        Tip = "color = log(x*(4^n)+1) / log((4^n)+1)"
                    }
                },
                { MandelColorFunction.Gamma,
                    new DescriptionTipHolder
                    {
                        Description = "Gamma",
                        Tip = "color = x^(1/n)"
                    }
                },
                { MandelColorFunction.HyperbolicTangent,
                    new DescriptionTipHolder
                    {
                        Description = "Hyperbolic Tangent",
                        Tip = "color = tanh(n*x) / tanh(n)"
                    }
                }
            };

            _lastDisplaySettings = new LastMandelDisplaySettings
            {
                ColorPalette = MandelColorPalette.BlueWhiteOrange,
                ColorFunction = MandelColorFunction.AutoLogarithmic,
                UseSmoothColoring = true,
                SmoothingIterations = 2,
                UseSineColorInterpolation = true,
                InterpolationPeriod = 1,
                PaletteOffset = 0,
                NValue = 1
            };
            ZoomFactor = 4;
            _paletteColors = 255;

            InitializeComponent();

            InitSetFunction(sizeX, sizeY);
        }

        private void SetIterating(bool i)
        {
            _isIterating = i;
            SetPositionPopup.Child.IsEnabled = !i;
            SetPositionPopupButton.IsEnabled = !i;
            SetDisplayPopup.Child.IsEnabled = !i;
            SetDisplayPopupButton.IsEnabled = !i;
            SetFunctionPopup.Child.IsEnabled = !i;
            SetFunctionPopupButton.IsEnabled = !i;
            IterByButton.IsEnabled = !i;
            SetIterButton.IsEnabled = !i;
            IterUpDown.IsEnabled = !i;
            SaveLoadButton.IsEnabled = !i;
            SaveLoadPopup.IsEnabled = !i;
        }

        public async Task IterateMandel(int numIters = 1)
        {
            if (numIters > 0)
            {
                MandelProgress.Value = 0;
                MandelProgress.Visibility = Visibility.Visible;
                MandelProgressTaskbar.ProgressValue = 0;
                MandelProgressTaskbar.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;

                IProgress<double> progress = new Progress<double>(percent =>
                {
                    MandelProgress.Value += percent * 100;
                    MandelProgressTaskbar.ProgressValue += percent;
                });

                await Task.Run(() =>
                {
                    _mandelbrot.Iterate(numIters, progress);
                });

                MandelProgress.Value = 0;
                MandelProgress.Visibility = Visibility.Collapsed;
                MandelProgressTaskbar.ProgressValue = 0;
                MandelProgressTaskbar.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                MandelIterCount.Text = _mandelbrot.TotalIterations.ToString();
            }
        }

        public void PrintMandel()
        {
            byte[] pixels = new byte[_sizeX * _sizeY];

            //var maxIt = _mandelbrot.TotalIterations;
            var minmax = _mandelbrot.MinMaxIterations;
            int minIt = minmax.Item1;
            int maxIt = minmax.Item2;
            if (maxIt > 0)
            {
                var smoothIters = _lastDisplaySettings.SmoothingIterations;

                bool doSmooth = _lastDisplaySettings.UseSmoothColoring
                    && _mandelbrot.Power.Imaginary == 0 && _mandelbrot.RealPowerGrowth > 1;
                if (doSmooth) maxIt += smoothIters + 1;
                bool noDiff = maxIt == minIt;
                var minLog = Math.Log(minIt);
                var totLog = Math.Log(maxIt) - minLog;
                var mprLog = Math.Log(_mandelbrot.RealPowerGrowth);
                var rad2Log = Math.Log(_mandelbrot.EscapeRadius2);

                Func<double, double, double> colorFunc;
                double linearize(double x) => (x - minIt) / (maxIt - minIt);
                switch (_lastDisplaySettings.ColorFunction)
                {
                    case MandelColorFunction.SquareRoot:
                        colorFunc = new Func<double, double, double>((i, n) =>
                        {
                            var x = linearize(i);
                            return x > 0 ? Math.Sqrt(x) : 0;
                        });
                        break;
                    case MandelColorFunction.Sine:
                        colorFunc = new Func<double, double, double>((i, n) =>
                            Math.Sin(linearize(i) * Math.PI / 2));
                        break;
                    case MandelColorFunction.AutoLogarithmic:
                        colorFunc = new Func<double, double, double>((i, n) =>
                        {
                            if (i <= 0) return 0;
                            return (Math.Log(i) - minLog) / totLog;
                        });
                        break;
                    case MandelColorFunction.Logarithmic:
                        colorFunc = new Func<double, double, double>((i, n) =>
                        {
                            var x = linearize(i);
                            var n4 = Math.Pow(4, n);
                            return Math.Log(x * n4 + 1) / Math.Log(n4 + 1);
                        });
                        break;
                    case MandelColorFunction.Gamma:
                        colorFunc = new Func<double, double, double>((i, n) =>
                        {
                            var x = linearize(i);
                            if (x <= 0 || n <= 0) return 0;
                            return Math.Pow(x, 1 / n);
                        });
                        break;
                    case MandelColorFunction.HyperbolicTangent:
                        colorFunc = new Func<double, double, double>((i, n) =>
                        {
                            if (n <= 0) return 0;
                            return Math.Tanh(linearize(i) * n) / Math.Tanh(n);
                        });
                        break;
                    default:
                        colorFunc = new Func<double, double, double>((i, n) => linearize(i));
                        break;
                }

                double interPeriod = _lastDisplaySettings.InterpolationPeriod;
                double pOffset = _lastDisplaySettings.PaletteOffset;
                double nVal = _lastDisplaySettings.NValue;
                Parallel.For(0, _sizeY, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, y =>
                {
                    var ypos = y * _sizeX;
                    for (int x = 0; x < _sizeX; x++)
                    {
                        var c = _mandelbrot[x, y];
                        if (c.IsEscaped)
                        {
                            byte bval = _paletteColors;
                            if (!noDiff)
                            {
                                double dval;
                                if (doSmooth)
                                {
                                    var newz = _mandelbrot.IterateValue(c.Z, c.C, smoothIters);
                                    var l = smoothIters + 1
                                        - Math.Log(
                                            Math.Log(newz.Real * newz.Real + newz.Imaginary * newz.Imaginary) / rad2Log) / mprLog;
                                    //var l = _smoothingIterations
                                    //    - Math.Log(Math.Log(newz.Magnitude)) / mprLog;
                                    dval = colorFunc(c.Iterations + l, nVal);
                                    if (dval < 0)
                                        dval = 0;
                                    else if (dval > 1)
                                        dval = 1;
                                }
                                else dval = colorFunc(c.Iterations, nVal);
                                if (interPeriod != 1 || pOffset != 0)
                                    dval = Math.Abs((dval * interPeriod + 1 - pOffset) % 2 - 1);

                                // Palette size - 1 to force range between 0 - 254 => 1 - 255...
                                bval = (byte)(dval * (_paletteColors - 1) + 1);
                            }
                            pixels[ypos + x] = bval;
                        }
                    }
                });
            }

            _wtb.WritePixels(new Int32Rect(0, 0, _sizeX, _sizeY), pixels, _sizeX, 0);

            // Prevent memory buildup
            pixels = null;
            GC.Collect();
        }

        private void IterByButton_Click(object sender, RoutedEventArgs e)
        {
            IterPrintMandel(IterUpDown.Value ?? 1);
        }

        private void SetIterButton_Click(object sender, RoutedEventArgs e)
        {
            var iterVal = IterUpDown.Value;
            if (iterVal.HasValue)
            {
                if (_mandelbrot.TotalIterations < iterVal)
                    IterPrintMandel(iterVal.Value - _mandelbrot.TotalIterations);
                else if (_mandelbrot.TotalIterations > iterVal)
                    InitFromCurrentBrot(_sizeX, _sizeY, iterVal);
            }
        }

        private async void IterPrintMandel(int numIter = 1)
        {
            SetIterating(true);
            await IterateMandel(numIter);
            PrintMandel();
            SetIterating(false);
        }

        private void InitFromCurrentBrot(int sizeX, int sizeY, int? newIters = null,
            double? newCenterReal = null, double? newCenterImag = null, double? newRange = null)
        {
            if (_sizeX != sizeX || _sizeY != sizeY
                || newIters.HasValue
                || newCenterReal.HasValue
                || newCenterImag.HasValue
                || newRange.HasValue)
                InitSetFunction(sizeX, sizeY,
                    _lastFunctionSettings.Function,
                    newIters ?? _mandelbrot.TotalIterations,
                    _mandelbrot.EscapeRadius,
                    _mandelbrot.Power.Real,
                    _mandelbrot.Power.Imaginary,
                    _mandelbrot.ZOffset.Real,
                    _mandelbrot.ZOffset.Imaginary,
                    newCenterReal ?? _mandelbrot.Center.Real,
                    newCenterImag ?? _mandelbrot.Center.Imaginary,
                    newRange ?? _mandelbrot.Range, false);
        }
        private void InitSetFunction(int sizeX, int sizeY,
            MandelFunction func = MandelFunction.Mandelbrot,
            int initIters = 10, double escapeRadius = 2,
            double powR = 2, double powI = 0,
            double zro = 0, double zio = 0,
            double centerR = 0, double centerI = 0,
            double range = 2,
            bool updateLastFunctionSettings = true,
            bool forceRebuildImage = false)
        {
            try
            {
                if (_mandelbrot == null)
                    _mandelbrot = new Mandelbrot(
                        sizeX, sizeY, func,
                        powR, powI, escapeRadius,
                        centerR, centerI, range,
                        zro, zio);
                else
                    _mandelbrot.ReinitializeMandelbrot(
                        sizeX, sizeY, func,
                        powR, powI, escapeRadius,
                        centerR, centerI, range,
                        zro, zio);

                _sizeX = sizeX;
                _sizeY = sizeY;
            }
            catch (OutOfMemoryException)
            {
                _sizeX = 500;
                _sizeY = 500;

                MessageBox.Show("Pixel sizes entered were too large!" + Environment.NewLine +
                    "Reducing image size to 500 x 500...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _mandelbrot.ReinitializeMandelbrot(
                        _sizeX, _sizeY, func,
                        powR, powI, escapeRadius,
                        centerR, centerI, range,
                        zro, zio);
            }

            if (updateLastFunctionSettings)
                _lastFunctionSettings =
                    new LastMandelFunctionSettings
                    {
                        Function = func,
                        InitialIterations = initIters,
                        EscapeRadius = escapeRadius,
                        PowerReal = powR,
                        PowerImag = powI,
                        ZRealOffset = zro,
                        ZImagOffset = zio
                    };

            _lastPositionSettings = new LastMandelPositionSettings
            {
                CenterReal = centerR,
                CenterImag = centerI,
                Range = range
            };

            PositionRealUpDown.Value = centerR;
            PositionImagUpDown.Value = centerI;
            PositionRangeUpDown.Value = range;

            if (forceRebuildImage || _wtb == null || _wtb.PixelWidth != _sizeX || _wtb.PixelHeight != _sizeY)
                InitMandelImage(_lastDisplaySettings.ColorPalette);

            // Just in case
            GC.Collect();

            IterPrintMandel(initIters);
        }

        private void InitMandelImage(MandelColorPalette palette, bool doPrint = false)
        {
            // Assumed that color 0 on any palette is the in set color
            var c = new List<Color>() { SET_COLOR };
            var p = _paletteColors % 2 == 1 ? _paletteColors : _paletteColors + 1;
            bool sine = _lastDisplaySettings.UseSineColorInterpolation;
            switch (palette)
            {
                case MandelColorPalette.BlueGreenRed:
                    c.AddRange(ColorUtils.RgbLinearInterpolate(Colors.Blue, Colors.Green, Colors.Red, p, sine));
                    break;
                case MandelColorPalette.GreenWhiteMagenta:
                    c.AddRange(ColorUtils.RgbLinearInterpolate(Colors.Green, Colors.White, Colors.Magenta, p, sine));
                    break;
                case MandelColorPalette.RedYellowGreen:
                    c.AddRange(ColorUtils.RgbLinearInterpolate(Colors.Red, Colors.Yellow, Colors.Green, p, sine));
                    break;
                case MandelColorPalette.IndigoOrangeWhite:
                    c.AddRange(ColorUtils.RgbLinearInterpolate(Colors.Indigo, Colors.Orange, Colors.White, p, sine));
                    break;
                case MandelColorPalette.GrayScale:
                    c.AddRange(ColorUtils.RgbLinearInterpolate(Color.FromRgb(1, 1, 1), Color.FromRgb(255, 255, 255), _paletteColors, sine));
                    break;
                case MandelColorPalette.RedScale:
                    c.AddRange(ColorUtils.RgbLinearInterpolate(Color.FromRgb(1, 0, 0), Color.FromRgb(255, 0, 0), _paletteColors, sine));
                    break;
                case MandelColorPalette.GreenScale:
                    c.AddRange(ColorUtils.RgbLinearInterpolate(Color.FromRgb(0, 1, 0), Color.FromRgb(0, 255, 0), _paletteColors, sine));
                    break;
                case MandelColorPalette.BlueScale:
                    c.AddRange(ColorUtils.RgbLinearInterpolate(Color.FromRgb(0, 0, 1), Color.FromRgb(0, 0, 255), _paletteColors, sine));
                    break;
                case MandelColorPalette.YellowScale:
                    c.AddRange(ColorUtils.RgbLinearInterpolate(Color.FromRgb(1, 1, 0), Color.FromRgb(255, 255, 0), _paletteColors, sine));
                    break;
                case MandelColorPalette.CyanScale:
                    c.AddRange(ColorUtils.RgbLinearInterpolate(Color.FromRgb(0, 1, 1), Color.FromRgb(0, 255, 255), _paletteColors, sine));
                    break;
                case MandelColorPalette.MagentaScale:
                    c.AddRange(ColorUtils.RgbLinearInterpolate(Color.FromRgb(1, 0, 1), Color.FromRgb(255, 0, 255), _paletteColors, sine));
                    break;
                case MandelColorPalette.PinkScale:
                    c.AddRange(ColorUtils.RgbLinearInterpolate(Color.FromRgb(1, 0, 1), Colors.HotPink, _paletteColors, sine));
                    break;
                case MandelColorPalette.WhitePinkScale:
                    c[0] = Colors.White;
                    c.AddRange(ColorUtils.RgbLinearInterpolate(Color.FromRgb(254, 254, 254), Colors.DeepPink, _paletteColors, sine));
                    break;
                case MandelColorPalette.JojoScale:
                    c[0] = Colors.White;
                    c.AddRange(ColorUtils.RgbLinearInterpolate(Colors.DeepPink, Color.FromRgb(255, 219, 230), _paletteColors, sine));
                    break;
                default:
                    c.AddRange(ColorUtils.RgbLinearInterpolate(Colors.Blue, Colors.White, Colors.Orange, p, sine));
                    break;
            }

            _wtb = new WriteableBitmap(_sizeX, _sizeY, 96, 96, PixelFormats.Indexed8, new BitmapPalette(c));
            MandelImage.Source = _wtb;
            if (doPrint) PrintMandel();
        }

        private void CloseAllPopups()
        {
            SetDisplayPopup.IsOpen = false;
            SetFunctionPopup.IsOpen = false;
            SetPositionPopup.IsOpen = false;
            SaveLoadPopup.IsOpen = false;
        }

        private void SetPopupOpen(MandelPopupEnum p, bool isOpen)
        {
            if (isOpen) CloseAllPopups();

            Popup pop = null;
            switch (p)
            {
                case MandelPopupEnum.Display:
                    pop = SetDisplayPopup;
                    break;
                case MandelPopupEnum.Function:
                    pop = SetFunctionPopup;
                    break;
                case MandelPopupEnum.Position:
                    pop = SetPositionPopup;
                    break;
                case MandelPopupEnum.SaveLoad:
                    pop = SaveLoadPopup;
                    break;
            }

            if (pop != null)
                pop.IsOpen = isOpen;
        }

        private void SetFunctionPopupButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetFunctionPopup.IsOpen) { SetPopupOpen(MandelPopupEnum.Function, false); return; }

            SelectedMandelFunction = _lastFunctionSettings.Function;
            NotifyPropertyChanged(nameof(SelectedMandelFunction));
            InitIterationsUpDown.Value = _lastFunctionSettings.InitialIterations;
            EscapeRadiusUpDown.Value = _lastFunctionSettings.EscapeRadius;
            PowerRealUpDown.Value = _lastFunctionSettings.PowerReal;
            PowerImagUpDown.Value = _lastFunctionSettings.PowerImag;
            ZRealUpDown.Value = _lastFunctionSettings.ZRealOffset;
            ZImagUpDown.Value = _lastFunctionSettings.ZImagOffset;

            SetPopupOpen(MandelPopupEnum.Function, true);
        }
        private void SetFunction_Apply_Click(object sender, RoutedEventArgs e)
        {
            InitSetFunction(_sizeX, _sizeY,
                SelectedMandelFunction,
                InitIterationsUpDown.Value ?? _lastFunctionSettings.InitialIterations,
                EscapeRadiusUpDown.Value ?? _lastFunctionSettings.EscapeRadius,
                PowerRealUpDown.Value ?? _lastFunctionSettings.PowerReal,
                PowerImagUpDown.Value ?? _lastFunctionSettings.PowerImag,
                ZRealUpDown.Value ?? _lastFunctionSettings.ZRealOffset,
                ZImagUpDown.Value ?? _lastFunctionSettings.ZImagOffset);
        }

        private void SetFunction_OK_Click(object sender, RoutedEventArgs e)
        {
            SetPopupOpen(MandelPopupEnum.Function, false);
            SetFunction_Apply_Click(sender, e);
        }

        private void SetPositionPopupButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetPositionPopup.IsOpen) { SetPopupOpen(MandelPopupEnum.Position, false); return; }

            PositionRealUpDown.Value = _lastPositionSettings.CenterReal;
            PositionImagUpDown.Value = _lastPositionSettings.CenterImag;
            PositionRangeUpDown.Value = _lastPositionSettings.Range;
            SetPopupOpen(MandelPopupEnum.Position, true);
        }

        private void SetPosition_OK_Click(object sender, RoutedEventArgs e)
        {
            SetPopupOpen(MandelPopupEnum.Position, false);
            SetPosition_Apply_Click(sender, e);
        }

        private void SetPosition_Apply_Click(object sender, RoutedEventArgs e)
        {
            double ncr = PositionRealUpDown.Value ?? _lastPositionSettings.CenterReal;
            double nci = PositionImagUpDown.Value ?? _lastPositionSettings.CenterImag;
            double nr = PositionRangeUpDown.Value ?? _lastPositionSettings.Range;
            if (ncr != _lastPositionSettings.CenterReal
                || nci != _lastPositionSettings.CenterImag
                || nr != _lastPositionSettings.Range)
                InitFromCurrentBrot(_sizeX, _sizeY,
                    newCenterReal: ncr, newCenterImag: nci, newRange: nr);
        }

        private void SetDisplayPopupButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetDisplayPopup.IsOpen) { SetPopupOpen(MandelPopupEnum.Display, false); return; }

            bool auto = UseInstantApplyCheckbox?.IsChecked == true;
            // Temporarily turn off auto updating
            if (auto) UseInstantApplyCheckbox.IsChecked = false;
            SizeXUpDown.Value = _sizeX;
            SizeYUpDown.Value = _sizeY;
            SelectedMandelColorPalette = _lastDisplaySettings.ColorPalette;
            NotifyPropertyChanged(nameof(SelectedMandelColorPalette));
            SelectedMandelColorFunction = _lastDisplaySettings.ColorFunction;
            NotifyPropertyChanged(nameof(SelectedMandelColorFunction));
            UseColorSmoothingCheckbox.IsChecked = _lastDisplaySettings.UseSmoothColoring;
            UseSineInterpolationCheckbox.IsChecked = _lastDisplaySettings.UseSineColorInterpolation;
            SmoothingIterationsUpDown.Value = _lastDisplaySettings.SmoothingIterations;
            InterpolationPeriodUpDown.Value = _lastDisplaySettings.InterpolationPeriod;
            PaletteOffsetUpDown.Value = _lastDisplaySettings.PaletteOffset;
            NValueUpDown.Value = _lastDisplaySettings.NValue;
            if (auto) UseInstantApplyCheckbox.IsChecked = true;
            SetPopupOpen(MandelPopupEnum.Display, true);
        }

        private void SetDisplay_OK_Click(object sender, RoutedEventArgs e)
        {
            SetPopupOpen(MandelPopupEnum.Display, false);
            UpdateDisplay(false);
        }
        private void SetDisplay_Apply_Click(object sender, RoutedEventArgs e) => UpdateDisplay(false);
        private void SetDisplayCheckChanged(object sender, RoutedEventArgs e) => UpdateDisplay(true);
        private void SetDisplayValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) => UpdateDisplay(true);
        private void SetDisplaySelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateDisplay(true);

        private void UpdateDisplay(bool fromInstantApply = false)
        {
            int sx, sy;
            var lds = _lastDisplaySettings;
            if (fromInstantApply)
            {
                if (UseInstantApplyCheckbox?.IsChecked != true) return;
                sx = _sizeX;
                sy = _sizeY;
            }
            else
            {
                sx = SizeXUpDown.Value ?? _sizeX;
                sy = SizeYUpDown.Value ?? _sizeY;
            }

            bool doSmooth = UseColorSmoothingCheckbox.IsChecked ?? lds.UseSmoothColoring;
            bool doSineSmooth = UseSineInterpolationCheckbox.IsChecked ?? lds.UseSineColorInterpolation;
            int smoothIters = SmoothingIterationsUpDown.Value ?? lds.SmoothingIterations;
            double interPeriod = InterpolationPeriodUpDown.Value ?? lds.InterpolationPeriod;
            double pOffset = PaletteOffsetUpDown.Value ?? lds.PaletteOffset;
            double nVal = NValueUpDown.Value ?? lds.NValue;
            var palette = SelectedMandelColorPalette;
            var function = SelectedMandelColorFunction;

            bool reloadBrot = false, reloadBitmap = false, doReprint = false;
            if (sx != _sizeX || sy != _sizeY)
                reloadBrot = true;
            if (palette != lds.ColorPalette
                || doSineSmooth != lds.UseSineColorInterpolation)
            {
                reloadBitmap = true;
            }
            if (function != lds.ColorFunction
                || doSmooth != lds.UseSmoothColoring
                || smoothIters != lds.SmoothingIterations
                || interPeriod != lds.InterpolationPeriod
                || pOffset != lds.PaletteOffset
                || nVal != lds.NValue)
            {
                doReprint = true;
            }

            _lastDisplaySettings = new LastMandelDisplaySettings
            {
                ColorPalette = palette,
                ColorFunction = function,
                UseSmoothColoring = doSmooth,
                SmoothingIterations = smoothIters,
                UseSineColorInterpolation = doSineSmooth,
                InterpolationPeriod = interPeriod,
                PaletteOffset = pOffset,
                NValue = nVal
            };

            if (reloadBrot) InitFromCurrentBrot(sx, sy);
            else if (reloadBitmap) InitMandelImage(palette, true);
            else if (doReprint) PrintMandel();
        }

        private void SaveLoadButton_Click(object sender, RoutedEventArgs e)
        {
            SetPopupOpen(MandelPopupEnum.SaveLoad, !SaveLoadPopup.IsOpen);
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            SetPopupOpen(MandelPopupEnum.SaveLoad, false);

            var d = new Microsoft.Win32.SaveFileDialog()
            {
                Title = "Save Mandelbrot Image",
                Filter = "PNG File (*.png)|*.png"
            };
            if (d.ShowDialog() == true)
            {
                try
                {
                    // Save the bitmap into a file.
                    using (FileStream stream =
                        new FileStream(d.FileName, FileMode.Create))
                    {
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(_wtb));
                        encoder.Save(stream);
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"Error saving mandelbrot image: {err.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveJson_Click(object sender, RoutedEventArgs e)
        {
            SetPopupOpen(MandelPopupEnum.SaveLoad, false);

            var d = new Microsoft.Win32.SaveFileDialog()
            {
                Title = "Save Mandelbrot Settings To Json",
                Filter = "Text File (*.txt)|*.txt|Json File (*.json)|*.json"
            };
            if (d.ShowDialog() == true)
            {
                var jsonstruct = new MandelJsonStruct
                {
                    Function = _lastFunctionSettings,
                    Position = _lastPositionSettings,
                    Display = _lastDisplaySettings,
                    Iterations = _mandelbrot.TotalIterations
                };

                try
                {
                    File.WriteAllText(d.FileName, JsonConvert.SerializeObject(jsonstruct, Formatting.Indented));
                }
                catch (Exception err)
                {
                    MessageBox.Show($"Error saving mandelbrot to Json: {err.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadJson_Click(object sender, RoutedEventArgs e)
        {
            SetPopupOpen(MandelPopupEnum.SaveLoad, false);

            var d = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "Load Mandelbrot Settings From Json",
                Filter = "Text File (*.txt)|*.txt|Json File (*.json)|*.json"
            };
            if (d.ShowDialog() == true)
            {
                MandelJsonStruct jsonstruct;
                try
                {
                    string s = File.ReadAllText(d.FileName);
                    jsonstruct = JsonConvert.DeserializeObject<MandelJsonStruct>(s);
                }
                catch (Exception err)
                {
                    MessageBox.Show($"Error loading mandelbrot from Json: {err.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _lastFunctionSettings = jsonstruct.Function;
                _lastDisplaySettings = jsonstruct.Display;
                InitSetFunction(_sizeX, _sizeY,
                    jsonstruct.Function.Function,
                    jsonstruct.Iterations,
                    jsonstruct.Function.EscapeRadius,
                    jsonstruct.Function.PowerReal,
                    jsonstruct.Function.PowerImag,
                    jsonstruct.Function.ZRealOffset,
                    jsonstruct.Function.ZImagOffset,
                    jsonstruct.Position.CenterReal,
                    jsonstruct.Position.CenterImag,
                    jsonstruct.Position.Range, false, true);
            }
        }

        private void MandelImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isIterating) return;
            if (e.ChangedButton != MouseButton.Left && e.ChangedButton != MouseButton.Right) return;
            if (e.ClickCount != 2) return;

            var p = e.GetPosition(MandelImage);
            var x = MandelImage.ActualWidth;
            var y = MandelImage.ActualHeight;

            InitSetFunction(_sizeX, _sizeY,
                _lastFunctionSettings.Function,
                _mandelbrot.TotalIterations,
                _lastFunctionSettings.EscapeRadius,
                _lastFunctionSettings.PowerReal,
                _lastFunctionSettings.PowerImag,
                _lastFunctionSettings.ZRealOffset,
                _lastFunctionSettings.ZImagOffset,
                // Find relative position on the image
                _mandelbrot.MinValue.Real + p.X / x * (_mandelbrot.MaxValue.Real - _mandelbrot.MinValue.Real),
                _mandelbrot.MinValue.Imaginary + p.Y / y * (_mandelbrot.MaxValue.Imaginary - _mandelbrot.MinValue.Imaginary),
                _mandelbrot.Range * (e.ChangedButton == MouseButton.Left ? 1 / ZoomFactor : ZoomFactor), false);

            e.Handled = true;
        }

        private void MandelPlayground_Deactivated(object sender, EventArgs e)
        {
            CloseAllPopups();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // http://www.java2s.com/Code/CSharp/2D-Graphics/RgbLinearInterpolate.htm
        private class ColorUtils
        {
            public static List<Color> RgbLinearInterpolate(Color start, Color end, int colorCount, bool useSineInterpolation = false)
            {
                List<Color> ret = new List<Color>();

                // linear interpolation lerp (r,a,b) = (1-r)*a + r*b = (1-r)*(ax,ay,az) + r*(bx,by,bz)
                for (int n = 0; n < colorCount; n++)
                {
                    double r = n / (double)(colorCount - 1);
                    if (useSineInterpolation)
                        r = (Math.Sin(r * Math.PI - Math.PI / 2) + 1) / 2;
                    double nr = 1.0 - r;
                    double A = (nr * start.A) + (r * end.A);
                    double R = (nr * start.R) + (r * end.R);
                    double G = (nr * start.G) + (r * end.G);
                    double B = (nr * start.B) + (r * end.B);

                    ret.Add(Color.FromArgb((byte)A, (byte)R, (byte)G, (byte)B));
                }

                return ret;
            }
            public static List<Color> RgbLinearInterpolate(Color start, Color middle, Color end, int colorCount, bool useSineInterpolation = false)
            {
                if (colorCount % 2 == 0)
                    throw new ArgumentException("colorCount should be an odd number. Currently it is: " + colorCount);

                List<Color> ret = new List<Color>();

                if (colorCount == 0)
                    return ret;

                int size = (colorCount + 1) / 2;

                List<Color> res = RgbLinearInterpolate(start, middle, size, useSineInterpolation);
                if (res.Count > 0)
                    res.RemoveAt(res.Count - 1);

                ret.AddRange(res);
                ret.AddRange(RgbLinearInterpolate(middle, end, size, useSineInterpolation));

                return ret;
            }
        }
    }
}
