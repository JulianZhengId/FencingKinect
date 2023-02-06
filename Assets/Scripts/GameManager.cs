using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private bool isPaused = false;
    [SerializeField] private Player player1Controller;
    [SerializeField] private Player player2Controller;

    [SerializeField]  private BoxCollider2D swordP1Collider;
    [SerializeField]  private BoxCollider2D swordP2Collider;

    [SerializeField]  private BoxCollider2D bodyP1Collider;
    [SerializeField]  private BoxCollider2D bodyP2Collider;

    public int clashCount = 0;
    public string winner = "Left Player";

    public SphereController rightHandObject;
    public GameObject pausePanel;

    public int ClashCount
    {
        get { return clashCount; }
        set
        {
            clashCount = value;
            if (clashCount >= 6)
            {
                clashCount = 0;
                player1Controller.MoveLeft();
                player2Controller.MoveRight();
            }
        }
    }

    private void Awake()
    {
        //Singleton
        if (GameManager.instance == null)
        {
            GameManager.instance = this;
        }
        else if (GameManager.instance != this)
        {
            Destroy(GameManager.instance.gameObject);
            GameManager.instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter) && isPaused)
        {
            Continue();
        }
    }
   
    public void Pause()
    {
        if (!isPaused)
        {
            Debug.Log("Pause");
            isPaused = true;
            Time.timeScale = 0;
            pausePanel.SetActive(true);
            rightHandObject.gameObject.SetActive(true);
        }
    }

    public void Continue()
    {
        Debug.Log("Continue");
        isPaused = false;
        Time.timeScale = 1;
        rightHandObject.gameObject.SetActive(false);
        pausePanel.SetActive(false);
    }

    public Player GetPlayer1Controller()
    {
        return player1Controller;
    }

    public Player GetPlayer2Controller()
    {
        return player2Controller;
    }

    private float GetDistance()
    {
        return Vector3.Distance(player1Controller.transform.position, player2Controller.transform.position);
    }

    public bool canMove()
    {
        return GetDistance() >= 2.5f;
    }

    public void AttackP1HandleCollider()
    {
        swordP1Collider.enabled = true;
        bodyP1Collider.enabled = false;
    }

    public void AttackP2HandleCollider()
    {
        swordP2Collider.enabled = true;
        bodyP2Collider.enabled = false;
    }

    public void IdleOrMoveP1HandleCollider() 
    {
        swordP1Collider.enabled = false;
        bodyP1Collider.enabled = true;
    }

    public void IdleOrMoveP2HandleCollider()
    {
        swordP2Collider.enabled = false;
        bodyP2Collider.enabled = true;
    }

    public bool GetIsPaused()
    {
        return isPaused;
    }
}
