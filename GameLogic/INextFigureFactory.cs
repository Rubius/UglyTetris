namespace UglyTetris.GameLogic
{
    public interface INextFigureFactory
    {
        Figure GetNextFigure();

        public Figure Top {get;}
    }
}