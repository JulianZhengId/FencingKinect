using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private Animator animator;
    private bool isPerforming = false;

    //movement attributes
    [SerializeField] private float moveSpeed = 0f;
    private int moveCharges = 5;
    private int maxCharges;
    public Slider movementCooldownSlider;
    private float moveTimer;
    private float timeToRefillCharge = 2f;
    [SerializeField] TextMeshProUGUI numberText;

    //attack attributes
    private float attackTimer;
    [SerializeField] private float attackCooldown = 0.5f;
    public Slider attackCooldownSlider;

    //defense attributes
    private float defenseTimer;
    [SerializeField] private float defenseCooldown = 1f;
    public Slider defenseCooldownSlider;

    //player
    public bool isPlayer1;

    //defending
    private bool isDefending = false;

    //stunned
    private bool isStunned = false;
    private float stunTimer = 0f;
    private float timeToRecover = 1.5f;

    //status
    [SerializeField] private TextMeshProUGUI statusText;

    private void Start()
    {
        animator = GetComponent<Animator>();
        moveTimer = 0f;
        attackTimer = attackCooldown;
        defenseTimer = defenseCooldown;
        maxCharges = moveCharges;
    }

    void Update()
    {        
        //Refill Charge
        if (moveCharges < maxCharges)
        {
            moveTimer += Time.deltaTime;
            movementCooldownSlider.value = moveTimer / timeToRefillCharge;
            if (moveTimer > timeToRefillCharge)
            {
                moveCharges += 1;
                numberText.text = moveCharges.ToString();
                if (moveCharges <= maxCharges)
                {
                    moveTimer = 0f;
                }
            }
        }

        if (isStunned)
        {
            stunTimer += Time.deltaTime;
            if (stunTimer >= timeToRecover)
            {
                isStunned = false;
            }
        }

        if (attackTimer < attackCooldown)
        {
            attackTimer += Time.deltaTime;
            attackCooldownSlider.value = attackTimer / attackCooldown;
        }

        if (defenseTimer < defenseCooldown)
        {
            defenseTimer += Time.deltaTime;
            defenseCooldownSlider.value = defenseTimer / defenseCooldown;
        }

        transform.position += new Vector3(moveSpeed, 0, 0) * Time.deltaTime;
        if (isPerforming || isStunned) return;

        if (isPlayer1)
        {
            if (Input.GetKeyDown(KeyCode.S) && moveCharges > 0)
            {
                MoveToLeft();
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                MoveToRight();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                Attack();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.K) && moveCharges > 0)
            {
                MoveToLeft();
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                MoveToRight();
            }
            else if (Input.GetKeyDown(KeyCode.O))
            {
                Attack();
            }
        }
    }

    public void MoveToRight()
    {
        if (isPerforming || moveCharges <= 0) return;
        moveCharges -= 1;
        numberText.text = moveCharges.ToString();
        MoveRight();
    }

    public void MoveRight()
    {
        if (isPlayer1)
        {
            if (!GameManager.instance.canMove()) return;
            animator.SetTrigger("MoveRight");
        }
        else
        {
            animator.SetTrigger("MoveLeft");
        }
        isPerforming = true;
    }

    public void MoveToLeft()
    {
        if (isPerforming || moveCharges <= 0 || !GameManager.instance.canMove()) return;
        moveCharges -= 1;
        numberText.text = moveCharges.ToString();
        MoveLeft();
    }

    public void MoveLeft()
    {
        if (isPlayer1)
        {
            animator.SetTrigger("MoveLeft");
        }
        else
        {
            if (!GameManager.instance.canMove()) return;
            animator.SetTrigger("MoveRight");
        }
        isPerforming = true;
    }

    public void Attack()
    {
        if (isPerforming || attackTimer < attackCooldown) return;
        AudioManager.playSwingSound();
        attackTimer = 0f;
        isPerforming = true;
        animator.SetTrigger("Attack");
        if (isPlayer1)
        {
            GameManager.instance.AttackP1HandleCollider();
        }
        else
        {
            GameManager.instance.AttackP2HandleCollider();
        }
    }

    public void PauseGame()
    {
        GameManager.instance.Pause();
    }

    public void Defense()
    {
        if (isPerforming || attackTimer < attackCooldown) return;
        defenseTimer = 0f;
        isPerforming = true;
        isDefending = true;
        animator.SetBool("Defense", true);
    }

    public void StartMovingRight()
    {
        if (isPlayer1)
        {
            moveSpeed = 5f;
        }
        else
        {
            moveSpeed = -5f;
        }
    }

    public void StartMovingLeft()
    {
        if (isPlayer1)
        {
            moveSpeed = -5f;
        }
        else
        {
            moveSpeed = 5f;
        }
        
    }

    public void StopMovingRight()
    {
        isPerforming = false;
        moveSpeed = 0f;
    }

    public void StopMovingLeft()
    {
        isPerforming = false;
        moveSpeed = 0f;
    }

    public void StopAttacking()
    {
        isPerforming = false;
        if (isPlayer1)
        {
            GameManager.instance.IdleOrMoveP1HandleCollider();
        }
        else
        {
            GameManager.instance.IdleOrMoveP2HandleCollider();
        }
    }

    public void StopDefending()
    {
        isPerforming = false;
        isDefending = false;
        animator.SetBool("Defense", false);
    }

    public bool GetIsDefending()
    {
        return isDefending;
    }

    public void Stunned()
    {
        Debug.Log("Stunned: " + this.transform.name);
        StartCoroutine(SetStunnedStatusText());
        isStunned = true;
        if (isPlayer1)
        {
            MoveLeft();
        }
        else
        {
            MoveRight();
        }
    }

    IEnumerator SetStunnedStatusText()
    {
        statusText.text = "Stunned";
        yield return new WaitForSeconds(1.5f);
        statusText.text = "";
    }
}
