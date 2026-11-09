using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GState { Menu, Playing, Dying, LevelClear, Victory, Paused }

/// <summary>
/// Controla el flujo del juego: vidas, monedas, puntos, tiempo, cambio de nivel,
/// muerte del jugador (vuelve al Nivel 1) y victoria al vencer al jefe.
/// Persiste entre escenas.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager I;

    public const int StartLives = 3;
    public const int MaxLives = 9;

    public int Lives = StartLives;
    public int Coins;
    public int Score;
    public int Level = 1;
    public float TimeLeft;
    public GState State = GState.Menu;
    public Vector2 RespawnPoint;
    public Boss ActiveBoss;
    public int HighScore;

    // Textos grandes en pantalla
    public string Banner, SubBanner;
    public float BannerTimer;   // <0 = permanente
    public float LevelIntroTimer;
    float acceptAt;

    public static void Ensure()
    {
        if (I != null) return;
        var go = new GameObject("GameManager");
        DontDestroyOnLoad(go);
        I = go.AddComponent<GameManager>();
        go.AddComponent<SoundManager>();
        go.AddComponent<HUD>();
    }

    void Awake()
    {
        HighScore = PlayerPrefs.GetInt("kage_highscore", 0);
    }

    // ------------------------------------------------------------ Flujo
    public void NewGame()
    {
        Lives = StartLives; Coins = 0; Score = 0;
        GoToLevel(1);
    }

    public void GoToLevel(int n)
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        Level = Mathf.Clamp(n, 1, 3);
        string sceneName = "Nivel" + Level;
        if (Application.CanStreamedLevelBeLoaded(sceneName)) SceneManager.LoadScene(sceneName);
        else { LevelBuilder.ClearAll(); BeginLevel(Level); }
    }

    public void GoToMenu()
    {
        StopAllCoroutines();
        Time.timeScale = 1f;
        State = GState.Menu;
        Banner = null;
        if (Application.CanStreamedLevelBeLoaded("Menu")) SceneManager.LoadScene("Menu");
        else { LevelBuilder.ClearAll(); MenuController.Build(); }
    }

    /// <summary>Lo llama SceneEntry al cargar la escena de un nivel.</summary>
    public void BeginLevel(int n)
    {
        Level = n;
        State = GState.Playing;
        ActiveBoss = null;
        TimeLeft = LevelData.TimeLimit[n - 1];
        LevelBuilder.Build(n);
        SoundManager.I.PlayMusic("level" + n);
        Banner = "NIVEL " + n;
        SubBanner = LevelData.Names[n - 1];
        BannerTimer = 2.5f;
        LevelIntroTimer = 7f;
    }

    void Update()
    {
        if (BannerTimer > 0) { BannerTimer -= Time.unscaledDeltaTime; if (BannerTimer <= 0) Banner = null; }
        if (LevelIntroTimer > 0) LevelIntroTimer -= Time.unscaledDeltaTime;

        switch (State)
        {
            case GState.Playing:
                TimeLeft -= Time.deltaTime;
                if (TimeLeft <= 0) { TimeLeft = 0; KillPlayer("¡Se acabó el tiempo!"); }
                if (GameInput.PauseDown) SetPause(true);
                break;
            case GState.Paused:
                if (GameInput.PauseDown) SetPause(false);
                else if (GameInput.MenuKeyDown) { SetPause(false); GoToMenu(); }
                break;
            case GState.Victory:
                if (Time.unscaledTime > acceptAt && GameInput.SubmitDown) GoToMenu();
                break;
        }
    }

    public void SetPause(bool p)
    {
        if (p && State != GState.Playing) return;
        if (!p && State != GState.Paused) return;
        State = p ? GState.Paused : GState.Playing;
        Time.timeScale = p ? 0f : 1f;
        SoundManager.I.SetPaused(p);
        SoundManager.Play("pause");
    }

    // ------------------------------------------------------------ Eventos del juego
    public void AddScore(int n) { Score += n; }

    public void AddCoin()
    {
        Coins++; Score += 100;
        SoundManager.Play("coin", 0.8f);
        if (Coins >= 100) { Coins -= 100; AddLife(); }
    }

    public void AddLife()
    {
        if (Lives < MaxLives) Lives++;
        else Score += 1000;
        SoundManager.Play("powerup");
    }

    /// <summary>El jugador recibió un golpe: pierde una vida (corazón).</summary>
    public void PlayerHurt()
    {
        if (State != GState.Playing) return;
        Lives--;
        if (Lives <= 0) { Lives = 0; KillPlayer("Te quedaste sin vidas"); }
    }

    /// <summary>El jugador cayó a un precipicio.</summary>
    public void PlayerFell()
    {
        if (State != GState.Playing) return;
        Lives--;
        if (Lives <= 0) { Lives = 0; KillPlayer("Caíste al vacío"); return; }
        SoundManager.Play("hurt");
        PlayerController.I?.Respawn(RespawnPoint);
        CameraFollow.I?.Snap();
    }

    /// <summary>Muerte: muestra el mensaje y regresa al primer nivel.</summary>
    public void KillPlayer(string reason)
    {
        if (State != GState.Playing) return;
        State = GState.Dying;
        PlayerController.I?.Die();
        SoundManager.I.PlayJingle("death");
        Banner = "¡KENJI HA MUERTO!";
        SubBanner = reason + "\nRegresando al Nivel 1...";
        BannerTimer = -1;
        SaveHighScore();
        StartCoroutine(AfterDeath());
    }

    IEnumerator AfterDeath()
    {
        yield return new WaitForSecondsRealtime(3.8f);
        Banner = null;
        Lives = StartLives; Coins = 0; Score = 0;
        GoToLevel(1);
    }

    public void LevelComplete()
    {
        if (State != GState.Playing) return;
        State = GState.LevelClear;
        int bonus = Mathf.CeilToInt(TimeLeft) * 10;
        Score += bonus;
        SoundManager.I.PlayJingle("clear");
        Banner = "¡NIVEL COMPLETADO!";
        SubBanner = "Bono de tiempo: +" + bonus;
        BannerTimer = -1;
        PlayerController.I?.Celebrate();
        StartCoroutine(AfterClear());
    }

    IEnumerator AfterClear()
    {
        yield return new WaitForSecondsRealtime(3.5f);
        Banner = null;
        GoToLevel(Level + 1);
    }

    public void StartBossMusic()
    {
        SoundManager.I.PlayMusic("boss");
        Banner = "¡EL SHOGUN OSCURO!";
        SubBanner = "Derrótalo para salvar la aldea";
        BannerTimer = 2.5f;
    }

    public void BossDefeated()
    {
        if (State != GState.Playing) return;
        State = GState.Victory;
        int bonus = Mathf.CeilToInt(TimeLeft) * 10 + 5000;
        Score += bonus;
        SaveHighScore();
        SoundManager.I.PlayMusic("victory");
        Banner = "¡VICTORIA!";
        SubBanner = "Kenji derrotó al Shogun Oscuro y recuperó el pergamino sagrado.\nPuntuación final: " + Score + "\n\nPresiona ENTER para volver al menú";
        BannerTimer = -1;
        PlayerController.I?.Celebrate();
        acceptAt = Time.unscaledTime + 2.5f; // evita saltar la pantalla con una tecla pulsada
    }

    void SaveHighScore()
    {
        if (Score > HighScore) { HighScore = Score; PlayerPrefs.SetInt("kage_highscore", HighScore); PlayerPrefs.Save(); }
    }
}
