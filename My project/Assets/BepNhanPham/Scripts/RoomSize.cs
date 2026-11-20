namespace BepNhanPham
{
    /// <summary>
    /// Kich thuoc can phong (constitution 1.5 Dieu 5: phong rong hon ban Art Bible 8 x 8 m).
    /// Moi goc (Goc1..4) duoc day ra phia tuong cua no them <see cref="Grow"/> met; do vat trong goc giu nguyen
    /// toa do theo khung cua goc nen khong lech nhau, chi co khoang trong giua phong rong ra.
    /// </summary>
    public static class RoomSize
    {
        /// <summary>Moi tuong lui ra them bay nhieu met.</summary>
        public const float Grow = 0.8f;

        /// <summary>Canh san phong (met): 8 + 2 x Grow.</summary>
        public const float Width = 8f + 2f * Grow;

        /// <summary>Nua canh san (tam phong toi mat trong tuong).</summary>
        public const float Half = Width * 0.5f;
    }
}
