using UnityEngine;

public class GameHandler : MonoBehaviour
{
    public static GameHandler Instance;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        Instance = this;
    }
}
