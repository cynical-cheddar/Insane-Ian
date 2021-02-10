using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum ReloadType
    {
        recharge,
        byClip,
        noReload
    }
    
    public enum DamageType
    {
        kinetic,
        energy,
        thermal,
        ramming,
        explosive

    }
    
   
    
    
    // Start is called before the first frame update
    [Header("Damage Falloff")]
    public AnimationCurve damageRampupMultiplierCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    public float damageMultiplierPlateuDistance = 100f;
    public float damageMultiplierClosestRampupThreshold = 10f;
    [Header("Salvo and Reloading")]
    [SerializeField]
    protected int salvoSize = 1;
    protected int currentSalvo=0;
    [SerializeField] protected ReloadType reloadType;
    [SerializeField] protected float reloadTime = 1f;
    [SerializeField] protected float fireRate = 0.5f;
    [Header("Damage")]
    [SerializeField] protected float baseDamage = 10f;
    [SerializeField] protected DamageType damageType;
    [SerializeField] protected float damageMultiplier = 1f;
    
    void Start()
    {
        
    }


    protected void decrementSalvo(){
        currentSalvo -=1;
        if(currentSalvo < 0) currentSalvo = 0;
    }
    protected void reloadShells(int amount){
        currentSalvo += amount;
        if(currentSalvo > salvoSize) currentSalvo = salvoSize;
    }
    protected void reloadFull(){
        currentSalvo += salvoSize;
        if(currentSalvo > salvoSize) currentSalvo = salvoSize;
    }

    protected void Update()
    {
        if (reloadType == ReloadType.recharge)
        {
            // recharge the current salvo
        }
    }

    public void Fire(Vector3 barrelEnd, Vector3 targetPoint)
    {
        
    }

    float CalculateDamageMultiplierCurve(float distance)
    {
        float damageRampupMultiplier = 1f;
        if(distance < damageMultiplierPlateuDistance){
            // calculate value
            if(distance < damageMultiplierClosestRampupThreshold){
                damageRampupMultiplier = damageRampupMultiplierCurve.Evaluate(0f);
            }
            else{
                float fraction = (distance - damageMultiplierClosestRampupThreshold)/(damageMultiplierPlateuDistance-damageMultiplierClosestRampupThreshold);
                damageRampupMultiplier = damageRampupMultiplierCurve.Evaluate(fraction);
            }
           
        }
        return damageRampupMultiplier;
    }
}
