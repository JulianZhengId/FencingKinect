using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public Animator animator;
    public BoxCollider2D playerCollider;

    public GameObject bulletPrefab;
    private int ammo = 6;
    private float reloadTime = 3f;
    private float shootCooldown = 1.5f;
    private int maxAmmo = 6;

    private bool isReloading = false;
    private bool canShoot = true;
    private bool canDuck = true;

    private SpriteRenderer playerRenderer;
    
    //score
    public GameObject textObject;
    private TextMeshProUGUI ammoText;
    
    public Transform parentTransform;
    public Transform handTip;
    public bool isPlayer1;

    private bool isDucking = false;
    public Transform shootPoint;

    public GameObject shield;

    private void Start()
    {
        ammoText = textObject.GetComponent<TextMeshProUGUI>();
        playerRenderer = GetComponent<SpriteRenderer>();
        updateScore(ammo);
    }

    private void Update()
    { 
        if (ammo == 0 && !isReloading)
        {
            StartCoroutine(Reload());
        }
        
        // Duck
        if (Input.GetMouseButtonDown(1))
        {
            playerCollider.enabled = false;
            playerRenderer.color = Color.red;
        }

        if (Input.GetMouseButtonUp(1))
        {
            playerCollider.enabled = true;
            playerRenderer.color = Color.green;
        }
    }

    IEnumerator FedeIn(float aTime)
    {
        Color oldColor = shield.transform.GetComponent<SpriteRenderer>().color;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            Color newColor = new Color(oldColor.r, oldColor.b, oldColor.g, Mathf.Lerp(0, 1, t));
            shield.transform.GetComponent<SpriteRenderer>().color = newColor;
            yield return null;
        }
    }
    IEnumerator FedeOut(float aTime)
    {
        Color oldColor = shield.transform.GetComponent<SpriteRenderer>().color;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            Color newColor = new Color(oldColor.r, oldColor.b, oldColor.g, Mathf.Lerp(1, 0, t));
            shield.transform.GetComponent<SpriteRenderer>().color = newColor;
            yield return null;
        }
    }

    public void RotateArm(float angle) 
    {
        if (isPlayer1)
        {
            parentTransform.rotation = Quaternion.Slerp(parentTransform.rotation, Quaternion.Euler(0.0f, 0.0f, angle), Time.deltaTime * 5f);
        }
        else
        {
            parentTransform.rotation = Quaternion.Slerp(parentTransform.rotation, Quaternion.Euler(0.0f, 0.0f, angle - 180), Time.deltaTime * 5f);
        }
    } 

    public void Duck()
    {
        if (!canDuck) return;
        canDuck = false;
        isDucking = true;
        shield.GetComponent<CircleCollider2D>().enabled = true;
        StartCoroutine(FedeIn(0.25f));
        Debug.Log("Duck: " + this.name);
        playerCollider.enabled = false;
        canShoot = false;
        animator.SetBool("Duck", true);
        //playerRenderer.color = Color.red;
    }

    public void Unduck()
    {
        if (!canDuck)
        {
            canShoot = true;
        }
        animator.SetBool("Duck", false);
        canDuck = true;
        shield.GetComponent<CircleCollider2D>().enabled = false;
        StartCoroutine(FedeOut(0.25f));
        isDucking = false;
        playerCollider.enabled = true;
    }

    private void updateScore(int score)
    {
        if (ammoText != null)
        {
            Debug.Log("changed text");
            ammoText.text = "Ammo: " + score;
        }
    }
    public IEnumerator Shoot(float angle)
    {
        if (ammo > 0 && !isReloading && canShoot && !isDucking)
        {
            Debug.Log("Shoot by " + this.name);
            AudioManager.playGunfire();
            canShoot = false;
            ammo--;
            updateScore(ammo);

            var bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.Euler(0, 0, -90 + angle));
            Vector3 direction = handTip.position - parentTransform.position + new Vector3(0, 0.25f, 0);
            bullet.GetComponent<BulletController>().bulletDirection = new Vector3(direction.x, direction.y, 0f);
            yield return new WaitForSeconds(shootCooldown);
            canShoot = true;
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        ammoText.text = "Reloading...";
        yield return new WaitForSeconds(reloadTime);
        ammo = maxAmmo;
        isReloading = false;
        updateScore(ammo);
    }
}
