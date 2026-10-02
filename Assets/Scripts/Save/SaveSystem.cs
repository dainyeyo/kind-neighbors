using System;
using System.IO;
using UnityEngine;

namespace KindNeighbors.Save
{
    /// <summary>세이브 슬롯 하나. persistentDataPath/save.json 에 JSON으로 저장한다.</summary>
    public static class SaveSystem
    {
        public static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

        public static bool HasSave => File.Exists(FilePath);

        public static void Save(SaveData data)
        {
            data.version = SaveData.CurrentVersion;
            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
            Debug.Log($"[Save] 저장: {data.Phase} → {FilePath}");
        }

        public static bool TryLoad(out SaveData data)
        {
            data = null;
            if (!HasSave)
                return false;

            try
            {
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Save] 세이브 파일을 읽지 못했습니다: {e.Message}");
                return false;
            }

            if (data == null || data.version > SaveData.CurrentVersion)
            {
                Debug.LogWarning("[Save] 지원하지 않는 세이브 버전입니다.");
                data = null;
                return false;
            }

            Migrate(data);
            return true;
        }

        public static void Delete()
        {
            if (HasSave)
                File.Delete(FilePath);
        }

        /// <summary>이전 버전 세이브를 현재 형식으로 변환한다. 버전이 올라갈 때마다 단계를 추가한다.</summary>
        static void Migrate(SaveData data)
        {
            // 예: if (data.version < 2) { ...v1 → v2 변환...; data.version = 2; }
            data.version = SaveData.CurrentVersion;
        }
    }
}
