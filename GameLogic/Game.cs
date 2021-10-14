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

        int MoveDownPeriodTicks { get; } = 10;

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

        private int _speed = 0;
        public int Speed
        {
            get => _speed;
            private set
            {
                _speed = value;
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

        private int SpeedFactor => MoveDownPeriodTicks - Speed / 3;
        private int MaxSpeedFactor = 3;
        private int LimitedSpeedFactor => SpeedFactor > MaxSpeedFactor ? SpeedFactor : MaxSpeedFactor;

        public void Tick()
        {
            if (State == GameState.GameOver)
            {
                return;
            }

            _tickCount++;
            _moveRightTicks--;
            _moveLeftTicks--;
            _moveRotateTicks--;

            bool rightAssist = _moveRightTicks > 0;
            bool leftAssist = _moveLeftTicks > 0;
            bool rotateAssist = _moveRotateTicks > 0;

            bool moveDown = IsFalling
                ? true//(_tickCount % FallDownPeriodTicks == 0)
                : (_tickCount % LimitedSpeedFactor == 0);

            if (moveDown)
            {
                var y = FigurePositionY + 1;
                var x = FigurePositionX;

                if (!Field.IsPossibleToPlaceFigure(Figure, x, y))
                {
                    if (rotateAssist)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Figure.RotateLeft();
                            if (Field.IsPossibleToPlaceFigure(Figure, x, y) && !IsBadMove(Figure, x, y))
                            {
                                FigurePositionX = x;
                                FigurePositionY = y;
                                RaiseFigureStateChanged();
                                return;
                            }
                        }
                    }
                    else if (rightAssist && Field.IsPossibleToPlaceFigure(Figure, x + 1, y) && !IsBadMove(Figure, x + 1, y))
                    {
                        MoveRight();
                        return;
                    }
                    else if (leftAssist && Field.IsPossibleToPlaceFigure(Figure, x - 1, y) && !IsBadMove(Figure, x - 1, y))
                    {
                        MoveLeft();
                        return;
                    }

                    Field.LockFigure(Figure, FigurePositionX, FigurePositionY, true);

                    var lineCount = Field.RemoveFullLines();

                    if (lineCount > 0) Speed++;
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

        private bool IsBadMove(Figure f, int figureX, int figureY)
        {
            if (Field.IsPossibleToPlaceFigure(f, figureX, figureY + 1))
            {
                return false;
            }

            var positionedFigure = new PositionedFigure(f, figureX, figureY);
            FieldMetrics fieldMetrics = new FieldMetrics(Field);
            return fieldMetrics.CalculateNewVoids(positionedFigure) > 1;
        }

        private bool IsWrongPosition(Figure f, int figureX, int figureY)
        {
            return !Field.IsPossibleToPlaceFigure(Figure, FigurePositionX, FigurePositionY) 
                   || IsBadMove(Figure, FigurePositionX, FigurePositionY);
        }

        private int _moveLeftTicks = 0;
        public void MoveLeft()
        {
            FigurePositionX--;
            _moveLeftTicks = 15;
            _moveRightTicks = 0;

            if (IsWrongPosition(Figure, FigurePositionX, FigurePositionY))
            {
                FigurePositionX++;
                return;
            }

            RaiseFigureStateChanged();
        }

        private int _moveRightTicks = 0;
        public void MoveRight()
        {
            FigurePositionX++;
            _moveRightTicks = 15;
            _moveLeftTicks = 0;

            if (IsWrongPosition(Figure, FigurePositionX, FigurePositionY))
            {
                FigurePositionX--;
                return;
            }

            RaiseFigureStateChanged();
        }

        private int _moveRotateTicks = 0;
        public void RotateAntiClockWise()
        {
            _moveRotateTicks = 15;
            Figure.RotateLeft();
            if (IsWrongPosition(Figure, FigurePositionX, FigurePositionY))
            {
                Figure.RotateRight();
            }
            RaiseFigureStateChanged();
        }

        public void RotateClockWise()
        {
            Figure.RotateRight();
            if (IsWrongPosition(Figure, FigurePositionX, FigurePositionY))
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

        IEnumerable<PositionedFigure> GetFigurePossiblePositons(Figure figure, IFigure otherFigures = null, bool checkRotations = true)
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

                if (!checkRotations) yield break;

                figure.RotateRight();
            }
        }

        private Figure CheatFigure(bool improve, string lastColor)
        {
            int counter = 0;

            bool changeFigure = true;
            var fieldMetrics = new FieldMetrics(Field);

            var lastTwoFigures = new HashSet<string>();
            lastTwoFigures.Add(lastColor);
            lastTwoFigures.Add(Figure.Color);

            var failedFigures = new HashSet<string>();
            const int maxFailedSteps = 7;

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
                bool checkRotation = !improve || counter > maxFailedSteps;

                foreach (var currentFigure in GetFigurePossiblePositons(Figure))
                {
                    foreach (var nextFigure in GetFigurePossiblePositons(_nextFigureFactory.Top, currentFigure, checkRotation))
                    {
                        var figuresCombination = new FiguresCombination(new[] { currentFigure, nextFigure });
                        var deletedLines = fieldMetrics.CalculateLinesToDelete(figuresCombination);

                        if (!improve)
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
                            if (deletedLines.Count() >= 3)
                            {
                                break;
                            }
                        }
                    }
                }

                if (improve && minNewVoids >= counter / maxFailedSteps)
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

            if (_random.Next(0, 10) != 0) CheatFigure(true, lastColor);

            NewFigure?.Invoke(this, new NewFigureEventArgs(_nextFigureFactory.Top));
            return true;
        }

        private INextFigureFactory _nextFigureFactory;
    }
}