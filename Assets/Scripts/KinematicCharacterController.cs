using UnityEngine;

public class KinematicCharacterController : MonoBehaviour
{
    // Object references
    private Rigidbody rb;
    private CapsuleCollider capsule;

    // Fields
    private float groundCheckOffset = 0.05f;
    public float speed = 3f;
    public float collisionBuffer = 0.025f;

    void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
        capsule = this.GetComponentInChildren<CapsuleCollider>();
    }

    void FixedUpdate()
    {
        Vector2 movementInputs = new Vector2(
            Input.GetAxis("Horizontal"), // left, right/a, d
            Input.GetAxis("Vertical") // forward, backward/w, s
        );

        Vector3 movement = (Vector3.forward * movementInputs.y + Vector3.right * movementInputs.x).normalized * speed;
        Vector3 currentPosition = rb.position;
        Vector3 nextPosition = GetNextPosition(currentPosition, movement);
        rb.MovePosition(nextPosition);
    }

    private Vector3 GetNextPosition(Vector3 currentPosition, Vector3 movement)
    {
        Vector3 frameAdjustedMovement = GetMovementThisFrame(movement);
        Vector3 nextPosition = ResolveMovement(currentPosition, frameAdjustedMovement);
        return nextPosition;
    }

    private Vector3 GetMovementThisFrame(Vector3 movement)
    {
        return movement * Time.fixedDeltaTime;
    }

    private Vector3 ResolveMovement(Vector3 currentPosition, Vector3 frameAdjustedMovement)
    {
        Vector3 finalPosition;

        RaycastHit hitInfo;
        bool collided = CastCharacter(currentPosition, frameAdjustedMovement, out hitInfo);
        if (collided) {
            Vector3 displacement = frameAdjustedMovement.normalized * (hitInfo.distance - collisionBuffer);
            finalPosition = currentPosition + displacement;
        }
        else
        {
            finalPosition = currentPosition + frameAdjustedMovement;
        }

        return finalPosition;
    }

    private bool CastCharacter(Vector3 origin, Vector3 desiredMovement, out RaycastHit hitInfo)
    {
        Vector3 capsulePosition = capsule.transform.position;
        float capsuleHeight = capsule.height;
        float capsuleRadius = capsule.radius;
        Vector3 capsuleTop = capsulePosition + Vector3.up * (capsuleHeight/2 - capsuleRadius);
        Vector3 capsuleBottom = capsulePosition - Vector3.up * (capsuleHeight/2 - capsuleRadius);
        return Physics.CapsuleCast(capsuleTop, capsuleBottom, capsuleRadius, desiredMovement.normalized, out hitInfo, desiredMovement.magnitude);
    }
}