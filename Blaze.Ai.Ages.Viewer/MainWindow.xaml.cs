using Blaze.Ai.Ages.Basic;
using Blaze.Core.Wpf;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Blaze.Ai.Ages.Viewer.GA;

namespace Blaze.Ai.Ages.Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _series = new Series();
            DataContext = _series;
            _cartesianChart.AxisX[0].LabelFormatter = (x) => (x).ToString();

            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(1000),
            };
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();

            Task.Run(() => Work());
        }

        private List<CartesianIndividual> _champs = new List<CartesianIndividual>(200);

        private void Work()
        {
            CartesianIndividual champ = _series.GA.Generation();
            float mse = champ.Score.Value * 1000;
            double logmse = Math.Log10(mse);

            _champs.Add(champ);

            var newPop = new ChartValues<ScatterPoint>();
            foreach (var ei in _series.GA.Ages.Population)
            {
                CartesianIndividual i = (CartesianIndividual)ei.Individual;
                if (i == champ)
                    newPop.Add(new ScatterPoint(i.Values[0], i.Values[1], 1));
                else
                    newPop.Add(new ScatterPoint(i.Values[0], i.Values[1], 0.1));
            }

            bool isNewChamp = _champs.Count == 1 
                || CartesianIndividual.Distance((CartesianIndividual)_champs[_champs.Count - 2], champ) > 0.001;

            ChartValues<ObservablePoint> actualValues = null;
            if (isNewChamp)
            {
                actualValues = new ChartValues<ObservablePoint>(
                    _series.GA.ExpectedValsX.Zip(
                        Helpers.EvaluatePolynomial(champ, _series.GA.PowsOfXForAllX),
                        (x, y) => new ObservablePoint(x, y)));
            }

            Dispatcher.Invoke(() =>
            {
                _series.Score.Values.Add(logmse);

                var nicheDensity = _series.GA.Ages.NicheStrat as Strats.NicheDensityStrategy;

                _series.R.Values.Add((double)(nicheDensity?.NicheRadius ?? 0));

                _series.Pop.Values = newPop;

                if (isNewChamp)
                {
                    _series.Actual.Values = actualValues;
                    _series.Champ.Values.Add(new ObservablePoint(champ.Values[0], champ.Values[1]));
                }

                for (int i = 0; i < champ.Values.Length && i < _series.Champs.Length; ++i)
                    _series.Champs[i].Values.Add(champ.Values[i]);

                _timer.Start();
            });
        }

        private void PrintGen()
        {
            StringBuilder gen = new StringBuilder();
            foreach (Ages.EvaluatedIndividual ei in _series.GA.Ages.Population)
            {
                gen.AppendLine(string.Format("{0}\t{1}", ei.Individual.Name, ei.Individual));
            }
            //Console.WriteLine(gen.ToString());
            System.IO.File.WriteAllText(string.Format("Generation{0}.txt", 0), gen.ToString());

        }
        DispatcherTimer _timer;

        private Series _series;

        private void _buttonStartPause_Click(object sender, RoutedEventArgs e)
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                _buttonStartPause.Content = "Start";
            }
            else
            {
                _timer.Start();
                _buttonStartPause.Content = "Pause";
            }
        }

        public class Series : ViewModel
        {
            public Series()
            {

                GA = new GA();

                var mapper = Mappers.Xy<ObservablePoint>()
                    .X(p => p.X)
                    .Y(p => Math.Log10(p.Y));

                Score = new LineSeries
                {
                    Title = "Score",
                    Values = new ChartValues<double> { },
                    Fill = Brushes.Transparent,
                };


                Champs = new LineSeries[GA.PolynomialOrder];
                for (int i = 0; i < GA.PolynomialOrder; ++i)
                {
                    Champs[i] = new LineSeries()
                    {
                        Title = $"I_{i}",
                        Values = new ChartValues<double>(),
                        StrokeThickness = 1,
                        PointGeometry = DefaultGeometries.None
                    };
                }

                R = new LineSeries()
                {
                    Title = "R",
                    Values = new ChartValues<double>(),
                };

                Actual = new LineSeries()
                {
                    Title = "Actual",
                    Values = new ChartValues<ObservablePoint>(),
                    PointGeometry = DefaultGeometries.None
                };

                Expected = new LineSeries()
                {
                    Title = "Expected",
                    Values = new ChartValues<ObservablePoint>(),
                    PointGeometry = DefaultGeometries.None
                };
                Expected.Values.AddRange(GA.ExpectedValsX.Zip(GA.ExpectedValsY, (x, y) => new ObservablePoint(x, y)));

                Pop = new ScatterSeries()
                {
                    Title = "Pop",
                    Values = new ChartValues<ScatterPoint>(),
                    PointGeometry = DefaultGeometries.Circle,
                    StrokeThickness = 1,
                    Fill = Brushes.Blue,
                    Stroke = Brushes.Blue,
                    MaxPointShapeDiameter = 8,
                    MinPointShapeDiameter = 2
                };

                ActualPop = new HeatSeries()
                {
                    Values = new ChartValues<HeatPoint>(),
                    GradientStopCollection = new GradientStopCollection()
                    {
                        new GradientStop(Color.FromArgb(0x99, 0xEE, 0x11, 0x11), 0),
                        new GradientStop(Color.FromArgb(0x99, 0xBB, 0x11, 0x44), 0.25),
                        new GradientStop(Color.FromArgb(0x99, 0x88, 0x11, 0x77), 0.5),
                        new GradientStop(Color.FromArgb(0x99, 0x55, 0x11, 0xAA), 0.75),
                        new GradientStop(Color.FromArgb(0x99, 0x22, 0x11, 0xDD), 1),
                    },
                    DataLabels = false,
                    DrawsHeatRange = false,
                     
                };

                int f = 4;
                ActualPop.Values.AddRange(Enumerable
                    .Range(-10/f, 21/f)
                    .Select(x =>
                        {
                            return Enumerable.Range(-10/f, 21/f)
                                .Select((y) =>
                                {
                                    var ind = new CartesianIndividual(2);
                                    ind.Values[0] = x*f;
                                    ind.Values[1] = y*f;
                                    return new HeatPoint(f * x, f * y, ind.PolynomialEval(GA.PowsOfXForAllX, GA.ExpectedValsY));
                                });
                        })
                    .SelectMany(hp => hp)
                    .ToList());

                Champ = new LineSeries()
                {
                    Title = "Champ",
                    Values = new ChartValues<ObservablePoint>(),
                    PointGeometry = DefaultGeometries.None,
                    //PointForeground = Brushes.Red,
                    Stroke = Brushes.Red,
                    Fill = Brushes.Transparent,
                    LineSmoothness = 0
                };

                ScoreCollection = new SeriesCollection() { Score };

                PopCollection = new SeriesCollection { Pop, Champ, };

                ChampionCollection = new SeriesCollection();
                ChampionCollection.AddRange(Champs);

                RadiusCollection = new SeriesCollection { R };

                PhenomeCollection = new SeriesCollection { Expected, Actual };

                Formatter = y => Math.Pow(y, 10).ToString("N");

                _actionText = "Start";
            }

            public GA GA { get; }

            public LineSeries Score { get; private set; }

            public LineSeries[] Champs { get; private set; } = new LineSeries[4];

            public LineSeries R { get; private set; }

            public LineSeries Expected { get; private set; }

            public LineSeries Actual { get; private set; }

            public ScatterSeries Pop { get; set; }

            public HeatSeries ActualPop { get; set; }

            public LineSeries Champ { get; set; }

            public SeriesCollection ScoreCollection { get; set; }

            public SeriesCollection ChampionCollection { get; set; }

            public SeriesCollection PopCollection { get; set; }

            public SeriesCollection RadiusCollection { get; set; }

            public SeriesCollection PhenomeCollection { get; set; }



            public Func<double, string> Formatter { get; set; }

            public string _actionText;
            public string ActionText
            {
                get { return _actionText; }
                set
                {
                    SetProperty(ref _actionText, value);
                }
            }

            private class Command : ICommand
            {
                public Command(Action<object> action)
                {
                    _action = action;
                }

                private Action<object> _action;

                public event EventHandler CanExecuteChanged;

                public bool CanExecute(object parameter)
                {
                    return true;
                }

                public void Execute(object parameter)
                {
                    _action(parameter);
                }
            }
        }
    }
}
