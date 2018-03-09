﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrandWizard : MonoBehaviour
{
    private float speed = 5f;
    private float movementTimer;
    private float movementCd = 1f;

    [SerializeField] private float leftEdge;
    [SerializeField] private float rightEdge;
    [SerializeField] private float bottomEdge;
    [SerializeField] private float topEdge;

    private Animator animator;

    private Vector2 movementVector;

    //Basic attack particles
    public GameObject attackParticlePrefab;

    //Unique water attack
    public GameObject uniqueWaterAttackMarkPrefab;
    public GameObject uniqueWaterAttackParticlePrefab;

    //Unique grand wizard attack
    public GameObject uniqueGrandAttackMarkPrefab;
    public GameObject uniqueGrandAttackParticlePrefab;

    private bool isAttacking = false;
    private float attackTimer;
    private float attackCd = 1f;
    private int basicAttacksCount = 0;

    private const string FIRE_ATTACK = "fire";
    private const string WATER_ATTACK = "water";
    private string uniqueAttackTurn = FIRE_ATTACK;

    private float uniqueFireAttackTimer;
    private float uniqueFireAttackCd = 0.3f;
    private int uniqueFireAttackCount = 0;

    private float uniqueWaterAttackTimer;
    private float uniqueWaterAttackCd = 0.75f;
    private int uniqueWaterAttackCount = 0;

    private float uniqueGrandAttackTimer;
    private float uniqueGrandAttackCd = 0.75f;
    private int uniqueGrandAttackCount = 0;
    private Vector2 uniqueAttackMarkPrint;

    private List<KeyValuePair<GameObject, Vector2>> attackParticlesList;
    private float attackAngle;

    void Start()
    {
        movementTimer = movementCd;
        movementVector = DefineMovementVector();

        attackTimer = attackCd;

        attackParticlesList = new List<KeyValuePair<GameObject, Vector2>>();

        animator = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        //Deletes every destroyed particle from the list
        attackParticlesList = attackParticlesList.Where(kvp => kvp.Key != null).ToList();

        if (attackParticlesList.Count > 0)
        {
            foreach (KeyValuePair<GameObject, Vector2> kvp in attackParticlesList)
                kvp.Key.transform.Translate(kvp.Value * Time.deltaTime * 5f);
        }

        if (!isAttacking && attackTimer > 0)
        {
            if (animator.GetBool("Attack"))
                animator.SetBool("Attack", false);

            if (animator.GetBool("RayAttack"))
                animator.SetBool("RayAttack", false);

            Movements();
            attackTimer -= Time.deltaTime;
        }
        else
        {
            if (!isAttacking && basicAttacksCount < 3)
            {
                BasicAttack();
                attackTimer = attackCd;
            }
            else
            {
                basicAttacksCount = 0;

                if (!isAttacking)
                    isAttacking = true;

                if (uniqueAttackTurn == FIRE_ATTACK)
                {
                    if (uniqueFireAttackTimer > 0)
                        uniqueFireAttackTimer -= Time.deltaTime;
                    else
                    {
                        UniqueFireAttack();
                        uniqueFireAttackTimer = uniqueFireAttackCd;
                    }

                    if (uniqueFireAttackCount == 3)
                    {
                        uniqueFireAttackCount = 0;
                        attackTimer = attackCd;
                        isAttacking = false;
                        uniqueAttackTurn = WATER_ATTACK;
                    }
                }
                else
                {
                    if (uniqueWaterAttackTimer > 0)
                        uniqueWaterAttackTimer -= Time.deltaTime;
                    else if (uniqueWaterAttackCount <= 2)
                    {
                        UniqueWaterAttack();
                        uniqueWaterAttackTimer = uniqueWaterAttackCd;
                    }
                    else
                    {
                        uniqueWaterAttackCount = 0;
                        isAttacking = false;
                        uniqueAttackTurn = FIRE_ATTACK;
                    }

                    if (uniqueWaterAttackCount == 2)
                    {
                        attackTimer = attackCd;
                        uniqueWaterAttackTimer = uniqueWaterAttackCd;
                        uniqueWaterAttackCount++;
                    }
                }
            }
        }

        //Grand master attack
        if (uniqueGrandAttackTimer > 0)
            uniqueGrandAttackTimer -= Time.deltaTime;
        else
            UniqueGrandAttack();
    }

    public void BasicAttack()
    {
        animator.SetBool("Attack", true);

        basicAttacksCount++;

        //The basic attack particle direction is defined by [player's position - enemy wizard position]. The particle's speed is defined in the Update function
        Vector2 particleVector = GameObject.FindGameObjectWithTag("Player").transform.position - transform.position;
        particleVector.x /= 3;
        particleVector.y /= 3;

        attackParticlesList.Add(CreateAttackParticle(2f, particleVector));
    }

    public void UniqueGrandAttack()
    {
        if (uniqueGrandAttackCount == 0)
        {
            uniqueAttackMarkPrint = new Vector2(Random.Range(-5.9f, 5.9f), Random.Range(-0.75f, 0.3f));
            GameObject attackMark = Instantiate(uniqueGrandAttackMarkPrefab, uniqueAttackMarkPrint, new Quaternion());
            Destroy(attackMark, 0.75f);

            uniqueGrandAttackTimer = uniqueGrandAttackCd;
            uniqueGrandAttackCount++;
        }
        else
        {
            GameObject attackParticle = Instantiate(uniqueGrandAttackParticlePrefab, uniqueAttackMarkPrint, new Quaternion());
            Destroy(attackParticle, 2f);

            uniqueGrandAttackTimer = 0;
            uniqueGrandAttackCount = 0;
        }
    }

    public void UniqueFireAttack()
    {
        animator.SetBool("Attack", true);

        uniqueFireAttackCount++;

        for (float f = -0.6f; f <= 0.6f; f += 0.2f)
        {
            Vector2 particleVector = new Vector2(f, -0.5f);

            attackParticlesList.Add(CreateAttackParticle(4f, particleVector));
        }
    }

    public void UniqueWaterAttack()
    {
        animator.SetBool("RayAttack", true);

        uniqueWaterAttackCount++;

        if (uniqueWaterAttackCount == 1)
        {
            GameObject attackParticle = Instantiate(uniqueWaterAttackMarkPrefab, new Vector2(transform.position.x, transform.position.y - 0.15f), new Quaternion());

            Vector3 playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;

            float adjacent = Mathf.Abs(transform.position.y - playerPosition.y);
            float opposite = Mathf.Abs(transform.position.x - playerPosition.x);
            float hypothenuse = Mathf.Sqrt((adjacent * adjacent) + (opposite * opposite));

            attackAngle = (opposite / hypothenuse) * (180f / Mathf.PI);

            if (transform.position.x > playerPosition.x)
                attackAngle = -attackAngle;

            attackParticle.transform.Rotate(new Vector3(0, 0, attackAngle));

            Destroy(attackParticle, 0.75f);
        }
        else if (uniqueWaterAttackCount == 2)
        {
            GameObject attackParticle = Instantiate(uniqueWaterAttackParticlePrefab, new Vector2(transform.position.x, transform.position.y - 0.15f), new Quaternion());

            attackParticle.transform.Rotate(new Vector3(0, 0, attackAngle));

            Destroy(attackParticle, 0.75f);
        }
    }

    public KeyValuePair<GameObject, Vector2> CreateAttackParticle(float lifeSpan, Vector2 particleVector)
    {
        GameObject attackParticle = Instantiate(attackParticlePrefab, new Vector2(transform.position.x, transform.position.y), new Quaternion());
        Destroy(attackParticle, lifeSpan);

        return new KeyValuePair<GameObject, Vector2>(attackParticle, particleVector);
    }

    public void Movements()
    {
        if (movementTimer > 0)
        {
            animator.SetFloat("HorizontalSpeed", movementVector.x);
            animator.SetFloat("VerticalSpeed", movementVector.y);

            movementTimer -= Time.deltaTime;
            transform.Translate(movementVector * Time.deltaTime * speed);

            //Blocks movement at the edge of the screen
            if (transform.position.x < leftEdge)
            {
                transform.position = new Vector2(leftEdge, transform.position.y);
                movementVector.x = -movementVector.x;
            }
            else if (transform.position.x > rightEdge)
            {
                transform.position = new Vector2(rightEdge, transform.position.y);
                movementVector.x = -movementVector.x;
            }

            if (transform.position.y < bottomEdge)
            {
                transform.position = new Vector2(transform.position.x, bottomEdge);
                movementVector.y = -movementVector.y;
            }
            else if (transform.position.y > topEdge)
            {
                transform.position = new Vector2(transform.position.x, topEdge);
                movementVector.y = -movementVector.y;
            }
        }
        else
        {
            movementTimer = movementCd;
            movementVector = DefineMovementVector();
        }
    }

    public Vector2 DefineMovementVector()
    {
        return new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.3f, 0.3f));
    }
}