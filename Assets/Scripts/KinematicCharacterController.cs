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
        velocityVector = (this.transform.forward * movementInputs.y + this.transform.right * movementInputs.x).normalized * moveSpeed;
        Vector3 newPosition = CalculateNewPosition(rb.position, velocityVector, collider);
        rb.MovePosition(newPosition);
    }

    // TODO: refactor
    private RaycastHit GroundCheck()
    {
        Physics.SphereCast(this.transform.position, collider.radius, Vector3.down, out RaycastHit hitInfo, collider.height / 2f + groundCheckOffset);

        return hitInfo;
    }

    private Vector3 CalculateNewPosition(Vector3 start, Vector3 velocityVector, CapsuleCollider playerCollider)
    {
        Vector3 moveVector = GetMovementThisFrame(velocityVector);
        Vector3 newMoveVector = ProjectMovement(start, playerCollider, moveVector);
        return start + newMoveVector;
    }

    private Vector3 GetMovementThisFrame(Vector3 velocityVector)
    {
        return velocityVector * Time.fixedDeltaTime;
    }

    private Vector3 ProjectMovement(Vector3 origin, CapsuleCollider collider, Vector3 moveVector)
    {
        Vector3 newMoveVector;
        Vector3 collidePosition;
        Vector3 remainingMove;

        (Vector3 top, Vector3 bottom) sphereCenters = CalculateSpheresInCapsule(origin, collider.height, collider.radius);
        Debug.DrawLine(sphereCenters.top, sphereCenters.bottom, Color.red);
        Debug.DrawLine(sphereCenters.top, sphereCenters.top + moveVector * 50f, Color.red);
        if (Physics.CapsuleCast(sphereCenters.top, sphereCenters.bottom, collider.radius, moveVector.normalized, out RaycastHit moveProjectionHitInfo, moveVector.magnitude))
        {
            collidePosition = origin + moveVector.normalized * (moveProjectionHitInfo.distance - colliderPadding);
            newMoveVector = moveVector.normalized * (moveProjectionHitInfo.distance - colliderPadding); // Need to use colliderPadding or else collider will clip through wall
            remainingMove = moveVector - newMoveVector;
            //newMoveVector = Vector3.ProjectOnPlane(moveVector, moveProjectionHitInfo.normal);
            newMoveVector = CollideAndSlide(collidePosition, remainingMove, moveProjectionHitInfo.normal);
        } 
        else
        {
            newMoveVector = moveVector;
        }

        return newMoveVector;
    }

    private Vector3 CollideAndSlide(Vector3 startPosition, Vector3 moveVector, Vector3 normal) // Add parameter to limit number of collideandslides
    {
        return Vector3.ProjectOnPlane(moveVector, normal);
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
