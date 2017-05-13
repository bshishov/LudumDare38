using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class TrackSeries : Series
    {
        public readonly Vector2[] Values;

        
        private uint _counter;

        public TrackSeries(uint capacity, string name, Color color)
            : base(name, color)
        {
            Values = new Vector2[capacity];
        }

        public void AddPoint(float x, float y)
        {
            Values[_counter % Values.Length].x = x;
            Values[_counter % Values.Length].y = y;
            _counter++;
            CalcBounds();
        }

        private void CalcBounds()
        {
            var v0 = Values[0];
            float minX = v0.x, minY = v0.y, maxX = v0.x, maxY = v0.y;

            // TODO: MAKE IT O(1)
            for (var i = 1; i < Values.Length && i < _counter; i++)
            {
                var v = Values[i];
                if (v.x > maxX)
                    maxX = v.x;

                if (v.x < minX)
                    minX = v.x;

                if (v.y > maxY)
                    maxY = v.y;

                if (v.y < minY)
                    minY = v.y;
            }

            Bounds.xMax = maxX;
            Bounds.xMin = minX;
            Bounds.yMax = maxY;
            Bounds.yMin = minY;
        }

        public override int NumberOfPoints
        {
            get { return Mathf.Min(Values.Length, (int)_counter); }
        }

        public override void GlDraw()
        {
            GL.Begin(GL.LINES);
            GL.Color(Color);

            for (var i = 1; i < Values.Length && i < _counter; i++)
            {
                GL.Vertex(Values[(_counter - i - 1) % Values.Length]);
                GL.Vertex(Values[(_counter - i) % Values.Length]);
            }
            GL.End();
        }
    }

    public abstract class Series
    {
        public string Name;
        public Color Color;
        public Rect Bounds;

        public abstract int NumberOfPoints { get; }

        protected Series(string name, Color color)
        {
            Name = name;
            Color = color;
        }

        public abstract void GlDraw();
    }
}