using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_CrystalLauncher : MonoBehaviour, IAbility
{
    [SerializeField] private float _damage;
    [SerializeField] private float _cooldown;
    [SerializeField] private float _range;
    [SerializeField] private int _maxEnemyHits;
    [SerializeField] private int _currEnemyHits;
    [SerializeField] private float _speed;
    [SerializeField] private float _accel;

    private bool _shooting = false;
    private bool _charged = true;

    [SerializeField] private float _poisonTime;
    [SerializeField] private float _burnTime;
    [SerializeField] private float _stunTime;

    public float PoisonTime { get => _poisonTime; set => _poisonTime = value; }
    public float BurnTime { get => _burnTime; set => _burnTime = value; }
    public float StunTime { get => _stunTime; set => _stunTime = value; }
    public float Damage { get => _damage; set => _damage = value; }
    public float Speed { get => _speed; set => _speed = value; }
    public float Acceleration { get => _accel; set => _accel = value; }
    public float Cooldown { get => _cooldown; set => _cooldown = value; }
    public float Range { get => _range; set => _range = value; }
    public float Damage1 { get => _damage; set => _damage = value; }
    GameObject _crystalLauncherPrefab;
    GameObject _crystalExplosionPrefab;


    // Update is called once per frame
    void Update()
    {
        if (_charged)
        {
            GameObject closestEnemy = GameManager.instance.GetNearestEnemy(transform.position, _range);
            if (closestEnemy != null) { SpawnCrystal(closestEnemy); }

        }
    }

    public void SpawnCrystal(GameObject targetedEnemy)
    {
        _charged = false;

        StartCoroutine(nameof(Recharge));
        Vector3 dirTowardsPlayer = (transform.position - targetedEnemy.transform.position); // get the direction the enemy is moving
        Vector3 dir = ((targetedEnemy.transform.position + dirTowardsPlayer /2) - transform.position).normalized; // get the direction from the crystal with projected enemy path
        Vector3 airDirection = dir;
        airDirection.y += 8;
        GameObject currCrystal = Instantiate(_crystalLauncherPrefab, transform.position,
            Quaternion.identity);
        currCrystal.transform.rotation = Quaternion.FromToRotation(Vector3.up, new Vector3(airDirection.x, airDirection.y, 0));
        currCrystal.AddComponent<AccelTowards>().SetSpeed(Speed, Acceleration, airDirection); // can set speed here
        StartCoroutine(FallDown(currCrystal, targetedEnemy, dirTowardsPlayer));
    }
    private IEnumerator FallDown(GameObject currCrystal, GameObject targetedEnemy, Vector3 dirTowardsPlayer)
    {
        yield return new WaitForSeconds(1);
        Vector3 dir = Vector3.down;
        if (targetedEnemy != null)
        {
            dir = ((targetedEnemy.transform.position + (currCrystal.transform.position - targetedEnemy.transform.position) / 2) - currCrystal.transform.position).normalized; // get the direction from the crystal with projected enemy path
        }
        currCrystal.GetComponent<AccelTowards>().SetSpeed(Speed*8, Acceleration*24, dir);
        StartCoroutine(Explode(currCrystal, targetedEnemy, dirTowardsPlayer));
    }
    private IEnumerator Explode(GameObject currCrystal, GameObject targetedEnemy, Vector3 dirTowardsPlayer)
    {
        yield return new WaitForSeconds(.7f);
        currCrystal.GetComponent<CircleCollider2D>().enabled = true;
        currCrystal.GetComponent<AccelTowards>().enabled = false;
        currCrystal.GetComponent<SpriteRenderer>().enabled = false;
        Destroy(currCrystal);
        currCrystal = Instantiate(_crystalExplosionPrefab, currCrystal.transform.position, Quaternion.identity);
        currCrystal.AddComponent<OnHit>().SetMaxPenetrationAmount(_maxEnemyHits);
        currCrystal.GetComponent<OnHit>().ParentCrystal = gameObject.GetComponent<Crystal>();
        StartCoroutine(Kill(currCrystal));
    }
    private IEnumerator Kill(GameObject currCrystal)
    {
        yield return new WaitForSeconds(1);
        Destroy(currCrystal);

    }

    private IEnumerator Recharge()
    {
        yield return new WaitForSeconds(_cooldown);
        _charged = true;
    }

    
    public void UpdateBuffs(float[] buffs)
    {
        _damage += buffs[0];
        _cooldown += buffs[1];
        _range += buffs[2];
    }

    public void OnHit(GameObject enemyHit, GameObject weapon)
    {
        // damage the enemy
        if (enemyHit.GetComponent<Enemy>().DecrementHealth(Damage)) // enemy's health is less than or equal to 0
        {
            List<Ability_Healing> connectedHealingCrystals = GetComponent<ClusterSlot>().ConnectedHealingCrystals();
            foreach(Ability_Healing healingCrystal in connectedHealingCrystals)
            {
                healingCrystal.HealPlayer(enemyHit.transform.position, enemyHit.GetComponent<Enemy>().MaxHealth);
            }
        }
        Crystal crystalScript = gameObject.GetComponent<Crystal>();     // get reference to the crystal script on the game object
        EffectApplication.instance.BaseEffectApplications               // get singleton for "static" BaseEffectAPplication function 
            (
            crystalScript.EffectList,                                   // input the effect list NOTE: initialized in framework
            crystalScript,                                              // get crystal script for variable reference
            enemyHit,                                                   // get enemy hit for effect application
            weapon
            );

    }

    public void Enable(bool enable)
    {
        this.enabled = enable;
    }

    // variables needed for arrow are
    // damage, speed, cooldown, range, maxEnemyHits
    public void SetVariables(List<VariableType> variables)
    {
        _damage = VariableFunctions.FindFloat("damage", variables);
        _speed = VariableFunctions.FindFloat("speed", variables);
        _accel = VariableFunctions.FindFloat("acceleration", variables);
        _cooldown = VariableFunctions.FindFloat("cooldown", variables);
        _range = VariableFunctions.FindFloat("range", variables);
        _maxEnemyHits = (int) VariableFunctions.FindFloat("maxEnemyHits", variables);
    }

    public void SetPrefabs(List<PrefabType> prefabs) 
    {
        _crystalLauncherPrefab = VariableFunctions.FindPrefab("crystalLauncherPrefab", prefabs);
        _crystalExplosionPrefab = VariableFunctions.FindPrefab("crystalExplosionPrefab", prefabs);

    }

    public void RemoveScript()
    {
        DestroyImmediate(this);
    }
}
