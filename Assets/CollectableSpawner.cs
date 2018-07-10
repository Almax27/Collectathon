using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollectableBurstSpawn
{
    public float time = 0;
    public uint count = 0;
}

public class CollectableSpawner : MonoBehaviour {

    public Collectable CollectablePrefab = null;
    public float spawnRate = 1.0f;
    public float duration = 1;
    public List<CollectableBurstSpawn> bursts;

    float burstTick = 0;
    float tick = 0;
    public List<Collectable> activeCollectables = new List<Collectable>();

	// Update is called once per frame
	void Update () {
        if (duration > 0)
        {
            if (burstTick + Time.deltaTime > duration)
            {
                burstTick -= duration;
            }
            foreach (CollectableBurstSpawn burst in bursts)
            {
                if (burstTick <= burst.time && burstTick + Time.deltaTime > burst.time)
                {
                    for (int i = 0; i < burst.count; i++)
                    {
                        DoSpawn();
                    }
                }
            }
            burstTick += Time.deltaTime;
        }
            
        tick += Time.deltaTime;
            
        if (spawnRate > 0)
        {
            float interval = 1.0f / spawnRate;
            while (tick > interval)
            {
                tick -= interval;
                DoSpawn();
            }
        }

        foreach(Collectable c in activeCollectables)
        {
            c.Tick(Time.deltaTime);
        }
    }

    void DoSpawn()
    {
        if (CollectablePrefab)
        {
            GameObject gobj = GameObjectPoolManager.Instance.GetOrCreate(CollectablePrefab.gameObject, transform.position, Quaternion.identity);
            if(gobj)
            {
                Collectable collectable = gobj.GetComponent<Collectable>();
                collectable.Velocity = Random.onUnitSphere * Random.Range(10, 30);
            }
        }
    }
}
