using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SmoothKeyframeCurves;

namespace XLShredReplayEditor {
    public class CameraCurveResult {
        public Vector3 position;
        public Quaternion rotation;
        public float fov;

        public void ApplyTo(Transform t) {
            t.position = position;
            t.rotation = rotation;
            t.GetComponent<Camera>().fieldOfView = this.fov;
        }

        public static CameraCurveResult Combine(CameraCurveResult r1, CameraCurveResult r2, float w1, float w2) {
            float totalWeight = w1 + w2;
            w2 = w2 / totalWeight;

            Vector3 pos = Vector3.Lerp(r1.position, r2.position, w2);
            Quaternion rot = Quaternion.Lerp(r1.rotation, r2.rotation, w2);
            float fov = Mathf.Lerp(r1.fov, r2.fov, w2);

            return new CameraCurveResult() {
                position = pos,
                rotation = rot,
                fov = fov
            };
        }
    }

    public class CameraCurve {
        public QuaternionCurve orientationCurve = new QuaternionCurve();
        public Vector3Curve positionCurve = new Vector3Curve();
        public FloatCurve focusYOffsetCurve = new FloatCurve();
        public FloatCurve fovCurve = new FloatCurve();
        public FloatCurve radiusCurve = new FloatCurve();
        public FloatCurve freeCamCurve = new FloatCurve();
        public FloatCurve orbitCamCurve = new FloatCurve();
        public FloatCurve tripodCamCurve = new FloatCurve();

        public CameraCurveResult Evaluate(float t) {
            float freeCamAmount = freeCamCurve.Evaluate(t);
            float orbitCamAmount = orbitCamCurve.Evaluate(t);
            float tripodCamAmount = tripodCamCurve.Evaluate(t);

            List<Tuple<float, CameraCurveResult>> results = new List<Tuple<float, CameraCurveResult>>();

            if (freeCamAmount > 0) {
                results.Add(new Tuple<float, CameraCurveResult>(freeCamAmount, FreeCameraKeyFrame.Evaluate(t, this)));
            }

            if (orbitCamAmount > 0) {
                results.Add(new Tuple<float, CameraCurveResult>(orbitCamAmount, OrbitCameraKeyFrame.Evaluate(t, this)));
            }

            if (tripodCamAmount > 0) {
                results.Add(new Tuple<float, CameraCurveResult>(tripodCamAmount, TripodCameraKeyFrame.Evaluate(t, this)));
            }
            switch (results.Count) {
                case 1:
                    return results[0].Item2;
                case 2:
                    return CameraCurveResult.Combine(
                       results[0].Item2, results[1].Item2,
                       results[0].Item1, results[1].Item1
                    );
                case 3:
                    results.Remove(results.OrderBy(v => v.Item1).First());  //Combine the two curves with the highest amount
                    goto case 2;
                default:
                    return new CameraCurveResult();
            }
        }

        public void CalculateCurveControlPoints() {
            orientationCurve.CalculateCurveControlPoints();
            focusYOffsetCurve.CalculateCurveControlPoints();
            positionCurve.CalculateCurveControlPoints();
            fovCurve.CalculateCurveControlPoints();
            radiusCurve.CalculateCurveControlPoints();
            freeCamCurve.CalculateCurveControlPoints();
            orbitCamCurve.CalculateCurveControlPoints();
            tripodCamCurve.CalculateCurveControlPoints();
        }

        public void Clear() {
            orientationCurve.Clear();
            focusYOffsetCurve.Clear();
            positionCurve.Clear();
            fovCurve.Clear();
            radiusCurve.Clear();
            freeCamCurve.Clear();
            orbitCamCurve.Clear();
            tripodCamCurve.Clear();
        }

        public void DeleteCurveKeys(int i) {
            orientationCurve.DeleteCurveKey(i);
            focusYOffsetCurve.DeleteCurveKey(i);
            positionCurve.DeleteCurveKey(i);
            fovCurve.DeleteCurveKey(i);
            radiusCurve.DeleteCurveKey(i);
            freeCamCurve.DeleteCurveKey(i);
            orbitCamCurve.DeleteCurveKey(i);
            tripodCamCurve.DeleteCurveKey(i);
        }
    }
}
