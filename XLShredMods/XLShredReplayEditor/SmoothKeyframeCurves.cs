using UnityEngine;
using System;
using System.Collections.Generic;

/* 
 * SmoothKeyframeCurves
 * Used to give smoother camera movements with non-constant keyframe intervals
 * Based on Bezier Curves for Camera Motion by Colm Buckley (1994) (using cubic implementation)
 * https://scss.tcd.ie/publications/tech-reports/reports.94/TCD-CS-94-18.pdf
 * Adapted by Matthew Fraser.
 */

namespace SmoothKeyframeCurves {
    public class CurveKey<T> {
        public T Value;
        public T C1, C2;
        public float Time;
        public bool isStopPoint;
    }

    public class CurveBase<T> where T : struct {
        public List<CurveKey<T>> Keys { get; set; } = new List<CurveKey<T>>();

        protected virtual T Interpolate(T a, T b, float t) { return new T(); }

        #region General Operations
        public void InsertCurveKey(T val, float t) {
            Keys.Insert(GetInsertPos(t), new CurveKey<T>() { Value = val, Time = t });
        }

        public void DeleteCurveKey(int i) {
            Keys.RemoveAt(i);
            CalculateCurveControlPoints();
        }

        public void Clear() {
            Keys.Clear();
        }

        public void CalculateCurveControlPoints() {
            for (int i = 0; i < Keys.Count - 1; i++) {
                CalculateKeyControlPoints(i);
            }
        }

        public T Evaluate(float t) {
            if (t < Keys[0].Time) {
                return Keys[0].Value;
            }
            if (t > Keys[Keys.Count - 1].Time) {
                return Keys[Keys.Count - 1].Value;
            }

            int index = GetSegment(t);
            CurveKey<T> k = Keys[index];
            CurveKey<T> kNext = Keys[index + 1];

            float factor = (t - k.Time) / (kNext.Time - k.Time);

            return Interpolate(
                Interpolate(Interpolate(k.Value, k.C1, factor),
                            Interpolate(k.C1, k.C2, factor),
                            factor),
                Interpolate(Interpolate(k.C1, k.C2, factor),
                            Interpolate(k.C2, kNext.Value, factor), 
                            factor),
                factor);
        }

        private int GetSegment(float t) {
            if (t <= Keys[0].Time) return 0;
            if (t >= Keys[Keys.Count - 2].Time) return Keys.Count - 2;

            return Keys.FindIndex(k => t <= k.Time) - 1;
        }

        private int GetInsertPos(float t) {
            if (Keys.Count == 0 || t <= Keys[0].Time) return 0;
            int i = Keys.FindIndex(k => t < k.Time);
            return (i == -1) ? Keys.Count : i;
        }
        #endregion

        private void CalculateKeyControlPoints(int seg) {
            CurveKey<T> k1 = Keys[seg];
            CurveKey<T> k0 = (seg == 0) ? k1 : Keys[seg - 1];
            CurveKey<T> k2 = Keys[seg + 1];
            CurveKey<T> k3 = (seg + 2 > Keys.Count - 1) ? k2 : Keys[seg + 2];

            float paramRatio = CalculateParamRatio(k0.Time, k1.Time, k2.Time);
            T r = (seg == 0) ? k1.Value : Interpolate(k0.Value, k1.Value, 1f + paramRatio);
            T t = Interpolate(r, k2.Value, 0.5f);

            float paramRatioNext = CalculateParamRatio(k1.Time, k2.Time, k3.Time);
            T rNext = Interpolate(k1.Value, k2.Value, 1f + paramRatioNext);
            T tNext = Interpolate(rNext, k3.Value, 0.5f);
            T c1Next = Interpolate(k2.Value, tNext, 1f / 3f);

            k1.C1 = (seg == 0 || k1.isStopPoint) ? k1.Value : Interpolate(k1.Value, t, 1f / 3f);
            k1.C2 = (seg == Keys.Count - 2 || k2.isStopPoint) ? k2.Value : Interpolate(k2.Value, c1Next, -1f / paramRatioNext);
        }

        private float CalculateParamRatio(float tPrev, float t, float tNext) {
            if (t - tPrev == 0f || tNext - t == 0f) {
                return 1f;
            }

            return (tNext - t) / (t - tPrev);
        }
    }

    #region Specialized Curves
    public class QuaternionCurve : CurveBase<Quaternion> {
        protected override Quaternion Interpolate(Quaternion a, Quaternion b, float t) {
            return Quaternion.SlerpUnclamped(a, b, t);
        }
    }

    public class Vector3Curve : CurveBase<Vector3> {
        protected override Vector3 Interpolate(Vector3 a, Vector3 b, float t) {
            return Vector3.LerpUnclamped(a, b, t);
        }
    }

    public class Vector2Curve : CurveBase<Vector2> {
        protected override Vector2 Interpolate(Vector2 a, Vector2 b, float t) {
            return Vector2.LerpUnclamped(a, b, t);
        }
    }

    public class FloatCurve : CurveBase<float> {
        protected override float Interpolate(float a, float b, float t) {
            return Mathf.LerpUnclamped(a, b, t);
        }
    }
    #endregion
}
