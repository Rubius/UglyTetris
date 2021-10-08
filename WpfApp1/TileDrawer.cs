using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using UglyTetris.GameLogic;

namespace WpfApp1
{
    internal class TileDrawer
    {
        public TileDrawer(Canvas canvas)
        {
            _canvas = canvas;
        }

        /// <summary>
        /// Draws or refreshes the set of tiles on canvas, removing all other tiles from the canvas 
        /// </summary>
        /// <param name="tiles"></param>
        public void DrawTiles(IEnumerable<TileXy> tiles)
        {
            var affectedTiles = new HashSet<Tile>();

            foreach (var tileXy in tiles)
            {
                DrawTile(tileXy);
                affectedTiles.Add(tileXy.Tile);
            }

            var orphanTiles = _tileRectangleMap.Keys.Where(t => !affectedTiles.Contains(t)).ToList();
            foreach (var orphanTile in orphanTiles)
            {
                _canvas.Children.Remove(_tileRectangleMap[orphanTile]);
                _tileRectangleMap.Remove(orphanTile);
            }
        }

        /// <summary>
        /// Refreshes the tile position or draws a new one if it did not exist in the canvas
        /// </summary>
        /// <param name="tileXy"></param>
        /// <returns>Rectangle object mapped to the tile</returns>
        public Rectangle DrawTile(TileXy tileXy)
        {
            if (tileXy.Tile == null)
            {
                return null;
            }
            
            Rectangle rectangle;

            if (_tileRectangleMap.ContainsKey(tileXy.Tile))
            {
                rectangle = _tileRectangleMap[tileXy.Tile];
            }
            else
            {
                var color = (Color)ColorConverter.ConvertFromString(tileXy.Tile.Color);
                rectangle = NewRectangle(color);
                _tileRectangleMap.Add(tileXy.Tile, rectangle);
            }
            Canvas.SetLeft(rectangle, tileXy.X * FieldHelper.BlockWidth + 1);
            Canvas.SetTop(rectangle, tileXy.Y * FieldHelper.BlockHeight + 1);
            return rectangle;
        }

        /// <summary>
        /// Removes the tile from canvas
        /// </summary>
        /// <param name="tile"></param>
        public void RemoveTile(Tile tile)
        {
            if (tile == null)
            {
                return;
            }
            
            if (_tileRectangleMap.ContainsKey(tile))
            {
                var rectangle = _tileRectangleMap[tile];
                _canvas.Children.Remove(rectangle);
                _tileRectangleMap.Remove(tile);
            }
        }

        private Rectangle NewRectangle(Color color)
        {
            var r = new Rectangle
            {
                Width = FieldHelper.BlockWidth * 0.9,
                Height = FieldHelper.BlockHeight * 0.9,
                Fill = new SolidColorBrush(color)
            };
            _canvas.Children.Add(r);
            return r;
        }

        private readonly Dictionary<Tile, Rectangle> _tileRectangleMap = new Dictionary<Tile, Rectangle>();
        private readonly Canvas _canvas;
    }
}