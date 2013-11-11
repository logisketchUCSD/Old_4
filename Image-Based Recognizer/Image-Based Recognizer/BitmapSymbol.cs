using System;
using System.Collections.Generic;
using System.Text;
using Sketch;
using Utilities;
using Utilities.Matrix;
using System.Diagnostics;
using RecognitionTemplates;

namespace ImageRecognizer
{
    [Serializable]
    [DebuggerDisplay("{_name}: Hdist={_Hdist}, Mdist={_Mdist}, Tdist={_Tdist}, Ydist={_Ydist}")]
    public class BitmapSymbol : RecognitionTemplate
    {
        #region Constants & Definitions
        private int NUMOF_TOP_POLAR = 5;
        private const int YS_GL_FONT_BITMAP_BASE = 1000;
        private const double INF = 10e6;
        private const double EPSILON = 10e-6;
        private const string _SCREEN_ = "_Screen\\";
        private const string _POLAR_ = "_Polar\\";

        private double ALLOWED_ROTATION_AMOUNT = 360.0;
        private const int GRID_X_SIZE = 24;
        private const int GRID_Y_SIZE = 24;
        private const double REL_DIST_SCALE_FACTOR = 2.4;
        private const double POLAR_WEIGHT_DECAY_RATE = 0.1;
        private const double HAUSDORFF_QUANTILE = 0.94;
        #endregion


        #region Member Variables
        private string _name;
        private double _bestRotAngle;
        private double _polarYmax;
        private System.Drawing.Rectangle _Bbox;

        private List<ScreenPoint> _screenPoints;
        private List<QuantizedPoint> _screenQuantizedPoints;
        private GeneralMatrix _sMesh;
        private GeneralMatrix _sDTM;
        
        private List<PolarPoint> _polarPoints;
        private List<QuantizedPoint> _polarQuantizedPoints;
        private GeneralMatrix _pMesh;
        private GeneralMatrix _pDTM;

        private double _Hdist;
        private double _Mdist;
        private double _Tdist;
        private double _Ydist;

        string _symbolType;
        string _symbolClass;
        string _userName;
        PlatformUsed _platformUsed;
        SymbolCompleteness _completeness;
        DrawingTask _drawingTask;

        #region Results
        Dictionary<string, List<SymbolRank>> _RecoResults;
        SymbolRank _BestMatch;
        #endregion
        
        #endregion


        #region Constructors

        public BitmapSymbol()
        {
            myInit();
        }

        public BitmapSymbol(Shape shape)
            : this(shape.SubstrokesL)
        {
            _name = shape.LowercasedType;
        }

        public BitmapSymbol(List<ScreenPoint> points)
        {
            myInit();
            _screenPoints = points;
            Process();
        }

        public BitmapSymbol(BitmapSymbol S)
        {
            BitmapSymbol temp = (BitmapSymbol)S.Clone();
            this._Bbox = temp._Bbox;
            this._bestRotAngle = temp._bestRotAngle;
            this._Hdist = temp._Hdist;
            this._Mdist = temp._Mdist;
            this._name = temp._name;
            this._pDTM = temp._pDTM;
            this._pMesh = temp._pMesh;
            this._polarPoints = temp._polarPoints;
            this._polarQuantizedPoints = temp._polarQuantizedPoints;
            this._polarYmax = temp._polarYmax;
            this._RecoResults = temp._RecoResults;
            this._screenPoints = temp._screenPoints;
            this._screenQuantizedPoints = temp._screenQuantizedPoints;
            this._sDTM = temp._sDTM;
            this._sMesh = temp._sMesh;
            this._Tdist = temp._Tdist;
            this._Ydist = temp._Ydist;
            this.ALLOWED_ROTATION_AMOUNT = temp.ALLOWED_ROTATION_AMOUNT;
            this.NUMOF_TOP_POLAR = temp.NUMOF_TOP_POLAR;
        }

        public object Clone()
        {
            BitmapSymbol symbol = (BitmapSymbol)this.MemberwiseClone();
            symbol._pDTM = (GeneralMatrix)this._pDTM.Clone();
            symbol._pMesh = (GeneralMatrix)this._pMesh.Clone();
            symbol._sDTM = (GeneralMatrix)this._sDTM.Clone();
            symbol._sMesh = (GeneralMatrix)this._sMesh.Clone();

            for (int i = 0; i < symbol._polarPoints.Count; i++)
                symbol._polarPoints[i] = (PolarPoint)this._polarPoints[i].Clone();
            for (int i = 0; i < symbol._screenPoints.Count; i++)
                symbol._screenPoints[i] = (ScreenPoint)this._screenPoints[i].Clone();
            for (int i = 0; i < symbol._polarQuantizedPoints.Count; i++)
                symbol._polarQuantizedPoints[i] = (QuantizedPoint)this._polarQuantizedPoints[i].Clone();
            for (int i = 0; i < symbol._screenQuantizedPoints.Count; i++)
                symbol._screenQuantizedPoints[i] = (QuantizedPoint)this._screenQuantizedPoints[i].Clone();

            return symbol;
        }

        public BitmapSymbol(List<Substroke> strokes)
            : base(strokes[0].FirstLabel)
        {
            myInit();
            foreach (Substroke s in strokes)
            {
                for (int i = 0; i < s.PointsL.Count - 1; i++)
                    AddPoint(s.PointsL[i], 0);
                AddPoint(s.PointsL[s.PointsL.Count - 1], 1); 
            }
            Process();
        }

        public BitmapSymbol(Substroke stroke, System.Drawing.Rectangle shapeBBox)
        {
            myInit();

            for (int i = 0; i < stroke.PointsL.Count - 1; i++)
                AddPoint(stroke.PointsL[i], 0);
            AddPoint(stroke.PointsL[stroke.PointsL.Count - 1], 1);

            Process(shapeBBox);
        }

        public BitmapSymbol(List<Point> points)
        {
            myInit();
            for (int i = 0; i < points.Count - 1; i++)
                AddPoint(points[i], 0);
            AddPoint(points[points.Count - 1], 1); 
            Process();

        }

        public BitmapSymbol(Point[] points)
        {
            myInit();
            for (int i = 0; i < points.Length - 1; i++)
                AddPoint(points[i], 0);
            AddPoint(points[points.Length - 1], 1);
            Process();
        }

        private void myInit()
        {
            _name = "unknown";
            _bestRotAngle = 0.0;
            _screenPoints = new List<ScreenPoint>();
            _screenQuantizedPoints = new List<QuantizedPoint>();
            _sMesh = new GeneralMatrix(GRID_Y_SIZE, GRID_X_SIZE, 0.0);
            _sDTM = new GeneralMatrix(GRID_Y_SIZE, GRID_X_SIZE, INF);
            _polarPoints = new List<PolarPoint>();
            _polarQuantizedPoints = new List<QuantizedPoint>();
            _pMesh = new GeneralMatrix(GRID_Y_SIZE, GRID_X_SIZE, 0.0);
            _pDTM = new GeneralMatrix(GRID_Y_SIZE, GRID_X_SIZE, INF);
            _Bbox = new System.Drawing.Rectangle();
            _polarYmax = 0.0;
            _Hdist = _Mdist = _Tdist = _Ydist = 0.0;
            _RecoResults = new Dictionary<string, List<SymbolRank>>();
        }

        #endregion


        #region Functions

        private void AddPoint(Point pt, int isEnd)
        {
            ScreenPoint p = new ScreenPoint(pt.X, pt.Y, isEnd);
            _screenPoints.Add(p);
        }

        public void Process()
        {
            findBBoxProperties();
            Process_Screen();
            Process_Polar();
        }

        public void Process(System.Drawing.Rectangle shapeBBox)
        {
            _Bbox = shapeBBox;
            Process_Screen();
            Process_Polar();
        }

        private void Process_Screen()
        {
            QuantizePoints(_screenPoints, ref _sMesh);
            ExtractQuantizedPointIndices(_sMesh, ref _screenQuantizedPoints);
            doDTMscreen();
        }

        private void Process_Polar()
        {
            Transform_Screen2Polar(_screenPoints, ref _polarPoints);
            QuantizePoints(_polarPoints, ref _pMesh);
            ExtractQuantizedPointIndices(_pMesh, ref _polarQuantizedPoints);
            doDTMpolar();
            _polarYmax = Ymax(_polarQuantizedPoints);
        }

        private void Clear()
        {
            _name = "";

            _screenPoints.Clear();
            _screenQuantizedPoints.Clear();
            _sMesh.Dispose();
            _sDTM.Dispose();

            _polarPoints.Clear();
            _polarQuantizedPoints.Clear();
            _pMesh.Dispose();
            _pDTM.Dispose();
        }

        public void Output(string filename)
        {
            // Check to see whether directories exist, if not then create them
            if (!System.IO.Directory.Exists(_SCREEN_))
                System.IO.Directory.CreateDirectory(_SCREEN_);
            if (!System.IO.Directory.Exists(_POLAR_))
                System.IO.Directory.CreateDirectory(_POLAR_);

            // Write out screen info
            System.IO.StreamWriter writer = new System.IO.StreamWriter(_SCREEN_ + filename);

            // Write out points
            writer.WriteLine(_screenPoints.Count.ToString());
            for (int i = 0; i < _screenPoints.Count; i++)
            {
                string line = _screenPoints[i].X.ToString("#0") + " "
                    + _screenPoints[i].Y.ToString("#0") + " "
                    + _screenPoints[i].IsEndofStroke.ToString("#0");
                writer.WriteLine(line);
            }

            writer.WriteLine(); writer.WriteLine();

            // Write out quantized points
            writer.WriteLine(_screenQuantizedPoints.Count.ToString());
            for (int i = 0; i < _screenQuantizedPoints.Count; i++)
            {
                string line = _screenQuantizedPoints[i].X.ToString("#0") + " "
                    + _screenQuantizedPoints[i].Y.ToString("#0");
                writer.WriteLine(line);
            }

            writer.WriteLine(); writer.WriteLine();
            writer.WriteLine(); writer.WriteLine();

            //have to printout sDTM matrix reversed in the y direction because Matrix(0,0) is at the top left
            //whereas usual x-y coordinates in RelDist/angle coordinate system has the origin at bottom left.
            writer.WriteLine(_sDTM.RowDimension.ToString() + " " + _sDTM.ColumnDimension.ToString());
            for (int i = _sDTM.RowDimension - 1; i >= 0; i--)
            {
                for (int j = 0; j < _sDTM.ColumnDimension; j++)
                    writer.Write(_sDTM.GetElement(i, j) + "  ");
                writer.WriteLine();
            }
            
            writer.Close();



            // Do the same for the polar
            writer = new System.IO.StreamWriter(_POLAR_ + filename);

            // Write out points
            writer.WriteLine(_polarPoints.Count.ToString());
            for (int i = 0; i < _polarPoints.Count; i++)
            {
                string line = _polarPoints[i].AngularPosition.ToString("#0.00000") + " "
                    + _polarPoints[i].RelativeDistance.ToString("#0.00000");
                writer.WriteLine(line);
            }

            writer.WriteLine(); writer.WriteLine();

            // Write out quantized points
            writer.WriteLine(_polarQuantizedPoints.Count.ToString());
            for (int i = 0; i < _polarQuantizedPoints.Count; i++)
            {
                string line = _polarQuantizedPoints[i].X.ToString("#0") + " "
                    + _polarQuantizedPoints[i].Y.ToString("#0");
                writer.WriteLine(line);
            }

            writer.WriteLine(); writer.WriteLine();
            writer.WriteLine(); writer.WriteLine();

            //have to printout pDTM matrix reversed in the y direction because Matrix(0,0) is at the top left
            //whereas usual x-y coordinates in RelDist/angle coordinate system has the origin at bottom left.
            writer.WriteLine(_pDTM.RowDimension.ToString() + " " + _pDTM.ColumnDimension.ToString());
            for (int i = _pDTM.RowDimension - 1; i >= 0; i--)
            {
                for (int j = 0; j < _pDTM.ColumnDimension; j++)
                    writer.Write(_pDTM.GetElement(i, j) + "  ");
                writer.WriteLine();
            }

            writer.Close();
        }

        public void Input(string filename, string folder)
        {
            Clear();
            _name = filename;

            System.IO.StreamReader reader;

            // Read in all Screen information
            if (folder != "")
                reader = new System.IO.StreamReader(folder + _SCREEN_ + filename);
            else
                reader = new System.IO.StreamReader(_SCREEN_ + filename);


            // Read Screen points
            string line = reader.ReadLine();
            int N = Convert.ToInt32(line);
            for (int i = 0; i < N; i++)
            {
                line = reader.ReadLine();
                string[] nums = line.Split(" ".ToCharArray());
                List<string> numsL = new List<string>(nums);
                numsL.RemoveAll(IsEmptyString);
                double x = Convert.ToDouble(numsL[0]);
                double y = Convert.ToDouble(numsL[1]);
                int isEnd = Convert.ToInt32(numsL[2]);
                _screenPoints.Add(new ScreenPoint(x, y, isEnd));
            }
            _screenQuantizedPoints.Clear();
            line = reader.ReadLine(); line = reader.ReadLine();

            // Read Quantized Points
            line = reader.ReadLine();
            N = Convert.ToInt32(line);
            for (int i = 0; i < N; i++)
            {
                line = reader.ReadLine();
                string[] nums = line.Split(" ".ToCharArray());
                List<string> numsL = new List<string>(nums);
                numsL.RemoveAll(IsEmptyString);
                int x = Convert.ToInt32(numsL[0]);
                int y = Convert.ToInt32(numsL[1]);
                _screenQuantizedPoints.Add(new QuantizedPoint(x, y));
            }

            line = reader.ReadLine(); line = reader.ReadLine();
            line = reader.ReadLine(); line = reader.ReadLine();

            // Read in the sDTM matrix
            //have to readin sDTM matrix reversed in the y direction because Matrix(0,0) is at the top left
            //whereas usual x-y coordinates in RelDist/angle coordinate system has the origin at bottom left.
            line = reader.ReadLine();
            string[] dims = line.Split(" ".ToCharArray());
            int columns = Convert.ToInt32(dims[0]);
            int rows = Convert.ToInt32(dims[1]);
            _sDTM = new GeneralMatrix(columns, rows, INF);
            for (int i = columns - 1; i >= 0; i--)
            {
                line = reader.ReadLine();
                string[] nums = line.Split(" ".ToCharArray());
                List<string> numsL = new List<string>(nums);
                numsL.RemoveAll(IsEmptyString);
                for (int j = 0; j < rows; j++)
                    _sDTM.SetElement(i, j, Convert.ToDouble(numsL[j]));
            }

            reader.Close();

            // Read in all Screen information
            reader = new System.IO.StreamReader(folder + _POLAR_ + filename);


            // Read Polar points
            line = reader.ReadLine();
            N = Convert.ToInt32(line);
            for (int i = 0; i < N; i++)
            {
                line = reader.ReadLine();
                string[] nums = line.Split(" ".ToCharArray());
                List<string> numsL = new List<string>(nums);
                numsL.RemoveAll(IsEmptyString);
                double angPos = Convert.ToDouble(numsL[0]);
                double relDis = Convert.ToDouble(numsL[1]);
                _polarPoints.Add(new PolarPoint(angPos, relDis));
            }
            _polarQuantizedPoints.Clear();
            line = reader.ReadLine(); line = reader.ReadLine();

            // Read Quantized Points
            line = reader.ReadLine();
            N = Convert.ToInt32(line);
            for (int i = 0; i < N; i++)
            {
                line = reader.ReadLine();
                string[] nums = line.Split(" ".ToCharArray());
                List<string> numsL = new List<string>(nums);
                numsL.RemoveAll(IsEmptyString);
                int x = Convert.ToInt32(numsL[0]);
                int y = Convert.ToInt32(numsL[1]);
                _polarQuantizedPoints.Add(new QuantizedPoint(x, y));
            }

            line = reader.ReadLine(); line = reader.ReadLine();
            line = reader.ReadLine(); line = reader.ReadLine();

            // Read in the sDTM matrix
            //have to readin sDTM matrix reversed in the y direction because Matrix(0,0) is at the top left
            //whereas usual x-y coordinates in RelDist/angle coordinate system has the origin at bottom left.
            line = reader.ReadLine();
            dims = line.Split(" ".ToCharArray());
            columns = Convert.ToInt32(dims[0]);
            rows = Convert.ToInt32(dims[1]);
            _pDTM = new GeneralMatrix(columns, rows, INF);
            for (int i = columns - 1; i >= 0; i--)
            {
                line = reader.ReadLine();
                string[] nums = line.Split(" ".ToCharArray());
                List<string> numsL = new List<string>(nums);
                numsL.RemoveAll(IsEmptyString);
                for (int j = 0; j < rows; j++)
                    _pDTM.SetElement(i, j, Convert.ToDouble(numsL[j]));
            }

            reader.Close();

            _polarYmax = Ymax(_polarQuantizedPoints);
        }

        public bool IsEmptyString(string str)
        {
            if (str == "")
                return true;
            else
                return false;
        }

        public void Refresh(string filename)
        {
            Clear();
            int N = 0;

            System.IO.StreamReader reader = new System.IO.StreamReader(filename);

            string line = reader.ReadLine();
            N = Convert.ToInt16(line);

            for (int i = 0; i < N; i++)
            {
                line = reader.ReadLine();
                string[] nums = line.Split(" ".ToCharArray());
                ScreenPoint pt = new ScreenPoint(Convert.ToInt32(nums[0]), Convert.ToInt32(nums[1]), Convert.ToInt32(nums[2]));
                _screenPoints.Add(pt);
            }

            reader.Close();

            Process();
            Output(filename);
        }

        private void findBBoxProperties()
        {
            double xmax, xmin, ymax, ymin;
            BoundingBox(_screenPoints, out xmax, out xmin, out ymax, out ymin);

            _Bbox = new System.Drawing.Rectangle((int)xmin, (int)ymin, (int)(xmax - xmin), (int)(ymax - ymin));
        }

        private void BoundingBox(List<ScreenPoint> SP, out double xmax, out double xmin, out double ymax, out double ymin)
        {
            xmax = ymax = -INF;
            xmin = ymin = INF;

            for (int i = 0; i < SP.Count; i++)
            {
                xmax = Math.Max(xmax, SP[i].X);
                ymax = Math.Max(ymax, SP[i].Y);
                xmin = Math.Min(xmin, SP[i].X);
                ymin = Math.Min(ymin, SP[i].Y);
            }
        }

        private void QuantizePoints(List<ScreenPoint> points, ref GeneralMatrix mesh)
        {
            if (points.Count == 0) return;
            
            double sq_side = Math.Max(_Bbox.Height, _Bbox.Width);

            double stepX = sq_side / (double)(GRID_X_SIZE - 1);
            double stepY = sq_side / (double)(GRID_Y_SIZE - 1);

            double cx = (double)(_Bbox.Left + _Bbox.Right) / 2.0;
            double cy = (double)(_Bbox.Top + _Bbox.Bottom) / 2.0;

            mesh = new GeneralMatrix(GRID_Y_SIZE, GRID_X_SIZE, 0.0);

            for (int i = 0; i < points.Count; i++)
            {
                int x_index = (int)Math.Floor(((points[i].X - cx) / stepX) + GRID_X_SIZE / 2);
                int y_index = (int)Math.Floor(((points[i].Y - cy) / stepY) + GRID_Y_SIZE / 2);
                
                if (x_index < 0)
                    x_index = 0;
                else if (x_index >= GRID_X_SIZE)
                    x_index = GRID_X_SIZE - 1;

                if (y_index < 0)
                    y_index = 0;
                else if (y_index >= GRID_Y_SIZE)
                    y_index = GRID_Y_SIZE - 1;

                mesh.SetElement(y_index, x_index, 1.0);
            }
        }

        private void QuantizePoints(List<PolarPoint> points, ref GeneralMatrix mesh)
        {
            if (points.Count == 0) return;

            mesh = new GeneralMatrix(GRID_Y_SIZE, GRID_X_SIZE, 0.0);

            double stepX = 2.0 * Math.PI / GRID_X_SIZE;
            double stepY = REL_DIST_SCALE_FACTOR / GRID_Y_SIZE;

            for (int i = 0; i < points.Count; i++)
            {
                double x = points[i].AngularPosition;
                double y = points[i].RelativeDistance;

                int x_index = (int)Math.Floor(((x + Math.PI) / stepX));
                int y_index = (int)Math.Floor((y / stepY));

                if (y_index < GRID_Y_SIZE && x_index < GRID_X_SIZE)
                    mesh.SetElement(y_index, x_index, 1.0);
            }
        }

        private void ExtractQuantizedPointIndices(GeneralMatrix mesh, ref List<QuantizedPoint> qPoints)
        {
            qPoints.Clear();

            for (int i = 0; i < GRID_Y_SIZE; i++)
            {
                for (int j = 0; j < GRID_X_SIZE; j++)
                {
                    if (mesh.GetElement(i, j) > 0.0)
                    {
                        qPoints.Add(new QuantizedPoint(j, i));
                    }
                }
            }
        }

        private void Transform_Screen2Polar(List<ScreenPoint> sPoints, ref List<PolarPoint> pPoints)
        {
            Coord center = Center(sPoints);

            double average_distance = 0.0;

            for (int i = 0; i < sPoints.Count; i++)
                average_distance += EuclideanDistance(sPoints[i].X, sPoints[i].Y, center.X, center.Y);

            average_distance /= sPoints.Count;

            pPoints.Clear();
            for (int i = 0; i < sPoints.Count; i++)
            {
                double angPosition = Math.Atan2(sPoints[i].Y - center.Y, sPoints[i].X - center.X);
                double relDistance = EuclideanDistance(sPoints[i].X, sPoints[i].Y, center.X, center.Y) / average_distance;
                pPoints.Add(new PolarPoint(angPosition, relDistance));
            }
        }

        private Coord Center(List<ScreenPoint> points)
        {
            double cx = 0.0;
            double cy = 0.0;

            if (points.Count == 0) return new Coord();

            double length = Length(points);

            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].IsEndofStroke == 1) continue;
                double x1 = points[i].X;
                double y1 = points[i].Y;
                double x2 = points[i + 1].X;
                double y2 = points[i + 1].Y;
                double w = EuclideanDistance(x1, y1, x2, y2) / length;
                double x = (x1 + x2) / 2.0;
                double y = (y1 + y2) / 2.0;

                cx += x * w;
                cy += y * w;
            }

            return new Coord(cx, cy);
        }

        private double Length(List<ScreenPoint> points)
        {
            double len = 0.0;

            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].IsEndofStroke == 0)
                    len += EuclideanDistance(points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y);
            }

            return len;
        }

        private double EuclideanDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        private double Ymax(List<QuantizedPoint> points)
        {
            double max = -INF;

            foreach (QuantizedPoint pt in points)
                max = Math.Max(max, pt.Y);

            return max;
        }

        private void doDTMscreen()
        {
            List<Coord> indices = new List<Coord>();
            _sDTM = new GeneralMatrix(GRID_Y_SIZE, GRID_X_SIZE, INF);

            for (int i = 0; i < GRID_Y_SIZE; i++)
            {
                for (int j = 0; j < GRID_X_SIZE; j++)
                {
                    if (_sMesh.GetElement(i, j) > 0.0)
                        indices.Add(new Coord(j, i));
                }
            }

            for (int i = 0; i < GRID_Y_SIZE; i++)
            {
                for (int j = 0; j < GRID_X_SIZE; j++)
                {
                    Coord cp = new Coord(j, i);

                    double mindist = INF;

                    for (int c = 0; c < indices.Count; c++)
                    {
                        double distan = cp.distanceTo(indices[c]);
                        if (distan < mindist)
                            mindist = distan;
                    }
                    _sDTM.SetElement(i, j, mindist);
                }
            }
        }

        private void doDTMpolar()
        {
            List<Coord> indices = new List<Coord>();
            _pDTM = new GeneralMatrix(GRID_Y_SIZE, GRID_X_SIZE, INF);

            for (int i = 0; i < GRID_Y_SIZE; i++)
            {
                for (int j = 0; j < GRID_X_SIZE; j++)
                {
                    if (_pMesh.GetElement(i, j) > 0.0)
                        indices.Add(new Coord(j, i));
                }
            }

            for (int i = 0; i < GRID_Y_SIZE; i++)
            {
                for (int j = 0; j < GRID_X_SIZE; j++)
                {
                    Coord cp = new Coord(j, i);

                    double mindist = INF;

                    for (int c = 0; c < indices.Count; c++)
                    {
                        double dx = Math.Abs(cp.X - indices[c].X);
                        double dy = Math.Abs(cp.Y - indices[c].Y);
                        double distan1 = Math.Sqrt(dx * dx + dy * dy);
                        dx = (double)GRID_X_SIZE - dx;
                        double distan2 = Math.Sqrt(dx * dx + dy * dy);
                        double distan = Math.Min(distan1, distan2);
                        if (distan < mindist)
                            mindist = distan;
                    }
                    _pDTM.SetElement(i, j, mindist);
                }
            }
        }

        #endregion


        #region Computations

        private double Polar_Mean_Distance(BitmapSymbol S)
        {
            int lower_rot_index;
            if (0.0 <= ALLOWED_ROTATION_AMOUNT && ALLOWED_ROTATION_AMOUNT <= 360.0) //check valid ALLOWED_ROTATION_AMOUNT
                lower_rot_index = (int)Math.Floor(ALLOWED_ROTATION_AMOUNT * GRID_X_SIZE / 2.0 / 360.0); //trick
            else
                lower_rot_index = (int)Math.Floor(GRID_X_SIZE / 2.0); //default to full rotation

            int upper_rot_index = GRID_X_SIZE - lower_rot_index; //trick

            List<QuantizedPoint> A = new List<QuantizedPoint>(_polarQuantizedPoints);
            GeneralMatrix A_DTM = new GeneralMatrix(_pDTM.ArrayCopy);
            double Aymax = _polarYmax;
            
            List<QuantizedPoint> B = new List<QuantizedPoint>(S._polarQuantizedPoints);
            GeneralMatrix B_DTM = new GeneralMatrix(S._pDTM.ArrayCopy);
            double Bymax = S._polarYmax;

            if (A.Count == 0 || B.Count == 0) return INF;

            double minA2B = INF;
            double distan = 0.0;
            int besti = 0;
            for (int i = 0; i < GRID_X_SIZE; i++)
            {
                distan = 0.0;
                for (int a = 0; a < A.Count; a++)
                {
                    int y_ind = A[a].Y;
                    int x_ind = A[a].X + i;
                    if (x_ind >= GRID_X_SIZE) x_ind -= GRID_X_SIZE;
                    //putting less weight on points that have small rel dist - y 
                    double weight = Math.Pow((double)y_ind / Aymax, POLAR_WEIGHT_DECAY_RATE);
                    distan += B_DTM.GetElement(y_ind, x_ind) * weight;
                }

                if (distan < minA2B && (i <= lower_rot_index || i >= upper_rot_index))
                {
                    minA2B = distan;
                    besti = i;
                }
            }

            _bestRotAngle = besti * 2.0 * Math.PI / (double)GRID_X_SIZE;

            distan = 0.0;
            for (int b = 0; b < B.Count; b++)
            {
                int y_ind = B[b].Y;
                int x_ind = B[b].X - besti; // slide B back by besti
                if (x_ind < 0) x_ind += GRID_X_SIZE;
                double weight = Math.Pow((double)y_ind / Bymax, POLAR_WEIGHT_DECAY_RATE);
                distan += A_DTM.GetElement(y_ind, x_ind) * weight;
            }
            double minB2A = distan;

            return Math.Max(minA2B / (double)A.Count, minB2A / (double)B.Count);
        }

        private double Hausdorff_Distance(BitmapSymbol S)
        {
            double hAB = directed_hauss(_screenQuantizedPoints, S._sDTM, HAUSDORFF_QUANTILE);
            double hBA = directed_hauss(S._screenQuantizedPoints, _sDTM, HAUSDORFF_QUANTILE);

            return Math.Max(hAB, hBA);
        }

        private double directed_hauss(List<QuantizedPoint> ModelPoints, GeneralMatrix ImageDTM, double quantile)
        {
            List<double> distances = new List<double>(ModelPoints.Count);
            for (int m = 0; m < ModelPoints.Count; m++)
            {
                double distan = ImageDTM.GetElement(ModelPoints[m].Y, ModelPoints[m].X);
                distances.Add(distan);
            }

            distances.Sort();
            if (distances.Count == 0) return INF;

            return distances[(int)Math.Floor(((distances.Count - 1) * quantile))];
        }

        private double Mean_Pixel_Distance(BitmapSymbol S)
        {
            List<QuantizedPoint> A = new List<QuantizedPoint>(_screenQuantizedPoints);
            GeneralMatrix A_DTM = new GeneralMatrix(_sDTM.ArrayCopy);

            List<QuantizedPoint> B = new List<QuantizedPoint>(S._screenQuantizedPoints);
            GeneralMatrix B_DTM = new GeneralMatrix(S._sDTM.ArrayCopy);

            double distan = 0.0;
            for (int i = 0; i < A.Count; i++)
                distan += B_DTM.GetElement(A[i].Y, A[i].X);

            double AB = distan / A.Count;

            distan = 0.0;
            for (int i = 0; i < B.Count; i++)
                distan += A_DTM.GetElement(B[i].Y, B[i].X);

            double BA = distan / B.Count;

            return Math.Max(AB, BA);
        }

        private double Tanimoto_Distance(BitmapSymbol S)
        {
            int a, b, c, d;
            a = b = c = d = 0;
            double E = 1.0 / 15.0 * Math.Sqrt(Math.Pow((double)GRID_X_SIZE, 2.0) + Math.Pow((double)GRID_Y_SIZE, 2.0));

            for (int i = 0; i < _sDTM.ColumnDimension; i++)
            {
                for (int j = 0; j < _sDTM.RowDimension; j++)
                {
                    if (_sDTM.GetElement(i, j) < E)
                        a++;

                    if (S._sDTM.GetElement(i, j) < E)
                        b++;

                    if (_sDTM.GetElement(i, j) < E && S._sDTM.GetElement(i, j) < E)
                        c++;

                    if (_sDTM.GetElement(i, j) >= E && S._sDTM.GetElement(i, j) >= E)
                        d++;
                }
            }

            double T1 = (double)c / (double)(a + b - c);
            double T0 = (double)d / (double)(a + b - 2 * c + d);
            double p = (double)(a + b) / 2.0 / _sDTM.ColumnDimension / _sDTM.RowDimension;
            double alpha = (3.0 - p) / 4.0;

            return 1.0 - (alpha * T1 + (1.0 - alpha) * T0);
        }

        private double Yule_Distance(BitmapSymbol S)
        {
            int n10, n01, n11, n00;
            n10 = n01 = n11 = n00 = 0;
            double E = 1.0 / 15.0 * Math.Sqrt(Math.Pow((double)GRID_X_SIZE, 2.0) + Math.Pow((double)GRID_Y_SIZE, 2.0));

            for (int i = 0; i < _sDTM.ColumnDimension; i++)
            {
                for (int j = 0; j < _sDTM.RowDimension; j++)
                {
                    if (_sDTM.GetElement(i, j) < E && S._sDTM.GetElement(i, j) >= E)
                        n10++;

                    if (_sDTM.GetElement(i, j) >= E && S._sDTM.GetElement(i, j) < E)
                        n01++;

                    if (_sDTM.GetElement(i, j) < E && S._sDTM.GetElement(i, j) < E)
                        n11++;

                    if (_sDTM.GetElement(i, j) >= E && S._sDTM.GetElement(i, j) >= E)
                        n00++;
                }
            }

            return 1.0 - (double)(n11 * n00 - n10 * n01) / (double)(n11 * n00 + n10 * n01);
        }

        #endregion


        #region Getters & Setters

        public string SymbolType
        {
            get 
            {
                if (_symbolType == null)
                    return "Unknown";
                else
                    return _symbolType; 
            }
            set { _symbolType = value; }
        }

        public string SymbolClass
        {
            get 
            {
                if (_symbolClass == null)
                    return "Unknown";
                else
                    return _symbolClass; 
            }
            set { _symbolClass = value; }
        }

        public string UserName
        {
            get 
            {
                if (_userName == null)
                    return "Unknown";
                else
                    return _userName; 
            }
            set { _userName = value; }
        }

        public PlatformUsed Platform
        {
            get { return _platformUsed; }
            set { _platformUsed = value; }
        }

        public SymbolCompleteness Completeness
        {
            get { return _completeness; }
            set { _completeness = value; }
        }

        public DrawingTask DrawTask
        {
            get { return _drawingTask; }
            set { _drawingTask = value; }
        }

        public double HausdorffDistance
        {
            get { return _Hdist; }
        }

        public double ModifiedHausdorffDistance
        {
            get { return _Mdist; }
        }

        public double TanimotoCoefficient
        {
            get { return _Tdist; }
        }

        public double YuleCoefficient
        {
            get { return _Ydist; }
        }

        public double BestRotationAngle
        {
            get { return _bestRotAngle; }
        }

        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public ScreenPoint BeginP
        {
            get { return _screenPoints[0]; }
        }

        public ScreenPoint EndP
        {
            get { return _screenPoints[_screenPoints.Count - 1]; }
        }

        public System.Drawing.Rectangle BBox
        {
            get { return _Bbox; }
        }

        public bool noInk
        {
            get
            {
                if (_screenPoints.Count <= 6) //6 or fewer points is considered no ink
                {
                    Clear();
                    return true;
                }
                else
                    return false;
            }
        }

        public Dictionary<string, List<SymbolRank>> RecoResults
        {
            get { return _RecoResults; }
        }

        public SymbolRank BestMatch
        {
            get { return _BestMatch; }
        }

        public List<System.Drawing.Point[]> Points
        {
            get 
            {
                List<System.Drawing.Point[]> strokes = new List<System.Drawing.Point[]>();

                List<System.Drawing.Point> points = new List<System.Drawing.Point>();
                for (int i = 0; i < _screenPoints.Count; i++)
                {
                    points.Add(new System.Drawing.Point((int)_screenPoints[i].X, (int)_screenPoints[i].Y));

                    if (_screenPoints[i].IsEndofStroke == 1)
                    {
                        System.Drawing.Point[] ptArray = points.ToArray();
                        strokes.Add(ptArray);
                        points.Clear();
                    }
                }

                return strokes;
            }
        }

        #endregion


        #region Spatial Operations

        private void Translate(double dx, double dy)
        {
            Translate(ref _screenPoints, dx, dy);

            findBBoxProperties();
        }

        private void Translate(ref List<ScreenPoint> points, double dx, double dy)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i].X += dx;
                points[i].Y += dy;
            }
        }

        private void MoveTo(double x, double y)
        {
            Coord center = Center(_screenPoints);
            Translate(ref _screenPoints, x - center.X, y - center.Y);
            findBBoxProperties();
        }

        private void Rotate(double theta)
        {
            Coord center = Center(_screenPoints);
            Rotate(ref _screenPoints, theta, center.X, center.Y);
            //findBBoxProperties();
        }

        private void Rotate(double theta, double ox, double oy)
        {
            Rotate(ref _screenPoints, theta, ox, oy);
            findBBoxProperties();
        }

        private void Rotate(ref List<ScreenPoint> points, double theta, double ox, double oy)
        {
            double sinus = Math.Sin(theta);
            double cosinus = Math.Cos(theta);

            for (int i = 0; i < points.Count; i++)
            {
                double cx = points[i].X - ox;
                double cy = points[i].Y - oy;

                points[i].X = cx * cosinus - cy * sinus;
                points[i].Y = cx * sinus + cy * cosinus;
                points[i].X += ox;
                points[i].Y += oy;
            }
        }

        private void Scale(double sx, double sy)
        {
            Coord center = Center(_screenPoints);
            Scale(ref _screenPoints, sx, sy, center.X, center.Y);
            findBBoxProperties();
        }

        private void Scale(double sx, double sy, double ox, double oy)
        {
            Scale(ref _screenPoints, sx, sy, ox, oy);
            findBBoxProperties();
        }

        private void Scale(ref List<ScreenPoint> points, double sx, double sy, double ox, double oy)
        {
            for (int i = 0; i < points.Count; i++)
            {
                double cx = points[i].X - ox;
                double cy = points[i].Y - oy;

                points[i].X = cx * sx;
                points[i].Y = cy * sy;
                points[i].X += ox;
                points[i].Y += oy;
            }
        }

        #endregion


        #region Recognition and Ranking

        private double PercentLoc(double min, double max, double val)
        {
            if (Math.Abs(max - min) < EPSILON) return INF;

            return (val - min) / (max - min) * 100.0;
        }

        public List<string> FindSimilarity_and_Rank(List<BitmapSymbol> defns, out Dictionary<string, List<SymbolRank>> SRs)
        {
            List<string> Ranked_Symbols = new List<string>();

            if (defns.Count == 0)
            {
                Ranked_Symbols.Add("-----");
                SRs = this.RecoResults;
                return Ranked_Symbols;
            }
            if (defns.Count == 1)
            {
                Ranked_Symbols.Add(defns[0].Name);
                SRs = this.RecoResults;
                return Ranked_Symbols;
            }
            if (defns.Count <= NUMOF_TOP_POLAR)
                NUMOF_TOP_POLAR = defns.Count;

            doPolarRecognition(this, defns);
            doScreenRecognition(this);
            Ranked_Symbols = doFusion();

            SRs = this.RecoResults;

            return Ranked_Symbols;
        }

        private void doPolarRecognition(BitmapSymbol unknownSymbol, List<BitmapSymbol> defns)
        {
            string resultClass = "Polar";
            if (_RecoResults.ContainsKey(resultClass))
                _RecoResults.Remove(resultClass);

            unknownSymbol.Process_Polar();

            List<SymbolRank> polar_results = new List<SymbolRank>();

            for (int i = 0; i < defns.Count; i++)
            {
                SymbolRank SR = new SymbolRank(defns[i].Polar_Mean_Distance(unknownSymbol), defns[i]);
                polar_results.Add(SR);
            }

            polar_results.Sort(delegate(SymbolRank SR1, SymbolRank SR2) { return SR1.Distance.CompareTo(SR2.Distance); });

            _RecoResults.Add(resultClass, polar_results);
        }

        private void doScreenRecognition(BitmapSymbol unknownSymbol)
        {
            List<SymbolRank> hauss_results = new List<SymbolRank>();
            List<SymbolRank> meand_results = new List<SymbolRank>();
            List<SymbolRank> tanim_results = new List<SymbolRank>();
            List<SymbolRank> yuled_results = new List<SymbolRank>();

            for (int i = 0; i < NUMOF_TOP_POLAR; i++)
            {
                BitmapSymbol defnS = _RecoResults["Polar"][i].Symbol;
                double alpha = defnS.BestRotationAngle;

                unknownSymbol.Rotate(-alpha);

                unknownSymbol.Process_Screen();

                SymbolRank SR = new SymbolRank(defnS.Hausdorff_Distance(unknownSymbol), defnS);
                hauss_results.Add(SR);
                defnS._Hdist = SR.Distance;

                SR = new SymbolRank(defnS.Mean_Pixel_Distance(unknownSymbol), defnS);
                meand_results.Add(SR);
                defnS._Mdist = SR.Distance;

                SR = new SymbolRank(defnS.Tanimoto_Distance(unknownSymbol), defnS);
                tanim_results.Add(SR);
                defnS._Tdist = SR.Distance;

                SR = new SymbolRank(defnS.Yule_Distance(unknownSymbol), defnS);
                yuled_results.Add(SR);
                defnS._Ydist = SR.Distance;

                unknownSymbol.Rotate(alpha);
            }

            hauss_results.Sort(delegate(SymbolRank SR1, SymbolRank SR2) { return SR1.Distance.CompareTo(SR2.Distance); });
            meand_results.Sort(delegate(SymbolRank SR1, SymbolRank SR2) { return SR1.Distance.CompareTo(SR2.Distance); });
            tanim_results.Sort(delegate(SymbolRank SR1, SymbolRank SR2) { return SR1.Distance.CompareTo(SR2.Distance); });
            yuled_results.Sort(delegate(SymbolRank SR1, SymbolRank SR2) { return SR1.Distance.CompareTo(SR2.Distance); });

            string resultClass = "Hauss";
            if (_RecoResults.ContainsKey(resultClass))
                _RecoResults.Remove(resultClass);
            _RecoResults.Add(resultClass, hauss_results);

            resultClass = "MeanD";
            if (_RecoResults.ContainsKey(resultClass))
                _RecoResults.Remove(resultClass);
            _RecoResults.Add(resultClass, meand_results);

            resultClass = "Tanim";
            if (_RecoResults.ContainsKey(resultClass))
                _RecoResults.Remove(resultClass);
            _RecoResults.Add(resultClass, tanim_results);

            resultClass = "YuleD";
            if (_RecoResults.ContainsKey(resultClass))
                _RecoResults.Remove(resultClass);
            _RecoResults.Add(resultClass, yuled_results);
        }

        private List<string> doFusion()
        {
            List<SymbolRank> fusio_results = new List<SymbolRank>();
            _BestMatch = new SymbolRank();

            double Hmin = _RecoResults["Hauss"][0].Distance;
            double Hmax = _RecoResults["Hauss"][NUMOF_TOP_POLAR - 1].Distance;
            double Mmin = _RecoResults["MeanD"][0].Distance;
            double Mmax = _RecoResults["MeanD"][NUMOF_TOP_POLAR - 1].Distance;
            double Tmin = _RecoResults["Tanim"][0].Distance;
            double Tmax = _RecoResults["Tanim"][NUMOF_TOP_POLAR - 1].Distance;
            double Ymin = _RecoResults["YuleD"][0].Distance;
            double Ymax = _RecoResults["YuleD"][NUMOF_TOP_POLAR - 1].Distance;

            for (int i = 0; i < NUMOF_TOP_POLAR; i++)
            {
                double distance = PercentLoc(Hmin, Hmax, _RecoResults["Polar"][i].Symbol._Hdist)
                    + PercentLoc(Mmin, Mmax, _RecoResults["Polar"][i].Symbol._Mdist)
                    + PercentLoc(Tmin, Tmax, _RecoResults["Polar"][i].Symbol._Tdist)
                    + PercentLoc(Ymin, Ymax, _RecoResults["Polar"][i].Symbol._Ydist);
                
                SymbolRank SR = new SymbolRank(distance, _RecoResults["Polar"][i].Symbol);

                if (SR.Distance < _BestMatch.Distance)
                    _BestMatch = SR;

                fusio_results.Add(SR);
            }
            fusio_results.Sort(delegate(SymbolRank SR1, SymbolRank SR2) { return SR1.Distance.CompareTo(SR2.Distance); });
            _RecoResults.Add("Fusio", fusio_results);

            List<string> result_names = new List<string>(fusio_results.Count);
            for (int i = 0; i < fusio_results.Count; i++)
                result_names.Add(fusio_results[i].SymbolName);

            return result_names;
        }

        public List<ImageScore> FindBestMatches(List<BitmapSymbol> defns, int numTopToConsider, double allowedRotationFromOrigin, List<double> rotationAngleOrigins)
        {
            if (defns.Count <= numTopToConsider)
                numTopToConsider = defns.Count;

            if (numTopToConsider == 0)
                return new List<ImageScore>();

            List<SymbolRank> polarResults = RecognizePolar(this, defns, allowedRotationFromOrigin, rotationAngleOrigins);
            Dictionary<string, List<SymbolRank>> Results = RecognizeScreen(this, defns, polarResults, numTopToConsider);

            return SortUsingFusion(Results, numTopToConsider);
        }

        private List<SymbolRank> RecognizePolar(BitmapSymbol unknownSymbol, List<BitmapSymbol> defns, double allowedRotationFromOrigin, List<double> rotationAngleOrigins)
        {
            unknownSymbol.Process_Polar();

            List<SymbolRank> polarResults = new List<SymbolRank>(defns.Count);
            foreach (BitmapSymbol def in defns)
                polarResults.Add(new SymbolRank(def.Polar_Mean_Distance(unknownSymbol, allowedRotationFromOrigin, rotationAngleOrigins), def));

            polarResults.Sort(delegate(SymbolRank SR1, SymbolRank SR2) { return SR1.Distance.CompareTo(SR2.Distance); });

            return polarResults;
        }

        private double Polar_Mean_Distance(BitmapSymbol S, double allowedRotationFromOrigin, List<double> rotationAngleOrigins)
        {
            List<IntRange> rotationIndexRanges = new List<IntRange>(rotationAngleOrigins.Count);
            foreach (double origin in rotationAngleOrigins)
            {
                int min = (int)Math.Floor(origin - allowedRotationFromOrigin);
                int max = (int)Math.Floor(origin + allowedRotationFromOrigin);
                rotationIndexRanges.Add(new IntRange(min, max));
            }

            List<QuantizedPoint> A = _polarQuantizedPoints;
            GeneralMatrix A_DTM = _pDTM;
            double Aymax = _polarYmax;

            List<QuantizedPoint> B = S._polarQuantizedPoints;
            GeneralMatrix B_DTM = S._pDTM;
            double Bymax = S._polarYmax;

            if (A.Count == 0 || B.Count == 0) return double.PositiveInfinity;

            double minA2B = double.PositiveInfinity;
            int besti = 0;
            for (int i = 0; i < GRID_X_SIZE; i++)
            {
                
                bool inRange = false;
                foreach (IntRange range in rotationIndexRanges)
                    if (range.IsInside(i))
                        inRange = true;

                if (inRange)
                {
                    double distan = 0.0;
                    for (int a = 0; a < A.Count; a++)
                    {
                        int y_ind = A[a].Y;
                        int x_ind = A[a].X + i;
                        if (x_ind >= GRID_X_SIZE) 
                            x_ind -= GRID_X_SIZE;
                        
                        //putting less weight on points that have small rel dist - y 
                        double weight = Math.Pow((double)y_ind / Aymax, POLAR_WEIGHT_DECAY_RATE);
                        distan += B_DTM.GetElement(y_ind, x_ind) * weight;
                    }

                    if (distan < minA2B)
                    {
                        minA2B = distan;
                        besti = i;
                    }
                }
            }

            _bestRotAngle = besti * 2.0 * Math.PI / (double)GRID_X_SIZE;

            double minB2A = 0.0;
            for (int b = 0; b < B.Count; b++)
            {
                int y_ind = B[b].Y;
                int x_ind = B[b].X - besti; // slide B back by besti
                if (x_ind < 0) 
                    x_ind += GRID_X_SIZE;

                double weight = Math.Pow((double)y_ind / Bymax, POLAR_WEIGHT_DECAY_RATE);
                minB2A += A_DTM.GetElement(y_ind, x_ind) * weight;
            }

            return Math.Max(minA2B/(double)A.Count, minB2A/(double)B.Count);
        }

        private Dictionary<string, List<SymbolRank>> RecognizeScreen(BitmapSymbol unknownSymbol, List<BitmapSymbol> defns, List<SymbolRank> polarResults, int numTopToConsider)
        {
            List<SymbolRank> hauss_results = new List<SymbolRank>();
            List<SymbolRank> meand_results = new List<SymbolRank>();
            List<SymbolRank> tanim_results = new List<SymbolRank>();
            List<SymbolRank> yuled_results = new List<SymbolRank>();

            foreach (SymbolRank polarRank in polarResults)
            {
                BitmapSymbol defnS = polarRank.Symbol;
                double alpha = defnS.BestRotationAngle;

                unknownSymbol.Rotate(-alpha);

                unknownSymbol.Process_Screen();

                SymbolRank SR = new SymbolRank(defnS.Hausdorff_Distance(unknownSymbol), defnS);
                hauss_results.Add(SR);
                defnS._Hdist = SR.Distance;

                SR = new SymbolRank(defnS.Mean_Pixel_Distance(unknownSymbol), defnS);
                meand_results.Add(SR);
                defnS._Mdist = SR.Distance;

                SR = new SymbolRank(defnS.Tanimoto_Distance(unknownSymbol), defnS);
                tanim_results.Add(SR);
                defnS._Tdist = SR.Distance;

                SR = new SymbolRank(defnS.Yule_Distance(unknownSymbol), defnS);
                yuled_results.Add(SR);
                defnS._Ydist = SR.Distance;

                unknownSymbol.Rotate(alpha);
            }

            hauss_results.Sort(delegate(SymbolRank SR1, SymbolRank SR2) { return SR1.Distance.CompareTo(SR2.Distance); });
            meand_results.Sort(delegate(SymbolRank SR1, SymbolRank SR2) { return SR1.Distance.CompareTo(SR2.Distance); });
            tanim_results.Sort(delegate(SymbolRank SR1, SymbolRank SR2) { return SR1.Distance.CompareTo(SR2.Distance); });
            yuled_results.Sort(delegate(SymbolRank SR1, SymbolRank SR2) { return SR1.Distance.CompareTo(SR2.Distance); });

            Dictionary<string, List<SymbolRank>> results = new Dictionary<string, List<SymbolRank>>(5);
            results.Add("Polar", polarResults); 
            results.Add("Hauss", hauss_results);
            results.Add("MeanD", meand_results);
            results.Add("Tanim", tanim_results);
            results.Add("YuleD", yuled_results);

            return results;
        }

        private List<ImageScore> SortUsingFusion(Dictionary<string, List<SymbolRank>> results, int numTopToConsider)
        {
            List<SymbolRank> fusio_results = new List<SymbolRank>(numTopToConsider);
            
            double Hmin = results["Hauss"][0].Distance;
            double Hmax = results["Hauss"][numTopToConsider - 1].Distance;
            double Mmin = results["MeanD"][0].Distance;
            double Mmax = results["MeanD"][numTopToConsider - 1].Distance;
            double Tmin = results["Tanim"][0].Distance;
            double Tmax = results["Tanim"][numTopToConsider - 1].Distance;
            double Ymin = results["YuleD"][0].Distance;
            double Ymax = results["YuleD"][numTopToConsider - 1].Distance;

            for (int i = 0; i < numTopToConsider; i++)
            {
                double distance = PercentLoc(Hmin, Hmax, results["Polar"][i].Symbol._Hdist)
                    + PercentLoc(Mmin, Mmax, results["Polar"][i].Symbol._Mdist)
                    + PercentLoc(Tmin, Tmax, results["Polar"][i].Symbol._Tdist)
                    + PercentLoc(Ymin, Ymax, results["Polar"][i].Symbol._Ydist);

                SymbolRank SR = new SymbolRank(distance, results["Polar"][i].Symbol);

                fusio_results.Add(SR);
            }

            fusio_results.Sort(delegate(SymbolRank SR1, SymbolRank SR2) { return SR1.Distance.CompareTo(SR2.Distance); });

            List<ImageScore> scores = new List<ImageScore>(fusio_results.Count);
            foreach (SymbolRank result in fusio_results)
                scores.Add(new ImageScore(result));

            return scores;
        }

        public override double Recognize(RecognitionTemplate unknown)
        {
            return 0.0;
        }

        #endregion


        #region Compare 2 Symbols

        static public double[] Compare(BitmapSymbol Defn, BitmapSymbol Unknown)
        {
            double[] scores = new double[5];
            scores[0] = FindBestAngle(Defn, Unknown);
            scores[1] = Defn.Hausdorff_Distance(Unknown);
            scores[2] = Defn.Mean_Pixel_Distance(Unknown);
            scores[3] = Defn.Tanimoto_Distance(Unknown);
            scores[4] = Defn.Yule_Distance(Unknown);

            return scores;
        }

        static private double FindBestAngle(BitmapSymbol Defn, BitmapSymbol Unknown)
        {
            double ALLOWED_ROTATION_AMOUNT = 360.0;
            int lower_rot_index;
            if (0.0 <= ALLOWED_ROTATION_AMOUNT && ALLOWED_ROTATION_AMOUNT <= 360.0) //check valid ALLOWED_ROTATION_AMOUNT
                lower_rot_index = (int)Math.Floor(ALLOWED_ROTATION_AMOUNT * GRID_X_SIZE / 2.0 / 360.0); //trick
            else
                lower_rot_index = (int)Math.Floor(GRID_X_SIZE / 2.0); //default to full rotation

            int upper_rot_index = GRID_X_SIZE - lower_rot_index; //trick

            List<QuantizedPoint> A = new List<QuantizedPoint>(Unknown._polarQuantizedPoints);
            GeneralMatrix A_DTM = new GeneralMatrix(Unknown._pDTM.ArrayCopy);
            double Aymax = Unknown._polarYmax;

            List<QuantizedPoint> B = new List<QuantizedPoint>(Defn._polarQuantizedPoints);
            GeneralMatrix B_DTM = new GeneralMatrix(Defn._pDTM.ArrayCopy);
            double Bymax = Defn._polarYmax;

            if (A.Count == 0 || B.Count == 0) return INF;

            double minA2B = INF;
            double distan = 0.0;
            int besti = 0;
            for (int i = 0; i < GRID_X_SIZE; i++)
            {
                distan = 0.0;
                for (int a = 0; a < A.Count; a++)
                {
                    int y_ind = A[a].Y;
                    int x_ind = A[a].X + i;
                    if (x_ind >= GRID_X_SIZE) x_ind -= GRID_X_SIZE;
                    //putting less weight on points that have small rel dist - y 
                    double weight = Math.Pow((double)y_ind / Aymax, POLAR_WEIGHT_DECAY_RATE);
                    distan += B_DTM.GetElement(y_ind, x_ind) * weight;
                }

                if (distan < minA2B && (i <= lower_rot_index || i >= upper_rot_index))
                {
                    minA2B = distan;
                    besti = i;
                }
            }

            Unknown._bestRotAngle = besti * 2.0 * Math.PI / (double)GRID_X_SIZE;

            distan = 0.0;
            for (int b = 0; b < B.Count; b++)
            {
                int y_ind = B[b].Y;
                int x_ind = B[b].X - besti; // slide B back by besti
                if (x_ind < 0) x_ind += GRID_X_SIZE;
                double weight = Math.Pow((double)y_ind / Bymax, POLAR_WEIGHT_DECAY_RATE);
                distan += A_DTM.GetElement(y_ind, x_ind) * weight;
            }
            double minB2A = distan;

            return Math.Max(minA2B / (double)A.Count, minB2A / (double)B.Count);
        }

        #endregion
    }
}
