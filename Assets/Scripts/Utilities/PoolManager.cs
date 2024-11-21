using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
#region Start
    Dictionary<string, Stack<PoolObject>> stackDictionary = new Dictionary<string, Stack<PoolObject>>();
    void Start()
    {
        PoolManager.Instance.Load();
    }
    #endregion
#region Load
    private void Load()
    {
        PoolObject[] poolObjects = Resources.LoadAll<PoolObject>("PoolObjects");
        foreach(PoolObject poolObject in poolObjects)
        {
            Stack<PoolObject> objStack = new Stack<PoolObject>();
            objStack.Push(poolObject); //push in
            stackDictionary.Add(poolObject.name, objStack); //name the stack
        }
    }
    #endregion
#region Spawn
    public PoolObject Spawn(string name)// spawn the object
    {
        
        Stack<PoolObject> objStack = stackDictionary[name]; //ensure it matches the correct name in the dictionary 
        if(objStack.Count == 1) 
        {
            PoolObject poolObject = objStack.Peek();//is there already some alive?
            PoolObject objectClone = Instantiate(poolObject);
            objectClone.name = poolObject.name;
            return objectClone;
        }
        PoolObject oldPoolObject = objStack.Pop();
        oldPoolObject.gameObject.SetActive(true);//spawn the objects
        return oldPoolObject;
    }
    #endregion
#region DeSpawn
    public void DeSpawn(PoolObject poolObject)
    {
        Stack<PoolObject> objStack = stackDictionary[poolObject.name];
        poolObject.gameObject.SetActive(false);//Despawn the object
        objStack.Push(poolObject);
    }
    #endregion
}
