using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Object references
    public GameObject playerCamera;
    public Transform cameraHolder;

    // Fields
    public float lookSpeed = 2.5f;

    // Global variables
    Vector2 lookInputs;
    Quaternion newRotation;

    void Update()
    {
        lookInputs = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        );

        newRotation = CalculateNewRotation(playerCamera.transform.rotation, lookInputs, lookSpeed);
    }

    void LateUpdate()
    {
        playerCamera.transform.position = cameraHolder.position;
        //playerCamera.transform.rotation = cameraHolder.rotation;
        playerCamera.transform.rotation = newRotation;
    }

    private Quaternion CalculateNewRotation(Quaternion currentRotation, Vector2 lookInputs, float lookSpeed)
    {
        Vector2 adjustedLookInputs = lookInputs * lookSpeed;
        float newXRotation = Mathf.Clamp(ConvertAngle(currentRotation.eulerAngles.x) - adjustedLookInputs.y, -90f, 90f);
        float newYRotation = ConvertAngle(currentRotation.eulerAngles.y) + adjustedLookInputs.x;

        return Quaternion.Euler(newXRotation, newYRotation, 0f);
    }

    private float ConvertAngle(float angleToConvert)
    {
        float convertedAngle;

        if (angleToConvert < 180f)
        {
            convertedAngle = angleToConvert;
        }
        else
        {
            convertedAngle = angleToConvert - 360f;
        }

        return convertedAngle;
    }
}
