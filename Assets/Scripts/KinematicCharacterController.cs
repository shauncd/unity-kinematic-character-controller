using UnityEngine;

public class KinematicCharacterController : MonoBehaviour
{
    // Object references
    private Rigidbody rb;
    private CapsuleCollider collider;

    // Fields
    private float groundCheckOffset = 0.05f;
    public float moveSpeed = 3f;
    public float colliderPadding = 0.05f; // Keep this 10% of collider radius

    // Global variables
    private RaycastHit groundCheckHitInfo;
    private RaycastHit moveProjectionHitInfo;
    private Vector2 movementInputs;
    private Vector3 velocityVector;

    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        collider = this.GetComponent<CapsuleCollider>();
    }

    void Update()
    {
        movementInputs = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );
    }

    void FixedUpdate()
    {
        groundCheckHitInfo = GroundCheck();
        if (groundCheckHitInfo.collider != null)
        {
            //Debug.Log("Ground detected");
            //Debug.Log(groundCheckHitInfo.distance);
        }

        velocityVector = (this.transform.forward * movementInputs.y + this.transform.right * movementInputs.x).normalized * moveSpeed;
        Vector3 newPosition = CalculateNewPosition(rb.position, collider, velocityVector);
        rb.MovePosition(newPosition);
        //Debug.Log(rb.position);
    }

    // TODO: refactor
    private RaycastHit GroundCheck()
    {
        Physics.SphereCast(this.transform.position, collider.radius, Vector3.down, out RaycastHit hitInfo, collider.height / 2f + groundCheckOffset);

        return hitInfo;
    }

    private Vector3 CalculateNewPosition(Vector3 start, CapsuleCollider collider, Vector3 velocityVector)
    {
        Vector3 moveVector = velocityVector * Time.fixedDeltaTime;
        Vector3 newMoveVector = ProjectMovement(start, collider, moveVector);
        return start + newMoveVector;
    }

    private Vector3 ProjectMovement(Vector3 origin, CapsuleCollider collider, Vector3 moveVector)
    {
        Vector3 newMoveVector;

        (Vector3 topPoint, Vector3 bottomPoint) spherePoints = CalculateSpheresInCapsule(origin, collider.height, collider.radius);
        Debug.DrawLine(spherePoints.topPoint, spherePoints.bottomPoint, Color.red);
        Debug.DrawLine(spherePoints.topPoint, spherePoints.topPoint + moveVector * 50f, Color.red);
        if (Physics.CapsuleCast(spherePoints.topPoint, spherePoints.bottomPoint, collider.radius, moveVector.normalized, out RaycastHit moveProjectionHitInfo, moveVector.magnitude))
        {
            newMoveVector = moveVector.normalized * (moveProjectionHitInfo.distance - colliderPadding); // Need to use colliderPadding or else collider will clip through wall
        }
        else
        {
            newMoveVector = moveVector;
        }

        return newMoveVector;
    }
    
    private (Vector3, Vector3) CalculateSpheresInCapsule(Vector3 capsuleCenter, float capsuleHeight, float capsuleRadius)
    {
        // TODO: this function needs to account for collider center relative to transform
        Vector3 topPoint, bottomPoint;
        float yOffset;

        yOffset = capsuleHeight / 2f;
        topPoint = capsuleCenter + new Vector3(0f, yOffset, 0f);
        bottomPoint = capsuleCenter - new Vector3(0f, yOffset, 0f);

        return (topPoint, bottomPoint);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(this.transform.position + (Vector3.down * (collider.height / 2f + groundCheckOffset)), collider.radius);
            (Vector3 topPoint, Vector3 bottomPoint) spherePoints = CalculateSpheresInCapsule(this.transform.position, collider.height, collider.radius);
            Vector3 moveVector = velocityVector * Time.fixedDeltaTime;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(spherePoints.topPoint + moveVector, collider.radius);
            Gizmos.DrawWireSphere(spherePoints.bottomPoint + moveVector, collider.radius);
        }
    }
}
