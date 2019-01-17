using System;
using System.Collections.Generic;
using UnityEngine;

namespace XLShredReplayEditor {

    public struct Vector3Radial {

        public float phi;

        public float theta;

        public float radius;

        public Vector3Radial(float p, float t, float r) {
            this.phi = p;
            this.theta = t;
            this.radius = r;
        }

        public Vector3Radial(Vector3 source) {
            this.radius = source.magnitude;
            this.phi = Mathf.Atan2(source.x, source.z);
            this.theta = Mathf.Acos(source.y / source.magnitude);
        }

        public Vector3 CartesianCoords {
            get {
                float SinTheta = Mathf.Sin(theta);
                return new Vector3(radius * SinTheta * Mathf.Sin(phi), radius * Mathf.Cos(theta), radius * SinTheta * Mathf.Cos(phi));
            }
        }

        public static Vector3Radial Lerp(Vector3Radial l, Vector3Radial r, float t) {
            return new Vector3Radial(Mathf.LerpAngle(l.phi, r.phi, t), Mathf.LerpAngle(l.theta, r.theta, t), Mathf.Lerp(l.radius, r.radius, t));
        }
    }
}
