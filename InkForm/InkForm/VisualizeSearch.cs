using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Clusters;
using ImageRecognizer;

namespace InkForm
{
    public partial class VisualizeSearch : Form
    {
        List<ImageResultSubForm> forms;

        public VisualizeSearch(List<Cluster> topMatches)
        {
            InitializeComponent();
            forms = new List<ImageResultSubForm>();

            int count = 0;
            foreach (Cluster c in topMatches)
            {
                count++;
                ImageResultSubForm subFormChild = new ImageResultSubForm(c);
                subFormChild.MdiParent = this;
                if (c.IsParent)
                    subFormChild.Text = "Parent: Rank = " + count.ToString();
                else
                    subFormChild.Text = "Child: Rank = " + count.ToString();
                subFormChild.Show();
                forms.Add(subFormChild);
            }

            //this.LayoutMdi(MdiLayout.Cascade);
        }

        public void moveChildren()
        {
            if (forms.Count > 0)
                forms[0].Location = new Point(0, 0);

            if (forms.Count > 1)
                forms[1].Location = new Point(forms[0].Right, 0);

            if (forms.Count > 2)
                forms[2].Location = new Point(forms[1].Right, 0);

            for (int i = 3; i < forms.Count; i++)
            {
                if (i % 3 == 0)
                    forms[i].Location = new Point(0, forms[i - 2].Bottom);
                else
                    forms[i].Location = new Point(forms[i - 1].Right, forms[i - 3].Bottom);
            }
        }
    }
}