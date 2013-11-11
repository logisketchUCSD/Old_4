using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace ImageRecognizer
{
    [Serializable]
    [DebuggerDisplay("Name: {SymbolName}, Distance = {_dist}")]
    public class SymbolRank
    {
        private double _dist;
        private BitmapSymbol _symbol;

        public SymbolRank()
        {
            _dist = double.MaxValue;
            _symbol = new BitmapSymbol();
        }

        public SymbolRank(double distance, BitmapSymbol symbol)
        {
            _dist = distance;
            _symbol = symbol;
        }

        public double Distance
        {
            get { return _dist; }
            set { _dist = value; }
        }

        public string SymbolName
        {
            get { return _symbol.Name; }
        }

        public BitmapSymbol Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }
    }
}
