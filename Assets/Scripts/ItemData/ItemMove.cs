using UnityEngine;

public class ItemMove : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        MoveItem();
    }

    void MoveItem()
    {
        //Fazer o item flutuar.
        float newY = Mathf.Sin(Time.time * moveSpeed) * 0.2f;
        transform.position = new Vector3(transform.position.x, newY + 1f, transform.position.z);
    }
}
