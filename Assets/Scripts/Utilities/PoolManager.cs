using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
#region Start
    Dictionary<string, Stack<GameObject>> stackDictionary = new Dictionary<string, Stack<GameObject>>();
    void Start()
    {
        PoolManager.Instance.Load();
    }
    #endregion
#region Load
    private void Load()
    {
        GameObject[] poolObjects = Resources.LoadAll<GameObject>("PoolObjects");
        foreach(GameObject poolObject in poolObjects)
        {
            Stack<GameObject> objStack = new Stack<GameObject>();
            objStack.Push(poolObject); //push in
            stackDictionary.Add(poolObject.name, objStack); //name the stack
        }
    }
    #endregion
#region Spawn
    public GameObject Spawn(string name)// spawn the object
    {
        
        Stack<GameObject> objStack = stackDictionary[name]; //ensure it matches the correct name in the dictionary 
        if(objStack.Count == 1) 
        {
            GameObject poolObject = objStack.Peek();//is there already some alive?
            GameObject objectClone = Instantiate(poolObject).gameObject;
            objectClone.name = poolObject.name;
            return objectClone;
        }
        GameObject oldPoolObject = objStack.Pop();
        oldPoolObject.gameObject.SetActive(true);//spawn the objects
        return oldPoolObject;
    }
    #endregion
#region DeSpawn
    public void DeSpawn(GameObject poolObject)
    {
        Stack<GameObject> objStack = stackDictionary[poolObject.name];
        poolObject.gameObject.SetActive(false);//Despawn the object
        objStack.Push(poolObject);
    }
    #endregion
}
