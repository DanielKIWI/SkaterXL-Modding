using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/*namespace XLShredReplayEditor {
    public static class Spline {

        public class QuaternionSphereData {
            public Vector2 sphereAxis;
            public float angle;

            public QuaternionSphereData(Vector2 sphereAxis, float angle) {
                this.sphereAxis = sphereAxis;
                this.angle = angle;
            }

            public QuaternionSphereData(Quaternion q) {
                q.ToAngleAxis(out angle, out Vector3 axis);

                sphereAxis = CartToSphere(axis);
            }

            public Quaternion ToQuaternion() {
                return Quaternion.AngleAxis(angle, SphereToCart(sphereAxis));
            }

            public static Quaternion ToQuaternion(QuaternionSphereData q) {
                return Quaternion.AngleAxis(q.angle, SphereToCart(q.sphereAxis));
            }

            private static Vector2 CartToSphere(Vector3 cart) {
                float x = (cart.x == 0) ? Mathf.Epsilon : cart.x;
                float y = cart.y; float z = cart.z;

                float theta = Mathf.Atan(z / x);
                if (x < 0) theta += Mathf.PI;
                float phi = Mathf.Asin(y);

                return new Vector2(theta * Mathf.Rad2Deg, phi * Mathf.Rad2Deg);
            }

            private static Vector3 SphereToCart(Vector2 sphere) {
                float xs = sphere.x * Mathf.Deg2Rad;
                float ys = sphere.y * Mathf.Deg2Rad;
                float cosPhi = Mathf.Cos(ys);
                float x = cosPhi * Mathf.Cos(xs);
                float y = Mathf.Sin(ys);
                float z = cosPhi * Mathf.Sin(xs);

                return new Vector3(x, y, z);
            }
        }

        public static Vector3 PointTangent(Vector3 p0, Vector3 p1, Vector3 p2, float t0, float t1, float t2) {
            float t01 = t1 - t0; Vector3 p01 = p1 - p0;
            float t12 = t2 - t1; Vector3 p12 = p2 - p1;
            float t02 = t2 - t0; Vector3 p02 = p2 - p0;

            if (t01 == 0) return Vector3.zero;
            if (t12 == 0) return Vector3.zero;

            return (p01 + p12 * (t01 / t12)) / t02;
        }

        public static QuaternionSphereData QuaternionTangent(QuaternionSphereData p0, QuaternionSphereData p1, QuaternionSphereData p2,
                                                                float t0, float t1, float t2) {
            return new QuaternionSphereData(SphericalTangent(p0.sphereAxis, p1.sphereAxis, p2.sphereAxis, t0, t1, t2),
                                                FloatTangent(p0.angle, p1.angle, p2.angle, t0, t1, t2));
        }

        public static float AngleTangent(float p0, float p1, float p2, float t0, float t1, float t2) {
            float t01 = t1 - t0; float p01 = LoopAngleDelta(p1 - p0);
            float t12 = t2 - t1; float p12 = LoopAngleDelta(p2 - p1);
            float t02 = t2 - t0; float p02 = LoopAngleDelta(p2 - p0);

            if (t01 == 0) return 0;
            if (t12 == 0) return 0;

            return (p01 + p12 * (t01 / t12)) / t02;
        }

        public static float FloatTangent(float p0, float p1, float p2, float t0, float t1, float t2) {
            float t01 = t1 - t0; float p01 = p1 - p0;
            float t12 = t2 - t1; float p12 = p2 - p1;
            float t02 = t2 - t0; float p02 = p2 - p0;

            if (t01 == 0) return 0;
            if (t12 == 0) return 0;

            return (p01 + p12 * (t01 / t12)) / t02;
        }

        public static Vector2 SphericalTangent(Vector2 p0, Vector2 p1, Vector2 p2, float t0, float t1, float t2) {
            return new Vector2(FloatTangent(p0.x, p1.x, p2.x, t0, t1, t2),
                                FloatTangent(p0.y, p1.y, p2.y, t0, t1, t2));
        }

        public static Vector3 PointSpline(Vector3 p1, Vector3 p2, Vector3 m1, Vector3 m2, float factor, float duration) {
            float factor2 = factor * factor;
            float factor3 = factor2 * factor;

            m1 *= duration;
            m2 *= duration;

            return factor3 * (2.0f * (p1 - p2) + m1 + m2) +
                    factor2 * (-3.0f * (p1 - p2) - m1 - m1 - m2) +
                    factor * m1 +
                    p1;
        }

        public static Quaternion QuaternionSpline(QuaternionSphereData p1, QuaternionSphereData p2,
                                                    QuaternionSphereData m1, QuaternionSphereData m2, float factor, float duration) {

            return new QuaternionSphereData(SphericalSpline(p1.sphereAxis, p2.sphereAxis, m1.sphereAxis, m2.sphereAxis, factor, duration),
                                            AngleSpline(p1.angle, p2.angle, m1.angle, m2.angle, factor, duration)).ToQuaternion();
        }

        public static float FloatSpline(float p1, float p2, float m1, float m2, float factor, float duration) {
            float factor2 = factor * factor;
            float factor3 = factor2 * factor;

            m1 *= duration;
            m2 *= duration;

            return factor3 * (2.0f * (p1 - p2) + m1 + m2) +
                    factor2 * (-3.0f * (p1 - p2) - m1 - m1 - m2) +
                    factor * (m1) +
                    p1;
        }

        public static float AngleSpline(float p1, float p2, float m1, float m2, float factor, float duration) {
            float factor2 = factor * factor;
            float factor3 = factor2 * factor;

            m1 *= duration;
            m2 *= duration;

            float val = factor3 * (2.0f * (p1 - p2) + m1 + m2) +
                        factor2 * (-3.0f * (p1 - p2) - m1 - m1 - m2) +
                        factor * (m1) +
                        p1;

            return Mathf.Repeat(val, 360f);
        }

        public static Vector2 SphericalSpline(Vector2 p1, Vector2 p2, Vector2 m1, Vector2 m2, float factor, float duration) {
            Vector2 val = new Vector2(AngleSpline(p1.x, p2.x, m1.x, m2.x, factor, duration),
                                AngleSpline(p1.y, p2.y, m1.y, m2.y, factor, duration));

            return val;
        }

        private static float LoopAngleDelta(float angleDelta) {
            angleDelta = Mathf.Repeat(angleDelta, 360f);
            if (angleDelta > 180f) angleDelta -= 360f;
            return angleDelta;
        }
    }
    // Arbitrarily timed keyframe spline
    public static class CentripetalSpline {
        
        public class QuaternionSphereData {
            public Vector2 sphereAxis;
            public float angle;

            public QuaternionSphereData(Vector2 sphereAxis, float angle) {
                this.sphereAxis = sphereAxis;
                this.angle = angle;
            }

            public QuaternionSphereData(Quaternion q) {
                q.ToAngleAxis(out angle, out Vector3 axis);

                sphereAxis = CartToSphere(axis);
            }

            public Quaternion ToQuaternion() {
                return Quaternion.AngleAxis(angle, SphereToCart(sphereAxis));
            }

            public static Quaternion ToQuaternion(QuaternionSphereData q) {
                return Quaternion.AngleAxis(q.angle, SphereToCart(q.sphereAxis));
            }

            private static Vector2 CartToSphere(Vector3 cart) {
                float x = (cart.x == 0) ? Mathf.Epsilon : cart.x;
                float y = cart.y; float z = cart.z;

                float theta = Mathf.Atan(z / x);
                if (x < 0) theta += Mathf.PI;
                float phi = Mathf.Asin(y);

                return new Vector2(theta * Mathf.Rad2Deg, phi * Mathf.Rad2Deg);
            }

            private static Vector3 SphereToCart(Vector2 sphere) {
                float xs = sphere.x * Mathf.Deg2Rad;
                float ys = sphere.y * Mathf.Deg2Rad;
                float cosPhi = Mathf.Cos(ys);
                float x = cosPhi * Mathf.Cos(xs);
                float y = Mathf.Sin(ys);
                float z = cosPhi * Mathf.Sin(xs);

                return new Vector3(x, y, z);
            }
        }

        public static Vector3 PointTangent(Vector3 p0, Vector3 p1, Vector3 p2, float t0, float t1, float t2) {
            float t01 = t1 - t0; Vector3 p01 = p1 - p0;
            float t12 = t2 - t1; Vector3 p12 = p2 - p1;
            float t02 = t2 - t0; Vector3 p02 = p2 - p0;

            if (t01 == 0) return Vector3.zero;
            if (t12 == 0) return Vector3.zero;

            return p12 + t12 * (p01 / t01 - p02 / t02);
        }

        public static QuaternionSphereData QuaternionTangent(   QuaternionSphereData p0, QuaternionSphereData p1, QuaternionSphereData p2,
                                                                float t0, float t1, float t2) {
            return new QuaternionSphereData(    SphericalTangent(p0.sphereAxis, p1.sphereAxis, p2.sphereAxis, t0, t1, t2),
                                                FloatTangent(p0.angle, p1.angle, p2.angle, t0, t1, t2));
        }

        public static float AngleTangent(float p0, float p1, float p2, float t0, float t1, float t2) {
            float t01 = t1 - t0; float p01 = LoopAngleDelta(p1 - p0);
            float t12 = t2 - t1; float p12 = LoopAngleDelta(p2 - p1);
            float t02 = t2 - t0; float p02 = LoopAngleDelta(p2 - p0);

            if (t01 == 0) return 0;
            if (t12 == 0) return 0;

            return p12 + t12 * (p01 / t01 - p02 / t02);
        }

        public static float FloatTangent(float p0, float p1, float p2, float t0, float t1, float t2) {
            float t01 = t1 - t0; float p01 = p1 - p0;
            float t12 = t2 - t1; float p12 = p2 - p1;
            float t02 = t2 - t0; float p02 = p2 - p0;

            if (t01 == 0) return 0;
            if (t12 == 0) return 0;

            return p12 + t12 * (p01 / t01 - p02 / t02);
        }

        public static Vector2 SphericalTangent(Vector2 p0, Vector2 p1, Vector2 p2, float t0, float t1, float t2) {
            return new Vector2( FloatTangent(p0.x, p1.x, p2.x, t0, t1, t2),
                                FloatTangent(p0.y, p1.y, p2.y, t0, t1, t2));
        }

        public static Vector3 PointSpline(Vector3 p1, Vector3 p2, Vector3 m1, Vector3 m2, float factor) {
            float factor2 = factor * factor;
            float factor3 = factor2 * factor;

            return  factor3 * (2.0f * (p1 - p2) + m1 + m2) +
                    factor2 * (-3.0f * (p1 - p2) - m1 - m1 - m2) +
                    factor * m1 +
                    p1;
        }

        public static Quaternion QuaternionSpline(  QuaternionSphereData p1, QuaternionSphereData p2, 
                                                    QuaternionSphereData m1, QuaternionSphereData m2, float factor) {

            return new QuaternionSphereData(SphericalSpline(p1.sphereAxis, p2.sphereAxis, m1.sphereAxis, m2.sphereAxis, factor),
                                            AngleSpline(p1.angle, p2.angle, m1.angle, m2.angle, factor)).ToQuaternion();
        }

        public static float FloatSpline(float p1, float p2, float m1, float m2, float factor) {
            float factor2 = factor * factor;
            float factor3 = factor2 * factor;

            return  factor3 * (2.0f * (p1 - p2) + m1 + m2) +
                    factor2 * (-3.0f * (p1 - p2) - m1 - m1 - m2) +
                    factor * (m1) +
                    p1;
        }

        public static float AngleSpline(float p1, float p2, float m1, float m2, float factor) {
            float factor2 = factor * factor;
            float factor3 = factor2 * factor;


            float val = factor3 * (2.0f * (p1 - p2) + m1 + m2) +
                        factor2 * (-3.0f * (p1 - p2) - m1 - m1 - m2) +
                        factor * (m1) +
                        p1;

            return Mathf.Repeat(val, 360f);
        }

        public static Vector2 SphericalSpline(Vector2 p1, Vector2 p2, Vector2 m1, Vector2 m2, float factor) {


            Vector2 val = new Vector2(AngleSpline(p1.x, p2.x, m1.x, m2.x, factor),
                                AngleSpline(p1.y, p2.y, m1.y, m2.y, factor));

            return val;
        }

        private static float LoopAngleDelta(float angleDelta) {
            angleDelta = Mathf.Repeat(angleDelta, 360f);
            if (angleDelta > 180f) angleDelta -= 360f;
            return angleDelta;
        }
    }
}
*/