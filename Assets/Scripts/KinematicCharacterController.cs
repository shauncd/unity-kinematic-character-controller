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
        
    }

    void FixedUpdate()
    {
        movementInputs = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );

        velocityVector = (this.transform.forward * movementInputs.y + this.transform.right * movementInputs.x).normalized * moveSpeed;
        Vector3 newPosition = CalculateNewPosition(rb.position, velocityVector, collider);
        rb.MovePosition(newPosition);
    }

    private Vector3 CalculateNewPosition(Vector3 start, Vector3 velocityVector, CapsuleCollider playerCollider)
    {
        Vector3 moveVector = GetMovementThisFrame(velocityVector);
        Vector3 endPosition = RecursiveMove(start, moveVector, 0, playerCollider);
        return endPosition;
    }

    private Vector3 GetMovementThisFrame(Vector3 velocityVector)
    {
        return velocityVector * Time.fixedDeltaTime;
    }

    private Vector3 RecursiveMove(Vector3 start, Vector3 moveVector, int numBounces, CapsuleCollider collider)
    {
        Vector3 newMoveVector;
        Vector3 newPosition;
        Vector3 remainingMoveVector;
        Vector3 adjustedMoveVector;

        RaycastHit moveProjectionHitInfo;
        bool collided = ProjectMovement(start, collider, moveVector, out moveProjectionHitInfo);
        if (!collided || numBounces > 5)
        {
            newPosition = start + moveVector;
            return newPosition;
        }
        else
        {
            newMoveVector = moveVector.normalized * (moveProjectionHitInfo.distance - colliderPadding);
            newPosition = start + newMoveVector;
            remainingMoveVector = moveVector - newMoveVector;
            adjustedMoveVector = Vector3.ProjectOnPlane(remainingMoveVector, moveProjectionHitInfo.normal);
            return RecursiveMove(newPosition, adjustedMoveVector, numBounces + 1, collider);
        }
    }

    private bool ProjectMovement(Vector3 origin, CapsuleCollider collider, Vector3 moveVector, out RaycastHit moveProjectionHitInfo)
    {
        (Vector3 top, Vector3 bottom) sphereCenters = CalculateSpheresInCapsule(origin, collider.height, collider.radius);
        float effectiveRadius = collider.radius - colliderPadding;
        return Physics.CapsuleCast(sphereCenters.top, sphereCenters.bottom, effectiveRadius, moveVector.normalized, out moveProjectionHitInfo, moveVector.magnitude);
    }
    
    private (Vector3, Vector3) CalculateSpheresInCapsule(Vector3 capsuleCenter, float capsuleHeight, float capsuleRadius)
    {
        // TODO: this function needs to account for collider center relative to transform
        Vector3 top, bottom;
        float yOffset;

        yOffset = capsuleHeight / 2f;
        top = capsuleCenter + new Vector3(0f, yOffset, 0f);
        bottom = capsuleCenter - new Vector3(0f, yOffset, 0f);

        return (top, bottom);
    }

    // TODO: refactor
    private RaycastHit GroundCheck()
    {
        Physics.SphereCast(this.transform.position, collider.radius, Vector3.down, out RaycastHit hitInfo, collider.height / 2f + groundCheckOffset);

        return hitInfo;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(this.transform.position + (Vector3.down * (collider.height / 2f + groundCheckOffset)), collider.radius);
            (Vector3 top, Vector3 bottom) sphereCenters = CalculateSpheresInCapsule(this.transform.position, collider.height, collider.radius);
            Vector3 moveVector = velocityVector * Time.fixedDeltaTime;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(sphereCenters.top + moveVector, collider.radius);
            Gizmos.DrawWireSphere(sphereCenters.bottom + moveVector, collider.radius);
        }
    }
}
