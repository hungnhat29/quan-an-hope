using System.Collections;
using QuanAnHope.Core;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>Truoc sprint 10 (constitution 2.0): khach het thanh cho bo di (B-19).</summary>
    public partial class GameController
    {
        /// <summary>
        /// F-092 AC-2: chup san icon (nguyen lieu, dung cu, lo gia vi, mon, relic) trong luc o menu, vai cai moi khung, de mo Tab /
        /// the huong dan khong bi khung > 50 ms vi chup icon lan dau.
        /// </summary>
        public bool IconsReady { get; private set; }

        IEnumerator PrewarmIcons()
        {
            yield return null;
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) yield break;
            RecipeBook book = Session != null ? Session.Recipes : null;
            if (book == null) yield break;
            int n = 0;
            foreach (SideIngredient x in book.sides) { PaperKit.Sticker(IconBook.Ingredient(x.id)); PaperKit.Sticker(IconBook.Ingredient(x.id, true)); if (++n % 3 == 0) yield return null; }
            foreach (MainIngredient x in book.mains) { PaperKit.Sticker(IconBook.Ingredient(x.id)); PaperKit.Sticker(IconBook.Ingredient(x.id, true)); if (++n % 3 == 0) yield return null; }
            for (int t = 0; t < 6; t++) { PaperKit.Sticker(IconBook.Ingredient("ca_chua", false, t)); if (++n % 3 == 0) yield return null; }
            foreach (string tool in RoomView.Tools) PaperKit.Sticker(IconBook.Tool(tool));
            PaperKit.Sticker(IconBook.Plate());
            PaperKit.Sticker(IconBook.Board());
            yield return null;
            foreach (string item in book.pantry)
            {
                Color c;
                if (Room == null || !Room.JarColor(item, out c)) c = UiKit.Cream;
                PaperKit.Sticker(IconBook.Jar(item, c));
                if (++n % 3 == 0) yield return null;
            }
            foreach (Recipe r in book.recipes) { PaperKit.Sticker(IconBook.Dish(r, book)); if (++n % 3 == 0) yield return null; }
            foreach (RelicDef r in Session.Balance.relics) { PaperKit.Sticker(IconBook.Relic(r)); if (++n % 3 == 0) yield return null; }
            if (Audio != null) Audio.Preload();
            if (book.sides.Length > 0) IconBook.WarmCrate(Ingredient.FromSide(book.sides[0], 2));
            yield return null;
            // dung san trang Thuc don (lan mo Tab dau khong phai dung ~40 dong), nap san moi bien the shader, don rac mot lan
            if (Tab != null) Tab.Prebuild();
            yield return null;
#pragma warning disable 618
            Shader.WarmupAllShaders();
#pragma warning restore 618
            System.GC.Collect();
            IconsReady = true;
        }

        /// <summary>Lan khach bo di gan nhat da hien (test doc).</summary>
        public LeaveEvent LastLeaveShown { get; private set; }

        /// <summary>Khach o ghe vua het thanh cho: mat gian, di ra cua, tien bay ra (constitution 2.0 muc 6.6).</summary>
        void OnCustomerLeft(LeaveEvent e)
        {
            LastLeaveShown = e;
            if (Room != null) Room.CustomerLeft(e.seat);
            if (reactionLeft != null && e.seat >= 0 && e.seat < reactionLeft.Length) reactionLeft[e.seat] = 0f;
            Sfx("khach_gian", "Customer_" + e.seat);
        }
    }
}
