using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_BlackHole : MonoBehaviour, IAbility
{
    [SerializeField] private float _damage;
    [SerializeField] private float _cooldown;
    [SerializeField] private float _range;
    [SerializeField] private int _maxEnemyHits;
    [SerializeField] private int _currEnemyHits;
    [SerializeField] private float _speed;
    private float _activationTime;
    private float _duration;

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
    public float Cooldown { get => _cooldown; set => _cooldown = value; }
    public float Range { get => _range; set => _range = value; }
    public float Damage1 { get => _damage; set => _damage = value; }
    GameObject _blackHolePrefab;

    // Update is called once per frame
    void Update()
    {
        if (_charged)
        {
            GameObject closestEnemy = GameManager.instance.GetNearestEnemy(transform.position, _range);
            if (closestEnemy != null) { SpawnBlackHole(closestEnemy); }
        }
    }

    public void SpawnBlackHole(GameObject targetedEnemy)
    {
        _charged = false;

        StartCoroutine(nameof(Recharge));
        Vector3 dirTowardsPlayer = (transform.position - targetedEnemy.transform.position); // get the direction the enemy is moving
        Vector3 dir = ((targetedEnemy.transform.position + dirTowardsPlayer /2) - transform.position).normalized; // get the direction from the crystal with projected enemy path

        //dir = new Vector3(dir.y, dir.x * -1, dir.z) * -1;

        GameObject currBlackHole = Instantiate(_blackHolePrefab, transform.position,
            Quaternion.identity);
        currBlackHole.AddComponent<OnHit>().SetMaxPenetrationAmount(_maxEnemyHits);
        currBlackHole.GetComponent<OnHit>().ParentCrystal = gameObject.GetComponent<Crystal>();
        currBlackHole.AddComponent<MoveTowards>().SetSpeed(Speed, dir); // can set speed here
        StartCoroutine(Activate(currBlackHole));
        StartCoroutine(Kill(currBlackHole));
    }

    private IEnumerator Activate(GameObject currBlackHole)
    {
        yield return new WaitForSeconds(_activationTime);
        currBlackHole.GetComponent<CircleCollider2D>().enabled = true;
        currBlackHole.GetComponent<MoveTowards>().enabled = false;
    }
    private IEnumerator Kill(GameObject currBlackHole)
    {
        yield return new WaitForSeconds(_duration);
        Destroy(currBlackHole);

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
        _cooldown = VariableFunctions.FindFloat("cooldown", variables);
        _range = VariableFunctions.FindFloat("range", variables);
        _maxEnemyHits = (int) VariableFunctions.FindFloat("maxEnemyHits", variables);
        _duration = VariableFunctions.FindFloat("duration", variables);
        _activationTime = VariableFunctions.FindFloat("activationTime", variables);
    }

    public void SetPrefabs(List<PrefabType> prefabs) 
    { 
         _blackHolePrefab = VariableFunctions.FindPrefab("blackHolePrefab", prefabs);
    }

    public void RemoveScript()
    {
        DestroyImmediate(this);
    }
}
