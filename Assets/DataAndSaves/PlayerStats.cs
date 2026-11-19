using UnityEngine;
[CreateAssetMenu(menuName="Mountain Adventure/Player Stats")]
public class PlayerStats : ScriptableObject {
    public float moveSpeed, jumpForce, invincibleTime;
    public int maxHealth, blinkCount;
    public float bulletSpeed, shotCooldown, muzzleOffset, bulletLifetime;
    public int bulletDamage;
}
