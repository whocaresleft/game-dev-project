 using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [Header("Ground Collision Check")]
    public float checkDistance;
    public Vector3 playerFeetPosition;
    public RaycastHit hit;
    public bool grounded;
    public Gravity gravity;

    [Header("Ground distance")]
    public float groundDistance;
    public float baseGroundDistanceTolerance;
    public float groundDistanceTolerance;

    [Header("Ground angle")]
    public float slopeAngle;
    public float maxSlopeAngle;
    public float slopeAngleTolerance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Riferimento allo script gravity, che conterrà la direzione della gravità
        gravity = GetComponent<Gravity>();
    }

    // Update is called once per frame
    void Update()
    {
        // Per trovare i piedi si parte dal centro e ci abbassiamo di metà altezza
        playerFeetPosition = transform.position + Vector3.down;
        Debug.DrawRay(playerFeetPosition, Vector3.down * checkDistance, Color.red);
        // Lanciamo un raycast verso la direzione della gravità
        if (Physics.Raycast(playerFeetPosition, gravity.direction, out hit, checkDistance))
        {

            // Se viene colpito qualcosa controlliamo che sia entro la distanza consentita, e che l'angolo sia entro il massimo consentito per essere considerati a 'terra'
            groundDistance = hit.distance;
            slopeAngle = Vector3.Angle(-1 * gravity.direction, hit.normal);
            groundDistanceTolerance = baseGroundDistanceTolerance + (Mathf.Tan(slopeAngle * Mathf.Deg2Rad) / 3.0f);
            grounded = (groundDistance <= groundDistanceTolerance) && (slopeAngle <= maxSlopeAngle + slopeAngleTolerance);
        }

        // Non e' stato colpito niente, siamo a mezz'aria
        else 
        {
            grounded = false;
            slopeAngle = 0.0f;
        }
    }
}
