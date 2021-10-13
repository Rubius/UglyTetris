namespace UglyTetris.GameLogic
{
    public class PositionedFigure : IFigure
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
        public int XMax => X + Figure.XMax;
        public int YMax => Y + Figure.YMax;

        public bool Check(int x, int y)
        {
            return Figure.Check(x - X, y - Y);
        }

        public bool IsOverlap(IFigure otherFigure)
        {
            for (var x = X; x <= XMax; x++)
            {
                for (var y = Y; y <= YMax; y++)
                {
                    if (Check(x, y) && otherFigure.Check(x, y))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
