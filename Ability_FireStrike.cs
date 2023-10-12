using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_FireStrike : MonoBehaviour, IAbility
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
    GameObject _fireStrikeVerticalPrefab;
    GameObject _fireStrikeHorizontalPrefab;

    // Update is called once per frame
    void Update()
    {
        if (_charged)
        {
            GameObject closestEnemy = GameManager.instance.GetNearestEnemy(transform.position, _range);
            SpawnFirePillars(closestEnemy);
        }
    }

    public void SpawnFirePillars(GameObject targetedEnemy)
    {
        _charged = false;

        StartCoroutine(nameof(Recharge));
        StartCoroutine(SpawnPillarUp(transform.position));
        StartCoroutine(SpawnPillarDown(transform.position));
        StartCoroutine(SpawnPillarLeft(transform.position));
        StartCoroutine(SpawnPillarRight(transform.position));

    }

    private IEnumerator SpawnPillarUp(Vector3 pos)
    {
        for (int i = 0; i < 10; i++)
        {
            pos.y += 1;
            yield return new WaitForSeconds(0.05f);
            GameObject pillar = Instantiate(_fireStrikeVerticalPrefab, pos, Quaternion.identity);
            pillar.AddComponent<OnHit>().SetMaxPenetrationAmount(_maxEnemyHits);
            pillar.GetComponent<OnHit>().ParentCrystal = gameObject.GetComponent<Crystal>();
            StartCoroutine(Kill(pillar));

        }
    }
    private IEnumerator SpawnPillarDown(Vector3 pos)
    {
        for (int i = 0; i < 10; i++)
        {
            pos.y -= 1;
            yield return new WaitForSeconds(0.05f);
            GameObject pillar = Instantiate(_fireStrikeVerticalPrefab, pos, Quaternion.identity);
            pillar.AddComponent<OnHit>().SetMaxPenetrationAmount(_maxEnemyHits);
            pillar.GetComponent<OnHit>().ParentCrystal = gameObject.GetComponent<Crystal>();
            StartCoroutine(Kill(pillar));

        }
    }
    private IEnumerator SpawnPillarLeft(Vector3 pos)
    {
        for (int i = 0; i < 5; i++)
        {
            pos.x += 2;
            yield return new WaitForSeconds(0.1f);
            GameObject pillar = Instantiate(_fireStrikeHorizontalPrefab, pos, Quaternion.identity);
            pillar.AddComponent<OnHit>().SetMaxPenetrationAmount(_maxEnemyHits);
            pillar.GetComponent<OnHit>().ParentCrystal = gameObject.GetComponent<Crystal>();
            StartCoroutine(Kill(pillar));

        }
    }
    private IEnumerator SpawnPillarRight(Vector3 pos)
    {
        for (int i = 0; i < 5; i++)
        {
            pos.x -= 2;
            yield return new WaitForSeconds(0.1f);
            GameObject pillar = Instantiate(_fireStrikeHorizontalPrefab, pos, Quaternion.identity);
            pillar.AddComponent<OnHit>().SetMaxPenetrationAmount(_maxEnemyHits);
            pillar.GetComponent<OnHit>().ParentCrystal = gameObject.GetComponent<Crystal>();
            StartCoroutine(Kill(pillar));
        }
    }

    private IEnumerator Kill(GameObject currPillar)
    {
        yield return new WaitForSeconds(_duration);
        Destroy(currPillar);

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
        if (enemyHit.GetComponent<Enemy>().DecrementHealth(Damage)) // enemy's health is less than or equal to 0
        {
            List<Ability_Healing> connectedHealingCrystals = GetComponent<ClusterSlot>().ConnectedHealingCrystals();
            foreach (Ability_Healing healingCrystal in connectedHealingCrystals)
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
        _cooldown = VariableFunctions.FindFloat("cooldown", variables);
        _range = VariableFunctions.FindFloat("range", variables);
        _maxEnemyHits = (int) VariableFunctions.FindFloat("maxEnemyHits", variables);
        _duration = VariableFunctions.FindFloat("duration", variables);
        _activationTime = VariableFunctions.FindFloat("activationTime", variables);
    }

    public void SetPrefabs(List<PrefabType> prefabs) 
    { 
         _fireStrikeVerticalPrefab = VariableFunctions.FindPrefab("fireStrikeVerticalPrefab", prefabs);
        _fireStrikeHorizontalPrefab = VariableFunctions.FindPrefab("fireStrikeHorizontalPrefab", prefabs);

    }

    public void RemoveScript()
    {
        DestroyImmediate(this);
    }
}
