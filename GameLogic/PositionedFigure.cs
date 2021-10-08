namespace UglyTetris.GameLogic
{
    public class PositionedFigure
    {
        public PositionedFigure(Figure figure, int x, int y)
        {
            Figure = figure;
            X = x;
            Y = y;
        }

        public readonly Figure Figure;
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int Width => Figure.Width;
        public int Height => Figure.Height;
        public int XMax => Figure.XMax;
        public int YMax => Figure.YMax;

        public bool Check(int x, int y)
        {
            return Figure.Check(x - X, y - Y);
        }
    }
}
