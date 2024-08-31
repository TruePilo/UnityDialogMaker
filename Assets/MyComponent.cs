using UnityEngine;

public class MyComponent : MonoBehaviour
{
    [SerializeField] public int myInt;
    public float myFloat;
    public string myString;

    public void start()
    {
        myInt = 0;
        myFloat = 0.0f;
        myString = myInt.ToString();
    }
}