﻿using UnityEngine;

public class OnEnemyRayAttackCollision : MonoBehaviour
{
    private const int PLAYER_DAMAGE = 4;

    private float attackTimer = 0.01f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
            collision.gameObject.GetComponent<Player>().PlayerHurt(PLAYER_DAMAGE); // Take off health
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (attackTimer > 0)
                attackTimer -= Time.fixedDeltaTime;
            else
            {
                attackTimer = 0.01f;
                collision.gameObject.GetComponent<Player>().PlayerHurt(PLAYER_DAMAGE);
            }
        }
    }
}