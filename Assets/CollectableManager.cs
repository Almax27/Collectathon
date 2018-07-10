using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableManager : SingletonBehaviour<CollectableManager> {

    HashSet<Collectable> activeCollectables = new HashSet<Collectable>();
    Queue<Collectable> additions = new Queue<Collectable>();
    Queue<Collectable> removals = new Queue<Collectable>();

    // Update is called once per frame
    void LateUpdate () {
        float deltaTime = Time.deltaTime;
        while (additions.Count > 0)
        {
            activeCollectables.Add(additions.Dequeue());
        }
        while (removals.Count > 0)
        {
            activeCollectables.Remove(removals.Dequeue());
        }
        foreach (Collectable collectable in activeCollectables)
        {
            collectable.Tick(deltaTime);
        }
	}

    public void RegisterActiveCollectable(Collectable collectable)
    {
        additions.Enqueue(collectable);
    }

    public void UnregisterActiveCollectable(Collectable collectable)
    {
        removals.Enqueue(collectable);
    }
}
