using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CollectableConfig", menuName = "Config/Collectable", order = 1)]
public class CollectableConfig : ScriptableObject
{
    [Header("Simulation")]
    public float physicalRadius = 0.5f;
    public float coeffRestitution = 0.5f;
    public float gravity = 30;
    public float maxSpeed = 50;
    public float restSpeed = 0.1f;
    public LayerMask hitMask = new LayerMask();

    [Header("Collection")]
    public float collectionRadius = 10.0f;
    public float collectionDelay = 1.0f;
    public MinMax collectionDuration = new MinMax(0.5f, 1.0f);
    public AnimationCurve attractionCurve = new AnimationCurve();
    public AnimationCurve heightCurve = new AnimationCurve();
    public float maxHeightOffset = 5.0f;
}
