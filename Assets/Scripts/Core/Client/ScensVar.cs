using UnityEngine;

public class ScensVar : MonoBehaviour
{
    static public int BossType = 0;
    static public int LevelId = 0;

    private void Start()
    {
        DontDestroyOnLoad(this);
    }
}
