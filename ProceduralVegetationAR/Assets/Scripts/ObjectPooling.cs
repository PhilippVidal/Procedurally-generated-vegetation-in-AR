using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string name;
        public GameObject[] models;
        public int amount;

        public Pool(string name, GameObject[] models, int amount)
        {
            this.name = name;
            this.models = models;
            this.amount = amount;
        }
    }


    GameSettings gameSettings; 
    public Dictionary<string, Queue<GameObject>> mObjectPoolsDictionary;
    List<Pool> mPools;
    Dictionary<string, Pool> mPoolDictionary;
 
    private void Start()
    {
        gameSettings = GameManager.SETTINGS; 

        mObjectPoolsDictionary = new Dictionary<string, Queue<GameObject>>();
        mPoolDictionary = new Dictionary<string, Pool>();
        FillDictionary();  
    }

    public void RefillPool(string name)
    {
        Pool pool = mPoolDictionary[name]; 
        int amountToRefill = (int)Mathf.Ceil((float)pool.amount * gameSettings.refillPercentage);
        //Debug.Log("Pool got refilled! With " + amountToRefill + " " + name);
        for (int i = 0; i < amountToRefill; i++)
        {
            int randIndex = Random.Range(0, pool.models.Length);
            GameObject obj = Instantiate(pool.models[randIndex], transform);
            obj.SetActive(false);
            mObjectPoolsDictionary[name].Enqueue(obj);
        }
    }

    public GameObject GetObjectFromPool(string name, Vector3 position, Quaternion rotation)
    {
        if (!mObjectPoolsDictionary.ContainsKey(name) && !mPoolDictionary.ContainsKey(name))
        {
            //Debug.LogWarning("No entry with name: " + name);
            return null;
        }


        if (mObjectPoolsDictionary[name].Count <= (int)Mathf.Ceil((float)mPoolDictionary[name].amount * gameSettings.percentageToRefillAt))
        {
            //Debug.Log("Refill amount reached!: " + (int)Mathf.Ceil((float)mPoolDictionary[name].amount * gameSettings.percentageToRefillAt));
            RefillPool(name);
        }

        GameObject obj = mObjectPoolsDictionary[name].Dequeue();
        obj.SetActive(true);
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        return obj; 
    }

    public GameObject GetObjectFromPool(string name)
    {
        if (!mObjectPoolsDictionary.ContainsKey(name) && !mPoolDictionary.ContainsKey(name))
        {
            //Debug.LogWarning("No entry with name: " + name);
            return null;
        }


        if (mObjectPoolsDictionary[name].Count <= (int)Mathf.Ceil((float)mPoolDictionary[name].amount * gameSettings.percentageToRefillAt))
        {
            //Debug.Log("Refill amount reached!: " + (int)Mathf.Ceil((float)mPoolDictionary[name].amount * gameSettings.percentageToRefillAt));
            RefillPool(name);
        }

        GameObject obj = mObjectPoolsDictionary[name].Dequeue();
        obj.SetActive(true);

        return obj;
    }
    void FillDictionary()
    {
        GeneratePools();

        for (int i = 0; i < mPools.Count; i++)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int j = 0; j < mPools[i].amount; j++)
            {
                int randIndex = Random.Range(0, mPools[i].models.Length);
                GameObject obj = Instantiate(mPools[i].models[randIndex], transform);
                obj.SetActive(false);
                obj.transform.parent = transform; 
                objectPool.Enqueue(obj);
            }

            mObjectPoolsDictionary.Add(mPools[i].name, objectPool);
            mPoolDictionary.Add(mPools[i].name, mPools[i]);
        }
    }

    void GeneratePools()
    {
        mPools = new List<Pool>();

        List<Plant> plants = GameManager.SETTINGS.Plants;

        for (int i = 0; i < plants.Count; i++)
        {
            mPools.Add(new Pool(plants[i].Name, plants[i].PlantModels, plants[i].RequiredAmount));
        }
    }
}
