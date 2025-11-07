using System.Collections.Generic;
using UnityEngine;

public abstract class ItemEffect
{

    protected Player player;
    public virtual void OnApply() { }
    public virtual void OnUpdate() { }
    public virtual void OnRemove() { }
    public virtual void OnKill() { }

    public virtual void OnMove(Vector3 p) { }
}
public enum ItemEffectType
{
    MoveSpeedPercent,
    CameraViewPercent,
    EnergyOnKill,
    NoEnergyUseHPToDash,
    DashHeal,
    BurnTrail,
    AutoSpike,
    DashArmorAndDamage,
    DodgeChance,
    FlipControl,
    CoinDropOnDash,
    EnergyRegen,
    RefillEnergy,
    DeathImmunity,
    EnemyKnockback
}

public class ItemEffectHandler : MonoBehaviour
{
    private List<ItemEffect> activeEffects=new List<ItemEffect>();

    void Update()
    {
        foreach (var effect in activeEffects)
        {
            effect.OnUpdate();
        }
    }

    public void MoveActivate(Vector3 pos) 
    {
        foreach (var effect in activeEffects)
        {
            effect.OnMove(pos);
        }
    }

    public void KillActivate() 
    {
        foreach (var effect in activeEffects)
        {
            effect.OnKill();
        }
    }

    public void AddEffect(ItemEffect effect)
    {
        activeEffects.Add(effect);
        effect.OnApply();
    }

    public void RemoveEffect(ItemEffect effect)
    {
        effect.OnRemove();
        activeEffects.Remove(effect);
    }
}
