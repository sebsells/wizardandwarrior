using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Singleton
    public GameState gameState { get; private set; } // Current game state

    // Gameplay objects
    [SerializeField] Player[] players; // Array containing the players
    [SerializeField] Boss boss; // Boss
    [SerializeField] AudioSource bgMusic; // Background music source

    // Statistics
    public float player0Damage { get; private set; }
    public float player1Damage { get; private set; }
    public float playerExchange { get; private set; }

    // Game over objects
    [SerializeField] List<GameObject> hideOnGameOver; // List with all objects to hide on game over sequence
    [SerializeField] GameObject gameOverUI; // Game over UI
    float gameOverTime; // Start time for game over sequence
    GameObject gameOverChar; // Player that died
    GameObject gameOverProjectile;
    Transform gameOverProjectileParent;

    // Stats screen objects
    [SerializeField] GameObject statsUI; // Stats screen game object

    public float startTime { get; private set; } // Time at which the game started
    public float endTime { get; private set; } // ^ when the game ended

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

        // Starting game state
        gameState = GameState.Intro;
        ResetGame();

        // Audio is paused instead of immediately played because it makes the game lag just enough to cause the transition from the intro to fuck up
        bgMusic.Play();
        bgMusic.Pause();

        // Move players depending on player count
        if (PlayerPrefs.GetInt("players") == 0)
        {
            players[1].GetComponentInChildren<SpriteRenderer>().color = Color.clear; // Hide sprite
            players[1].isActive = false; // Disable player
        }
        else if (PlayerPrefs.GetInt("players") == 1)
        {
            players[0].GetComponentInChildren<SpriteRenderer>().color = Color.clear; // Hide sprite
            players[0].isActive = false; // Disable player
        }

        // Set last boss save data so player can leave and come back
        PlayerPrefs.SetInt("lastBoss", SceneManager.GetActiveScene().buildIndex);
        PlayerPrefs.Save();
    }

    private void Update()
    {
        switch (gameState)
        {
            case GameState.Intro:
                break;
            case GameState.Playing:
                break;
            case GameState.GameOver:
                // Explode at 2 seconds into sequence
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

                // Show game over text at 4 seconds into sequence
                else if (Time.time >= gameOverTime + 4f)
                {
                    // Game over text
                    if (boss.isDead)
                    {
                        if (SceneManager.GetActiveScene().buildIndex == 3) gameOverUI.GetComponent<GameOverText>().FinalWinText(); // Final boss win text
                        else gameOverUI.GetComponent<GameOverText>().WinText(); // Boss win text
                    }
                    gameOverUI.SetActive(true); // Display game over screen text

                    if (Input.GetKeyDown(KeyCode.Space)) // Reset after game over
                    {
                        // On win
                        if (boss.isDead)
                        {
                            if (SceneManager.GetActiveScene().buildIndex == 3) SceneManager.LoadScene(0); // Final boss killed, load main menu
                            else SceneManager.LoadScene(Mathf.Min(3, SceneManager.GetActiveScene().buildIndex + 1)); // Boss killed, load next boss scene
                        }

                        // On lose
                        else
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

                            ResetGame();
                            StartGame();
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.Return))
                    {
                        // Hide game over ui, show stats ui and swap game state
                        gameOverUI.SetActive(false);
                        statsUI.SetActive(true);
                        gameState = GameState.Stats;
                    }
                    else if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        SceneManager.LoadScene(0);
                    }
                }

                // Skip ahead 2 seconds in sequence
                if (Input.GetKeyDown(KeyCode.Space)) gameOverTime -= 2f;

                break;
            case GameState.Stats:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    // Hide stats ui, show game over ui and swap game state
                    gameOverUI.SetActive(true);
                    statsUI.SetActive(false);
                    gameState = GameState.GameOver;
                }

                break;
        }
    }

    // Starts the game after intro
    public void StartGame()
    {
        // Change game state
        gameState = GameState.Playing;

        // Unpause/play music
        bgMusic.UnPause();
        if (!bgMusic.isPlaying) bgMusic.Play();

        // Start timer
        startTime = Time.time;
    }

    // Starts game over sequence, called by projectile that deals final blow to character
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
        gameState = GameState.GameOver;
        gameOverTime = Time.time;
        gameOverChar = character;
        gameOverProjectile = projectile;
        gameOverProjectileParent = projectileParent;

        endTime = Time.time;
    }

    // Resets game back to beginning
    public void ResetGame()
    {
        // Reset stats
        player0Damage = 0f;
        player1Damage = 0f;
        playerExchange = 0f;

        // Reset game timer
        startTime = Time.time;
    }

    // Returns a specific player
    public Player GetPlayer(int playerId) { return players[playerId]; } // what could go wrong
    public Wizard GetWizard() { return players[0].GetComponent<Wizard>(); }
    public Warrior GetWarrior() { return players[1].GetComponent<Warrior>(); }
    public Player GetOtherPlayer(int playerId) { return playerId == 0 ? players[1] : players[0]; } // Returns player based off opposite of the given id
    public Boss GetBoss() { return boss; }

    // Stat trackers
    public void AddDamageStat(GameObject shooter, float damage)
    {
        // Check if shooter was player
        Player playerComp = shooter.GetComponent<Player>();
        if (playerComp != null)
        {
            // Increase stat for relevant player
            if (playerComp.playerId == 0) player0Damage += damage;
            else player1Damage += damage;
        }
    }
    public void AddExchangeStat(float exchange) { playerExchange += exchange; }
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

public enum GameState
{
    Intro, Playing, GameOver, Stats
}
