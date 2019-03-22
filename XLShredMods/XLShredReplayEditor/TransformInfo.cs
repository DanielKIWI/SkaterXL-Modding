using System;
using UnityEngine;

namespace XLShredReplayEditor {


    [Serializable]
    public class TransformInfo {

        public TransformInfo(Transform t) {
            _position = new SerializableVector3(t.position);
            _rotation = new SerializableQuaternion(t.rotation);
            _scale = new SerializableVector3(t.localScale);
        }


        public void ApplyTo(Transform t) {
            t.position = position;
            t.rotation = rotation;
            t.localScale = scale;
        }


        private TransformInfo(Vector3 pos, Quaternion rot, Vector3 scale) {
            _position = new SerializableVector3(pos);
            _rotation = new SerializableQuaternion(rot);
            _scale = new SerializableVector3(scale);
        }


        public static TransformInfo Lerp(TransformInfo a, TransformInfo b, float t) {
            return new TransformInfo(Vector3.Lerp(a.position, b.position, t), Quaternion.Lerp(a.rotation, b.rotation, t), Vector3.Lerp(a.scale, b.scale, t));
        }

        public Vector3 position {
            get { return _position.Value; }
            set { _position.Value = value; }
        }
        private SerializableVector3 _position;
        
        public Quaternion rotation {
            get { return _rotation.Value; }
            set { _rotation.Value = value; }
        }
        private SerializableQuaternion _rotation;
        
        public Vector3 scale {
            get { return _scale.Value; }
            set { _scale.Value = value; }
        }
        private SerializableVector3 _scale;
        
    }

    [Serializable]
    public struct SerializableVector3 {
        float x, y, z;
        public SerializableVector3(Vector3 v) {
            x = v.x; y = v.y; z = v.z;
        }
        public Vector3 Value {
            get {
                return new Vector3(x, y, z);
            }
            set {
                x = value.x;
                y = value.y;
                z = value.z;
            }
        }
        public void Set(Vector3 vector) {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }
    }

    [Serializable]
    public struct SerializableQuaternion {
        float x, y, z, w;
        public SerializableQuaternion(Quaternion q) {
            x = q.x; y = q.y; z = q.z; w = q.w;
        }
        public Quaternion Value {
            get {
                return new Quaternion(x, y, z, w);
            }
            set {
                x = value.x;
                y = value.y;
                z = value.z;
                w = value.w;
            }
        }
    }

}
