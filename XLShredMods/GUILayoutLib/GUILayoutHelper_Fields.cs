using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GUILayoutLib {

    public class GUIValueCache {
        public Dictionary<string, object> valuesDict;
        public bool TryGetValueForKey(string key, out object value) {
            if (!valuesDict.ContainsKey(key)) {
                value = 0;
                return false;
            }
            value = valuesDict[key];
            return true;
        }
        public bool ContainsValueForKey(string key) {
            return valuesDict.ContainsKey(key);
        }
        public object GetValueForKey(string key) {
            return valuesDict[key];
        }
        public void Add(string key, object value) {
            valuesDict.Add(key, value);
        }
        public GUIValueCache() {
            valuesDict = new Dictionary<string, object>();
        }
    }
    public abstract class GUIField {
        public string Name;
        public abstract void ApplyCachedValue(GUIValueCache cache);
    }
    public class GUIFloatField : GUIField {
        public Action<float> Setter;

        public override void ApplyCachedValue(GUIValueCache cache) {
            if (cache.TryGetValueForKey(Name, out object value)) {
                if (value is float lastFloat) {
                    Setter(lastFloat);
                } else {
                    Debug.LogWarning("lastValue for Float Field " + Name + " is not a float!");
                }
            } else {
                Debug.LogWarning("No lastValue found for Field " + Name + "!");
            }
        }
    }
    public class GUIIntField : GUIField {
        public Action<int> Setter;

        public override void ApplyCachedValue(GUIValueCache cache) {
            if (cache.TryGetValueForKey(Name, out object value)) {
                if (value is int lastFloat) {
                    Setter(lastFloat);
                } else {
                    Debug.LogWarning("lastValue for Float Field " + Name + " is not a float!");
                }
            } else {
                Debug.LogWarning("No lastValue found for Field " + Name + "!");
            }
        }
    }
    public class GUIVector3Field : GUIField {
        public Action<Vector3> Setter;

        public override void ApplyCachedValue(GUIValueCache cache) {
            if (cache.TryGetValueForKey(Name, out object value)) {
                if (value is Vector3 lastVector) {
                    Setter(lastVector);
                } else {
                    Debug.LogWarning("lastValue for Float Field " + Name + " is not a Vector3!");
                }
            } else {
                Debug.LogWarning("No lastValue found for Field " + Name + "!");
            }
        }
    }
    public class GUIFieldGroup {
        public bool editMode;
        public List<GUIField> fields;
        public GUIFieldGroup(bool editMode) {
            fields = new List<GUIField>();
            this.editMode = editMode;
        }
        public void ApplyCachedValues(GUIValueCache cache) {
            foreach (var field in fields) {
                field.ApplyCachedValue(cache);
            }
        }
        public void AddFloatField(string name, Action<float> setter) {
            fields.Add(new GUIFloatField() {
                Name = name,
                Setter = setter
            });
        }
        public void AddIntField(string name, Action<int> setter) {
            fields.Add(new GUIIntField() {
                Name = name,
                Setter = setter
            });
        }
        public void AddVector3Field(string name, Action<Vector3> setter) {
            fields.Add(new GUIVector3Field() {
                Name = name,
                Setter = setter
            });
        }
    }

    public static partial class GUILayoutHelper {
        public static GUIFieldGroup currentGroup = null;
        public static void BeginFieldGroup(bool editMode = true) {
            if (currentGroup != null) {
                Debug.LogError("Tried to begin a GUI Field Group while another isn't ended yet.");
            }
            currentGroup = new GUIFieldGroup(editMode);
        }
        public static void EndFieldGroup(GUIValueCache cache) {
            if (currentGroup == null) {
                Debug.LogError("Tried to end a GUI Field Group but there wasn't a 'BeginFieldGroup' call");
                return;
            }
            if (currentGroup.editMode && GUILayout.Button("Apply")) {
                currentGroup.ApplyCachedValues(cache);
            }
            currentGroup = null;
        }
        public static bool FloatField(string name, GUIValueCache cache, Func<float> getter, Action<float> setter, float min = 0f, float max = 2f, float saveButtonWidth = 40f) {
            if (currentGroup != null) {
                currentGroup.AddFloatField(name, setter);
            }
            bool didSave = false;
            if (!cache.ContainsValueForKey(name)) {
                cache.valuesDict.Add(name, getter());
            }
            float num = (float)cache.valuesDict[name];
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.Label(name);
                    float.TryParse(GUILayout.TextField(num.ToString("0.000000")), out num);
                    num = GUILayout.HorizontalSlider(num, min, max);
                    cache.valuesDict[name] = num;
                }
                GUILayout.EndVertical();
                if (currentGroup == null) {
                    if (GUILayout.Button("Apply", GUILayout.ExpandHeight(true), GUILayout.Width(saveButtonWidth))) {
                        setter(num);
                        didSave = true;
                    }
                }
            }
            GUILayout.EndHorizontal();
            return didSave;
        }
        public static bool IntField(string name, GUIValueCache cache, Func<int> getter, Action<int> setter, int min = 0, int max = 10, float saveButtonWidth = 40f) {
            if (currentGroup != null) {
                currentGroup.AddIntField(name, setter);
            }
            bool didSave = false;
            if (!cache.ContainsValueForKey(name)) {
                cache.valuesDict.Add(name, getter());
            }
            int num = (int)cache.valuesDict[name];
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.Label(name);
                    int.TryParse(GUILayout.TextField(num.ToString("0")), out num);
                    num = Mathf.RoundToInt(GUILayout.HorizontalSlider((float)num, (float)min, (float)max));
                    cache.valuesDict[name] = num;
                }
                GUILayout.EndVertical();
                if (currentGroup == null) {
                    if (GUILayout.Button("Apply", GUILayout.ExpandHeight(true), GUILayout.Width(saveButtonWidth))) {
                        setter(num);
                        didSave = true;
                    }
                }
            }
            GUILayout.EndHorizontal();
            return didSave;
        }

        public static bool Vector3Field(string name, GUIValueCache cache, Func<Vector3> getter, Action<Vector3> setter, float saveButtonWidth = 40f) {
            if (currentGroup != null) {
                currentGroup.AddVector3Field(name, setter);
            }
            bool didSave = false;
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label(name);
            if (!cache.ContainsValueForKey(name)) {
                cache.valuesDict.Add(name, getter());
            }
            Vector3 vector = (Vector3)cache.valuesDict[name];
            float x = vector.x;
            float y = vector.y;
            float z = vector.z;
            float.TryParse(GUILayout.TextField(x.ToString("0.00")), out x);
            float.TryParse(GUILayout.TextField(y.ToString("0.00")), out y);
            float.TryParse(GUILayout.TextField(z.ToString("0.00")), out z);
            Vector3 newValue = new Vector3(x, y, z);
            cache.valuesDict[name] = newValue;
            GUILayout.EndVertical();
            if (currentGroup == null) {
                if (GUILayout.Button("Apply", GUILayout.ExpandHeight(true), GUILayout.Width(saveButtonWidth))) {
                    setter(newValue);
                    didSave = true;
                }
            }
            GUILayout.EndHorizontal();
            return didSave;
        }

        public static void Vector3Label(string name, Vector3 value) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name);
            GUILayout.FlexibleSpace();
            GUILayout.Label(String.Format("x: {0,-10} y: {1,-10} z: {2,-10}", value.x, value.y, value.z));
            GUILayout.EndHorizontal();
        }

        public static void QuaternionLabel(string name, Quaternion value) {
            Vector3Label(name, value.eulerAngles);
        }
    }
}
