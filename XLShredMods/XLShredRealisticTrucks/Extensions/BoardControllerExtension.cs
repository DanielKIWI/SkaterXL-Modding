using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XLShredRealisticTrucks.Extensions {
    public static class BoardControllerExtension {

        public static float FrontTruckDamper {
            get {
                return PlayerController.Instance.boardController.frontTruckJoint.angularXDrive.positionDamper;
            }
            set {
                JointDrive angularXDrive = PlayerController.Instance.boardController.frontTruckJoint.angularXDrive;
                angularXDrive.positionDamper = value;
                PlayerController.Instance.boardController.frontTruckJoint.angularXDrive = angularXDrive;
            }
        }

        public static float FrontTruckSpring {
            get {
                return PlayerController.Instance.boardController.frontTruckJoint.angularXDrive.positionSpring;
            }
            set {
                JointDrive angularXDrive = PlayerController.Instance.boardController.frontTruckJoint.angularXDrive;
                angularXDrive.positionSpring = value;
                PlayerController.Instance.boardController.frontTruckJoint.angularXDrive = angularXDrive;
            }
        }

        public static float BackTruckDamper {
            get {
                return PlayerController.Instance.boardController.backTruckJoint.angularXDrive.positionDamper;
            }
            set {
                JointDrive angularXDrive = PlayerController.Instance.boardController.backTruckJoint.angularXDrive;
                angularXDrive.positionDamper = value;
                PlayerController.Instance.boardController.backTruckJoint.angularXDrive = angularXDrive;
            }
        }

        public static float BackTruckSpring {
            get {
                return PlayerController.Instance.boardController.backTruckJoint.angularXDrive.positionSpring;
            }
            set {
                JointDrive angularXDrive = PlayerController.Instance.boardController.backTruckJoint.angularXDrive;
                angularXDrive.positionSpring = value;
                PlayerController.Instance.boardController.backTruckJoint.angularXDrive = angularXDrive;
            }
        }

        public static Vector3 FrontTruckKingPinEuler {
            get {
                return Quaternion.FromToRotation(Vector3.down, PlayerController.Instance.boardController.frontTruckJoint.axis).eulerAngles;
            }
            set {
                Quaternion rot = Quaternion.Euler(value);
                PlayerController.Instance.boardController.frontTruckJoint.axis = rot * Vector3.down;
            }
        }

        public static Vector3 BackTruckKingPinEuler {
            get {
                return Quaternion.FromToRotation(Vector3.down, PlayerController.Instance.boardController.backTruckJoint.axis).eulerAngles;
            }
            set {
                Quaternion rot = Quaternion.Euler(value);
                PlayerController.Instance.boardController.backTruckJoint.axis = rot * Vector3.down;
            }
        }
    }
}
