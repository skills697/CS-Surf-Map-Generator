﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Linq;

namespace VMFConverter
{
    public class PolygonTriangulator
    {

        public static List<List<Vector2>> Triangulate(List<Vector2> Polygon)
        {
            //for (int i = 0; i < Polygon.Count; i++)
            //{
            //    Polygon[i] /= 128;
            //}
            List<Vector2> sharedValues = Polygon.GroupBy(x => x).Where(g => g.Count() > 1).Select(x => x.Key).ToList();

            float amountToRaise = 0.1f;
            List<int> higherIndices = new List<int>();

            Dictionary<Vector2, int> highestIndex = new Dictionary<Vector2, int>();
            Dictionary<Vector2, float> currentHighest = new Dictionary<Vector2, float>();
            List<Vector2> onesToReset = new List<Vector2>();

            for (int i = 0; i < sharedValues.Count; i++)
            {
                highestIndex.Add(sharedValues[i], -1);
                currentHighest.Add(sharedValues[i], 0);
            }

            for (int i = 0; i < Polygon.Count; i++)
            {
                if (!sharedValues.Contains(Polygon[i]))
                {
                    continue;
                }

                int indexA = (i - 1) % Polygon.Count;
                if (indexA < 0)
                {
                    indexA = Polygon.Count - 1;
                }
                int indexB = (i) % Polygon.Count;
                int indexC = (i + 1) % Polygon.Count;

                Vector2 prev = Polygon[indexA];
                Vector2 curr = Polygon[indexB];
                Vector2 next = Polygon[indexC];

                if ((prev.Y + next.Y) > currentHighest[Polygon[i]])
                {
                    currentHighest[Polygon[i]] = (prev.Y + next.Y);
                    highestIndex[Polygon[i]] = i;
                }
            }

            foreach (Vector2 pos in highestIndex.Keys)
            {
                Polygon[highestIndex[pos]] = new Vector2(Polygon[highestIndex[pos]].X, Polygon[highestIndex[pos]].Y + amountToRaise);
                onesToReset.Add(Polygon[highestIndex[pos]]);
            }

            List<Vector2> remainingValues = new List<Vector2>(Polygon);
            List<List<Vector2>> result = new List<List<Vector2>>();

            int c = 0;
            int index = -1;
            bool triangleMade = true;
            while (triangleMade)
            {
                triangleMade = false;
                for (int v = Polygon.Count + 2; v >= 0; v--)
                {
                    index = v;

                    int indexA = (index - 1) % Polygon.Count;
                    if (indexA < 0)
                    {
                        indexA = Polygon.Count - 1;
                    }
                    int indexB = (index) % Polygon.Count;
                    int indexC = (index + 1) % Polygon.Count;
                    Vector2 prev = Polygon[indexA];
                    Vector2 curr = Polygon[indexB];
                    Vector2 next = Polygon[indexC];

                    Vector2 up = new Vector2(0, 1);

                    Vector2 abNormal = Vector2.Normalize(Shape.GetNormal2D(prev, curr));
                    Vector2 bcNormal = Vector2.Normalize(Shape.GetNormal2D(curr, next));
                    Vector2 vertexNormalabcInner = -Vector2.Normalize((abNormal + bcNormal)) * 100;

                    Vector2 abDir = Vector2.Normalize((prev + abNormal) - (curr + vertexNormalabcInner));
                    Vector2 bcDir = Vector2.Normalize((next + abNormal) - (curr + vertexNormalabcInner));

                    float distanceBetweenPrevNext = Vector2.Distance(prev, next);
                    float distanceExtended = Vector2.Distance(prev + abNormal, next + bcNormal);

                    if (distanceExtended < distanceBetweenPrevNext)
                    {
                        continue;
                    }

                    bool noGood = false;
                    for (int i = 0; i < remainingValues.Count; i++)
                    {
                        if (remainingValues[i] == prev || remainingValues[i] == curr || remainingValues[i] == next)
                        {
                            continue;
                        }
                        if (PointInTriangle(remainingValues[i], prev, curr, next))
                        {
                            noGood = true;
                            break;
                        }
                    }
                    if (noGood)
                    {
                        continue;
                    }

                    result.Add(new List<Vector2>()
                    {
                        prev,
                        curr,
                        next
                    });
                    Polygon.Remove(curr);

                    triangleMade = true;

                    if(Polygon.Count > 2)
                    {
                        float scale = 0.1f;
                        Pen whitePen = new Pen(Color.White, 3);
                        Pen greyPen = new Pen(Color.Gray, 3);
                        Pen blackPen = new Pen(Color.Black, 3);
                        Pen redPen = new Pen(Color.Red, 3);
                        Pen bluePen = new Pen(Color.Blue, 3);
                        Pen greenPen = new Pen(Color.Green, 3);
                        using (Bitmap canvas = new Bitmap(500, 500))
                        {
                            using (Graphics g = Graphics.FromImage(canvas))
                            {
                                g.FillRectangle(Brushes.White, new Rectangle(0, 0, 500, 500));

                                for (int i = 0; i < Polygon.Count; i++)
                                {
                                    int iN = (i + 1) % Polygon.Count;
                                    Point p1 = new Point((int)(Polygon[i].X * scale + 100), (int)(Polygon[i].Y * scale + 100));
                                    Point p2 = new Point((int)(Polygon[iN].X * scale + 100), (int)(Polygon[iN].Y * scale + 100));

                                    g.DrawLine(greyPen, p1, p2);
                                }

                                for (int i = 0; i < result.Count; i++)
                                {
                                    for (int j = 0; j < result[i].Count; j++)
                                    {
                                        int iN = (j + 1) % result[i].Count;
                                        Point p1 = new Point((int)(result[i][j].X * scale + 100), (int)(result[i][j].Y * scale + 100));
                                        Point p2 = new Point((int)(result[i][iN].X * scale + 100), (int)(result[i][iN].Y * scale + 100));

                                        g.DrawLine(blackPen, p1, p2);
                                    }
                                }

                                Point ver = new Point((int)((curr + vertexNormalabcInner).X * scale) + 100, (int)((curr + vertexNormalabcInner).Y * scale) + 100);

                                g.DrawLine(redPen, new Point((int)(curr.X * scale) + 100, (int)(curr.Y * scale) + 100), ver);
                                g.DrawLine(bluePen,
                                    new Point((int)(prev.X * scale) + 100, (int)(prev.Y * scale) + 100),
                                    new Point((int)(prev.X * scale) + (int)(abNormal.X * 30) + 100, (int)(prev.Y * scale) + (int)(abNormal.Y * 30) + 100));
                                g.DrawLine(bluePen,
                                    new Point((int)(next.X * scale) + 100, (int)(next.Y * scale) + 100),
                                    new Point((int)(next.X * scale) + (int)(bcNormal.X * 30) + 100, (int)(next.Y * scale) + (int)(bcNormal.Y * 30) + 100));

                                g.DrawEllipse(redPen, prev.X * scale + 100, prev.Y * scale + 100, 10, 10);
                                g.DrawEllipse(bluePen, curr.X * scale + 100, curr.Y * scale + 100, 10, 10);
                                g.DrawEllipse(greenPen, next.X * scale + 100, next.Y * scale + 100, 10, 10);

                                Font font = new Font(FontFamily.GenericSansSerif, 20, SystemFonts.DefaultFont.Style);
                                for (int i = 0; i < remainingValues.Count; i++)
                                {
                                    Point p1 = new Point((int)(remainingValues[i].X * scale + 100), (int)(remainingValues[i].Y * scale + 100));
                                    g.DrawString(i.ToString(), font, Brushes.DarkViolet, new PointF(p1.X, p1.Y));
                                }
                            }

                            canvas.Save(@"C:\Users\funny\source\repos\VMFGenerator\tri" + c + ".png");
                        }
                    }

                    c++;
                }
            }

            for (int i = 0; i < result.Count; i++)
            {
                for (int j = 0; j < result[i].Count; j++)
                {
                    if (onesToReset.Contains(result[i][j]))
                    {
                        result[i][j] = new Vector2(result[i][j].X, result[i][j].Y - amountToRaise);
                    }
                }
            }

            return result;
        }

        private static float sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        private static bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = sign(pt, v1, v2);
            d2 = sign(pt, v2, v3);
            d3 = sign(pt, v3, v1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }
    }
}

public class Delauney
{
    public static List<Vector2> Run(List<Vector2> points)
    {
        List<Vector2> result = new List<Vector2>();

        int n = 0;
        for (int x = 0; x < 500; x++)
        {
            for (int y = 0; y < 500; y++)
            {
                n = 0;
                int nX = (int)points[n].X;
                int nY = (int)points[n].Y;
                for (byte i = 0; i < 10; i++)
                {
                    int cX = (int)points[i].X;
                    int cY = (int)points[i].Y;
                    if (Vector2.Distance(new Vector2(cX, cY), new Vector2(x, y)) < Vector2.Distance(new Vector2(nX, nY), new Vector2(x, y)))
                        n = i;
                    nX = (int)points[n].X;
                    nY = (int)points[n].Y;
                }

                result.Add(new Vector2(nX, nY));
            }
        }

        return result;
    }
}