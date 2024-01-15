using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Singleton

    [SerializeField] Player[] players; // Array containing the players
    [SerializeField] Boss boss; // Boss

    [SerializeField] List<GameObject> hideOnGameOver; // List with all objects to hide on game over sequence
    [SerializeField] GameObject gameOverUI; // Game over UI
    public bool gameOver { get; private set; } = false; // True when the game is over
    float gameOverTime; // Start time for game over sequence
    GameObject gameOverChar; // Player that died
    GameObject gameOverProjectile;
    Transform gameOverProjectileParent;

    public static PlayerKeys player0Keys = new PlayerKeys(KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H);
    public static PlayerKeys player1Keys = new PlayerKeys(KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3);

    private void Awake()
    {
        // Create singleton
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;

        // Set the players' ids
        players[0].playerId = 0;
        players[1].playerId = 1;
    }

    private void Update()
    {
        // Game Over Sequence
        if (gameOver)
        {
            if (Time.time >= gameOverTime + 2f && gameOverChar.activeInHierarchy)
            {
                // Explode
                Instantiate(Resources.Load("Explosion"), gameOverChar.transform.position, Quaternion.identity);

                // Reset projectile
                Animator projectileAnimator = gameOverProjectile.GetComponentInChildren<Animator>();
                if (projectileAnimator != null) projectileAnimator.speed = 1f;
                gameOverProjectile.GetComponent<Projectile>().Deactivate();

                // Hide character and projectile
                gameOverChar.SetActive(false);
                gameOverProjectile.transform.parent = gameOverProjectileParent;
            }
            else if (Time.time >= gameOverTime + 4f)
            {
                gameOverUI.SetActive(true);
                if (Input.anyKeyDown) // Reset after game over
                {
                    gameOverUI.SetActive(false); // Hide game over UI
                    foreach (GameObject go in hideOnGameOver)
                    {
                        if (go.GetComponent<Character>() != null)
                        {
                            go.GetComponent<Character>().Reset(); // Reset characters back to normal
                        }
                        else if (go.GetComponent<ProjectilePoolController>() != null)
                        {
                            go.GetComponent<ProjectilePoolController>().DeactivateAll();
                        }
                        go.SetActive(true);
                    }

                    gameOverTime = 0f;
                    gameOver = false;
                }
            }

            // if (Input.anyKeyDown) gameOverTime -= 2f;
        }
    }

    public void GameOver(GameObject character, GameObject projectile, Transform projectileParent)
    {
        // Unparent projectile from projectile pool
        projectile.transform.parent = null;

        // Pause projectile
        Animator projectileAnimator = projectile.GetComponentInChildren<Animator>();
        if (projectileAnimator != null) projectileAnimator.speed = 0f;
        projectile.GetComponent<Projectile>().isActive = false;

        // Hide all gameobjects
        foreach (GameObject go in hideOnGameOver)
        {
            if (go == character) continue; // Ignore player that was killed
            go.SetActive(false);
        }

        // Start game over sequence
        gameOver = true;
        gameOverTime = Time.time;
        gameOverChar = character;
        gameOverProjectile = projectile;
        gameOverProjectileParent = projectileParent;
    }

    // Returns a specific player
    public Player GetPlayer(int playerId) { return players[playerId]; } // what could go wrong
    public Wizard GetWizard() { return players[0].GetComponent<Wizard>(); }
    public Warrior GetWarrior() { return players[1].GetComponent<Warrior>(); }
    public Player GetOtherPlayer(int playerId) { return playerId == 0 ? players[1] : players[0]; } // Returns player based off opposite of the given id
    public Boss GetBoss() { return boss; }
}

public struct PlayerKeys
{
    public PlayerKeys(KeyCode a_up, KeyCode a_down, KeyCode a_left, KeyCode a_right, KeyCode a_light, KeyCode a_heavy, KeyCode a_exchange)
    {
        up = a_up;
        down = a_down;
        left = a_left;
        right = a_right;
        light = a_light;
        heavy = a_heavy;
        exchange = a_exchange;
    }

    public KeyCode up;
    public KeyCode down;
    public KeyCode left;
    public KeyCode right;
    public KeyCode light;
    public KeyCode heavy;
    public KeyCode exchange;
}
