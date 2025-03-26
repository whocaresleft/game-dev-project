using UnityEngine;
public class Gravity : MonoBehaviour
{
    [Header("Gravity")]
    public Vector3 direction;
    public float speed;
    public float g;
    public bool applyGravity;

    [Header("Utils")]
    public GroundCheck groundCheck;
    public CharacterController controller;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        groundCheck = GetComponent<GroundCheck>();
        direction = Vector3.down;
        speed = 0.0f;
        applyGravity = true;
    }

    // Update is called once per frame
    void Update()
    {

        // Incrementiamo la velocità di caduta, a meno che non tocchiamo il pavimento, resettandola in caso
        if (applyGravity) UpdateSpeed();
        controller.Move(direction * speed * Time.deltaTime);
    }

    private void UpdateSpeed() 
    {
        speed = groundCheck.grounded ? 0.0f : speed + g * Time.deltaTime;
    }
}
