using System;
using System.Collections.Generic;
using System.Text;

namespace Grouper
{
    /// <summary>
    /// abstract class domain is like an interface for it's derived classes,
    /// which will implement the cluster method separately.
    /// </summary>
    internal abstract class Domain
    {
        /// <summary>
        /// the sketch we pass in
        /// </summary>
        private Sketch.Sketch sketch;

        /// <summary>
        /// construtor for Domain class
        /// </summary>
        /// <param name="sketch">sketch we want to analyze</param>
        public Domain(Sketch.Sketch sketch)
        {
            this.sketch = sketch;
        }

        /// <summary>
        /// this method will take a label group specific to the given domain and group
        /// it into clusters which will be turned into shapes
        /// </summary>
        /// <param name="labelGroup"></param>
        public abstract void cluster(LabelGroup labelGroup);
    }
}
