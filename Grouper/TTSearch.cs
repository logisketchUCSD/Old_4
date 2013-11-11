using System;
using System.Collections.Generic;
using System.Text;

namespace Grouper
{
    internal class TTSearch : BaseSearch, ISearch
    {
        public TTSearch(Sketch.Sketch sketch) : base(sketch)
        {
            // Nothing to do here; parent constructor does it for us
        }

        public override void search(bool verbose)
        {
            // TODO: Implement
        }
    }
}
