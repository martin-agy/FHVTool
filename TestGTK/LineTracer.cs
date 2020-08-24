using System;
using System.Collections.Generic;
using System.Linq;
using Cairo;

namespace TestGTK
{
    public class LineTracer
    {
        List<PointD> points;
        PointD start, last;
        double ang = 0.0;

        public LineTracer()
        {
            points = new List<PointD>();
            start = new PointD(0, 0);
            last = start;
        }

        public bool Step(String command, double dx, double dang)
        {
            switch (command)
            {
                case "f":
                    last = new PointD(last.X + dx * Math.Cos(ang), last.Y + dx * Math.Sin(ang));
                    points.Add(last); 
                    return true;
                case "h":
                    ang += dang;
                    break;
                case "v":
                    ang -= dang;
                    break;
                default:
                    throw new Exception(String.Format("Unknown command '{0}'", command));
            }
            return false;
        }

        public void DrawLine(Cairo.Context cr, Gdk.Rectangle rect)
        {
            cr.SetSourceRGB(1, 1, 1);
            cr.Paint();
            cr.SetSourceRGB(1.0, 0.2, 0.0);

            cr.Translate(rect.Width / 2, rect.Height / 10);

            cr.LineCap = Cairo.LineCap.Round;
            cr.MoveTo(start);

            cr.LineWidth = 1;
            int n = points.Count - 4;
            if (n > 0)
                foreach (var p in points.Take(n))
                    cr.LineTo(p);
            cr.Stroke();
            for (int i = Math.Max(0, n); i < points.Count; i++)
            {
                if (i < 1)
                    cr.MoveTo(start);
                else
                    cr.MoveTo(points[i - 1]);
                cr.LineWidth = Convert.ToDouble(i - points.Count + 6);
                cr.LineTo(points[i]);
                cr.Stroke();
            }
        }
    }
}
