using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using UglyTetris.GameLogic;

namespace WpfApp1
{
    class AccurateTimer
    {
        private delegate void TimerEventDel(int id, int msg, IntPtr user, int dw1, int dw2);
        private const int TIME_PERIODIC = 1;
        private const int EVENT_TYPE = TIME_PERIODIC;// + 0x100;  // TIME_KILL_SYNCHRONOUS causes a hang ?!
        [DllImport("winmm.dll")]
        private static extern int timeBeginPeriod(int msec);
        [DllImport("winmm.dll")]
        private static extern int timeEndPeriod(int msec);
        [DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution, TimerEventDel handler, IntPtr user, int eventType);
        [DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);

        Action mAction;
        private int mTimerId;
        private TimerEventDel mHandler;  // NOTE: declare at class scope so garbage collector doesn't release it!!!

        public AccurateTimer(Action action, int delay)
        {
            mAction = action;
            timeBeginPeriod(1);
            mHandler = new TimerEventDel(TimerCallback);
            mTimerId = timeSetEvent(delay, 0, mHandler, IntPtr.Zero, EVENT_TYPE);
        }

        public void Stop()
        {
            int err = timeKillEvent(mTimerId);
            timeEndPeriod(1);
            System.Threading.Thread.Sleep(100);// Ensure callbacks are drained
        }

        private void TimerCallback(int id, int msg, IntPtr user, int dw1, int dw2)
        {
            if (mTimerId != 0)
                mAction();
        }
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            _figureDrawer = new FigureDrawer(new TileDrawer(MainCanvas));
            _fieldDrawer = new FieldDrawer(new TileDrawer(MainCanvas));
            _nextFigureDrawer = new FigureDrawer(new TileDrawer(NextFigureCanvas));

            Game = new Game(new RandomNextFigureFactory());
            Game.FigureStateChanged += GameOnFigureStateChanged;
            Game.NewFigure += GameOnNewFigure; ;
            Game.LinesChanged += GameOnLinesChanged;
            Game.StateChanged += GameOnStateChanged;
            
            Game.Field = Field.CreateField(FieldHelper.FieldDefaultWidth, FieldHelper.FieldDefaultHeight, "DimGray");
            Game.ResetFigure();

            _fieldDrawer.AttachToField(Game.Field);

            _timer = new AccurateTimer(new Action(GameTick), 10);
        }

        private void GameOnNewFigure(object sender, NewFigureEventArgs e)
        {
            if (e.Figure == null)
            {
                return;
            }

            _nextFigureDrawer.DrawFigure(e.Figure, 0, 0);
        }

        private void GameTick()
        {
            if (App.Current == null)
            {
                _timer.Stop();
                return;
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                Game.Tick();
            });
        }

        private void GameOnStateChanged(object sender, EventArgs e)
        {
            if (Game.State == GameState.GameOver)
            {
                _timer.Stop();
            }
        }

        private void GameOnLinesChanged(object sender, EventArgs e)
        {
            LineCountTextBlock.Text = Game.Lines.ToString(CultureInfo.InvariantCulture);
        }

        private void GameOnFigureStateChanged(object sender, EventArgs e)
        {
            _figureDrawer.DrawFigure(Game.Figure, Game.FigurePositionX, Game.FigurePositionY);
        }


        public Game Game;
        private readonly AccurateTimer _timer;

        private FieldDrawer _fieldDrawer;
        private FigureDrawer _figureDrawer;
        private FigureDrawer _nextFigureDrawer;

        private void MoveLeft()
        {
            Game.MoveLeft();
        }

        private void MoveRight()
        {
            Game.MoveRight();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MoveLeft();
        }

        private void RotateAntiClockWise()
        {
            Game.RotateAntiClockWise();
        }

        private void RotateClockWise()
        {
            Game.RotateClockWise();
        }

        private new void Drop()
        {
            Game.Drop();
        }
        
        private FigureFactory _figureFactory = new FigureFactory();

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.IsRepeat)
            //{
            //    return;
            //}

            if (e.Key == Key.Left)
            {
                MoveLeft();
            }
            else if (e.Key == Key.Right)
            {
                MoveRight();
            }
            else if (e.Key == Key.Up)
            {
                RotateClockWise();
            }
            else if (e.Key == Key.Down || e.Key == Key.Space)
            {
                Drop();
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RotateAntiClockWise();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            RotateClockWise();
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MoveRight();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Drop();
        }
    }

    internal class RandomNextFigureFactory : INextFigureFactory
    {
        public RandomNextFigureFactory()
        {
            Top = _figureFactory.CreateRandomFigure();
        }
        public Figure GetNextFigure()
        {
            var top = Top;
            Top = _figureFactory.CreateRandomFigure();
            return top;
        }

        public Figure Top { get; private set; }

        readonly FigureFactory _figureFactory = new FigureFactory();
    }
}
