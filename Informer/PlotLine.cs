using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PiramidaInformer
{
    class PlotLine
    {
        private PointF[] points = new PointF[48];
        private string[] labels = new string[48];
        private Pen pen;
        private string title;
        public PlotLine(PointF[] plotPoints,string[] labels, string color,string plotTitle)
        {
            plotPoints.CopyTo(points, 0);
            labels.CopyTo(this.labels, 0);
            pen = new Pen(Color.FromName(color), 2);
            title = plotTitle;
        }

        public PointF[] Points
        {
            get
            {
                return points;
            }
        }

        public string Title
        {
            get
            {
                return title;
            }
        }

        public Pen PlotPen
        {
        get
            {
                return pen;
            }
        }

        public float MaxPoint
        {
            get
            {
                return points.Max((PointF p) => p.Y);
            }
        }

        public string[] Labels
        {
            get
            {
                return labels;
            }
        }
    }
}
