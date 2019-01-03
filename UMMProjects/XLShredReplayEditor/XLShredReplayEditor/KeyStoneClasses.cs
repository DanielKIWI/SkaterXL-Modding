using System;
using System.Collections.Generic;
using UnityEngine;

namespace XLShredReplayEditor {


    [Serializable]
    public class KeyStone {
        public virtual void ApplyTo(Transform t) {
            t.position = this.position;
            t.rotation = this.rotation;
        }

        public float time;

        public Vector3 position;

        public Quaternion rotation;
        public float cameraFOV;
    }

    [Serializable]
    public class FreeCameraKeyStone : KeyStone {
        public FreeCameraKeyStone(Transform cameraTransform, float fov, float t) {
            this.position = cameraTransform.position;
            this.rotation = cameraTransform.rotation;
            this.time = t;
            this.cameraFOV = fov;
        }

        public FreeCameraKeyStone(Vector3 p, Quaternion r, float fov, float t) {
            this.position = p;
            this.rotation = r;
            this.time = t;
            this.cameraFOV = fov;
        }

        public static FreeCameraKeyStone Lerp(KeyStone a, KeyStone b, float time) {
            float t = (time - a.time) / (b.time - a.time);
            return new FreeCameraKeyStone(Vector3.Lerp(a.position, b.position, t), Quaternion.Lerp(a.rotation, b.rotation, t), Mathf.Lerp(a.cameraFOV, b.cameraFOV, t), time);
        }

        public FreeCameraKeyStone(KeyStone ks) {
            this.position = ks.position;
            this.rotation = ks.rotation;
            this.time = ks.time;
            this.cameraFOV = ks.cameraFOV;
        }
    }

    [Serializable]
    public class OrbitCameraKeyStone : KeyStone {
        public OrbitCameraKeyStone(Vector3Radial radialPos, float fov, float t) {
            this.radialPos = radialPos;
            this.position = PlayerController.Instance.skaterController.skaterTransform.position + radialPos.cartesianCoords;
            this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - this.position, Vector3.up);
            this.time = t;
            this.cameraFOV = fov;
        }
        public OrbitCameraKeyStone(Vector3 v, float fov, float t) {
            this.radialPos = new Vector3Radial(v);
            this.position = PlayerController.Instance.skaterController.skaterTransform.position + this.radialPos.cartesianCoords;
            this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - this.position, Vector3.up);
            this.time = t;
            this.cameraFOV = fov;
        }

        public override void ApplyTo(Transform t) {
            t.position = PlayerController.Instance.skaterController.skaterTransform.position + this.radialPos.cartesianCoords;
            t.LookAt(PlayerController.Instance.skaterController.skaterTransform, Vector3.up);
        }

        public static OrbitCameraKeyStone Lerp(OrbitCameraKeyStone a, OrbitCameraKeyStone b, float time) {
            float t = (time - a.time) / (b.time - a.time);
            return new OrbitCameraKeyStone(Vector3Radial.Lerp(a.radialPos, b.radialPos, t), Mathf.Lerp(a.cameraFOV, b.cameraFOV, t), time);
        }


        public Vector3Radial radialPos;
    }

    [Serializable]
    public class TripodCameraKeyStone : KeyStone {
        public TripodCameraKeyStone(Vector3 p, float fov, float t) {
            this.position = p;
            this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - this.position, Vector3.up);
            this.time = t;
            this.cameraFOV = fov;
        }
        public TripodCameraKeyStone(Transform cameraTransform, float fov, float t) {
            this.position = cameraTransform.position;
            this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position - this.position, Vector3.up);
            this.time = t;
            this.cameraFOV = fov;
        }

        public override void ApplyTo(Transform t) {
            t.position = this.position;
            t.LookAt(PlayerController.Instance.skaterController.skaterTransform, Vector3.up);
        }

        public static TripodCameraKeyStone Lerp(KeyStone a, KeyStone b, float time) {
            float t = (time - a.time) / (b.time - a.time);
            return new TripodCameraKeyStone(Vector3.Lerp(a.position, b.position, t), Mathf.Lerp(a.cameraFOV, b.cameraFOV, t), time);
        }
    }
}
