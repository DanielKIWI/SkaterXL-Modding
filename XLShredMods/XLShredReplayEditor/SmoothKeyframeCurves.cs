﻿using UnityEngine;
using System;
using System.Collections.Generic;

/* 
 * SmoothKeyframeCurves
 * Used to give smoother camera movements with non-constant keyframe intervals
 * Based on Bezier Curves for Camera Motion by Colm Buckley (1994)
 * https://scss.tcd.ie/publications/tech-reports/reports.94/TCD-CS-94-18.pdf
 * Adapted by Matthew Fraser.
 */

namespace SmoothKeyframeCurves {
    public class CurveKey<T> {
        public T Value;
        public T C1, C2, C3, C4;
        public float Time;
        public bool isStopPoint;

        public override string ToString() {
            if (Value.GetType() == typeof(Vector3)) {
                return
$@"---------------
CurveKey<{Value.GetType()}> @ {Time}s
Val: {((Vector3)(object)Value).ToString("F9")}
C1: {((Vector3)(object)C1).ToString("F9")}
C2: {((Vector3)(object)C2).ToString("F9")}
C3: {((Vector3)(object)C3).ToString("F9")}
C4: {((Vector3)(object)C4).ToString("F9")}
---------------
";
            }
            return
$@"---------------
CurveKey<{Value.GetType()}> @ {Time}s
Val: {Value}
C1: {C1}
C2: {C2}
C3: {C3}
C4: {C4}
---------------
";
        }
    }

    public class CurveBase<T> where T : struct {
        public List<CurveKey<T>> Keys { get; set; } = new List<CurveKey<T>>();

        #region Caches
        private Dictionary<int, float> ParamRatioCache = new Dictionary<int, float>();
        private Dictionary<int, T> RCache = new Dictionary<int, T>();
        private Dictionary<int, T> TCache = new Dictionary<int, T>();
        private Dictionary<int, T> XCache = new Dictionary<int, T>();
        private Dictionary<int, T> YCache = new Dictionary<int, T>();
        private Dictionary<int, T> AInCache = new Dictionary<int, T>();
        private Dictionary<int, T> AOutCache = new Dictionary<int, T>();
        private Dictionary<int, T> A0Cache = new Dictionary<int, T>();
        private Dictionary<int, T> A1Cache = new Dictionary<int, T>();

        private void ClearCaches() {
            ParamRatioCache.Clear();
            RCache.Clear();
            TCache.Clear();
            XCache.Clear();
            YCache.Clear();
            AInCache.Clear();
            AOutCache.Clear();
            A0Cache.Clear();
            A1Cache.Clear();
        }
        #endregion

        #region Math
        // Override these, the other functions use them to do their work
        protected virtual T Sum(T a, T b) { return new T(); }
        protected virtual T Times(T a, float times) { return new T(); }
        protected virtual T Diff(T a, T b) { return new T(); }

        private T Interpolate(T a, T b, float t) {
            return Sum(a, Times(Diff(b, a), t));
        }
        #endregion

        #region General Operations
        public void InsertCurveKey(T val, float t) {
            Keys.Insert(GetInsertPos(t), new CurveKey<T>() { Value = val, Time = t });
        }

        public void DeleteCurveKey(int i) {
            Keys.RemoveAt(i);
        }

        public void CalculateCurveControlPoints() {
            for (int i = 0; i < Keys.Count; i++) {
                CalculateKeyControlPoints(i);
            }
        }

        private void CalculateKeyControlPoints(int seg) {
            if (seg < 0 || seg > Keys.Count - 2) return;

            ClearCaches();
            CurveKey<T> k = Keys[seg];

            k.C1 = CalculateC1(seg);
            k.C2 = CalculateC2(seg);
            k.C4 = CalculateC4(seg);
            k.C3 = CalculateC3(seg);

            if (k.Value.GetType() == typeof(Vector3)) {
                Console.WriteLine(k);
            }
        }

        private List<T> Bezier(List<T> controlPoints, float t) {
            List<T> newControlPoints = new List<T>();

            if (controlPoints.Count == 1) {
                return controlPoints;
            }

            for (int i = 0; i < controlPoints.Count - 1; i++) {
                newControlPoints.Add(Interpolate(controlPoints[i], controlPoints[i + 1], t));
            }
            return Bezier(newControlPoints, t);
        }

        public T Evaluate(float t) {
            int index = GetSegment(t);
            CurveKey<T> k = Keys[index];

            float factor = (t - k.Time) / (Keys[index + 1].Time - k.Time);

            //T[] controlPoints = new T[] { k.Value, k.C1, k.C2, k.C3, k.C4, Keys[index + 1].Value };
            T[] controlPoints = new T[] { k.Value, k.C1, k.C4, Keys[index + 1].Value };

            return Bezier(new List<T>(controlPoints), factor)[0];
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

        #region Calculations
        private float CalculateParamRatio(int seg) {
            if (ParamRatioCache.ContainsKey(seg)) {
                return ParamRatioCache[seg];
            }

            float val;

            if (seg + 1 > Keys.Count - 1) {
                val = 1f;
                ParamRatioCache[seg] = val;
                return val;
            }

            float tPrev = Keys[seg - 1].Time;
            float t = Keys[seg].Time;
            float tNext = Keys[seg + 1].Time;

            val = (tNext - t) / (t - tPrev);
            ParamRatioCache[seg] = val;
            return val;
        }

        private T CalculateR(int seg) {
            if (RCache.ContainsKey(seg)) {
                return RCache[seg];
            }
            T val;
            CurveKey<T> k = Keys[seg];

            if (seg == 0) {
                val = k.Value;
                RCache[seg] = val;
                return val;
            }

            CurveKey<T> kPrev = Keys[seg - 1];

            val = Interpolate(kPrev.Value, k.Value, 1f + CalculateParamRatio(seg));
            RCache[seg] = val;
            return val;
        }

        private T CalculateT(int seg) {
            if (TCache.ContainsKey(seg)) {
                return TCache[seg];
            }
            
            T vNext = (seg + 1 > Keys.Count - 1) ? Keys[seg].Value : Keys[seg + 1].Value;

            T val = Interpolate(CalculateR(seg), vNext, 0.5f);
            TCache[seg] = val;
            return val;
        }

        private T CalculateX(int seg) {
            if (XCache.ContainsKey(seg)) {
                return XCache[seg];
            }

            CurveKey<T> k = Keys[seg];

            T val = Interpolate(k.Value, CalculateT(seg), 1f / 3f);
            XCache[seg] = val;
            return val;
        }

        private T CalculateY(int seg) {
            if (YCache.ContainsKey(seg)) {
                return YCache[seg];
            }

            CurveKey<T> kNext = Keys[seg + 1];

            T val = Interpolate(kNext.Value, CalculateX(seg + 1), -1f / CalculateParamRatio(seg + 1));
            YCache[seg] = val;
            return val;
        }

        private T CalculateAIn(int seg) {
            if (AInCache.ContainsKey(seg)) {
                return AInCache[seg];
            }

            CurveKey<T> k = Keys[seg];
            T x = CalculateX(seg);

            T val = Diff(Diff(CalculateY(seg), x), Diff(x, k.Value));
            AInCache[seg] = val;
            return val;
        }

        private T CalculateAOut(int seg) {
            if (AOutCache.ContainsKey(seg)) {
                return AOutCache[seg];
            }

            CurveKey<T> kNext = Keys[seg + 1];
            T y = CalculateY(seg);

            T val = Diff(Diff(CalculateX(seg), y), Diff(y, kNext.Value));
            AOutCache[seg] = val;
            return val;
        }

        private T CalculateA0(int seg) {
            if (A0Cache.ContainsKey(seg)) {
                return A0Cache[seg];
            }

            T val = Interpolate(Times(CalculateAOut(seg - 1), CalculateParamRatio(seg)), CalculateAIn(seg), 0.5f);
            A0Cache[seg] = val;
            return val;
        }

        private T CalculateA1(int seg) {
            if (A1Cache.ContainsKey(seg)) {
                return A1Cache[seg];
            }

            T val = Interpolate(CalculateAOut(seg), Times(CalculateAIn(seg + 1), 1f / CalculateParamRatio(seg + 1)), 0.5f);
            A1Cache[seg] = val;
            return val;
        }

        private T CalculateC1(int seg) {
            CurveKey<T> k = Keys[seg];

            if (seg == 0 || k.isStopPoint) {
                return k.Value;
            }

            //return Interpolate(k.Value, CalculateT(seg), 0.2f);
            return CalculateX(seg);
        }

        private T CalculateC2(int seg) {
            CurveKey<T> k = Keys[seg];

            if (seg == 0 || k.isStopPoint) {
                return k.Value;
            }

            return Sum(k.C1, Sum(Diff(k.C1, k.Value), CalculateA0(seg)));
        }

        private T CalculateC3(int seg) {
            CurveKey<T> k = Keys[seg];
            CurveKey<T> kNext = Keys[seg + 1];

            if (seg == Keys.Count - 2 || kNext.isStopPoint) {
                return kNext.Value;
            }

            return Sum(k.C4, Sum(Diff(k.C4, kNext.Value), CalculateA1(seg)));
        }

        private T CalculateC4(int seg) {
            CurveKey<T> kNext = Keys[seg + 1];

            if (seg == Keys.Count - 2 || kNext.isStopPoint) {
                return kNext.Value;
            }

            //return Interpolate(kNext.Value, CalculateC1(seg + 1), -1f / CalculateParamRatio(seg + 1));
            return CalculateY(seg);
        }
        #endregion
    }

    #region Specialized Curves
    public class QuaternionCurve : CurveBase<Quaternion> {
        protected override Quaternion Sum(Quaternion a, Quaternion b) {
            return b * a;
        }

        protected override Quaternion Times(Quaternion a, float times) {
            return a.Pow(times);
        }

        protected override Quaternion Diff(Quaternion a, Quaternion b) {
            return a * Quaternion.Inverse(b);
        }
    }

    public class Vector3Curve : CurveBase<Vector3> {
        protected override Vector3 Sum(Vector3 a, Vector3 b) {
            return a + b;
        }

        protected override Vector3 Times(Vector3 a, float times) {
            return a * times;
        }

        protected override Vector3 Diff(Vector3 a, Vector3 b) {
            return a - b;
        }
    }

    public class Vector2Curve : CurveBase<Vector2> {
        protected override Vector2 Sum(Vector2 a, Vector2 b) {
            return a + b;
        }

        protected override Vector2 Times(Vector2 a, float times) {
            return a * times;
        }

        protected override Vector2 Diff(Vector2 a, Vector2 b) {
            return a - b;
        }
    }

    public class FloatCurve : CurveBase<float> {
        protected override float Sum(float a, float b) {
            return a + b;
        }

        protected override float Times(float a, float times) {
            return a * times;
        }

        protected override float Diff(float a, float b) {
            return a - b;
        }
    }
    #endregion

    public static class QuaternionExtensions {
        public static Quaternion Pow(this Quaternion input, float power) {
            float inputMagnitude = input.Magnitude();
            Vector3 nHat = new Vector3(input.x, input.y, input.z).normalized;
            Quaternion vectorBit = new Quaternion(nHat.x, nHat.y, nHat.z, 0)
                .ScalarMultiply(power * Mathf.Acos(input.w / inputMagnitude))
                    .Exp();
            return vectorBit.ScalarMultiply(Mathf.Pow(inputMagnitude, power));
        }

        public static Quaternion Exp(this Quaternion input) {
            float inputA = input.w;
            Vector3 inputV = new Vector3(input.x, input.y, input.z);
            float outputA = Mathf.Exp(inputA) * Mathf.Cos(inputV.magnitude);
            Vector3 outputV = Mathf.Exp(inputA) * (inputV.normalized * Mathf.Sin(inputV.magnitude));
            return new Quaternion(outputV.x, outputV.y, outputV.z, outputA);
        }

        public static float Magnitude(this Quaternion input) {
            return Mathf.Sqrt(input.x * input.x + input.y * input.y + input.z * input.z + input.w * input.w);
        }

        public static Quaternion ScalarMultiply(this Quaternion input, float scalar) {
            return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
        }
    }
}