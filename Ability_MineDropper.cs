using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_MineDropper : MonoBehaviour, IAbility
{
    private float _damage = 1f;
    private float _cooldown = 5f;
    private float _range = 10f;

    private int _maxEnemyHits = 3;

    private int _currEnemyHits = 0;
    
    private float _speed = 12f;

    private bool _charged = true;

    private float _poisonTime;
    private float _burnTime;
    private float _stunTime;

    //private FMOD.Studio.EventInstance minedropperFlySoundInstance;

    public float PoisonTime { get => _poisonTime; set => _poisonTime = value; }
    public float BurnTime { get => _burnTime; set => _burnTime = value; }
    public float StunTime { get => _stunTime; set => _stunTime = value; }
    public float Damage { get => _damage; set => _damage = value; }
    public float Speed { get => _speed; set => _speed = value; }
    public float Cooldown { get => _cooldown; set => _cooldown = value; }
    public float Range { get => _range; set => _range = value; }
    GameObject _minePrefab;

    // Update is called once per frame
    void Update()
    {
        if (_charged)
        {
            GameObject closestEnemy = GameManager.instance.GetNearestEnemy(transform.position, _range);
            SpawnMineDropper();
        }

    }
    public void SpawnMineDropper()
    {
        _charged = false;


        StartCoroutine(nameof(Recharge));

        GameObject currMine = Instantiate(_minePrefab, transform.position,
            Quaternion.identity);
        currMine.AddComponent<OnHit>().SetMaxPenetrationAmount(_maxEnemyHits);
        currMine.GetComponent<OnHit>().ParentCrystal = gameObject.GetComponent<Crystal>();
        StartCoroutine(TimerDetonate(currMine));
    }

    private IEnumerator TimerDetonate(GameObject currMine)
    {
        yield return new WaitForSeconds(5);
        StartCoroutine(Explode(currMine));
    }

    private IEnumerator Recharge()
    {
        yield return new WaitForSeconds(_cooldown);
        _charged = true;
    }

    public IEnumerator Explode(GameObject currMine)
    {
        if (currMine != null)
        {
            currMine.GetComponent<Animator>().enabled = true;
            currMine.GetComponent<CircleCollider2D>().radius = 9f;
            yield return new WaitForSeconds(1);
            Destroy(currMine);
        }
    }

    
    public void UpdateBuffs(float[] buffs)
    {
        Damage += buffs[0];
        _cooldown += buffs[1];
        _range += buffs[2];
    }

    public void OnHit(GameObject enemyHit, GameObject weapon)
    {
        // damage the enemy
        StartCoroutine(Explode(weapon));

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

    public void SetVariables(List<VariableType> variables)
    {
        _damage = VariableFunctions.FindFloat("damage", variables);
        _speed = VariableFunctions.FindFloat("speed", variables);
        _cooldown = VariableFunctions.FindFloat("cooldown", variables);
        _range = VariableFunctions.FindFloat("range", variables);
        _maxEnemyHits = (int)VariableFunctions.FindFloat("maxEnemyHits", variables);
    }

    public void SetPrefabs(List<PrefabType> prefabs)
    {
        _minePrefab = VariableFunctions.FindPrefab("minePrefab", prefabs);
    }

    public void Enable(bool enabled)
    {
        this.enabled = enabled;
    }

    public void RemoveScript()
    {
        DestroyImmediate(this);
    }
}
