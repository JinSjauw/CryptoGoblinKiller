using System;

public class EnemyHit : EventArgs
{
    public float Damage { get; set; }
    public HitBoxType HitBoxType { get; set; }
    public EnemyHitBox HitBox { get; set; }
    
    public EnemyHit(float damage, HitBoxType type, EnemyHitBox hitBox)
    {
        Damage = damage;
        HitBoxType = type;
        HitBox = hitBox;
    }
}
