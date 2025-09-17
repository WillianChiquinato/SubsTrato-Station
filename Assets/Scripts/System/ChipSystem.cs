using UnityEngine;

public class ChipSystem : MonoBehaviour
{
    public int chipSystemCount = 0;
    public bool AberturaFinal = false;
    public GameObject DoorFinal;

    void Update()
    {
        if (chipSystemCount >= 3)
        {
            AberturaFinal = true;
            if (AberturaFinal)
            {
                DoorFinal.GetComponent<Animator>().SetTrigger("Open");
                DoorFinal.GetComponent<BoxCollider>().enabled = false;
            }
        }
    }

    public void Interact()
    {
        Debug.Log("Interacted with chip: " + gameObject.name);
        chipSystemCount++;
        //Adicionar particulas após pegar o chip (TODO).
    }
}
