using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.DataVisualization.Charting;

namespace AmdalPrinciple
{
    public partial class MainWindow : Window
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly List<Action> _workLoad = new List<Action>();
        private readonly List<int> _workTimes = new List<int>();
        private readonly List<KeyValuePair<long, int>> _parallelSerie = new List<KeyValuePair<long, int>>();
        private readonly List<KeyValuePair<int, double>> _speedupSerie = new List<KeyValuePair<int, double>>();
        private readonly double _serialTime;
        public MainWindow()
        {
            InitializeComponent();
            ParallelLevel.Minimum = 1;
            ParallelLevel.Maximum = 15;
            var random = new Random();
            ((LineSeries)ValuesChart.Series[0]).ItemsSource = new List<KeyValuePair<DateTime, int>>();
            for (int i = 0; i < 10; i++)
            {
                int delay = random.Next(100, 700);
                _workLoad.Add(new Action(() =>
                {
                    Thread.Sleep(delay);
                }));
                _workTimes.Add(delay);
                TimesDataGrid.Items.Add(new { Name = delay });
            }
            _serialTime = _workTimes.Sum();
        }
        private void ParallelLevel_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ParLevelText.Text = ParallelLevel.Value.ToString(CultureInfo.CurrentCulture);
        }
        private void ButtonBase1_OnClick(object sender, RoutedEventArgs e)
        {
            long time = 0;
            var parallelDegree = Convert.ToInt32(ParallelLevel.Value);
            _stopwatch.Reset();
            _stopwatch.Start();
            Parallel.ForEach(_workLoad, new ParallelOptions()
            {
                MaxDegreeOfParallelism = parallelDegree
            }, (workToDo) => workToDo());
            _stopwatch.Stop();
            time = _stopwatch.ElapsedMilliseconds;
            double speedup = _serialTime / time;
            ParallelResultTextBlock.Text = time.ToString();
            UpdateValueChart(time, speedup);
        }
        private void UpdateValueChart(long time, double speedup)
        {

            _parallelSerie.Add(new KeyValuePair<long, int>(time, Convert.ToInt32(ParallelLevel.Value)));
            _speedupSerie.Add(new KeyValuePair<int, double>(Convert.ToInt32(ParallelLevel.Value), speedup));
            ((LineSeries)ValuesChart.Series[0]).ItemsSource = null;
            ((LineSeries)ValuesChart.Series[0]).ItemsSource = _parallelSerie;
            ((LineSeries)SpeedupChart.Series[0]).ItemsSource = null;
            ((LineSeries)SpeedupChart.Series[0]).ItemsSource = _speedupSerie;
        }
    }
}

