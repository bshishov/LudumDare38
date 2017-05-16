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

        public enum Alignment
        {
            ByMutation,
            ByFood,
            ByEnemy
        }

        private Vector2 _scrollPosition = Vector2.zero;
        private bool _showMutations = true;
        private bool _showEnemies = false;
        private bool _showFood = false;
        private readonly Rect _canvas = new Rect(0, 0, 2000, 2000);
        private SpeciesWindow[] _nodes;
        private const int IconWidth = 50;
        private const int IconHeight = 50;
        private const int Padding = 10;
        private const int VSpace = 20;
        private readonly Color _mutationConnectionColor = Color.black;
        private readonly Color _enemyConnectionColor = new Color(0.8f, 0f, 0f);
        private readonly Color _feedConnectionColor = new Color(0, 0.8f, 0f);

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

            Placing(Alignment.ByMutation);
        }

        IEnumerable<SpeciesWindow> IterateChildren(SpeciesWindow w, Alignment alignment)
        {
            if (alignment == Alignment.ByMutation)
            {
                foreach (var mutation in w.Species.Mutations)
                {
                    if (mutation.Target != null)
                    {
                        var childWindow = _nodes.FirstOrDefault(n => n.Species == mutation.Target);
                        if(childWindow != null)
                            yield return childWindow;
                    }
                }
            }

            if (alignment == Alignment.ByEnemy)
            {
                foreach (var enemy in w.Species.Enemies)
                {
                    if (enemy != null)
                    {
                        var childWindow = _nodes.FirstOrDefault(n => n.Species == enemy);
                        if (childWindow != null)
                            yield return childWindow;
                    }
                }
            }

            if (alignment == Alignment.ByFood)
            {
                foreach (var feedGroup in w.Species.Feed)
                {
                    if (feedGroup != null)
                    {
                        foreach (var targetNode in _nodes)
                        {
                            if (targetNode.Species != null && targetNode.Species.IsInGroup(feedGroup))
                                yield return targetNode;
                        }
                    }
                }
            }
        }
        
        int PlaceNodes(SpeciesWindow window, int startX, int y, List<SpeciesWindow> placed, Alignment alignment)
        {
            var x = startX;
            window.Window.position = new Vector2(
                Padding + x * (IconWidth + 4 * Padding),
                Padding * 2 + y * (IconHeight + 10 * Padding));
            placed.Add(window);

            var i = 0;

            foreach (var child in IterateChildren(window, alignment))
            {
                if (!placed.Contains(child))
                {
                    x = PlaceNodes(child, x, y + 1, placed, alignment);
                    x += 1;
                    i++;
                }
            }

            if(i > 0)
                return x - 1;
            return x;
        }

        void Placing(Alignment alignment)
        {
            var root = new List<SpeciesWindow>(_nodes.ToArray());

            // Remove non-root elements
            foreach (var node in _nodes)
            {
                foreach (var child in IterateChildren(node, alignment))
                {
                    if (root.Contains(child))
                        root.Remove(child);
                }
            }

            var placed = new List<SpeciesWindow>();
            var x = 0;
            foreach (var node in root)
            {
                x = PlaceNodes(node, x, 0, placed, alignment);
                x += 1;
            }
        }

        void DrawConnections()
        {
            if(_nodes == null)
                return;

            foreach (var node in _nodes)
            {
                if (_showMutations)
                {
                    foreach (var child in IterateChildren(node, Alignment.ByMutation))
                    {
                        DrawNodeCurve(node.Window, child.Window, _mutationConnectionColor);
                    }
                }

                if (_showEnemies)
                {
                    foreach (var child in IterateChildren(node, Alignment.ByEnemy))
                    {
                        DrawNodeCurve(node.Window, child.Window, _enemyConnectionColor);
                    }
                }

                if (_showFood)
                {
                    foreach (var child in IterateChildren(node, Alignment.ByFood))
                    {
                        DrawNodeCurve(node.Window, child.Window, _feedConnectionColor);
                    }
                }
            }
        }

        void OnGUI()
        {
            _scrollPosition = GUI.BeginScrollView(new Rect(0, 16, position.width, position.height - 16), _scrollPosition, _canvas);
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
            EndWindows();
            GUI.EndScrollView();


            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            DrawToolbar();
            GUILayout.EndHorizontal();
        }

        void DrawToolbar()
        {
            if (GUILayout.Button("Reload", EditorStyles.toolbarButton))
            {
                Init();
            }

            _showMutations = GUILayout.Toggle(_showMutations, "Mutations", EditorStyles.toolbarButton);
            _showEnemies = GUILayout.Toggle(_showEnemies, "Enemies", EditorStyles.toolbarButton);
            _showFood = GUILayout.Toggle(_showFood, "Food", EditorStyles.toolbarButton);

            if (GUILayout.Button("Align by mutation", EditorStyles.toolbarButton))
            {
                Placing(Alignment.ByMutation);
            }

            if (GUILayout.Button("Align by enemies", EditorStyles.toolbarButton))
            {
                Placing(Alignment.ByEnemy);
            }

            if (GUILayout.Button("Align by food", EditorStyles.toolbarButton))
            {
                Placing(Alignment.ByFood);
            }

            GUILayout.FlexibleSpace();
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

        void DrawNodeCurve(Rect start, Rect end, Color color)
        {
            var startPos = new Vector3(start.x + start.width / 2, start.y + start.height, 0);
            var endPos = new Vector3(end.x + end.width / 2, end.y - 10, 0);
            var startTan = startPos + Vector3.up * 50;
            var endTan = endPos + Vector3.down * 50;

            Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 1.5f);

            Handles.color = Color.black;
            Handles.ConeCap(0, endPos + new Vector3(0, 5, -5f), Quaternion.Euler(-90, 0, 0), 10f);
        }
    }
}
