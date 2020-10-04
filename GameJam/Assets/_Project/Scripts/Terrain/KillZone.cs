﻿namespace Terrain
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using GameBase;

    public class KillZone : MonoBehaviour
    {
        public int gameOverIndex;
        public float moveSpeed;
        public float timeToStrike;
        public float timeToDeploy;

        public Transform moveTowardsPos;
        public Transform setTrapPos;
        public BoxCollider2D attackBox;
        public Rigidbody2D trapRB;
        public Animator trapAnimation;

        private bool hasBeenSetOff = false;
        private float currTime;
        private Vector2 origPos;

        [SerializeField]
        private TrapStates currState;

        private void Start()
        {
            origPos = trapRB.position;
            currState = TrapStates.INACTIVE;
        }

        private void Update()
        {
            switch(currState)
            {
                case TrapStates.INACTIVE:
                    // Out of view
                    currTime += Time.deltaTime;
                    if (currTime >= timeToDeploy)
                    {
                        attackBox.enabled = false;
                        trapAnimation.SetBool("isAttacking", false);
                        currState = TrapStates.DEPLOYING;
                        currTime = 0f;
                    }
                    break;
                case TrapStates.DEPLOYING:
                    // Moving into pos
                    MoveToPosition(setTrapPos.position, TrapStates.WAITING);
                    break;
                case TrapStates.WAITING:
                    // Preparing to attack
                    currTime += Time.deltaTime;
                    if(currTime >= timeToStrike)
                    {
                        attackBox.enabled = true;
                        trapAnimation.SetBool("isAttacking", true);
                        currState = TrapStates.ATTACK;
                        currTime = 0f;
                    }
                    break;
                case TrapStates.ATTACK:
                    if(trapAnimation.GetCurrentAnimatorStateInfo(0).IsName("DONE"))
                    {
                        currState = TrapStates.RETREAT;
                    }
                    break;
                case TrapStates.RETREAT:
                    MoveToPosition(origPos, TrapStates.INACTIVE);
                    break;
            }
        }

        private void MoveToPosition(Vector2 destination, TrapStates stateToEndIn)
        {
            if (Mathf.Abs(Vector2.Distance(destination, trapRB.position)) > 0.1f)
            {
                Vector2 newVelocity = (destination- trapRB.position) * Time.deltaTime;
                trapRB.velocity = newVelocity.normalized * moveSpeed;
            }
            else
            {
                trapRB.velocity = Vector2.zero;
                trapRB.angularVelocity = 0f;
                currState = stateToEndIn;
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (hasBeenSetOff == false & collision.CompareTag("Player"))
            {
                hasBeenSetOff = true;
                collision.GetComponentInChildren<Animator>().SetBool("hasLost", true);
                StartCoroutine(StartEvent(collision.GetComponent<Rigidbody2D>(), collision.GetComponent<BoxCollider2D>()));
            }
        }

        private IEnumerator StartEvent(Rigidbody2D player, BoxCollider2D playerHitbox)
        {
            yield return new WaitForFixedUpdate();

            playerHitbox.enabled = false;
            player.velocity = Vector2.zero;
            player.angularVelocity = 0f;

            while(currState != TrapStates.INACTIVE)
            {
                player.position = Vector2.MoveTowards(player.position, moveTowardsPos.position, moveSpeed * Time.deltaTime);
                yield return new WaitForFixedUpdate();
            }

            // WIP
            GameManager.Instance.ResetSavedData();
            SceneManager.LoadScene(gameOverIndex);
        }
    
    }

}