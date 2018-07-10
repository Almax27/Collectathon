using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : SingletonBehaviour<PlayerController> {

    public float maxGroundSpeed = 10.0f;
    public float maxAirSpeed = 10.0f;
    public float accelerationTime = 0.1f;
    public float decelerationTime = 0.1f;
    public float jumpHeight = 2.0f;
    public float jumpTime = 0.5f;
    public float gravity = -100;

    CharacterController character = null;

    // Use this for initialization
    void Start () {
        character = GetComponent<CharacterController>();
        Debug.Assert(character);
        base.Start();
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateCharacter(Time.deltaTime);
    }

    void UpdateCharacter(float dt)
    {
        float maxSpeed = character.isGrounded ? maxGroundSpeed : maxAirSpeed;

        Vector3 velocity = character.velocity;
        Vector3 inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));
        Vector3 inputVelocity = new Vector3(velocity.x, 0, velocity.z);

        if (inputVector.sqrMagnitude > 0 && accelerationTime > 0)
        {
            //rotate input vector relative to camera
            if (Camera.main)
            {
                float cameraRotY = Camera.main.transform.rotation.eulerAngles.y;
                inputVector = Quaternion.Euler(0, cameraRotY, 0) * inputVector;
            }

            float deltaSpeed = (maxSpeed / accelerationTime) * dt;
            Vector3 deltaVelocity = inputVector * deltaSpeed;
            inputVelocity += deltaVelocity;
            inputVelocity = inputVelocity.normalized * Mathf.Min(inputVelocity.magnitude, maxSpeed);
        }
        else if (decelerationTime > 0)
        {
            //decelerate down to rest
            float deltaSpeed = (maxSpeed / decelerationTime) * dt;
            inputVelocity -= inputVelocity.normalized * Mathf.Min(inputVelocity.magnitude, deltaSpeed);
        }
        else
        {
            inputVelocity = Vector3.zero;
        }

        velocity.x = inputVelocity.x;
        velocity.z = inputVelocity.z;

        if (character.isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                float jumpSpeed = Mathf.Sqrt(2.0f * gravity * jumpHeight);
                velocity.y = Mathf.Abs(jumpSpeed);
            }
        }

        velocity.y -= gravity * dt;

        character.Move(velocity * dt);
    }

    private void OnDrawGizmos()
    {
        if (character)
        {
            Gizmos.color = character.isGrounded ? Color.red : Color.white;
            Gizmos.DrawWireSphere(transform.position + new Vector3(0, 1), 0.2f);
        }
    }
}
