using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Clusters;
using ImageRecognizer;
using Microsoft.Ink;

namespace InkForm
{
    public partial class ImageResultSubForm : Form
    {
        public ImageResultSubForm(Cluster cluster)
        {
            InitializeComponent();
            
            if (!cluster.HasBeenScored || cluster.Score.TopMatch == null)
                return;

            ImageScore score = cluster.Score.TopMatch;
            labelNames.Text = score.SymbolClass + ": " + score.SymbolType;
            labelHausdorff.Text += score.HausdorfScore.ToString("#0.000");
            labelModifiedHausdorff.Text += score.ModifiedHausdorfScore.ToString("#0.000");
            labelTanimoto.Text += score.TanimotoScore.ToString("#0.000");
            labelYule.Text += score.YuleScore.ToString("#0.000");
            labelUserName.Text += score.UserName;
            labelCompleteness.Text += score.Completeness.ToString();
            labelPlatform.Text += score.Platform.ToString();

            InkOverlay ink = new InkOverlay(panelInk);
            foreach (Sketch.Substroke s in cluster.Strokes)
            {
                ink.Ink.CreateStroke(s.PointsAsSysPoints);
                ink.Ink.Strokes[ink.Ink.Strokes.Count - 1].DrawingAttributes.Color = Color.Blue;
            }
            ScaleAndMoveInk(ref ink);

            InkOverlay symbolInk = new InkOverlay(panelInk);
            List<Point[]> strokes = score.TemplateSymbol.Points;
            foreach (Point[] points in strokes)
            {
                symbolInk.Ink.CreateStroke(points);
                symbolInk.Ink.Strokes[symbolInk.Ink.Strokes.Count - 1].DrawingAttributes.Color = Color.Red;
            }
            ScaleAndMoveInk(ref symbolInk);

        }

        private void ScaleAndMoveInk(ref InkOverlay ink)
        {
            if (ink.Ink.Strokes.Count == 0)
                return;

            Rectangle box = ink.Ink.GetBoundingBox();
            Graphics g = this.panelInk.CreateGraphics();

            float scaleX = 1.0f;
            float scaleY = 1.0f;
            float Scale = 1.0f;

            Point pt = new Point(this.panelInk.Width, this.panelInk.Height);

            ink.Renderer.PixelToInkSpace(g, ref pt);

            scaleX = (float)pt.X / (float)box.Width * 0.9f;
            scaleY = (float)pt.Y / (float)box.Height * 0.9f;

            Scale = Math.Min(scaleX, scaleY);

            ink.Ink.Strokes.Scale(Scale, Scale);

            float offset = 300.0f;

            box = ink.Ink.GetBoundingBox();

            float InkMovedX = -(float)box.X + offset;
            float InkMovedY = -(float)(box.Y) + offset;

            box = ink.Ink.GetBoundingBox();
            ink.Ink.Strokes.Move(InkMovedX, InkMovedY);
        }
    }
}