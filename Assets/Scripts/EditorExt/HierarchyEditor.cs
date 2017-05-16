using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.EditorExt
{
    public class HierarchyEditor : EditorWindow
    {
        public class SpeciesWindow
        {
            public Rect Window;
            public Species Species;
        }

        Vector2 _scrollPosition = Vector2.zero;
        private readonly Rect _canvas = new Rect(0, 0, 2000, 2000);
        private SpeciesWindow[] _nodes;
        private const int IconWidth = 50;
        private const int IconHeight = 50;
        private const int Padding = 10;
        private const int VSpace = 20;

        [MenuItem("Window/Hierarchy editor")]
        static void ShowEditor()
        {
            var editor = EditorWindow.GetWindow<HierarchyEditor>();
            editor.titleContent = new GUIContent("Species heirarchy");
            editor.Init();
        }

        public void Init()
        {
            var guids = AssetDatabase.FindAssets("t:Species", new string[]{ "Assets/Resources" });
            Debug.LogFormat("Found {0} assets", guids.Length);

            var species = new List<Species>();
            foreach (var guid in guids)
            {
                var s = AssetDatabase.LoadAssetAtPath<Species>(AssetDatabase.GUIDToAssetPath(guid));
                if (s != null)
                {
                    species.Add(s);
                }
            }

            _nodes = new SpeciesWindow[species.Count];
            for (var i = 0; i < species.Count; i++)
            {
                var s = species[i];
                var w = new SpeciesWindow()
                {
                    Species = s,
                    Window = new Rect(20, i*40, IconWidth + Padding, IconHeight + Padding + VSpace)
                };
                _nodes[i] = w;
            }

            Placing();
        }
        
        int PlaceNodes(SpeciesWindow window, int startX, int y, List<SpeciesWindow> placed)
        {
            var x = startX;
            window.Window.position = new Vector2(
                Padding + x * (IconWidth + 4 * Padding),
                Padding + y * (IconHeight + 10 * Padding));
            placed.Add(window);

            var i = 0;
            
            foreach (var mutation in window.Species.Mutations)
            {
                if (mutation.Target != null)
                {
                    var childWindow = _nodes.FirstOrDefault(n => n.Species == mutation.Target);
                    if (!placed.Contains(childWindow))
                    {
                        x = PlaceNodes(childWindow, x, y + 1, placed);
                        x += 1;
                        i++;
                    }
                }
            }
            if(i > 0)
                return x - 1;
            return x;
        }

        void Placing()
        {
            var root = new List<SpeciesWindow>(_nodes.ToArray());

            // Remove non-root elements
            foreach (var node in _nodes)
            {
                foreach (var mutation in node.Species.Mutations)
                {
                    if (mutation.Target != null)
                    {
                        var child = root.FirstOrDefault(c => c.Species == mutation.Target);
                        if (child != null)
                            root.Remove(child);
                    }
                }
            }

            var placed = new List<SpeciesWindow>();
            var x = 0;
            foreach (var node in root)
            {
                x = PlaceNodes(node, x, 0, placed);
            }
        }

        void DrawConnections()
        {
            if(_nodes == null)
                return;

            foreach (var node in _nodes)
            {
                foreach (var mutation in node.Species.Mutations)
                {
                    if (mutation.Target != null)
                    {
                        var targetNode = _nodes.FirstOrDefault(n => n.Species.Equals(mutation.Target));
                        DrawNodeCurve(node.Window, targetNode.Window);
                    }
                }
            }
        }

        void OnGUI()
        {
            if (GUI.Button(new Rect(position.width - 80, 10, 50, 20), "Reload"))
            {
                Init();
            }

            _scrollPosition = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), _scrollPosition, _canvas);
            DrawConnections();

            BeginWindows();
            if (_nodes != null)
            {
                for (int index = 0; index < _nodes.Length; index++)
                {
                    var node = _nodes[index];
                    _nodes[index].Window = GUI.Window(index, node.Window, DrawNodeWindow, node.Species.name);
                }
            }
            else
            {
                Debug.LogWarning("Nodes is null");
            }
            EndWindows();
            GUI.EndScrollView();
        }

        void DrawNodeWindow(int index)
        {
            var species = _nodes[index].Species;
            GUI.backgroundColor = Color.black;
            if (GUI.Button(new Rect(Padding / 2, VSpace + Padding / 2, IconWidth, IconHeight), species.Icon.texture))
            {
                Selection.objects = new Object[] { species };
            }
            GUI.DragWindow();
        }

        void DrawNodeCurve(Rect start, Rect end)
        {
            var startPos = new Vector3(start.x + start.width / 2, start.y + start.height, 0);
            var endPos = new Vector3(end.x + end.width / 2, end.y - 10, 0);
            var startTan = startPos + Vector3.up * 50;
            var endTan = endPos + Vector3.down * 50;
            
            Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1.5f);

            Handles.color = Color.black;
            Handles.ConeCap(0, endPos + new Vector3(0, 5, -10f), Quaternion.Euler(-90, 0, 0), 10f);
        }
    }
}
