using System.Collections;
using System.Data;
using System.Numerics;
using AO;
using static System.Formats.Asn1.AsnWriter;

public enum GameMatchState { Idle, StartingUp, Countdown, Playing }

public partial class GameManagerBomberman : Component //System<GameManagerBomberman>
{
    // ===============================================================================================================================

    public static GameManagerBomberman instance;

    // TODO: Make the bomb positions spawned on a grid instead of hand placed for a dynamic level
    //private Vector2 startingBombSpotPositions = new Vector2(0, 0);
    //private Vector2 bombSpacingDistance = new Vector2(0, 0);

    // TODO: Deathmatch timeout variables
    //private const float deathmatchFireStartingTime = 60f;
    //private const float deathmatchFireTimeInterval = 2f;
    //private const float deathmatchFirePrimingTime = 1f;

    public Entity spawnableBombSpot;

    public GameMatchState currentMatchState = GameMatchState.Idle;


    // TODO: Figure out a better way to sync these as SyncVar does not seem to work on lists
    //public List<SyncVar<BomberPlayer>> playersInQueue = new();
    //public SyncVar<List<BomberPlayer>> playersInCurrentMatch = new(new());

    public List<BomberPlayer> playersInQueue = new();
    public List<BomberPlayer> playersInCurrentMatch = new();

    public List<Entity> gameSpawnPoints = new();

    private Coroutine queueCountdownCoroutine;
    public float queueCountdownTimer = 0;

    private Coroutine matchCountdownCoroutine;
    public float matchCountdownTimer = 0;

    private const float QUEUE_COUNTDOWN_LENGTH = 10f;
    private const float MATCH_COUNTDOWN_LENGTH = 3f;

    public static int MIN_PLAYER_COUNT = 2;
    public static int MAX_WINNER_COUNT = 1;

    // ===============================================================================================================================

    public override void Awake()
    {
        instance = this;
    }

    // ===============================================================================================================================

    public void AddPlayerToQueue(BomberPlayer player)
    {
        if (Network.IsServer)
        {
            player.isPlayerInQueue.Set(true);
        }

        playersInQueue.Add(player);
        UpdatePlayerCounts();

        UIStatusManager.instance.UpdateMenus();
    }

    public void RemovePlayerFromQueue(BomberPlayer player)
    {
        if (Network.IsServer)
        {
            player.isPlayerInQueue.Set(false);
        }

        playersInQueue.Remove(player);
        UpdatePlayerCounts();

        UIStatusManager.instance.UpdateMenus();
    }

    public void AddPlayerToMatch(BomberPlayer player)
    {
        if (Network.IsServer)
        {
            player.isPlayerInGame.Set(true);
        }

        playersInCurrentMatch.Add(player);
        // Does not use UpdatePlayerCounts() as to not trigger an early win

        UIStatusManager.instance.UpdateMenus();
    }

    public void RemovePlayerFromMatch(BomberPlayer player)
    {
        if (Network.IsServer)
        {
            player.isPlayerInGame.Set(false);
        }

        playersInCurrentMatch.Remove(player);
        UpdatePlayerCounts();

        UIStatusManager.instance.UpdateMenus();
    }

    // ===============================================================================================================================

    public void WarpPlayersIntoMatch()
    {
        playersInCurrentMatch.Clear();

        // TODO: Grab these a better way then by Name
        gameSpawnPoints.Clear();
        gameSpawnPoints.Add(Entity.FindByName("GAME_SPAWN_POINT_1"));
        gameSpawnPoints.Add(Entity.FindByName("GAME_SPAWN_POINT_2"));
        gameSpawnPoints.Add(Entity.FindByName("GAME_SPAWN_POINT_3"));
        gameSpawnPoints.Add(Entity.FindByName("GAME_SPAWN_POINT_4"));
        gameSpawnPoints.Add(Entity.FindByName("GAME_SPAWN_POINT_5"));
        gameSpawnPoints.Add(Entity.FindByName("GAME_SPAWN_POINT_6"));

        // Shuffle spawn points
        var random = new Random();
        gameSpawnPoints = gameSpawnPoints.OrderBy(x => random.Next()).ToList();

        // Assign players to spawn points
        for (int i = 0; i < playersInQueue.Count; i++)
        {
            BomberPlayer player = playersInQueue[i];
            AddPlayerToMatch(player);

            Entity spawnPoint = gameSpawnPoints[i];
            player.SetToGameCamera();

            if (Network.IsServer)
            {
                player.Teleport(spawnPoint.Position);
            }
            else
            {
                player.RequestSetPosition(player, spawnPoint.Position);
            }

            player.LockMovement();
        }

        UpdatePlayerCounts();
    }

    public void WarpPlayersIntoLobby()
    {
        for (int i = 0; i < playersInCurrentMatch.Count; i++)
        {
            BomberPlayer player = playersInCurrentMatch[i];
            RemovePlayerFromMatch(player);

            // TODO: Use a better method here instead of revival
            player.PlayerRevival();
        }
    }

    public void StartMatch()
    {
        Notifications.Show("Fight!");
        currentMatchState = GameMatchState.Playing;

        // Release all player movement
        for (int i = 0; i < playersInCurrentMatch.Count; i++)
        {
            BomberPlayer player = playersInCurrentMatch[i];
            player.UnlockMovement();
        }

        for (int i = playersInQueue.Count - 1; i >= 0; i--)
        {
            RemovePlayerFromQueue(playersInQueue[i]);
        }
        playersInQueue.Clear();

        UIStatusManager.instance.UpdateMenus();

        // TODO: Start Music here
    }

    public void EndMatch()
    {
        // Make sure these have stopped
        CancelQueueCountdown();
        CancelMatchCountdown();

        currentMatchState = GameMatchState.Idle;

        if (playersInCurrentMatch.Count > 0 && playersInCurrentMatch[0] != null)
        {
            BomberPlayer winnerPlayer = playersInCurrentMatch[0];
            Notifications.Show("And the Winner is... " + winnerPlayer.Name + "!");
        }
        else
        {
            Log.Warn($"ZACH LOG - Invalid Winner, Fix this");
            Notifications.Show("And the Winner is... No one?");
        }

        for (int i = 0; i < playersInCurrentMatch.Count; i++)
        {
            BomberPlayer player = playersInCurrentMatch[i];
            player.UnlockMovement();
        }

        WarpPlayersIntoLobby();
        UIStatusManager.instance.UpdateMenus();
    }

    public void UpdatePlayerCounts()
    {
        switch (currentMatchState)
        {
            case GameMatchState.Idle:
                if (playersInQueue.Count >= MIN_PLAYER_COUNT)
                {
                    // Enough players reached, Start Match Countdown    
                    currentMatchState = GameMatchState.StartingUp;
                    StartQueueCountdown();
                }
                break;
            case GameMatchState.StartingUp:
                if (playersInQueue.Count < MIN_PLAYER_COUNT)
                {
                    // Not enough players, Cancel Match
                    currentMatchState = GameMatchState.Idle;
                    CancelQueueCountdown();
                }
                break;
            case GameMatchState.Countdown:
            case GameMatchState.Playing:
                if (playersInCurrentMatch.Count < MIN_PLAYER_COUNT)
                {
                    // Players left the game, end match with winners
                    EndMatch();
                }
                else if (playersInCurrentMatch.Count <= MAX_WINNER_COUNT)
                {
                    // Players left the game, end match with winners
                    EndMatch();
                }
                break;
            default:
                break;
        }
    }

    // ===============================================================================================================================

    public void StartQueueCountdown()
    {
        CancelQueueCountdown();
        queueCountdownCoroutine = Coroutine.Start(Entity, QueueCountdownTimer(QUEUE_COUNTDOWN_LENGTH));
    }

    public void CancelQueueCountdown()
    {
        if (queueCountdownCoroutine != null)
        {
            queueCountdownCoroutine.Stop();
            queueCountdownCoroutine = null;
        }
    }

    public IEnumerator QueueCountdownTimer(float duration)
    {
        queueCountdownTimer = duration;
        while (queueCountdownTimer > 0)
        {
            UIStatusManager.instance.UpdateMenus();
            yield return new WaitForSeconds(1f);
            queueCountdownTimer--;
        }

        currentMatchState = GameMatchState.Countdown;
        WarpPlayersIntoMatch();
        StartMatchCountdown();
    }

    // ===============================================================================================================================
    public void StartMatchCountdown()
    {
        CancelMatchCountdown();
        matchCountdownCoroutine = Coroutine.Start(Entity, MatchCountdownTimer(MATCH_COUNTDOWN_LENGTH));
    }

    public void CancelMatchCountdown()
    {
        if (matchCountdownCoroutine != null)
        {
            matchCountdownCoroutine.Stop();
            matchCountdownCoroutine = null;
        }
    }

    public IEnumerator MatchCountdownTimer(float duration)
    {
        matchCountdownTimer = duration;
        while (matchCountdownTimer > 0)
        {
            UIStatusManager.instance.UpdateMenus();
            yield return new WaitForSeconds(1f);
            matchCountdownTimer--;
        }

        StartMatch();
    }

    // ===============================================================================================================================
}
