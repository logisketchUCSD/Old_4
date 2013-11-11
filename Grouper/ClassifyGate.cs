using System;
using System.Collections.Generic;
using System.Text;

namespace Svm
{
    class ClassifyGate : ClassifySVM
    {
        /// <summary>
        /// Classify Gates
        /// </summary>
        /// <param name="modelFile"></param>
        public ClassifyGate(string modelFile)
            : base(modelFile) { }

        /// <summary>
        /// Get the gate associated with the prediction
        /// </summary>
        /// <param name="predict"></param>
        /// <returns></returns>
        public override string predictToString(double predict)
        {
            if (predict == 1.0)
                return "AND";
            else if (predict == 2.0)
                return "NAND";
            else if (predict == 3.0)
                return "NOR";
            else if (predict == 4.0)
                return "NOT";
            else if (predict == 5.0)
                return "OR";
            else if (predict == 6.0)
                return "OTHER";
            else
                return "unknown";
        }
    }
}
