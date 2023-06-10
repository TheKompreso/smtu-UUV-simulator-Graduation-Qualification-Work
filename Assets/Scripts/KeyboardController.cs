using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardController : MonoBehaviour
{
    public static KeyboardController Instance;
    public GameObject vehicle;
    public Camera onVehicleCamera;
    public Rigidbody rb;

    [HideInInspector] public float boardPower;
    public float rotatePower = 540f;
    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        NPA.speed = rb.velocity.magnitude;
    }
    void FixedUpdate()
    {
        AxisMovement();
        RotationAroundAxis();
        CameraControl();
    }
    void AxisMovement()
    {
        float moveForward = Input.GetAxis("MouseX");
        float moveSide = Input.GetAxis("MouseY");
        float moveVertical = Input.GetAxis("Vertical");

        moveForward = Math.Clamp(moveForward, MoveEngine.Instance.ForwardSpeedLimit.x, MoveEngine.Instance.ForwardSpeedLimit.y);
        moveSide = Math.Clamp(moveSide, MoveEngine.Instance.SideSpeedLimit.x, MoveEngine.Instance.SideSpeedLimit.y);
        moveVertical = Math.Clamp(moveVertical, MoveEngine.Instance.VerticalSpeedLimit.x, MoveEngine.Instance.VerticalSpeedLimit.y);

        NPA.moveChargeConsuption(Math.Abs(moveForward) + Math.Abs(moveSide) + Math.Abs(moveVertical));

        rb.AddForce(transform.right * moveSide * boardPower);
        rb.AddForce(transform.up * moveForward * boardPower);
        rb.AddForce(transform.forward * -moveVertical * boardPower);
    }
    void RotationAroundAxis()
    {
        float xaw = Input.GetAxis("HorizontalRotate");
        rb.AddTorque(0, rotatePower * xaw, 0);
        NPA.moveChargeConsuption(Math.Abs(xaw));
    }
    void CameraControl()
    {
        float camPosX = Math.Clamp(180 - onVehicleCamera.transform.eulerAngles.x + (float)(Input.GetAxis("VerticalRotate")), 110f, 180f);
        onVehicleCamera.transform.localRotation = Quaternion.Euler(camPosX, 180f, 180f);
    }
}
