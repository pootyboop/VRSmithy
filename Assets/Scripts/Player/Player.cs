using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;
    public Transform handParent;
    void Awake()
    {
        instance = this;
    }
}
