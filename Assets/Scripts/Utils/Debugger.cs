using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class Debugger : LazySingleton<Debugger>
    {
        const float Padding = 10f;
        const float LineHeight = 20f;
        const float HeaderColumn = 200f;

        private static GUIStyle _boxStyle;
        private static GUIStyle _selectedBoxStyle;
        private static GUIStyle _contentStyle;
        private static Font _font;

        public static Font DefaultFont
        {
            get
            {
                if (_font != null)
                    return _font;

                _font = Font.CreateDynamicFontFromOSFont("Consolas", 14);
                return _font;
            }
        }

        public static GUIStyle HeaderStyle
        {
            get
            {
                if (_boxStyle != null)
                    return _boxStyle;

                var style = new GUIStyle(GUI.skin.label)
                {
                    normal =
                    {
                        background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.8f))
                    },
                    contentOffset = new Vector2(5f, 0f),
                    font = DefaultFont
                };
                _boxStyle = style;
                return style;
            }
        }

        public static GUIStyle SelectedHeaderStyle
        {
            get
            {
                if (_selectedBoxStyle != null)
                    return _selectedBoxStyle;

                var style = new GUIStyle(GUI.skin.label)
                {
                    normal =
                    {
                        background = MakeTex(2, 2, new Color(0.4f, 0.0f, 0.0f, 0.8f))
                    },
                    contentOffset = new Vector2(5f, 0f),
                    font = DefaultFont
                };
                _selectedBoxStyle = style;
                return style;
            }
        }

        public static GUIStyle ContentStyle
        {
            get
            {
                if (_contentStyle != null)
                    return _contentStyle;

                var style = new GUIStyle(GUI.skin.label)
                {
                    normal =
                    {
                        background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.8f))
                    },
                    font = DefaultFont
                };
                _contentStyle = style;
                return style;
            }
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        public abstract class Payload
        {
            public readonly Vector2 Size;

            protected Payload(Vector2 size)
            {
                Size = size;
            }

            public abstract void Draw(Rect rect);
        }

        public class StringPayload : Payload
        {
            public string Value;

            public StringPayload(string value) 
                : base(GetSize(value))
            {
                Value = value;
            }

            public override void Draw(Rect rect)
            {
                GUI.Label(rect, Value);
            }

            public static Vector2 GetSize(string value)
            {
                var size = new Vector2(value.Length * 10f, LineHeight);
                size.x = Mathf.Max(size.x, 200);
                return size;
            }
        }

        public class TexturePayload : Payload
        {
            public Texture Value;

            public TexturePayload(Texture value, float width=256, float height=256)
                : base(new Vector2(width, height))
            {
                Value = value;
            }

            public override void Draw(Rect rect)
            {
                GUI.DrawTexture(rect, Value);
            }
        }

        public class Logger : Payload
        {
            private readonly FixedSizeStack<string> _messages;
            private readonly bool _unityLog;

            public Logger(int size = 20, bool unityLog=true)
                : base(new Vector2(400f, LineHeight * size))
            {
                _messages = new FixedSizeStack<string>(size);
                _unityLog = unityLog;
            }

            public void Log(string message)
            {
                _messages.Push(message);
                if(_unityLog)
                    Debug.Log(message);
            }

            public void LogFormat(string message, params object[] args)
            {
                _messages.Push(string.Format(message, args));
                if (_unityLog)
                    Debug.LogFormat(message, args);
            }

            public override void Draw(Rect rect)
            {
                var currentY = rect.y;
                foreach (var message in _messages)
                {
                    GUI.Label(new Rect(rect.x, currentY, rect.width, LineHeight), message);
                    currentY += LineHeight;
                }
            }
        }

        public class DebugNode
        {
            public string Name;
            public bool IsExpanded = false;
            public float CacheTime = 1f;
            public Payload Payload;

            private float _lastUpdate;
            private readonly Dictionary<string, DebugNode> _children 
                = new Dictionary<string, DebugNode>();

            public DebugNode(string name)
            {
                Name = name;
            }

            public void Draw(ref float y, ref int index, ref bool collapseRequested, int depth, int cursorIndex)
            {
                var x = depth * Padding;

                var style = HeaderStyle;
                if (cursorIndex == index)
                {
                    style = SelectedHeaderStyle;
                    if (collapseRequested)
                    {
                        IsExpanded = !IsExpanded;
                        collapseRequested = false;
                    }
                }

                string header;
                if (IsExpanded)
                {
                    header = string.Format("- {0}", Name);
                }
                else
                {
                    header = string.Format("+ {0}", Name);
                }

                var headerRect = new Rect(x, y, HeaderColumn - x, LineHeight);
               

                GUI.Label(headerRect, header, style);


                if (IsExpanded && Payload != null)
                {
                    var payloadRect = new Rect(HeaderColumn, y, Payload.Size.x, Payload.Size.y);
                    GUI.Box(payloadRect, GUIContent.none, ContentStyle);
                    Payload.Draw(payloadRect);
                    y += Payload.Size.y - LineHeight;
                }

                y += LineHeight;
                index += 1;
                
                if (IsExpanded)
                {
                    foreach (var node in _children.Values)
                    {
                        node.Draw(ref y, ref index, ref collapseRequested, depth + 1, cursorIndex);
                    }
                }
            }

            public DebugNode GetOrCreateChild(string name)
            {
                if (_children.ContainsKey(name))
                    return _children[name];

                var node = new DebugNode(name);
                _children.Add(name, node);
                return node;
            }

            public void Touch()
            {
                _lastUpdate = Time.deltaTime;
            }
        }

        public const char PathSeparator = '/';

        public KeyCode OpenKey = KeyCode.F3;
        public KeyCode CollapseKey = KeyCode.F5;
        public KeyCode NavigateUp = KeyCode.PageUp;
        public KeyCode NavigateDown = KeyCode.PageDown;

        private bool _collapseRequested;
        private bool _isOpened;
        private int _cursor = 0;
        private readonly DebugNode _root = new DebugNode("Debug")
        {
            IsExpanded = true,
            Payload = new StringPayload("F5 - Expand/Collapse, PageUp/PageDown - Navigation")
        };
        private Logger _defaultLog;

        void Awake()
        {
            _defaultLog = GetLogger("Log");
        }
        
        void Update()
        {
            if (Input.GetKeyDown(OpenKey))
            {
                if(!_isOpened)
                    Open();
                else
                    Close();
            }

            if (Input.GetKeyDown(NavigateDown))
            {
                _cursor += 1;
            }

            if (Input.GetKeyDown(NavigateUp))
            {
                _cursor -= 1;
            }

            if (Input.GetKeyDown(CollapseKey))
            {
                _collapseRequested = true;
            }
        }

        public void Open()
        {
            _isOpened = true;
        }

        public void Close()
        {
            _isOpened = false;
        }

        public void Log(string message)
        {
            _defaultLog.Log(message);
        }

        public void LogFormat(string message, params object[] args)
        {
            _defaultLog.Log(string.Format(message, args));
        }

        public void Log(string message, UnityEngine.Object context)
        {
            _defaultLog.Log(message);
        }

        public DebugNode GetNode(string path)
        {
            var parts = path.Split(PathSeparator);
            return GetNode(parts);
        }

        public DebugNode GetNode(params string[] path)
        {
            var node = _root;
            foreach (var nodeName in path)
            {
                node = node.GetOrCreateChild(nodeName);
            }

            return node;
        }

        public void Display(DebugNode node, string value)
        {
            var payload = node.Payload as StringPayload;
            if (payload != null)
            {
                // Existing payload
                payload.Value = value;
            }
            else
            {
                // New payload
                node.Payload = new StringPayload(value);
            }
            node.Touch();
        }
        
        public void Display(DebugNode node, Texture texture)
        {
            var payload = node.Payload as TexturePayload;
            if (payload != null)
            {
                payload.Value = texture;
            }
            else
            {
                node.Payload = new TexturePayload(texture);
            }

            node.Touch();
        }

        public void Display(DebugNode node, float value)
        {
            Display(node, value.ToString(CultureInfo.InvariantCulture));
        }

        public void Display(string path, string value)
        {
            Display(GetNode(path), value);
        }

        public void Log(DebugNode node, string message)
        {
            var payload = node.Payload as Logger;
            if (payload != null)
            {
                // Existing payload
                payload.Log(message);
            }
            else
            {
                // New payload
                var p = new Logger();
                p.Log(message);
                node.Payload = p;
            }
            node.Touch();
        }

        public void DisplayFullPath(string value, params string[] path)
        {
            Display(GetNode(path), value);
        }

        public void Display(string path, float value)
        {
            Display(GetNode(path), value);
        }

        public void Log(string path, string message)
        {
            Log(GetNode(path), message);
        }

        public void LogFormat(string path, string message, params object[] args)
        {
            Log(GetNode(path), string.Format(message, args));
        }

        public void Display(string path, Texture texture)
        {
            Display(GetNode(path), texture);
        }

        public Logger GetLogger(string path)
        {
            var node = GetNode(path);
            var payload = GetNode(path).Payload as Logger;
            if (payload != null)
                return payload;
            
            // New payload
            var p = new Logger();
            node.Payload = p;
            return p;
        }

        void OnGUI()
        {
            if(!_isOpened)
                return;

            var y = 0f;
            var index = 0;
            var collapseRequested = _collapseRequested;
            _root.Draw(ref y, ref index, ref collapseRequested, depth:0, cursorIndex: _cursor);
            _collapseRequested = false;

            _cursor = Mathf.Clamp(_cursor, 0, index - 1);
        }
    }
}
