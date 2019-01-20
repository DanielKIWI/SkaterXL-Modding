using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace XLShredSessionMarkerManager {
    [Serializable]
    public class TeleporterLocation {
        public string name;
        public Vector3[] positions;
        public Quaternion[] rotations;
        public void SaveToDirectory(string dirPath, bool overrideExisting = true) {
            string path = dirPath + "\\" + name + ".json";
            string json = JsonUtility.ToJson(this);
            if (File.Exists(path) && !overrideExisting) {
                return;
            }
            File.WriteAllText(path, json);
        }
        public static TeleporterLocation LoadFromFile(string path) {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<TeleporterLocation>(json);
        }
        public TeleporterLocation(string name) {
            this.name = name;
            positions = Main.setPosField.Value;
            rotations = Main.setRotField.Value;
        }
        public void Apply() {
            Main.setPosField.Value = positions;
            Main.setRotField.Value = rotations;
        }
    }
}
