using System;
using System.Collections.Generic;
using UnityEngine;

namespace XLShredReplayEditor {


    [Serializable]
    public class KeyStone {
        public virtual void ApplyTo(Camera c) {
            c.transform.position = this.position;
            c.transform.rotation = this.rotation;
            c.fieldOfView = cameraFOV;
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
    public abstract class KeyStoneWithYOffset : KeyStone {
        public float focusOffsetY;
    }
    [Serializable]
    public class OrbitCameraKeyStone : KeyStoneWithYOffset {
        public Vector3Radial radialPos;

        public OrbitCameraKeyStone(Vector3Radial radialPos, float yOffset, float fov, float t) {
            this.radialPos = radialPos;
            this.position = PlayerController.Instance.skaterController.skaterTransform.position + yOffset * Vector3.up + radialPos.cartesianCoords;
            this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position + yOffset * Vector3.up - this.position, Vector3.up);
            this.time = t;
            this.cameraFOV = fov;
            this.focusOffsetY = yOffset;
        }
        public OrbitCameraKeyStone(Vector3 v, float yOffset, float fov, float t) {
            this.radialPos = new Vector3Radial(v);
            this.position = PlayerController.Instance.skaterController.skaterTransform.position + yOffset * Vector3.up + this.radialPos.cartesianCoords;
            this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position + yOffset * Vector3.up - this.position, Vector3.up);
            this.time = t;
            this.cameraFOV = fov;
            this.focusOffsetY = yOffset;
        }

        public override void ApplyTo(Camera c) {
            c.transform.position = PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up + this.radialPos.cartesianCoords;
            c.transform.LookAt(PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up, Vector3.up);
            c.fieldOfView = cameraFOV;
        }

        public static OrbitCameraKeyStone Lerp(OrbitCameraKeyStone a, OrbitCameraKeyStone b, float time) {
            float t = (time - a.time) / (b.time - a.time);
            return new OrbitCameraKeyStone(Vector3Radial.Lerp(a.radialPos, b.radialPos, t), Mathf.Lerp(a.focusOffsetY, b.focusOffsetY, t), Mathf.Lerp(a.cameraFOV, b.cameraFOV, t), time);
        }
    }

    [Serializable]
    public class TripodCameraKeyStone : KeyStoneWithYOffset {
        public TripodCameraKeyStone(Vector3 p, float yOffset, float fov, float t) {
            this.position = p;
            this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position + yOffset * Vector3.up - this.position, Vector3.up);
            this.time = t;
            this.cameraFOV = fov;
            this.focusOffsetY = yOffset;
        }
        public TripodCameraKeyStone(Transform cameraTransform, float yOffset, float fov, float t) {
            this.position = cameraTransform.position;
            this.rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position + yOffset * Vector3.up - this.position, Vector3.up);
            this.time = t;
            this.cameraFOV = fov;
            this.focusOffsetY = yOffset;
        }

        public override void ApplyTo(Camera c) {
            c.transform.position = this.position;
            c.transform.LookAt(PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up, Vector3.up);
            c.fieldOfView = cameraFOV;
        }

        public static TripodCameraKeyStone Lerp(KeyStone a, KeyStone b, float time) {
            float t = (time - a.time) / (b.time - a.time);
            float yOffset = 0f;
            KeyStoneWithYOffset ayo = a as KeyStoneWithYOffset;
            KeyStoneWithYOffset byo = b as KeyStoneWithYOffset;
            if (ayo != null) {
                if (byo != null) {
                    yOffset = Mathf.Lerp(ayo.focusOffsetY, byo.focusOffsetY, t);
                } else {
                    yOffset = ayo.focusOffsetY;
                }
            } else if (byo != null) {
                yOffset = byo.focusOffsetY;
            }
            return new TripodCameraKeyStone(Vector3.Lerp(a.position, b.position, t), yOffset, Mathf.Lerp(a.cameraFOV, b.cameraFOV, t), time);
        }
    }
}
