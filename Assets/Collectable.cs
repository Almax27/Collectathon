using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public CollectableConfig config = null;

    public Vector3 Velocity
    {
        get { return velocity; }
        set { velocity = value; }
    }
    Vector3 velocity = Vector3.zero;

    enum State
    {
        Uninitialised,
        Simulating,
        Resting,
        Collecting,
        Collected
    }

    State state = State.Uninitialised;
    PlayerController targetPlayer = null;
    float tick = 0;
    float collectionDistance = 0;
    float collectionHeight = 0;
    float collectionDuration = 0;

    private void OnEnable()
    {
        Debug.Assert(config, "No config on collectable instance");

        SetState(State.Simulating);

        targetPlayer = PlayerController.Instance;

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.material.color = Random.ColorHSV(0, 1, 0.8f, 1);
        }

        StartCoroutine("CheckForPlayerPickup");
    }

    private void OnDisable()
    {
        SetState(State.Uninitialised);
        StopCoroutine("CheckForPlayerPickup");
    }

    void SetState(State newState)
    {
        if (newState == state)
            return;

        tick = 0.0f;
        bool isActive = false;
        switch (newState)
        {
            case State.Uninitialised:
                break;
            case State.Simulating:
                isActive = true;
                break;
            case State.Resting:
                break;
            case State.Collecting:
                isActive = true;
                collectionDistance = Vector3.Distance(targetPlayer.transform.position, transform.position);
                collectionHeight = transform.position.y;
                collectionDuration = config.collectionDuration.RandomInRange();
                break;
            case State.Collected:
                GameObjectPoolManager.Instance.ReturnToPool(this.gameObject);
                break;
        }

        if (isActive)
            CollectableManager.Instance.RegisterActiveCollectable(this);
        else
            CollectableManager.Instance.UnregisterActiveCollectable(this);

        state = newState;
    }

    IEnumerator CheckForPlayerPickup()
    {
        yield return new WaitForSeconds(config.collectionDelay);
        while (true)
        {
            if (config && state < State.Collecting)
            {
                Vector3 playerVector = targetPlayer.transform.position - this.transform.position;
                if (playerVector.sqrMagnitude < config.collectionRadius * config.collectionRadius)
                {
                    SetState(State.Collecting);
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void Tick(float dt)
    {
        tick += dt;

        switch (state)
        {
            case State.Simulating:
                PerformMove(dt);
                break;
            case State.Collecting:
                TrackPlayer(dt);
                break;
        }
    }

    void TrackPlayer(float dt)
    {
        if (targetPlayer && tick < collectionDuration)
        {
            float tval = tick / collectionDuration;

            Vector3 playerVector = targetPlayer.transform.position - transform.position;
            float playerDistance = playerVector.magnitude;
            playerVector /= playerDistance;
            playerVector *= Mathf.Min(playerDistance, config.attractionCurve.Evaluate(tval) * collectionDistance);

            Vector3 pos = targetPlayer.transform.position - playerVector;
            pos.y = collectionHeight + config.heightCurve.Evaluate(tval) * config.maxHeightOffset;

            transform.position = pos;
        }
        else
        {
            SetState(State.Collected);
        }
    }

    void PerformMove(float dt)
    {
        velocity.y -= config.gravity * dt;

        Vector3 position = transform.position;
        float timeToTravel = dt;
        RaycastHit hitInfo;
        while (timeToTravel > 0.0001f)
        {
            float speed = velocity.magnitude;

            if (speed < config.restSpeed)
            {
                SetState(State.Resting);
                break;
            }

            float distance = speed * timeToTravel;
            Vector3 direction = velocity / speed;
            if (Physics.SphereCast(position, config.physicalRadius, direction, out hitInfo, distance, config.hitMask))
            {
                float timeTraveled = hitInfo.distance / speed;
                timeToTravel -= timeTraveled;
                position += velocity * timeTraveled;
                if (Vector3.Dot(direction, hitInfo.normal) < 0)
                {
                    velocity = Vector3.Reflect(velocity, hitInfo.normal) * config.coeffRestitution;
                }
            }
            else
            {
                position += velocity * timeToTravel;
                break;
            }
        }
        transform.position = position;
    }

    /*
    private void OnDrawGizmosSelected()
    {
        DebugExtension.DebugWireSphere(transform.position, Color.red, physicalRadius);
        DebugExtension.DebugWireSphere(transform.position, Color.yellow, collectionRadius);
        DebugExtension.DrawArrow(transform.position, velocity.normalized, Color.red);
    }
    */
}
