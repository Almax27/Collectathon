using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameObjectPool
{
    public GameObject templateObject = null;

    //Minimum number of objects in pool, this will be used to preallocate
    public int minObjects = 0;

    //Maxium number of objects this pool can allocate, create() will return null when this limit is reached. Negative numbers are considered infinte
    public int maxObjects = -1;

    HashSet<GameObject> activeInstances = new HashSet<GameObject>();
    Queue<GameObject> inactiveInstances = new Queue<GameObject>();

    GameObject rootObject;

    public void Initialise(Transform parent)
    {
        if(templateObject == null)
        {
            Debug.LogError("Failed to initialise Pool, no templateObject defined");
            return;
        }

        rootObject = new GameObject();
        rootObject.name = "Pool_" + templateObject.name;
        rootObject.transform.parent = parent;

        for (int i = 0; i < minObjects; i++)
        {
            GameObject gobj = GetOrCreate(Vector3.zero, Quaternion.identity);
            gobj.SetActive(false);
            inactiveInstances.Enqueue(gobj);
        }
    }

    public void Rebuild()
    {
        foreach(Transform child in rootObject.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.activeSelf)
                activeInstances.Add(child.gameObject);
            else
                inactiveInstances.Enqueue(child.gameObject);
        }
    }

    public GameObject GetOrCreate(Vector3 position, Quaternion rotation)
    {
        GameObject instance = null;

        //First try and return an inactive object in the pool
        while(!instance && inactiveInstances.Count > 0)
        {
            instance = inactiveInstances.Dequeue();
        }

        //Otherwise spawn a new one if we are allowed
        if(instance == null && (maxObjects < 0 || activeInstances.Count < maxObjects))
        {
            if(templateObject != null)
            {
                instance = GameObject.Instantiate<GameObject>(templateObject);
            }
        }

        //if we obtained an instance then initialise it
        if(instance)
        {
            instance.transform.parent = rootObject.transform;
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            activeInstances.Add(instance);
        }

        return instance;
    }

    public bool ReturnToPool(GameObject instance)
    {
        if (instance && activeInstances.Remove(instance))
        {
            instance.transform.parent = rootObject.transform;
            instance.SetActive(false);
            inactiveInstances.Enqueue(instance);
            return true;
        }
        return false;
    }
}

public class GameObjectPoolManager : SingletonBehaviour<GameObjectPoolManager>
{
    public List<GameObjectPool> staticPools = new List<GameObjectPool>();

    [Header("Dynamic Pools")]
    public int dynamicPoolMinObjects = 0;
    public int dynamicPoolMaxObjects = -1;

    List<GameObjectPool> dynamicPools = new List<GameObjectPool>();
    Dictionary<int, GameObjectPool> poolsByInstanceID = new Dictionary<int, GameObjectPool>();
    bool initialised = false;

    protected void OnEnable()
    {
        if (!initialised)
        {
            for (int i = 0; i < staticPools.Count; i++)
            {
                GameObjectPool pool = staticPools[i];
                if (pool.templateObject == null) 
                {
                    Debug.LogWarningFormat("Static pool {0} has no templateObject defined", i);
                }
                else
                {
                    poolsByInstanceID[pool.templateObject.GetInstanceID()] = pool;
                }
            }
            foreach (var pair in poolsByInstanceID)
            {
                pair.Value.Initialise(this.transform);
            }
            initialised = true;
        }
        else
        {
            poolsByInstanceID.Clear();
            foreach (GameObjectPool pool in staticPools)
            {
                pool.Rebuild();
                poolsByInstanceID[pool.templateObject.GetInstanceID()] = pool;
            }
            foreach (GameObjectPool pool in dynamicPools)
            {
                pool.Rebuild();
                poolsByInstanceID[pool.templateObject.GetInstanceID()] = pool;
            }
        }
    }

    public GameObject GetOrCreate(GameObject templateObject)
    {
        return GetOrCreate(templateObject, Vector3.zero, Quaternion.identity);
    }

    public GameObject GetOrCreate(GameObject templateObject, Vector3 position, Quaternion rotation)
    {
        if (templateObject == null) return null;

        //find the pool with the given gameobject
        int instanceID = templateObject.GetInstanceID();
        GameObjectPool pool = null;
        if (!poolsByInstanceID.TryGetValue(instanceID, out pool))
        {
            //if there was no pool for this object create a new one
            pool = new GameObjectPool();
            pool.templateObject = templateObject;
            pool.minObjects = dynamicPoolMinObjects;
            pool.maxObjects = dynamicPoolMaxObjects;
            pool.Initialise(this.transform);
            poolsByInstanceID[instanceID] = pool;
        }

        return pool.GetOrCreate(position, rotation);
    }

    public void ReturnToPool(GameObject instance)
    {
        foreach (var pair in poolsByInstanceID)
        {
            if(pair.Value.ReturnToPool(instance))
            {
                break;
            }
        }
    }
}
