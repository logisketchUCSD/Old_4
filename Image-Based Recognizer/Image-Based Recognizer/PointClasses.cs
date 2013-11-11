using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace ImageRecognizer
{
    [Serializable]
    [DebuggerDisplay("x={_x} y={_y} isEndofStroke={_isEndofStroke}")]
    public class ScreenPoint : ICloneable
    {
        private double _x;
        private double _y;
        private int _isEndofStroke;
        private Guid _id;

        public ScreenPoint()
        {
            _id = Guid.NewGuid();
            _x = 0.0;
            _y = 0.0;
            _isEndofStroke = 0;
        }

        public ScreenPoint(double x, double y, int isEndofStroke)
        {
            _id = Guid.NewGuid();
            _x = x;
            _y = y;
            _isEndofStroke = isEndofStroke;
        }

        public ScreenPoint(ScreenPoint pt)
        {
            ScreenPoint temp = (ScreenPoint)pt.Clone();
            this._id = temp._id;
            this._isEndofStroke = temp._isEndofStroke;
            this._x = temp._x;
            this._y = temp._y;
        }

        public object Clone()
        {
            ScreenPoint pt = (ScreenPoint)this.MemberwiseClone();
            return pt;
        }

        public Guid Id
        {
            get { return _id; }
        }

        public double X
        {
            get { return _x; }
            set { _x = value; }
        }

        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public int IsEndofStroke
        {
            get { return _isEndofStroke; }
            set { _isEndofStroke = value; }
        }
    }

    [Serializable]
    [DebuggerDisplay("angPosition={_angPosition} relDistance={_relDistance}")]
    public class PolarPoint: ICloneable
    {
        private double _angPosition;
        private double _relDistance;
        private Guid _id;

        public PolarPoint()
        {
            _id = Guid.NewGuid();
            _angPosition = 0.0;
            _relDistance = 0.0;
        }

        public PolarPoint(double angPosition, double relDistance)
        {
            _id = Guid.NewGuid();
            _angPosition = angPosition;
            _relDistance = relDistance;
        }

        public PolarPoint(PolarPoint pt)
        {
            PolarPoint temp = (PolarPoint)pt.Clone();
            this._id = temp._id;
            this._angPosition = temp._angPosition;
            this._relDistance = temp._relDistance;
        }

        public object Clone()
        {
            PolarPoint pt = (PolarPoint)this.MemberwiseClone();
            return pt;
        }

        public Guid Id
        {
            get { return _id; }
        }

        public double AngularPosition
        {
            get { return _angPosition; }
            set { _angPosition = value; }
        }

        public double RelativeDistance
        {
            get { return _relDistance; }
            set { _relDistance = value; }
        }
    }

    [Serializable]
    [DebuggerDisplay("x={_x} y={_y}")]
    public class QuantizedPoint: ICloneable
    {
        private int _x;
        private int _y;
        private Guid _id;

        public QuantizedPoint()
        {
            _id = Guid.NewGuid();
            _x = 0;
            _y = 0;
        }

        public QuantizedPoint(int x, int y)
        {
            _id = Guid.NewGuid();
            _x = x;
            _y = y;
        }

        public QuantizedPoint(QuantizedPoint pt)
        {
            QuantizedPoint temp = (QuantizedPoint)pt.Clone();
            this._id = pt._id;
            this._x = pt._x;
            this._y = pt._y;
        }

        public object Clone()
        {
            QuantizedPoint pt = (QuantizedPoint)this.MemberwiseClone();
            return pt;
        }

        public Guid Id
        {
            get { return _id; }
        }

        public int X
        {
            get { return _x; }
            set { _x = value; }
        }

        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }
    }

    [Serializable]
    [DebuggerDisplay("x={_x} y={_y}")]
    public class Coord: ICloneable
    {
        const double COORD_EPSILON = 10e-6;
        double _x;
        double _y;
        Guid _id;

        public Coord()
        {
            _x = 0.0;
            _y = 0.0;
            _id = Guid.NewGuid();
        }

        public Coord(Coord c)
        {
            Coord temp = (Coord)c.Clone();
            this._id = temp._id;
            this._x = temp._x;
            this._y = temp._y;
        }

        public Coord(double x, double y)
        {
            _x = x;
            _y = y;
            _id = Guid.NewGuid();
        }

        public Coord(int x, int y)
        {
            _x = (double)x;
            _y = (double)y;
            _id = Guid.NewGuid();
        }

        public object Clone()
        {
            Coord c = (Coord)this.MemberwiseClone();
            return c;
        }

        public bool Equals(Coord c)
        {
            return (Math.Abs(_x - c._x) < COORD_EPSILON && Math.Abs(_x - c._x) < COORD_EPSILON);
        }

        public Coord Plus(Coord c)
        {
            return new Coord(_x + c._x, _y + c._y);
        }

        public Coord Minus(Coord c)
        {
            return new Coord(_x - c._x, _y - c._y);
        }

        public double distanceTo(Coord c)
        {
            double dx = _x - c._x;
            double dy = _y - c._y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public void Translate(double dx, double dy)
        {
            _x += dx;
            _y += dy;
        }

        public void Rotate(double theta)
        {
            double oldx = _x;
            double oldy = _y;
            _x = oldx * Math.Cos(theta) - oldy * Math.Sin(theta);
            _y = oldx * Math.Sin(theta) + oldy * Math.Cos(theta);
        }

        public void Scale(double sx, double sy)
        {
            _x *= sx;
            _y *= sy;
        }

        private void copy(Coord c)
        {
            _x = c._x;
            _y = c._y;
            _id = c._id;
        }

        public double X
        {
            get { return _x; }
        }

        public double Y
        {
            get { return _y; }
        }

        public Guid Id
        {
            get { return _id; }
        }
    }
}
