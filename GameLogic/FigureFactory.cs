using System;

namespace UglyTetris.GameLogic
{
    public enum FigureType
    {
        I,O,T,S,Z,J,L
    }

    public class FigureFactory
    {
        public Figure CreateStandardFigure(FigureType figureType)
        {
            string tiles = "";
            var nl = Environment.NewLine;
            string color;


            switch (figureType)
            {
                case FigureType.I:
                    tiles = "  x " + nl +
                            "  x " + nl +
                            "  x " + nl +
                            "  x ";
                    color = "LightSkyBlue";
                    break;

                case FigureType.O:
                    tiles = "xx" + nl +
                            "xx";
                    color = "Goldenrod";
                    break;

                case FigureType.T:
                    tiles = "   " + nl +
                            "xxx" + nl +
                            " x ";
                    color = "MediumPurple";
                    break;

                case FigureType.S:
                    tiles = " x " + nl +
                            " xx" + nl +
                            "  x";
                    color = "Crimson";
                    break;

                case FigureType.Z:
                    tiles = " x " + nl +
                            "xx " + nl +
                            "x  ";
                    color = "Peru";
                    break;

                case FigureType.J:
                    tiles = "  x" + nl +
                            "  x" + nl +
                            " xx";
                    color = "DodgerBlue";
                    break;

                case FigureType.L:
                    tiles = "x  " + nl +
                            "x  " + nl +
                            "xx ";
                    color = "LimeGreen";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new Figure(tiles, color);
        }

        private Random _random = new Random();

        public Figure CreateRandomFigure()
        {
            var randomType = _random.Next(0, 7); // code smell
            return CreateStandardFigure((FigureType) randomType);
        }
    }
}
