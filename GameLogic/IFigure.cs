namespace UglyTetris.GameLogic
{
    public interface IFigure
    {
        int X { get;}
        int Y { get;}
        int XMax { get;}
        int YMax { get; }
        bool Check(int x, int y);
        bool IsOverlap(IFigure otherFigure);
    }
}
