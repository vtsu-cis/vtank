using System;
using System.Collections.Generic;
using System.Text;
using AIFramework.Bot.Game;

namespace MinotaurPathfinder
{
    class Pathfinder : IDisposable
    {
        /*
         * 
         * Movements is an array of various directions.
         * 
         * */
        private Map map;
        Point[] _movements;
        CompleteSquare[,] _squares;
        int startingX = -1;
        int startingY = -1;
        int destinationX = -1;
        int destinationY = -1;
        private List<CompleteSquare> path;

        /*
         * 
         * Squares is an array of square objects.
         * 
         * */
        public Pathfinder(Map _map)
        {
            InitMovements(4);
            _squares = new CompleteSquare[_map.Width, _map.Height];
            map = _map;
            path = new List<CompleteSquare>();
        }

        public void InitMovements(int movementCount)
        {
            /*
             * 
             * Just do some initializations.
             * 
             * */
            if (movementCount == 4)
            {
                _movements = new Point[]
                {
                    new Point(0, -1),
                    new Point(1, 0),
                    new Point(0, 1),
                    new Point(-1, 0)
                };
            }
            else
            {
                _movements = new Point[]
                {
                    new Point(-1, -1),
                    new Point(0, -1),
                    new Point(1, -1),
                    new Point(1, 0),
                    new Point(1, 1),
                    new Point(0, 1),
                    new Point(-1, 1),
                    new Point(-1, 0)
                };
            }
        }

        public void ClearLogic()
        {
            /*
             * Reset some information about the squares.
             * */
            foreach (Point point in AllSquares())
            {
                int x = point.X;
                int y = point.Y;
                _squares[x, y].DistanceSteps = 10000;
                _squares[x, y].IsPath = false;
            }

            path.Clear();
        }

        public void HighlightPath()
        {
            /*
             * 
             * Mark the path from monster to hero.
             * 
             * */
            Point startingPoint = new Point(destinationX, destinationY);
            int pointX = startingPoint.X;
            int pointY = startingPoint.Y;
            if (pointX == -1 && pointY == -1)
            {
                return;
            }

            int marked = 0;
            while (true)
            {
                /*
                 * 
                 * Look through each direction and find the square
                 * with the lowest number of steps marked.
                 * 
                 * */
                Point lowestPoint = Point.Empty;
                int lowest = 10000;

                foreach (Point movePoint in ValidMoves(pointX, pointY))
                {
                    int count = _squares[movePoint.X, movePoint.Y].DistanceSteps;
                    if (count < lowest)
                    {
                        lowest = count;
                        lowestPoint.X = movePoint.X;
                        lowestPoint.Y = movePoint.Y;
                    }
                }
                if (lowest != 10000)
                {
                    /*
                     * 
                     * Mark the square as part of the path if it is the lowest
                     * number. Set the current position as the square with
                     * that number of steps.
                     * 
                     * */
                    _squares[lowestPoint.X, lowestPoint.Y].IsPath = true;
                    pointX = lowestPoint.X;
                    pointY = lowestPoint.Y;
                    _squares[lowestPoint.X, lowestPoint.Y].X = lowestPoint.X;
                    _squares[lowestPoint.X, lowestPoint.Y].Y = lowestPoint.Y;
                    path.Add(_squares[lowestPoint.X, lowestPoint.Y]);
                    marked++;
                }
                else
                {
                    break;
                }

                if (_squares[pointX, pointY].ContentCode == SquareContent.Starting)
                {
                    /*
                     * 
                     * We went from monster to hero, so we're finished.
                     * 
                     * */
                    break;
                }
            }

            path.Reverse();
        }

        public void Pathfind(Map map, Player player, int destinationX, int destinationY)
        {
            /*
             * 
             * Find path from hero to monster. First, get coordinates
             * of hero.
             * 
             * */
            Point startingPoint = new Point((int)player.Position.x / 64, -(int)player.Position.y / 64);
            startingX = startingPoint.X;
            startingY = startingPoint.Y;
            this.destinationX = destinationX / 64;
            this.destinationY = (destinationY / 64);

            // Initialize path.
            for (int y = 0; y < map.Height; ++y)
            {
                for (int x = 0; x < map.Width; ++x)
                {
                    SquareContent content = SquareContent.Empty;
                    Tile tile = map.GetTile(x, y);
                    if (!tile.IsPassable)
                    {
                        content = SquareContent.Wall;
                    }
                    else if (x == startingX && y == startingY)
                    {
                        content = SquareContent.Starting;
                    }

                    _squares[x, y] = new CompleteSquare()
                    {
                        X = -1, Y = -1,
                        IsPath = false,
                        ContentCode = content,
                        DistanceSteps = 10000
                    };
                }
            }

            /*
             * 
             * Hero starts at distance of 0.
             * 
             * */
            _squares[startingPoint.X, startingPoint.Y].DistanceSteps = 0;

            while (true)
            {
                bool madeProgress = false;

                /*
                 * 
                 * Look at each square on the board.
                 * 
                 * */
                foreach (Point mainPoint in AllSquares())
                {
                    int x = mainPoint.X;
                    int y = mainPoint.Y;

                    /*
                     * 
                     * If the square is open, look through valid moves given
                     * the coordinates of that square.
                     * 
                     * */
                    Tile relevantTile = map.GetTile(x, y);
                    if (relevantTile.IsPassable)
                    {
                        int passHere = _squares[x, y].DistanceSteps;

                        foreach (Point movePoint in ValidMoves(x, y))
                        {
                            int newX = movePoint.X;
                            int newY = movePoint.Y;
                            int newPass = passHere + 1;

                            if (_squares[newX, newY].DistanceSteps > newPass)
                            {
                                _squares[newX, newY].DistanceSteps = newPass;
                                madeProgress = true;
                            }
                        }
                    }
                }
                if (!madeProgress)
                {
                    break;
                }
            }

            HighlightPath();
        }

        private bool ValidCoordinates(int x, int y)
        {
            /*
             * 
             * Our coordinates are constrained between 0 and 14.
             * 
             * */
            if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
            {
                return false;
            }
            
            return true;
        }

        private IEnumerable<Point> AllSquares()
        {
            /*
             * 
             * Return every point on the board in order.
             * 
             * */
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    yield return new Point(x, y);
                }
            }
        }

        public IEnumerable<MetaCompleteSquare> FullPath()
        {
            for (int i = 0; i < path.Count; ++i)
            {
                CompleteSquare square = path[i];
                yield return new MetaCompleteSquare(square, square.X, square.Y);
            }
            /*int x = startingX, y = startingY;
            int lastX = -1, lastY = -1; // for quick checking.
            int destinationTileX = destinationX;
            int destinationTileY = destinationY;
            List<CompleteSquare> closedList = new List<CompleteSquare>();

            while (x != destinationTileX && y != destinationTileY)
            {
                bool delivered = false;
                for (int a = x - 1; a < x + 1; ++a)
                {
                    for (int b = y - 1; b < y + 1; ++b)
                    {
                        if (_squares[a, b].IsPath)
                        {
                            if (!closedList.Contains(_squares[a, b]))
                            {
                                lastX = a;
                                lastY = b;
                                closedList.Add(_squares[a, b]);
                                delivered = true;

                                yield return new MetaCompleteSquare(_squares[a, b], a, b);
                            }
                        }
                    }
                }

                if (!delivered)
                {
                    throw new Exception("No path to destination.");
                }

                x = lastX;
                y = lastY;
            }*/
        }

        private IEnumerable<Point> ValidMoves(int x, int y)
        {
            /*
             * 
             * Return each valid square we can move to.
             * 
             * */
            foreach (Point movePoint in _movements)
            {
                int newX = x + movePoint.X;
                int newY = y + movePoint.Y;

                if (ValidCoordinates(newX, newY) &&
                    map.GetTile(newX, newY).IsPassable)
                {
                    yield return new Point(newX, newY);
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            map = null;
            path.Clear();
            path = null;
            _squares = null;
        }

        #endregion
    }

    public struct Point
    {
        public int X
        {
            get;
            set;
        }

        public int Y
        {
            get;
            set;
        }

        public static Point Empty
        {
            get
            {
                return new Point(0, 0);
            }
        }

        public Point(int x, int y)
            : this()
        {
            X = x;
            Y = y;
        }
    }
}
