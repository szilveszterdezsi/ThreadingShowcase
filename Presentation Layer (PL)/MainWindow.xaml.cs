/// ---------------------------
/// Author: Szilveszter Dezsi
/// Created: 2019-11-04
/// Modified: n/a
/// ---------------------------

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static PL.ChronoPanel;

namespace PL
{
    /// <summary>
    /// Partial presentation class that initializes and handles GUI components and threads.
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChronoPanel chronometer;
        private ChronoPanel chronograph;
        private BouncePanel animation;
        private Thread chronometerThread;
        private Thread chronographThread;
        private Thread animationThread;
        private ManualResetEvent chronometerRunning;
        private ManualResetEvent chronographRunning;
        private ManualResetEvent animationRunning;
        private Stopwatch stopWatch;

        /// <summary>
        /// Constrctor that initializes GUI components and threads.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            InitializeGUI();
            InitializeThreads();
        }

        /// <summary>
        /// Initializes GUI components and threads.
        /// </summary>
        private void InitializeGUI()
        {
            chronometer = new ChronoPanel(ChronoType.Meter, TickType.Soft, Brushes.WhiteSmoke, Brushes.Black);
            Grid.SetRow(chronometer, 0);
            gAnalogClock.Children.Add(chronometer);
            chronograph = new ChronoPanel(ChronoType.Graph, TickType.Hard, Brushes.Black, Brushes.WhiteSmoke);
            Grid.SetRow(chronograph, 0);
            gStopWatch.Children.Add(chronograph);
            animation = new BouncePanel();
            Grid.SetRow(animation, 0);
            gAnimation.Children.Add(animation);
            btnAnalogClockTickSoft.Visibility = Visibility.Collapsed;
            btnResumeCurrentTime.Visibility = Visibility.Collapsed;
            btnAnalogStopWatchTickHard.Visibility = Visibility.Collapsed;
            btnStopAnalogStopWatch.Visibility = Visibility.Collapsed;
            btnUnFreezeAnimation.Visibility = Visibility.Collapsed;
            btnResetAnalogStopWatch.IsEnabled = false;
        }

        /// <summary>
        /// Initializes threads.
        /// </summary>
        private void InitializeThreads()
        {
            chronometerThread = new Thread(Chronometer);
            chronographThread = new Thread(Chronograph);
            animationThread = new Thread(Animation);
            chronometerRunning = new ManualResetEvent(true);
            chronographRunning = new ManualResetEvent(false);
            animationRunning = new ManualResetEvent(true);
            chronometerThread.Start();
            chronographThread.Start();
            animationThread.Start();
            stopWatch = new Stopwatch();
        }

        /// <summary>
        /// Thread that runs ChronoPanel configured as an analog clock (chronometer).
        /// Analog clock is updated (ticks) every 100 ms.
        /// </summary>
        private void Chronometer()
        {
            while (true)
            {
                Dispatcher.Invoke(() => { chronometer.ChronometerTick(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond); });
                Thread.Sleep(100);
                chronometerRunning.WaitOne(Timeout.Infinite);
            }
        }

        /// <summary>
        /// Thread that runs ChronoPanel configured as an analog stop watch (chronograph).
        /// Thread automatcially stops and resets when stop watch reaches 60 min (1 hour).
        /// Analog stop watch is updated (ticks) every 30 ms.
        /// </summary>
        private void Chronograph()
        {
            while (true)
            {
                chronographRunning.WaitOne(Timeout.Infinite);
                Dispatcher.Invoke(() => { chronograph.ChonographTick(stopWatch.Elapsed.TotalMinutes, stopWatch.Elapsed.TotalSeconds); });
                Thread.Sleep(30);
                if (stopWatch.Elapsed.Hours >= 1)
                {
                    Dispatcher.Invoke(() => {
                        chronograph.ChonographTick(0, 0);
                        stopWatch.Reset();
                        btnResetAnalogStopWatch.IsEnabled = false;
                        btnStartAnalogStopWatch.Visibility = Visibility.Visible;
                        btnStopAnalogStopWatch.Visibility = Visibility.Collapsed;
                        chronographRunning.Reset();
                    });
                }
            }
        }

        /// <summary>
        /// Thread that runs BouncerPanel animation.
        /// Animation is incremented (steps) every 30 ms.
        /// </summary>
        private void Animation()
        {
            while (true)
            {
                Dispatcher.Invoke(() => { animation.Step(); });
                Thread.Sleep(30);
                animationRunning.WaitOne(Timeout.Infinite);
            }
        }

        /// <summary>
        /// Detects when application is closing and terminates all threads.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (chronometerThread.IsAlive)
            {
                chronometerThread.Abort();
            }
            if (chronographThread.IsAlive)
            {
                chronographThread.Abort();
            }
            if (animationThread.IsAlive)
            {
                animationThread.Abort();
            }
        }

        /// <summary>
        /// Detects then button to freeze current time of analog clock is clicked. 
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">RoutedEventArgs.</param>
        private void BtnFreezeCurrentTime_Click(object sender, RoutedEventArgs e)
        {
            chronometerRunning.Reset();
            btnFreezeCurrentTime.Visibility = Visibility.Collapsed;
            btnResumeCurrentTime.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Detects then button to resume current time of analog clock is clicked.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">RoutedEventArgs.</param>
        private void BtnResumeCurrentTime_Click(object sender, RoutedEventArgs e)
        {
            chronometerRunning.Set();
            btnFreezeCurrentTime.Visibility = Visibility.Visible;
            btnResumeCurrentTime.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Detects then button to stop time of analog stop watch is clicked.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">RoutedEventArgs.</param>
        private void BtnStopAnalogStopWatch_Click(object sender, RoutedEventArgs e)
        {
            chronographRunning.Reset();
            stopWatch.Stop();
            btnResetAnalogStopWatch.IsEnabled = true;
            btnStartAnalogStopWatch.Visibility = Visibility.Visible;
            btnStopAnalogStopWatch.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Detects then button to start time of analog stop watch is clicked.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">RoutedEventArgs.</param>
        private void BtnStartAnalogStopWatch_Click(object sender, RoutedEventArgs e)
        {
            chronographRunning.Set();
            stopWatch.Start();
            btnResetAnalogStopWatch.IsEnabled = false;
            btnStartAnalogStopWatch.Visibility = Visibility.Collapsed;
            btnStopAnalogStopWatch.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Detects then button to switch to soft tick type of analog clock is clicked.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">RoutedEventArgs.</param>
        private void BtnAnalogClockTickSoft_Click(object sender, RoutedEventArgs e)
        {
            chronometer.SetTickType(TickType.Soft);
            btnAnalogClockTickSoft.Visibility = Visibility.Collapsed;
            btnAnalogClockTickHard.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Detects then button to switch to hard tick type of analog clock is clicked.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">RoutedEventArgs.</param>
        private void BtnAnalogClockTickHard_Click(object sender, RoutedEventArgs e)
        {
            chronometer.SetTickType(TickType.Hard);
            btnAnalogClockTickHard.Visibility = Visibility.Collapsed;
            btnAnalogClockTickSoft.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Detects then button to switch to soft tick type of analog stop watch is clicked.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">RoutedEventArgs.</param>
        private void BtnAnalogStopWatchTickSoft_Click(object sender, RoutedEventArgs e)
        {
            chronograph.SetTickType(TickType.Soft);
            btnAnalogStopWatchTickSoft.Visibility = Visibility.Collapsed;
            btnAnalogStopWatchTickHard.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Detects then button to switch to hard tick type of analog stop watch is clicked.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">RoutedEventArgs.</param>
        private void BtnAnalogStopWatchTickHard_Click(object sender, RoutedEventArgs e)
        {
            chronograph.SetTickType(TickType.Hard);
            btnAnalogStopWatchTickHard.Visibility = Visibility.Collapsed;
            btnAnalogStopWatchTickSoft.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Detects then button to reset analog stop watch is clicked.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">RoutedEventArgs.</param>
        private void BtnResetAnalogStopWatch_Click(object sender, RoutedEventArgs e)
        {
            chronograph.ChonographTick(0, 0);
            stopWatch.Reset();
            btnResetAnalogStopWatch.IsEnabled = false;
        }

        /// <summary>
        /// Detects then button to freeze animation of bouncer panel is clicked.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">RoutedEventArgs.</param>
        private void BtnFreezeAnimation_Click(object sender, RoutedEventArgs e)
        {
            animationRunning.Reset();
            btnFreezeAnimation.Visibility = Visibility.Collapsed;
            btnUnFreezeAnimation.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Detects then button to unfreeze animation of bouncer panel is clicked.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">RoutedEventArgs.</param>
        private void BtnUnFreezeAnimation_Click(object sender, RoutedEventArgs e)
        {
            animationRunning.Set();
            btnFreezeAnimation.Visibility = Visibility.Visible;
            btnUnFreezeAnimation.Visibility = Visibility.Collapsed;
        }
    }
}
