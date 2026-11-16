using QuanAnHope.Core;
using QuanAnHope.Game;
using UnityEditor;
using UnityEngine;

namespace QuanAnHope.EditorTools
{
    /// <summary>
    /// Lenh go loi "cho toi relic X" (docs/sprint-3/impl-plan.md muc 5): vong 1 chi 3 ngay nen kho thay thuong bo.
    /// Chi co trong Editor, chi bam duoc khi dang Play.
    /// </summary>
    public class RelicDebugWindow : EditorWindow
    {
        Vector2 scroll;

        [MenuItem("Quán Ăn Hope/Gỡ lỗi: cho relic")]
        static void Open()
        {
            GetWindow<RelicDebugWindow>("Cho relic");
        }

        void OnGUI()
        {
            GameController g = Application.isPlaying ? FindAnyObjectByType<GameController>() : null;
            if (g == null || g.Session == null)
            {
                EditorGUILayout.HelpBox("Bấm Play trong cảnh NhaHang rồi chọn relic.", MessageType.Info);
                return;
            }
            // Sprint 4: nhay thang toi vong 2 / ngay mat dien de thu (khong thu no, cong them tien).
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Tới ngày 4 (+2000G)")) g.DebugJumpToDay(4, 2000);
                if (GUILayout.Button("Tới ngày 8 mất điện (+2000G)")) g.DebugJumpToDay(8, 2000);
            }
            EditorGUILayout.LabelField(QuanAnHope.Game.RoomView.ShelfSummary(g.Session.Relics));
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (RelicDef r in g.Session.Balance.relics)
            {
                bool owned = g.Session.Relics.Has(r.id);
                using (new EditorGUI.DisabledScope(owned || g.Session.Relics.IsFull))
                    if (GUILayout.Button(r.id + " · " + r.name + (owned ? " (đang có)" : ""))) g.DebugGiveRelic(r.id);
            }
            EditorGUILayout.EndScrollView();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
