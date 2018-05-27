using Blaze.Core.Wpf;
using LiveCharts;
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
                Interval = TimeSpan.FromMilliseconds(500),
            };
            _timer.Tick += Timer_Tick;
            //_timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            PolynomialIndividual champ = _series.GA.Generation();
            float mse = champ.MseScore * 1000;
            _series.Score.Values.Add((double)mse);
            _timer.Start();
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
                SeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Score",
                        Values = new ChartValues<double> { },
                        Fill = Brushes.Transparent,
                    }
                };

                GA = new GA();
                _actionText = "Start";
            }

            public GA GA { get; }

            public SeriesCollection SeriesCollection { get; set; }

            public LineSeries Score => (LineSeries)SeriesCollection[0];

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
