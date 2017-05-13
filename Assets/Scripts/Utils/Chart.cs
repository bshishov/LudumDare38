using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Utils
{
    [RequireComponent(typeof(RawImage))]
    public class Chart : MonoBehaviour
    {
        public Color BackgroundColor;
        public Color AxisColor;
        [Range(0f, 0.2f)]
        public float Padding = 0.05f;
        public bool FixedScaleRatio = true;
        public Material Mat;


        // Componenets
        public Transform Legend { get; private set; }
        public Text TitleLabel { get; private set; }
        public Text XAxisLabel { get; private set; }
        public Text YAxisLabel { get; private set; }
        

        public string Title
        {
            get { return TitleLabel != null ? TitleLabel.text : null; }

            set
            {
                if (TitleLabel != null)
                {
                    TitleLabel.text = value;
                }
            }
        }

        public string XAxisName
        {
            get { return XAxisLabel != null ? XAxisLabel.text : null; }

            set
            {
                if (XAxisLabel != null)
                {
                    XAxisLabel.text = value;
                }
            }
        }

        public string YAxisName
        {
            get { return YAxisLabel != null ? YAxisLabel.text : null; }

            set
            {
                if (YAxisLabel != null)
                {
                    YAxisLabel.text = value;
                }
            }
        }


        private readonly List<Series> _series = new List<Series>();
        private readonly Dictionary<Series, GameObject> _legendLabels = new Dictionary<Series, GameObject>();

        private RenderTexture _target;
        private RawImage _rawImage;
        
        private Transform _legendLabelTemplate;
        private Transform _xTickTemplate;
        private Transform _yTickTemplate;
        private readonly Text[] _xTicksPool = new Text[12];
        private readonly Text[] _yTicksPool = new Text[12];

        void Start ()
        {
            _rawImage = GetComponent<RawImage>();
            _rawImage.color = Color.white;
            _target = new RenderTexture((int) _rawImage.rectTransform.rect.width,
                (int) _rawImage.rectTransform.rect.height, 0, RenderTextureFormat.ARGB32)
            {
                //antiAliasing = 4,
                //anisoLevel = 2
            };


            var titleLableObject = transform.Find("TitleLabel");
            if (titleLableObject != null)
                TitleLabel = titleLableObject.GetComponent<Text>();

            var xLableObject = transform.Find("XAxisLabel");
            if (xLableObject != null)
                XAxisLabel = xLableObject.GetComponent<Text>();

            var yLableObject = transform.Find("YAxisLabel");
            if (yLableObject != null)
                YAxisLabel = yLableObject.GetComponent<Text>();

            Legend = transform.Find("Legend");
            if (Legend != null)
            {
                _legendLabelTemplate = Legend.transform.Find("LabelTemplate");

                // Deactivate template
                if (_legendLabelTemplate != null)
                {
                    _legendLabelTemplate.gameObject.SetActive(false);
                }
            }

            _xTickTemplate = transform.Find("XTickTemplate");
            if (_xTickTemplate != null)
            {
                _xTickTemplate.gameObject.SetActive(false);

                for (var i = 0; i < _xTicksPool.Length; i++)
                {
                    _xTicksPool[i] = Instantiate(_xTickTemplate.gameObject, transform).GetComponent<Text>();
                }
            }

            _yTickTemplate = transform.Find("YTickTemplate");
            if (_yTickTemplate != null)
            {
                _yTickTemplate.gameObject.SetActive(false);

                for (var i = 0; i < _yTicksPool.Length; i++)
                {
                    _yTicksPool[i] = Instantiate(_yTickTemplate.gameObject, transform).GetComponent<Text>();
                }
            }
        }

        public void AddSeries(Series series)
        {
            if(_series.Contains(series))
                return;
            _series.Add(series);

            if (Legend != null && _legendLabelTemplate != null && !string.IsNullOrEmpty(series.Name))
            {
                var label = (GameObject)Instantiate(_legendLabelTemplate.gameObject, Legend);
                label.SetActive(true);

                var labelText = label.GetComponent<Text>();
                labelText.text = series.Name;
                labelText.color = series.Color;

                //if(!_legendLabels.ContainsKey(series))
                    _legendLabels.Add(series, label);
            }
        }

        public void RemoveSeries(Series series)
        {
            _series.Remove(series);

            if (_legendLabels.ContainsKey(series))
            {
                Destroy(_legendLabels[series]);
                _legendLabels.Remove(series);
            }
        }

        public void ClearAll()
        {
            foreach (var s in _series.ToList())
            {
                RemoveSeries(s);
            }
        }

        void Update ()
        {
            if (_rawImage.enabled)
            {
                Render();
                _rawImage.texture = _target;
            }
        }

        void Render()
        {
            if(_series.Count < 1)
                return;


            var s0 = _series[0];
            var xMin = s0.Bounds.xMin;
            var xMax = s0.Bounds.xMax;

            var yMin = s0.Bounds.yMin;
            var yMax = s0.Bounds.yMax;

            for (var i = 0; i < _series.Count; i++)
            {
                var s = _series[i];

                if(s.NumberOfPoints < 2)
                    continue;

                if (s.Bounds.xMax > xMax)
                    xMax = s.Bounds.xMax;

                if (s.Bounds.xMin < xMin)
                    xMin = s.Bounds.xMin;

                if (s.Bounds.yMax > yMax)
                    yMax = s.Bounds.yMax;

                if (s.Bounds.yMin < yMin)
                    yMin = s.Bounds.yMin;
            }
         

            var xRange = xMax - xMin;
            var yRange = yMax - yMin;

            if (FixedScaleRatio)
            {
                // Rectangular
                var centerX = (xMin + xMax)*0.5f;
                var centerY = (yMin + yMax)*0.5f;
                var halfRange = Mathf.Max(xRange, yRange)*0.5f;

                xMin = centerX - halfRange;
                xMax = centerX + halfRange;
                yMin = centerY - halfRange;
                yMax = centerY + halfRange;

                yRange = 2 * halfRange;
                xRange = 2 * halfRange;
            }

            // Padding
            xMin -= xRange * Padding;
            xMax += xRange * Padding;
            yMin -= yRange * Padding;
            yMax += yRange * Padding;
            xRange = xMax - xMin;
            yRange = yMax - yMin;


            Graphics.SetRenderTarget(_target);
            GL.Clear(false, true, BackgroundColor);
            //GL.PushMatrix();
            GL.LoadPixelMatrix(xMin, xMax, yMin, yMax);
            Mat.SetPass(0);



            // HORIZONTAL TICKS (Y)
            var ylogRange = Mathf.Log10(yRange);
            var sy1 = Mathf.Floor(ylogRange);
            var sy2 = Mathf.Ceil(ylogRange);

            var ay2 = (sy2 - ylogRange);
            var ay1 = (ylogRange - sy1);
            

            var dy1 = Mathf.Pow(10f, sy1 - 1); // Small ticks
            var dy2 = Mathf.Pow(10f, sy2 - 1); // Big ticks
            var y1 = (Mathf.Floor(yMin / dy1) + 1) * dy1;
            var y2 = (Mathf.Floor(yMin / dy2) + 1) * dy2;

            // VERTICAL TICKS (X)
            var xlogRange = Mathf.Log10(xRange);
            var sx1 = Mathf.Floor(xlogRange);
            var sx2 = Mathf.Ceil(xlogRange);

            var ax2 = (sx2 - xlogRange);
            var ax1 = (xlogRange - sx1);


            var dx1 = Mathf.Pow(10f, sx1 - 1); // Small ticks
            var dx2 = Mathf.Pow(10f, sx2 - 1); // Big ticks
            var x1 = (Mathf.Floor(xMin / dx1) + 1) * dx1;
            var x2 = (Mathf.Floor(xMin / dx2) + 1) * dx2;
            



            GL.Begin(GL.LINES);
            // Y BIG TICKS
            var yTickLabelIndex = 0;
            GL.Color(AxisColor);
            while (y2 < yMax)
            {
                GL.Vertex3(xMin, y2, 0);
                GL.Vertex3(xMax, y2, 0);

                var tickLabel = _yTicksPool[yTickLabelIndex++];
                tickLabel.text = string.Format("{0}", y2);

                var labelRelPos = new Vector2(0, _rawImage.rectTransform.rect.height * (y2 - yMin) / yRange);
                tickLabel.rectTransform.anchoredPosition = labelRelPos;
                tickLabel.gameObject.SetActive(true);

                y2 += dy2;
            }

            while (yTickLabelIndex < _xTicksPool.Length)
                _yTicksPool[yTickLabelIndex++].gameObject.SetActive(false);

            // Y SMALL TICKS
            GL.Color(AxisColor * ay2 * ay2);
            while (y1 <=yMax)
            {
                GL.Vertex3(xMin, y1, 0);
                GL.Vertex3(xMax, y1, 0);
                y1 += dy1;
            }

            // X BIG TICKS
            var xTickLabelIndex = 0;
            GL.Color(AxisColor);
            while (x2 < xMax)
            {
                GL.Vertex3(x2, yMin, 0);
                GL.Vertex3(x2, yMax, 0);

                var tickLabel = _xTicksPool[xTickLabelIndex++];
                tickLabel.text = string.Format("{0}", x2);

                var labelRelPos = new Vector2(_rawImage.rectTransform.rect.width * (x2 - xMin) / xRange, 0);
                tickLabel.rectTransform.anchoredPosition = labelRelPos;
                tickLabel.gameObject.SetActive(true);

                x2 += dx2;
            }

            while (xTickLabelIndex < _xTicksPool.Length)
                _xTicksPool[xTickLabelIndex++].gameObject.SetActive(false);


            // X SMALL TICKS
            GL.Color(AxisColor * ax2 * ax2);
            while (x1 < xMax)
            {
                GL.Vertex3(x1, yMin, 0);
                GL.Vertex3(x1, yMax, 0);
                x1 += dx1;
            }

            GL.End();


            foreach (var series in _series)
            {
                series.GlDraw();
            }

           
            //GL.PopMatrix();
        }
    }
}
