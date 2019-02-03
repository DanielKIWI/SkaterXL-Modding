using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XLShredRealisticTrucks.Extensions {

    public static class PlayerControllerExtension {

        public static void ApplyWeightOnBoard(this PlayerController playerController, float zOffset) {
            float num = playerController.boardController.TurnTarget * Main.settings.MaxWeightOnBoardXOffset;
            if (playerController.GetBoardBackwards()) {
                num *= -1f;
            }
            if (PlayerController.Instance.IsSwitch) {
                num *= -1f;
            }
            Vector3 position = playerController.boardController.boardRigidbody.position + playerController.boardController.boardTransform.TransformDirection(Vector3.right * num + Vector3.forward * zOffset);
            playerController.boardController.boardRigidbody.AddForceAtPosition(-playerController.skaterController.skaterTransform.up * 80f * playerController.impactBoardDownForce, position, ForceMode.Force);
        }
    }
}
