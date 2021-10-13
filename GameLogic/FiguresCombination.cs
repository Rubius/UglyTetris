using System.Linq;
using System.Collections.Generic;

namespace UglyTetris.GameLogic
{
    public class FiguresCombination : IFigure
    {
        private IEnumerable<IFigure> _figures;

        public FiguresCombination(IEnumerable<PositionedFigure> figures)
        {
            _figures = figures;
        }

        public int X => _figures.Min(x => x.X);
        public int Y => _figures.Min(y => y.Y);
        public int XMax => _figures.Max(x => x.XMax);
        public int YMax => _figures.Max(y => y.YMax);

        public bool Check(int x, int y)
        {
            return _figures.Any(figure => figure.Check(x, y));
        }

        public bool IsOverlap(IFigure otherFigure)
        {
            return _figures.Any(figure => figure.IsOverlap(otherFigure));
        }
    }
}
