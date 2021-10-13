using System;
using System.Collections.Generic;
using System.Linq;

namespace UglyTetris.GameLogic
{
    public class NewFigureEventArgs : EventArgs
    {
        public NewFigureEventArgs(Figure figure)
        {
            Figure = figure;
        }
        public Figure Figure;
    }

    public class Game
    {
        public Game(INextFigureFactory nextFigureFactory)
        {
            _nextFigureFactory = nextFigureFactory;
        }

        public bool IsFalling { get; set; }

        private int _tickCount = 0;

        int MoveDownPeriodTicks { get; } = 20;

        private int FallDownPeriodTicks { get; } = 3;


        private int _lines = 0;
        public int Lines
        {
            get => _lines;
            private set
            {
                _lines = value;
                LinesChanged?.Invoke(this, EventArgs.Empty);
            }
        }


        private GameState _state = GameState.Running;
        public GameState State
        {
            get => _state;
            private set
            {
                _state = value;
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Tick()
        {
            if (State == GameState.GameOver)
            {
                return;
            }

            _tickCount++;

            int speedFactor = MoveDownPeriodTicks - Lines/2;

            bool moveDown = IsFalling
                ? true//(_tickCount % FallDownPeriodTicks == 0)
                : (_tickCount % speedFactor == 0);

            if (moveDown)
            {
                var y = FigurePositionY + 1;
                var x = FigurePositionX;

                if (!Field.IsPossibleToPlaceFigure(Figure, x, y))
                {
                    Field.LockFigure(Figure, FigurePositionX, FigurePositionY, true);

                    var lineCount = Field.RemoveFullLines();
                    Lines += lineCount;

                    RaiseFigureStateChanged();

                    if (!ResetFigure())
                    {
                        State = GameState.GameOver;
                    }

                    _tickCount = 0;
                    IsFalling = false;
                }
                else
                {
                    FigurePositionX = x;
                    FigurePositionY = y;
                    RaiseFigureStateChanged();
                }
            }
        }

        public void MoveLeft()
        {
            FigurePositionX--;

            if (!Field.IsPossibleToPlaceFigure(Figure, FigurePositionX, FigurePositionY))
            {
                FigurePositionX++;
                return;
            }

            RaiseFigureStateChanged();
        }

        public void MoveRight()
        {
            FigurePositionX++;

            if (!Field.IsPossibleToPlaceFigure(Figure, FigurePositionX, FigurePositionY))
            {
                FigurePositionX--;
                return;
            }

            RaiseFigureStateChanged();
        }

        public void RotateAntiClockWise()
        {
            Figure.RotateLeft();
            if (!Field.IsPossibleToPlaceFigure(Figure, FigurePositionX, FigurePositionY))
            {
                Figure.RotateRight();
            }
            RaiseFigureStateChanged();
        }

        public void RotateClockWise()
        {
            Figure.RotateRight();
            if (!Field.IsPossibleToPlaceFigure(Figure, FigurePositionX, FigurePositionY))
            {
                Figure.RotateLeft();
            }
            RaiseFigureStateChanged();
        }

        public void Drop()
        {
            IsFalling = true;
        }

        public Figure Figure { get; private set; } = new Figure();

        public event EventHandler<NewFigureEventArgs> NewFigure;
        public event EventHandler FigureStateChanged;
        protected void RaiseFigureStateChanged()
        {
            FigureStateChanged?.Invoke(this, EventArgs.Empty);
        }
        
        public event EventHandler LinesChanged; // may be replaced with INotifyPropertyChanged interface implementation
        
        public event EventHandler StateChanged; // may be replaced with INotifyPropertyChanged interface implementation

        public int FigurePositionX { get; private set; } = 6;
        public int FigurePositionY { get; private set; } = 0;

        public Field Field;

        IEnumerable<PositionedFigure> GetFigurePossiblePositons(Figure figure, IFigure otherFigures = null)
        {
            for (int r = 0; r < 4; r++)
            {
                for (int x = -2; x < Field.Xmax; x++)
                {
                    bool wasValid = false;
                    int y = 0;
                    for (y = 1; y <= Field.Ymax; y++)
                    {
                        if (!Field.IsPossibleToPlaceFigure(figure, x, y) ||
                            (otherFigures != null && otherFigures.IsOverlap(new PositionedFigure(figure, x, y))))
                        {
                            y--;
                            break;
                        }

                        wasValid = true;
                    }

                    if (!wasValid) continue;

                    yield return new PositionedFigure(figure, x, y);
                }
                figure.RotateRight();
            }
        }

        private bool _goodGuy = false;
        private int _goodGuyHeight = 10;
        private Figure CheatFigure(string lastColor)
        {
            int counter = 0;

            bool changeFigure = true;
            var fieldMetrics = new FieldMetrics(Field);

            if (!_goodGuy && fieldMetrics.CalculateHeight() > _goodGuyHeight)
            {
                _goodGuyHeight += 3;
                _goodGuy = true;
            }
            else if (_goodGuy && fieldMetrics.CalculateHeight() <= 2)
            {
                _goodGuy = false;
            }

            var lastTwoFigures = new HashSet<string>();
            lastTwoFigures.Add(lastColor);
            lastTwoFigures.Add(Figure.Color);

            var failedFigures = new HashSet<string>();
            const int maxFailedSteps = 5;

            while (changeFigure)
            {
                counter++;
                if (counter % maxFailedSteps == 0)
                {
                    failedFigures.Clear();
                }

                changeFigure = false;
                _nextFigureFactory.GetNextFigure();
                var currentColor = _nextFigureFactory.Top.Color;

                if (lastTwoFigures.Contains(currentColor) || 
                    failedFigures.Contains(currentColor))
                {
                    changeFigure = true;
                    continue;
                }

                int minNewVoids = int.MaxValue;
                foreach (var currentFigure in GetFigurePossiblePositons(Figure))
                {
                    foreach (var nextFigure in GetFigurePossiblePositons(_nextFigureFactory.Top, currentFigure))
                    {
                        var figuresCombination = new FiguresCombination(new[] { currentFigure, nextFigure });
                        var deletedLines = fieldMetrics.CalculateLinesToDelete(figuresCombination);

                        if (!_goodGuy)
                        {
                            if (deletedLines.Count() > counter / maxFailedSteps)
                            {
                                failedFigures.Add(currentColor);
                                changeFigure = true;
                                break;
                            }
                        }
                        else
                        {
                            minNewVoids = Math.Min(minNewVoids, fieldMetrics.CalculateNewVoids(figuresCombination, deletedLines));
                        }
                    }
                }

                if (_goodGuy && minNewVoids > counter / maxFailedSteps)
                {
                    failedFigures.Add(currentColor);
                    changeFigure = true;
                }
            }

            return _nextFigureFactory.Top;
        }

        private Random _random = new Random();
        public bool ResetFigure()
        {
            string lastColor = Figure.Color ?? string.Empty;
            Figure = _nextFigureFactory.GetNextFigure();

            FigurePositionX = (Field.Xmax - Field.Xmin) / 2;
            FigurePositionY = 0;

            if (!Field.IsPossibleToPlaceFigure(Figure, FigurePositionX, FigurePositionY))
            {
                //GAME OVER: cannot reset figure
                return false;
            }

            if (_random.Next(0, 10) != 0) CheatFigure(lastColor);

            NewFigure?.Invoke(this, new NewFigureEventArgs(_nextFigureFactory.Top));
            return true;
        }

        private INextFigureFactory _nextFigureFactory;
    }
}