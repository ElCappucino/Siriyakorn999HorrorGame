using UnityEngine;
using System.Collections.Generic;
using System.Net;
using TMPro;

public class CameraControl : MonoBehaviour
{

    [Header("Look At")]
    [SerializeField] private Transform cameraLookat;
    [SerializeField] private Vector2 lookAtRangeX;
    [SerializeField] private Vector2 lookAtRangeY;
    [SerializeField] private Vector2 currentLookatPos;
    [SerializeField] private float moveSpeedX;
    [SerializeField] private float moveSpeedY;

    [Header("Hand")]
    [SerializeField] private Transform handPivot;
    [SerializeField] private float handRotationAmountX;
    [SerializeField] private float handRotationAmountY;
    [SerializeField] private float rotationSpeed;
    private float targetHandRotationX = 0;
    private float targetHandRotationY = 0;

    [Header("Talisman")]
    [SerializeField] private GameObject talismanHoldUI;
    [SerializeField] private Transform talismanObject;
    [SerializeField] private Transform talisman_holdPos;
    [SerializeField] private Transform talisman_defaultPos;
    [SerializeField] private float talismanMoveSpeed;
    private bool isHoldTalisman;

    private Vector2 MoveInput;

    private void Update()
    {
        Quaternion currentRotation = handPivot.rotation;
        Quaternion targetRotation = Quaternion.Euler(targetHandRotationY, targetHandRotationX, 0);
        handPivot.rotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * rotationSpeed);

        Vector3 talismanTargetPosition;
        if (isHoldTalisman)
        {
            talismanTargetPosition = talisman_holdPos.position;
        }
        else
        {
            talismanTargetPosition = talisman_defaultPos.position;
        }

        talismanObject.position = Vector3.Lerp(talismanObject.position, talismanTargetPosition, talismanMoveSpeed * Time.deltaTime);
    }

    public void PanCamera(Vector2 direction)
    {
        Vector3 lookatPos = cameraLookat.position;

        lookatPos.x += direction.x * moveSpeedX;
        if (lookatPos.x >= lookAtRangeX.y) lookatPos.x = lookAtRangeX.y;
        if (lookatPos.x <= lookAtRangeX.x) lookatPos.x = lookAtRangeX.x;

        lookatPos.y += direction.y * moveSpeedY;
        if (lookatPos.y >= lookAtRangeY.y) lookatPos.y = lookAtRangeY.y;
        if (lookatPos.y <= lookAtRangeY.x) lookatPos.y = lookAtRangeY.x;

        if (direction.x > 0 && targetHandRotationX < handRotationAmountX)
            targetHandRotationX += handRotationAmountX;
        else if (direction.x < 0 && targetHandRotationX > -handRotationAmountX)
            targetHandRotationX -= handRotationAmountX;

        if (direction.y > 0 && targetHandRotationY > -handRotationAmountY)
            targetHandRotationY -= handRotationAmountY;
        else if (direction.y < 0 && targetHandRotationY < handRotationAmountY)
            targetHandRotationY += handRotationAmountY;

        cameraLookat.position = lookatPos;
        currentLookatPos = lookatPos;
    }

    public void HoldTalisman(bool isHold)
    {
        isHoldTalisman = isHold;
        talismanHoldUI.SetActive(!isHold);
    }

}
