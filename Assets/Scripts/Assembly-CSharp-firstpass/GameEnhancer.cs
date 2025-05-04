using UnityEngine;

public class GameEnhancer
{
    public float voucher;
    public float crystals;
    public float cash;

    // Constructor to initialize the values
    public GameEnhancer(float voucher, float crystals, float cash)
    {
        this.voucher = Mathf.Clamp(voucher, 0f, 5000f);
        this.crystals = Mathf.Clamp(crystals, 0f, 2500f);
        this.cash = Mathf.Clamp(cash, 0f, 7500000f);
    }

    // Method to clamp the values again if needed
    public void ClampValues()
    {
        voucher = Mathf.Clamp(voucher, 0f, 5000f);
        crystals = Mathf.Clamp(crystals, 0f, 2500f);
        cash = Mathf.Clamp(cash, 0f, 7500000f);
    }
}
