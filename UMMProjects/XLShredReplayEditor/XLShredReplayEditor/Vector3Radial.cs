using System;
using System.Collections.Generic;
using UnityEngine;

namespace XLShredReplayEditor {

    public struct Vector3Radial {
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

        public Vector3 cartesianCoords {
            get {
                return new Vector3(this.radius * Mathf.Sin(this.theta) * Mathf.Sin(this.phi), this.radius * Mathf.Cos(this.theta), this.radius * Mathf.Sin(this.theta) * Mathf.Cos(this.phi));
            }
        }

        public static Vector3Radial Lerp(Vector3Radial l, Vector3Radial r, float t) {
            return new Vector3Radial(Mathf.LerpAngle(l.phi, r.phi, t), Mathf.LerpAngle(l.theta, r.theta, t), Mathf.Lerp(l.radius, r.radius, t));
        }

        public float phi;

        public float theta;

        public float radius;
    }
}
