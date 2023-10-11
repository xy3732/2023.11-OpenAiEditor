using UnityEngine;

public class PlayerManager : MonoBehaviour 
{
    public int money;

    private void Awake()
    {
        money = 0;
    }

    public void moneyAdd(int amount)
    {
        money += amount;
    }

    public void moneyAdd()
    {
        money += 100;
    }
}