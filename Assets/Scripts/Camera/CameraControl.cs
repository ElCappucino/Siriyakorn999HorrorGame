using UnityEngine;
using System.Collections.Generic;
using System.Net;

public class CameraControl : MonoBehaviour
{

    [Header("Look At")]
    [SerializeField] private Transform cameraLookat;
    [SerializeField] private Vector2 lookAtRangeX;
    [SerializeField] private Vector2 lookAtRangeY;
    [SerializeField] private Vector2 currentLookatPos;
    [SerializeField] private float moveSpeedX;
    [SerializeField] private float moveSpeedY;

    private Vector2 MoveInput;

    private void Update()
    {
        
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

        cameraLookat.position = lookatPos;
        currentLookatPos = lookatPos;
    }

}
