using System.Collections;
using UnityEngine;

public class KeyboardMovement : MonoBehaviour
{

    [Header("Walking")]
    public Vector3 movement;
    public float speed;

    [Header("Jumping")]
    public KeyCode jumpKey;
    public float jumpForce;

    [Header("Utils")]
    public CharacterController controller;
    public GroundCheck groundCheck;
    public Gravity gravity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        groundCheck = GetComponent<GroundCheck>();
        gravity = GetComponent<Gravity>();
    }

    // Update is called once per frame
    void Update()
    {
        // Acquisiamo la direzione di movimento
        movement = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical")).normalized;

        // Trasformiamo il vettore per passare dagli assi globali a quelli locali
        movement = transform.TransformDirection(movement);

        // Controllo se viene premuto il pulsante di salto e siamo a terra
        if (Input.GetKeyDown(jumpKey) && groundCheck.grounded) 
        {
            StartCoroutine(Jump());
        }

        // Prendiamo la componente parallela al piano di movimento, per evitare di slanciarsi quando scendiamo
        movement = Vector3.ProjectOnPlane(movement, groundCheck.hit.normal).normalized;

        // Aggiustiamo la velocità per adattarci al piano e applichiamo il movimento
        controller.Move(movement * speed * Mathf.Cos(groundCheck.slopeAngle * Mathf.Deg2Rad) * Time.deltaTime);
    }

    private IEnumerator Jump() 
    { 
        
        // Impostiamo una velocità positiva verticale (opposta a quella di gravità)
        gravity.applyGravity = false; // Altrimenti nello script gravity si resetterebbe subito in quanto siamo a terra
        gravity.speed = -jumpForce;
        yield return new WaitForSeconds(0.1f);
        gravity.applyGravity = true; // Possiamo riabilitare la gravità per cominciare a cadere di nuovo
    }
}
