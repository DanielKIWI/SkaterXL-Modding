using System;
using System.Collections.Generic;
using UnityEngine;

namespace XLShredReplayEditor {

    #region Data Types
    public class QuaternionSphereData {
        public Vector3Radial sphereAxis;
        public float angle;

        public QuaternionSphereData(Vector3Radial sphereAxis, float angle) {
            this.sphereAxis = sphereAxis;
            this.angle = angle;
        }

        public QuaternionSphereData(Quaternion q) {
            q.ToAngleAxis(out angle, out Vector3 axis);

            sphereAxis = new Vector3Radial(axis);
        }

        public Quaternion ToQuaternion() {
            return Quaternion.AngleAxis(angle, sphereAxis.CartesianCoords);
        }

        public static Quaternion ToQuaternion(QuaternionSphereData q) {
            return Quaternion.AngleAxis(q.angle, q.sphereAxis.CartesianCoords);
        }
    }

    public enum CurveKeyType {
        PosX, PosY, PosZ, SphereRotTheta, SphereRotPhi, SphereRotAngle, Fov, Radius, FocusYOffset, FreeCam, OrbitCam, TripodCam
    }

    public class KeyStoneResult {
        public Vector3 position;
        public Quaternion rotation;
        public float fov;

        public void ApplyTo(Transform t) {
            t.position = position;
            t.rotation = rotation;
            t.GetComponent<Camera>().fieldOfView = this.fov;
        }
    }
    #endregion

    #region General Classes
    public static class KeyStoneCurves {
        public static Dictionary<CurveKeyType, AnimationCurve> animationCurves = new Dictionary<CurveKeyType, AnimationCurve>();

        static KeyStoneCurves() {
            foreach (CurveKeyType type in (CurveKeyType []) Enum.GetValues(typeof(CurveKeyType))) {
                animationCurves.Add(type, new AnimationCurve());
            }
        }

        public static void RemoveKeyframes(int num) {
            if (animationCurves.Count == 0) {
                return;
            }

            foreach (KeyValuePair<CurveKeyType, AnimationCurve> e in animationCurves) {
                e.Value.RemoveKey(num);
            }

            SmoothTangents();
        }

        public static KeyStoneResult EvaluateCurves(float time) {
            KeyStoneResult freeCamResult, orbitCamResult, tripodCamResult;
            freeCamResult = orbitCamResult = tripodCamResult = null;

            float freeCamAmount = animationCurves[CurveKeyType.FreeCam].Evaluate(time);
            float orbitCamAmount = animationCurves[CurveKeyType.FreeCam].Evaluate(time);
            float tripodCamAmount = animationCurves[CurveKeyType.FreeCam].Evaluate(time);

            if (freeCamAmount > 0) {
                freeCamResult = FreeCameraKeyStone.Evaluate(time);
            }

            //Console.WriteLine($"{{x: {time}, y: {freeCamResult.position.z} }},");

            return freeCamResult;
        }

        public static void SmoothTangents() {
            
            foreach (KeyValuePair<CurveKeyType, AnimationCurve> e in KeyStoneCurves.animationCurves) {
                for (int i = 1; i < e.Value.keys.Length -1; i++) {
                    e.Value.SmoothTangents(i, 0);
                }
            }
        }
    }

    [Serializable]
    public class KeyStone {
        public Vector3 position;
        public QuaternionSphereData rotation;
        public float time;
        public float fov;
        private Vector3 velocity = Vector3.zero;

        protected Dictionary<CurveKeyType, Keyframe> keys = new Dictionary<CurveKeyType, Keyframe>();

        public virtual void AddKeyframes() {
            // In subclass, call this after setting all the key values
            foreach (KeyValuePair<CurveKeyType, AnimationCurve> e in KeyStoneCurves.animationCurves) {
                e.Value.AddKey(keys[e.Key]);
            }

            KeyStoneCurves.SmoothTangents();
        }

        public static KeyStoneResult Evaluate(float time) { return null; }
    }

    public abstract class KeyStoneWithYOffset : KeyStone {
        public float focusOffsetY;
    }
    #endregion

    #region KeyStone Implementations
    [Serializable]
    public class FreeCameraKeyStone : KeyStone {

        public FreeCameraKeyStone(Transform cameraTransform, float fov, float time) {
            position = cameraTransform.position;
            rotation = new QuaternionSphereData(cameraTransform.rotation);
            this.time = time;
            this.fov = fov;
        }

        public FreeCameraKeyStone(KeyStone ks) {
            position = ks.position;
            rotation = ks.rotation;

            time = ks.time;
            fov = ks.fov;
        }

        public override void AddKeyframes() {
            keys[CurveKeyType.PosX] = new Keyframe(time, position.x);
            keys[CurveKeyType.PosY] = new Keyframe(time, position.y);
            keys[CurveKeyType.PosZ] = new Keyframe(time, position.z);
            keys[CurveKeyType.SphereRotTheta] = new Keyframe(time, rotation.sphereAxis.theta);
            keys[CurveKeyType.SphereRotPhi] = new Keyframe(time, rotation.sphereAxis.phi);
            keys[CurveKeyType.SphereRotAngle] = new Keyframe(time, rotation.angle);
            keys[CurveKeyType.Fov] = new Keyframe(time, fov);
            keys[CurveKeyType.Radius] = new Keyframe(time, 0f);
            keys[CurveKeyType.FocusYOffset] = new Keyframe(time, 0f);
            keys[CurveKeyType.FreeCam] = new Keyframe(time, 1f);
            keys[CurveKeyType.OrbitCam] = new Keyframe(time, 0f);
            keys[CurveKeyType.TripodCam] = new Keyframe(time, 0f);

            base.AddKeyframes();
        }

        public new static KeyStoneResult Evaluate(float time) {
            Vector3 pos = new Vector3(  KeyStoneCurves.animationCurves[CurveKeyType.PosX].Evaluate(time),
                                        KeyStoneCurves.animationCurves[CurveKeyType.PosY].Evaluate(time),
                                        KeyStoneCurves.animationCurves[CurveKeyType.PosZ].Evaluate(time));
            Vector3Radial sphereAxis = new Vector3Radial(KeyStoneCurves.animationCurves[CurveKeyType.SphereRotPhi].Evaluate(time),
                                                KeyStoneCurves.animationCurves[CurveKeyType.SphereRotTheta].Evaluate(time), 1f);
            Quaternion rot = new QuaternionSphereData(sphereAxis, KeyStoneCurves.animationCurves[CurveKeyType.SphereRotAngle].Evaluate(time)).ToQuaternion();
            float fov = KeyStoneCurves.animationCurves[CurveKeyType.Fov].Evaluate(time);

            return new KeyStoneResult() {
                position = pos,
                rotation = rot,
                fov = fov
            };
        }
    }
    [Serializable]
    public class OrbitCameraKeyStone : KeyStoneWithYOffset {
        public Vector3Radial radialPos;

        public OrbitCameraKeyStone(Vector3Radial radialPos, float yOffset, float fov, float t) {
            this.radialPos = radialPos;
            position = PlayerController.Instance.skaterController.skaterTransform.position + yOffset * Vector3.up + radialPos.CartesianCoords;
            rotation = new QuaternionSphereData(Quaternion.LookRotation(-radialPos.CartesianCoords, Vector3.up));
            this.time = t;
            this.fov = fov;
            this.focusOffsetY = yOffset;
        }
        /*
        public override void ApplyTo(Transform t) {
            t.position = PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up + this.radialPos.cartesianCoords;
            t.LookAt(PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up, Vector3.up);
            t.GetComponent<Camera>().fieldOfView = this.fov;
        }

        public static OrbitCameraKeyStone Lerp(OrbitCameraKeyStone a, OrbitCameraKeyStone b, float time) {
            float t = (time - a.Time) / (b.Time - a.Time);
            return new OrbitCameraKeyStone(Vector3Radial.Lerp(a.radialPos, b.radialPos, Mathf.SmoothStep(0f, 1f, t)), Mathf.Lerp(a.focusOffsetY, b.focusOffsetY, Mathf.SmoothStep(0f, 1f, t)), Mathf.Lerp(a.fov, b.fov, Mathf.SmoothStep(0f, 1f, t)), time);
        }

        public new static KeyStoneResult Evaluate(float time) {



            float fov = KeyStoneCurves.animationCurves[CurveKeyType.Fov].Evaluate(time);

            return new KeyStoneResult() {
                position = pos,
                rotation = rot,
                fov = fov
            };
        }*/
    }

    [Serializable]
    public class TripodCameraKeyStone : KeyStoneWithYOffset {
        public TripodCameraKeyStone(Transform cameraTransform, float yOffset, float fov, float t) {
            position = cameraTransform.position;
            //rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position + yOffset * Vector3.up - position, Vector3.up);
            this.time = t;
            this.fov = fov;
            this.focusOffsetY = yOffset;
        }

        /*
        public override void ApplyTo(Transform t) {
            t.position = position;
            t.LookAt(PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up, Vector3.up);
            t.GetComponent<Camera>().fieldOfView = this.fov;
        }

        public static TripodCameraKeyStone Lerp(KeyStone a, KeyStone b, float time) {
            float t = (time - a.Time) / (b.Time - a.Time);
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
            return new TripodCameraKeyStone(Vector3.Lerp(a.position, b.position, Mathf.SmoothStep(0f, 1f, t)), Mathf.Lerp(a.fov, b.fov, Mathf.SmoothStep(0f, 1f, t)), yOffset, time);
        }*/
    }
    #endregion
}
