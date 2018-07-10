using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {

    public Transform target;
    public Vector3 targetOffset = Vector3.zero;
    public float maxDistanceFromTarget = 10;
    public float minDistanceFromTarget = 5;

    public AnimationCurve pitchDistanceCurve = new AnimationCurve();
    public AnimationCurve pitchOffsetCurve = new AnimationCurve();

    public bool invertY = true;
    public float minPitch = 0.0f;
    public float maxPitch = 90.0f;

    public Vector2 rotationSpeed = Vector2.zero;
    public float smoothDampTime = 0.1f;

    [Header("Debug Visualisation")]
    public bool showDebugTargetPosition = false;

    Vector2 rotation = Vector2.zero;
    Vector2 smoothRotation = Vector2.zero;
    Vector2 rotationVelocity = Vector2.zero;    

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void LateUpdate ()
    {
        if(target)
        {
            rotation.x += (Input.GetAxis("Mouse X") + Input.GetAxis("LookX")) * rotationSpeed.x * Time.deltaTime;
            //rotation.x = rotation.x % 360.0f;
            rotation.y += (Input.GetAxis("Mouse Y") + Input.GetAxis("LookY")) * rotationSpeed.y * Time.deltaTime * (invertY ? -1 : 1);
            rotation.y = Mathf.Clamp(rotation.y, minPitch, maxPitch);

            smoothRotation = Vector2.SmoothDamp(smoothRotation, rotation, ref rotationVelocity, smoothDampTime, float.PositiveInfinity, Time.deltaTime);

            Quaternion rotationAboutTarget = Quaternion.Euler(smoothRotation.y, smoothRotation.x, 0.0f);
            float distanceFromTarget = minDistanceFromTarget + (pitchDistanceCurve.Evaluate(rotation.y / 90.0f) * (maxDistanceFromTarget - minDistanceFromTarget));
            Vector3 relativePosition = rotationAboutTarget * (-Vector3.forward * distanceFromTarget);

            Vector3 lookAtPos = target.position + (rotationAboutTarget * targetOffset * pitchOffsetCurve.Evaluate(rotation.y / 90.0f));
            this.transform.position = lookAtPos + relativePosition;
            this.transform.LookAt(lookAtPos);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if(showDebugTargetPosition && target)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.position + targetOffset, 0.2f);
        }
    }
}
