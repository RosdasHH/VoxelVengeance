using UnityEngine;

public class WeaponData : MonoBehaviour
{
    public GameObject bullet;
    public Transform bulletSpawn;
    public Sprite crosshair;
    public float crosshairRange;
    public ParticleSystem MuzzleFlash;
    public int damage;
    public float cooldown;
    public bool autofire;
    public float bloom;
}