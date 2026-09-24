using System.Collections.Generic;
using System.Linq;
using Oathsunder.Combat.Content;
using Oathsunder.Combat.Definitions;
using Oathsunder.Gameplay.Combat;
using UnityEditor;
using UnityEngine;

namespace Oathsunder.Editor.Combat
{
    /// <summary>
    /// Designer window: frame data table plus a per-move timeline (startup, active, recovery, invulnerability,
    /// cancel windows), computed live from the JSON so it can never disagree with the game.
    /// </summary>
    public sealed class FrameDataWindow : EditorWindow
    {
        private const float RowHeight = 18f;
        private const float NameWidth = 220f;
        private const float StatsWidth = 330f;

        private static readonly Color Startup = new Color(0.45f, 0.45f, 0.45f);
        private static readonly Color Active = new Color(0.9f, 0.25f, 0.25f);
        private static readonly Color Recovery = new Color(0.25f, 0.45f, 0.9f);
        private static readonly Color Invulnerable = new Color(1f, 1f, 1f, 0.55f);
        private static readonly Color Cancel = new Color(0.3f, 0.9f, 0.4f, 0.8f);

        private string[] _weapons = new string[0];
        private int _weaponIndex;
        private FighterBlueprint _blueprint;
        private List<FrameDataRow> _rows = new List<FrameDataRow>();
        private Vector2 _scroll;
        private float _pixelsPerFrame = 6f;
        private string _error;

        [MenuItem("Oathsunder/Combat/Frame Data", priority = 20)]
        private static void Open() => GetWindow<FrameDataWindow>("Frame Data").Refresh();

        private void OnEnable() => Refresh();

        private void OnFocus() => Refresh();

        private void Refresh()
        {
            var set = CombatContentValidator.LoadAll(out _);
            _weapons = set.MoveSetIds.Where(id => id != CombatMatchConfig.UniversalMoveSetId).OrderBy(id => id).ToArray();
            _weaponIndex = Mathf.Clamp(_weaponIndex, 0, Mathf.Max(0, _weapons.Length - 1));
            _error = null;
            _blueprint = null;
            _rows.Clear();
            if (_weapons.Length == 0 || !set.FighterIds.Any())
            {
                _error = "No weapon move sets or fighters found under " + CombatContentValidator.ContentFolder;
                return;
            }

            try
            {
                _blueprint = set.BuildBlueprint(set.FighterIds.First(), CombatMatchConfig.UniversalMoveSetId, _weapons[_weaponIndex]);
                _rows = FrameDataCalculator.Calculate(_blueprint);
            }
            catch (ContentValidationException exception)
            {
                _error = exception.Message;
            }

            Repaint();
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                int selected = EditorGUILayout.Popup(_weaponIndex, _weapons, EditorStyles.toolbarPopup, GUILayout.Width(220f));
                if (selected != _weaponIndex)
                {
                    _weaponIndex = selected;
                    Refresh();
                }

                GUILayout.Label("Zoom", EditorStyles.miniLabel, GUILayout.Width(36f));
                _pixelsPerFrame = GUILayout.HorizontalSlider(_pixelsPerFrame, 2f, 16f, GUILayout.Width(120f));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Copy Markdown", EditorStyles.toolbarButton))
                {
                    EditorGUIUtility.systemCopyBuffer = FrameDataCalculator.ToMarkdown(_rows);
                }

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
                {
                    Refresh();
                }
            }

            if (_error != null)
            {
                EditorGUILayout.HelpBox(_error, MessageType.Error);
                return;
            }

            if (_blueprint == null)
            {
                return;
            }

            DrawLegend();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var row in _rows)
            {
                var move = _blueprint.FindMove(row.MoveId);
                Rect rect = GUILayoutUtility.GetRect(position.width, RowHeight);
                DrawRow(rect, row, move);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawLegend()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                Swatch(Startup, "Startup");
                Swatch(Active, "Active");
                Swatch(Recovery, "Recovery");
                Swatch(Invulnerable, "Invulnerable");
                Swatch(Cancel, "Cancel window");
                GUILayout.FlexibleSpace();
            }
        }

        private static void Swatch(Color color, string label)
        {
            Rect rect = GUILayoutUtility.GetRect(12f, 12f, GUILayout.Width(12f));
            EditorGUI.DrawRect(rect, color);
            GUILayout.Label(label, EditorStyles.miniLabel);
        }

        private void DrawRow(Rect rect, FrameDataRow row, MoveDefinition move)
        {
            var nameRect = new Rect(rect.x, rect.y, NameWidth, rect.height);
            GUI.Label(nameRect, $"{row.Name}  ({row.MoveId})", EditorStyles.miniLabel);

            string hit = row.OnHit.HasValue ? row.OnHit.Value.ToString("+0;-0;0") : "KD";
            string block = row.OnBlock.HasValue ? row.OnBlock.Value.ToString("+0;-0;0") : "—";
            var statsRect = new Rect(nameRect.xMax, rect.y, StatsWidth, rect.height);
            GUI.Label(statsRect, $"dmg {row.Damage}  s{row.Startup} a{row.Active} r{row.Recovery}  hit {hit}  block {block}", EditorStyles.miniLabel);

            float x = statsRect.xMax;
            for (int frame = 1; frame <= row.Total; frame++)
            {
                Color color = frame < row.Startup ? Startup : (frame <= row.Startup + row.Active - 1 ? Active : Recovery);
                var cell = new Rect(x + (frame - 1) * _pixelsPerFrame, rect.y + 3f, _pixelsPerFrame - 1f, rect.height - 6f);
                EditorGUI.DrawRect(cell, color);
                if (move == null)
                {
                    continue;
                }

                foreach (var invulnerability in move.Invulnerability)
                {
                    if (invulnerability.Window.Contains(frame))
                    {
                        EditorGUI.DrawRect(new Rect(cell.x, cell.y, cell.width, 3f), Invulnerable);
                    }
                }

                foreach (var cancel in move.Cancels)
                {
                    if (cancel.Window.Contains(frame))
                    {
                        EditorGUI.DrawRect(new Rect(cell.x, cell.yMax - 3f, cell.width, 3f), Cancel);
                    }
                }
            }
        }
    }
}
