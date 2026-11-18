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
            // Sprint 5 (impl plan muc 5): bat dau tu vong 3 / 4 / ngay chu no voi tien va relic trung vi cua bot gom bo
            // (balance/bot_report.md, 4.000 van, 16/11/2026). Ngay 21 kem mot dia Tuyet hao trong tu lanh nhu bot.
            EditorGUILayout.LabelField("Bắt đầu như bot gom bộ (trung vị):");
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Ngày 9 (1.885G)")) g.DebugStartAt(9, 1885, new[] { "V3", "V4", "C2", "S1", "F4", "F3", "F2", "F1" });
                if (GUILayout.Button("Ngày 15 (3.685G)")) g.DebugStartAt(15, 3685, new[] { "S4", "S1", "F10", "F9", "F2", "S3", "S2", "C3" });
                if (GUILayout.Button("Ngày 21 (18.322G)")) g.DebugStartAt(21, 18322, new[] { "S3", "F2", "F4", "S2", "S1", "F10", "S4", "F1" }, "ga__bi_do");
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
