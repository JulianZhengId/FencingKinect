using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] playerSprites;
    [SerializeField] Image[] heartContainers;
    [SerializeField] private int currentHealth;
    private int maxHealth;
    
    private bool isInvincible;
    private float invincibleTimer;
    private float invincibleDuration = 2f;

    private bool blinking = false;

    private bool isPlayer1;

    [SerializeField] private TextMeshProUGUI statusText;

    public int PlayerHealth
    {
        get { return currentHealth; }
        set
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);
            SetHealth();
            if (currentHealth == 0)
            {
                if (isPlayer1)
                {
                    Debug.Log("Right Player Winner");
                    GameManager.instance.winner = "Right Player";
                }
                Debug.Log("Left Player Winner");
                SceneManager.LoadScene("End Game");
            }
        }
    }

    public bool IsInvincible
    {
        get { return isInvincible; }
        set
        {
            isInvincible = value;
        }
    }

    private void Start()
    {
        maxHealth = heartContainers.Length;
        PlayerHealth = maxHealth;
    }

    private void Update()
    {
        if (!isInvincible) return;

        invincibleTimer += Time.deltaTime;
        blinking = !blinking;
        Debug.Log("Test");
        SetSpriteRenderer(blinking);
        if (invincibleTimer >= invincibleDuration)
        {
            SetSpriteRenderer(true);
            isInvincible = false;
            invincibleTimer = 0f;
        }
    }

    private void SetSpriteRenderer(bool b)
    {
        foreach (var sr in playerSprites)
        {
            sr.enabled = b;
        }
    }

    private void SetHealth()
    {
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        for (int i = 0; i < maxHealth; i++)
        {
            if (i < currentHealth)
            {
                heartContainers[i].enabled = true;
            }
            else
            {
                heartContainers[i].enabled = false;
            }
        }
    }

    public void TakeDamage()
    {
        if (isInvincible) return;
        isInvincible = true;
        PlayerHealth -= 1;

        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (sceneIndex == 1)
        {
            AudioManager.RandomHitSound();
        }
        else if(sceneIndex == 2)
        {
            AudioManager.playGunHit();
        }

        if (statusText)
        {
            StartCoroutine(SetInvincibleStatusText());
        }
    }

    IEnumerator SetInvincibleStatusText()
    {
        statusText.text = "Invincible";
        yield return new WaitForSeconds(2f);
        statusText.text = "";
    }
}
