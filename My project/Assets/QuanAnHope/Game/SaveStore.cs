using System;
using System.IO;
using QuanAnHope.Core;
using UnityEngine;

namespace QuanAnHope.Game
{
    /// <summary>
    /// File luu game (F-052): mot ban luu duy nhat, ghi vao dau moi ngay, xoa khi thang / thua.
    /// Ghi ra file tam roi doi ten de mat dien giua chung khong lam hong ban luu cu.
    /// </summary>
    public static class SaveStore
    {
        const string FileName = "save.json";

        /// <summary>Thu muc luu; test doi sang thu muc tam de khong dong vao ban luu that.</summary>
        public static string Directory
        {
            get { return directory ?? (directory = Application.persistentDataPath); }
            set { directory = value; }
        }
        static string directory;

        public static string PathOf { get { return Path.Combine(Directory, FileName); } }

        public static bool Exists { get { return Load() != null; } }

        public static void Save(SaveData data)
        {
            if (data == null) return;
            System.IO.Directory.CreateDirectory(Directory);
            string tmp = PathOf + ".tmp";
            File.WriteAllText(tmp, JsonUtility.ToJson(data, true));
            if (File.Exists(PathOf)) File.Delete(PathOf);
            File.Move(tmp, PathOf);
        }

        /// <summary>Ban luu hop le hoac null (khong co file, file hong, khac phien ban).</summary>
        public static SaveData Load()
        {
            try
            {
                if (!File.Exists(PathOf)) return null;
                SaveData d = JsonUtility.FromJson<SaveData>(File.ReadAllText(PathOf));
                if (d == null || d.version != SaveData.CurrentVersion || d.day < 1) return null;
                return d;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Quán Ăn Hope: không đọc được bản lưu (" + e.Message + ").");
                return null;
            }
        }

        public static void Delete()
        {
            if (File.Exists(PathOf)) File.Delete(PathOf);
        }
    }
}
