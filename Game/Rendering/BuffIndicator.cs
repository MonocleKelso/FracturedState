using FracturedState.Game;
using UnityEngine.UI;

public class BuffIndicator : DamageIndicator
{

    public void ApplyBuff(string buff, float amount)
    {
        string b = amount > 0 ? "+" : "-";
        b += buff;
        Text[] tex = GetComponentsInChildren<Text>();
        foreach (var t in tex)
        {
            t.text = b;
        }
    }
    
    protected override void Return()
    {
        ObjectPool.Instance.ReturnBuffHelper(gameObject);
    }
}