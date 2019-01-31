using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SmoothKeyframeCurves;

namespace XLShredReplayEditor {
    public class CameraCurve {
        QuaternionCurve orientationCurve = new QuaternionCurve();
        Vector3Curve positionCurve = new Vector3Curve();
        FloatCurve fovCurve = new FloatCurve();
    }
}
