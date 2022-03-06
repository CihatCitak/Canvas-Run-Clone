using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    #region Singleton

    public static ObjectPooler Instance { get { return instance; } }
    private static ObjectPooler instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            poolDictinary = new Dictionary<string, Queue<GameObject>>();

            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab);
                    obj.SetActive(false);
                    obj.transform.parent = transform;
                    objectPool.Enqueue(obj);
                }

                poolDictinary.Add(pool.tag, objectPool);
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    #endregion


    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictinary;

    void Start()
    {

    }

    public GameObject DequeueFromPool(string tag)
    {
        if (!poolDictinary.ContainsKey(tag))
        {
            Debug.LogError("Pool with tag " + tag + " doesn't excist.");
            return null;
        }

        var queue = poolDictinary[tag];
        GameObject go = null;

        if (queue.Count > 0)
        {
            go = queue.Dequeue();
        }
        else
        {
            //Find Prefab and pool
            Pool pool = null;
            foreach (Pool tempPool in pools)
            {
                if (tempPool.tag.Equals(tag))
                {
                    pool = tempPool;
                }
            }

            go = Instantiate(pool.prefab);
        }

        go.SetActive(true);
        return go;
    }

    public void EnqueueToPool(string tag, GameObject gameObject)
    {
        if (!poolDictinary.ContainsKey(tag))
        {
            Debug.LogError("Pool with tag " + tag + " doesn't excist.");
            return;
        }

        //Enqueue transform settings
        gameObject.transform.parent = transform;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.SetActive(false);

        poolDictinary[tag].Enqueue(gameObject);
    }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }
}
