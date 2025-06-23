using UnityEngine;

public class FaunaManager : MonoBehaviour
{
    public static FaunaManager instance;
    public PhysicsMaterial defaultFaunaMaterial;
    public int faunaLayer = 9;

    void Awake()
    {
        instance = this;
    }
}
