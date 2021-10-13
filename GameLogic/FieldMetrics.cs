using System;
using System.Collections.Generic;
using System.Linq;

namespace UglyTetris.GameLogic
{
    public class FieldMetrics
    {
        public readonly Field Field;

        public int WallThickness {get; set;} = 1;
        public int PillarDepth { get; set; } = 3;

        public FieldMetrics(Field field)
        {
            Field = field;
        }

        public HashSet<int> CalculateLinesToDelete(IFigure figure)
        {
            var linesToDelete = new HashSet<int>();
            for (int y = figure.Y; y <= figure.YMax; y++)
            {
                if (y >= Field.Ymax) break;
                bool deleteLine = true;
                for (int x = WallThickness; x <= Field.Xmax - WallThickness; x++)
                {
                    if (Field.GetTile(x, y) == null && !figure.Check(x, y))
                    {
                        deleteLine = false;
                        break;
                    }
                }

                if (deleteLine)
                {
                    linesToDelete.Add(y);
                }
            }

            return linesToDelete;
        }

        private int[] CalculateColumnHeights(IFigure figure = null, HashSet<int> linesToDelete = null)
        {
            var columnHeights = new int[Field.Xmax];
            for (int x = WallThickness; x <= Field.Xmax - WallThickness; x++)
            {
                int currentHeight = 0;
                int reduceY = 0;
                for (int y = Field.Ymax; y > 0; y--)
                {
                    if (linesToDelete != null && 
                        linesToDelete.Contains(y))
                    {
                        reduceY++;
                        continue;
                    }

                    if (Field.GetTile(x, y) != null ||
                        (figure!=null && figure.Check(x, y)))
                    {
                        currentHeight = Field.Ymax - y - reduceY;
                    }
                }

                columnHeights[x] = currentHeight;
            }

            return columnHeights;
        }

        public int CalculateHeight(IFigure figure = null, HashSet<int> linesToDelete = null)
        {
            return CalculateColumnHeights(figure, linesToDelete).Max();
        }

        private int? _memorizedVoidsCount = null;
        public int CalculateVoids(IFigure figure = null, HashSet<int> linesToDelete = null)
        {
            bool isEmptyField = figure == null && linesToDelete == null;
            if (isEmptyField && _memorizedVoidsCount.HasValue)
            {
                return _memorizedVoidsCount.Value;
            }

            int result = 0;
            var columnHeights = CalculateColumnHeights(figure, linesToDelete);
            for (int x = WallThickness; x <= Field.Xmax - WallThickness; x++)
            {
                for (int y = Field.Ymax; y > Field.Ymax - columnHeights[x] - 1; y--)
                {
                    if (linesToDelete != null &&
                        linesToDelete.Contains(y))
                    {
                        continue;
                    }

                    if (Field.GetTile(x, y) == null &&
                        (figure == null || !figure.Check(x, y)))
                    {
                        result++;
                    }
                }
            }

            if (isEmptyField)
            {
                _memorizedVoidsCount = result;
            }

            return result;
        }

        public int CalculateNewVoids(IFigure figure = null, HashSet<int> linesToDelete = null)
        {
            return CalculateVoids(figure, linesToDelete) - CalculateVoids();
        }

        public int CalculatePillars(IFigure figure = null, HashSet<int> linesToDelete = null)
        {
            var columnHeights = CalculateColumnHeights(figure, linesToDelete);
            int pillarsCount = 0;
            for (int x = WallThickness; x < Field.Xmax - WallThickness; x++)
            {
                int depth = Math.Min(columnHeights[x - 1], columnHeights[x + 1]) - columnHeights[x];
                if (depth >= PillarDepth) pillarsCount++;
            }
            return pillarsCount;
        }
    }
}
