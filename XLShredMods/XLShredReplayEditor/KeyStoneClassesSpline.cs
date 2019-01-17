/*using System;
using System.Collections.Generic;
using UnityEngine;

namespace XLShredReplayEditor {

    [Serializable]
    public class KeyStone {
        public virtual void ApplyTo(Transform t) {
            t.position = Position;
            t.rotation = Rotation;
            t.GetComponent<Camera>().fieldOfView = this.CameraFOV;
        }

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public float Time { get; set; }
        public float SmoothTime { get; set; } = 0.3F;
        public float CameraFOV { get; set; }

        private Vector3 velocity = Vector3.zero;
        public Spline.QuaternionSphereData rotSphere;
        public Vector3 posTan;
        public Spline.QuaternionSphereData rotSphereTan;

        protected static 

        protected static float GetSplineTangent(float past, float cur, float fut, float pastToCurrent = 1, float currentToFuture = 1) {
            float deltaPastToCur = Mathf.Repeat(cur - past, 360);
            float deltaCurToFut = Mathf.Repeat(fut - cur, 360);

            if (deltaPastToCur > 180) deltaPastToCur -= 360;
            if (deltaCurToFut > 180) deltaCurToFut -= 360;

            if (currentToFuture == 0) {
                return deltaPastToCur / pastToCurrent;
            }
            if (pastToCurrent == 0) {
                return deltaCurToFut / currentToFuture;
            }
            return (deltaPastToCur + deltaCurToFut * (pastToCurrent / currentToFuture)) / (pastToCurrent + currentToFuture);
        }

        protected static float Spline(float pos0, float pos1, float tan0, float tan1, float factor, float duration) {
            float factor2 = factor * factor;
            float factor3 = factor2 * factor;

            float val = pos0 * (2 * factor3 - 3 * factor2 + 1) +
                   pos1 * (3 * factor2 - 2 * factor3) +
                   (tan0 * (factor3 - 2 * factor2 + factor) +
                   tan1 * (factor3 - factor2)) * duration;

            if (val > 360) val -= 360;
            else if (val < 0) val += 360;

            return val;
        }

        protected static Vector2 GetSplineTangent(Vector2 past, Vector2 cur, Vector2 fut, float pastToCurrent = 1, float currentToFuture = 1) {
            return new Vector2( GetSplineTangent(past.x, cur.x, fut.x, pastToCurrent, currentToFuture),
                                GetSplineTangent(past.y, cur.y, fut.y, pastToCurrent, currentToFuture) );
        }

        protected static Vector2 Spline(Vector2 pos0, Vector2 pos1, Vector2 tan0, Vector2 tan1, float factor, float duration) {
            return new Vector2( Spline(pos0.x, pos1.x, tan0.x, tan1.x, factor, duration),
                                Spline(pos0.y, pos1.y, tan0.y, tan1.y, factor, duration) );
        }

        protected static Vector3 GetSplineTangent(Vector3 past, Vector3 cur, Vector3 fut, float pastToCurrent = 1, float currentToFuture = 1) {
            if (currentToFuture == 0) {
                return (cur - past) / pastToCurrent;
            }
            if (pastToCurrent == 0) {
                return (fut - cur) / currentToFuture;
            }
            return (cur - past + (fut - cur) * (pastToCurrent / currentToFuture)) / (pastToCurrent + currentToFuture);
        }

        protected static Vector3 Spline(Vector3 pos0, Vector3 pos1, Vector3 tan0, Vector3 tan1, float factor, float duration) {
            float duration = b.Time - a.Time;
            float t = (time - a.Time) / duration;

            float factor2 = factor * factor;
            float factor3 = factor2 * factor;

            return pos0 * (2 * factor3 - 3 * factor2 + 1) +
                 pos1 * (3 * factor2 - 2 * factor3) +
                (tan0 * (factor3 - 2 * factor2 + factor) +
                 tan1 * (factor3 - factor2));
        }

        public void CalculateTangent(KeyStone prev, KeyStone next) {
            posTan = Spline.PointTangent(prev.Position, Position, next.Position, prev.Time, Time, next.Time);
            prev.rotSphere = new Spline.QuaternionSphereData(prev.Rotation);
            rotSphere = new Spline.QuaternionSphereData(Rotation);
            next.rotSphere = new Spline.QuaternionSphereData(next.Rotation);
            rotSphereTan = Spline.QuaternionTangent(prev.rotSphere, rotSphere, next.rotSphere, prev.Time, Time, next.Time);
        }
    }

    [Serializable]
    public class FreeCameraKeyStone : KeyStone {
        public FreeCameraKeyStone(Transform cameraTransform, float fov, float t) {
            Position = cameraTransform.position;
            Rotation = cameraTransform.rotation;
            cameraTransform.rotation.ToAngleAxis(out float angle, out Vector3 axis);
            Time = t;
            CameraFOV = fov;
        }

        public FreeCameraKeyStone(Vector3 p, Quaternion r, float fov, float t) {
            Position = p;
            Rotation = r;
            r.ToAngleAxis(out float angle, out Vector3 axis);
            this.Time = t;
            this.CameraFOV = fov;
        }

        public static FreeCameraKeyStone Lerp(KeyStone a, KeyStone b, float time) {
            float duration = b.Time - a.Time;
            float t = (time - a.Time) / duration;


            Vector3 pos = Spline.PointSpline(a.Position, b.Position, a.posTan, b.posTan, t, duration);
            Quaternion rot = Spline.QuaternionSpline(a.rotSphere, b.rotSphere, a.rotSphereTan, b.rotSphereTan, t, duration);

            Console.WriteLine($"{{x: {time}, y: {pos.z} }},");
            return new FreeCameraKeyStone(pos, rot, Mathf.Lerp(a.CameraFOV, b.CameraFOV, t), time);
        }

        public FreeCameraKeyStone(KeyStone ks) {
            Position = ks.Position;
            Rotation = ks.Rotation;

            this.Time = ks.Time;
            this.CameraFOV = ks.CameraFOV;
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
            Position = PlayerController.Instance.skaterController.skaterTransform.position + yOffset * Vector3.up + radialPos.cartesianCoords;
            Rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position + yOffset * Vector3.up - Position, Vector3.up);
            this.Time = t;
            this.CameraFOV = fov;
            this.focusOffsetY = yOffset;
        }
        public OrbitCameraKeyStone(Vector3 v, float yOffset, float fov, float t) {
            this.radialPos = new Vector3Radial(v);
            Position = PlayerController.Instance.skaterController.skaterTransform.position + yOffset * Vector3.up + this.radialPos.cartesianCoords;
            Rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position + yOffset * Vector3.up - Position, Vector3.up);
            this.Time = t;
            this.CameraFOV = fov;
            this.focusOffsetY = yOffset;
        }

        public override void ApplyTo(Transform t) {
            t.position = PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up + this.radialPos.cartesianCoords;
            t.LookAt(PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up, Vector3.up);
            t.GetComponent<Camera>().fieldOfView = this.CameraFOV;
        }

        public static OrbitCameraKeyStone Lerp(OrbitCameraKeyStone a, OrbitCameraKeyStone b, float time) {
            float t = (time - a.Time) / (b.Time - a.Time);
            return new OrbitCameraKeyStone(Vector3Radial.Lerp(a.radialPos, b.radialPos, Mathf.SmoothStep(0f, 1f, t)), Mathf.Lerp(a.focusOffsetY, b.focusOffsetY, Mathf.SmoothStep(0f, 1f, t)), Mathf.Lerp(a.CameraFOV, b.CameraFOV, Mathf.SmoothStep(0f, 1f, t)), time);
        }
    }

    [Serializable]
    public class TripodCameraKeyStone : KeyStoneWithYOffset {
        public TripodCameraKeyStone(Vector3 p, float yOffset, float fov, float t) {
            Position = p;
            Rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position + yOffset * Vector3.up - Position, Vector3.up);
            this.Time = t;
            this.CameraFOV = fov;
            this.focusOffsetY = yOffset;
        }
        public TripodCameraKeyStone(Transform cameraTransform, float yOffset, float fov, float t) {
            Position = cameraTransform.position;
            Rotation = Quaternion.LookRotation(PlayerController.Instance.skaterController.skaterTransform.position + yOffset * Vector3.up - Position, Vector3.up);
            this.Time = t;
            this.CameraFOV = fov;
            this.focusOffsetY = yOffset;
        }

        public override void ApplyTo(Transform t) {
            t.position = Position;
            t.LookAt(PlayerController.Instance.skaterController.skaterTransform.position + focusOffsetY * Vector3.up, Vector3.up);
            t.GetComponent<Camera>().fieldOfView = this.CameraFOV;
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
            return new TripodCameraKeyStone(Vector3.Lerp(a.Position, b.Position, Mathf.SmoothStep(0f, 1f, t)), Mathf.Lerp(a.CameraFOV, b.CameraFOV, Mathf.SmoothStep(0f, 1f, t)), yOffset, time);
        }
    }
}
*/