using UnityEngine;

// Every tunable value that is not per-lane. Edit the GameConfig asset, not this file.
[CreateAssetMenu(fileName = "GameConfig", menuName = "Frogger/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Board")]
    [Tooltip("Board width in cells. The board is centred on x = 0, so its edges are at +-boardWidth / 2.")]
    public float boardWidth = 14f;

    public float BoardLeftX => -boardWidth / 2f;
    public float BoardRightX => boardWidth / 2f;

    [Header("Hop")]
    public float cellSize = 1f;
    public float hopDuration = 0.12f;
    public float stickDeadzone = 0.5f;

    [Header("Lives & timer")]
    public int startingLives = 3;
    public int perLifeTimer = 30;
    public int timerWarningSeconds = 5;

    [Header("Turtles")]
    public float turtleSafeTime = 5f;
    public float turtleWarningTime = 1.5f;
    public float turtleDiveTime = 2f;

    [Header("Scoring")]
    public int rowScore = 10;
    public int homeScore = 50;
    public int timeBonusPerSecond = 10;
    public int roundClearBonus = 1000;

    [Header("Flow")]
    public float deathAnimDuration = 0.75f;
    public float restartLockout = 1f;
    public float homeRespawnDelay = 1f;
    public float roundClearFlashDuration = 1.5f;
    public float roundSpeedMultiplier = 1.2f;
}
