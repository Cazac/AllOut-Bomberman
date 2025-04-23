using System.Collections;
using AO;

public enum CameraTargetingState { Lobby, Spectator, Game }

public partial class BomberPlayer : Player
{
    // ===============================================================================================================================

    private PlayerCamera playerCamera;

    private const float LOBBY_ZOOM_LEVEL = 1.2f;
    private const float SPECTATOR_ZOOM_LEVEL = 2.8f;
    private const float GAME_ZOOM_LEVEL = 1.8f;

    private const float LOBBY_ZOOM_SPEED = 1.5f;
    private const float SPECTATOR_ZOOM_SPEED = 1.3f;
    private const float DEATH_SPECTATOR_ZOOM_SPEED = 0.7f;
    private const float GAME_ZOOM_SPEED = 2f;

    private const float DEATH_LENGTH = 3f;

    // TODO: Use Serialized instead of searching by name
    private Entity spectatorCameraTarget;
    private Entity spectatorRespawnTarget;

    // TODO: Use a Freeze Effect instead movement speed modifier
    private float movementSpeedModifier = 1f;

    public SyncVar<bool> isPlayerInGame = new(false);
    public SyncVar<bool> isPlayerInQueue = new(false);
    public SyncVar<bool> isDead = new(false);

    // TODO: Slow down bomb placement speed
    // private Coroutine bombPlacementCooldownCoroutine;
    // public float bombPlacementCooldownTimer = 0;

    // TODO: Add multiple heath for players
    // public SyncVar<int> healthRemaining = new(0);

    // ===============================================================================================================================

    public override void Awake()
    {
        // TODO: Find is probably not the best choice, Serialized scene reff might be better or a manager place for scene refs? 
        spectatorCameraTarget = Entity.FindByName("SpectatorCameraSpot");
        spectatorRespawnTarget = Entity.FindByName("SpectatorRespawnTarget");

        if (SpineAnimator.Alive())
        {
            SpineAnimator.Awaken();

            var stateMachine = SpineAnimator.SpineInstance.StateMachine;
            var aoLayer = stateMachine.TryGetLayerByIndex(0);

            var aoIdleState = aoLayer.TryGetStateByName("Idle");
            var deathState = aoLayer.CreateState("Death", 0, false);

            var deathTrigger = stateMachine.CreateVariable("Death", StateMachineVariableKind.TRIGGER);
            aoLayer.CreateGlobalTransition(deathState).CreateTriggerCondition(deathTrigger);

            var idleTrigger = stateMachine.CreateVariable("idle", StateMachineVariableKind.TRIGGER);
            aoLayer.CreateGlobalTransition(aoIdleState).CreateTriggerCondition(idleTrigger);
        }

        if (IsLocal)
        {
            // Calculate all the queued and in game players for the manager list as the list does not seem to be able to use SyncVar
            // The lists are not synced but all info is gathered on join through these searches
            // TODO: There is most certainly a better way to sync these types of variables / info
            if (Network.IsClient)
            {
                GameManagerBomberman.instance.playersInCurrentMatch.AddRange(Scene.Components<BomberPlayer>().Where(p => p.isPlayerInGame.Value));
                GameManagerBomberman.instance.playersInQueue.AddRange(Scene.Components<BomberPlayer>().Where(p => p.isPlayerInQueue.Value));
            }

            playerCamera = new PlayerCamera();
            playerCamera.Init(Entity, PlayerCamera.DEFAULT_ZOOM, PlayerCamera.DEFAULT_ZOOM_SPEED);

            SetToLobbyCamera();
        }

        UIStatusManager.instance.UpdateMenus();
    }

    public override void Update()
    {
        if (IsLocal)
        {
            playerCamera.Update();
        }
    }

    public override Vector2 CalculatePlayerVelocity(Vector2 currentVelocity, Vector2 input, float deltaTime)
    {
        // TODO: make this ragdoll slide across the map
        // Does not seem to like it though and keeps teleporting player backwards...
        // Seems to be a freeze effect as well which may be better?
        Vector2 velocity = DefaultPlayerVelocityCalculation(currentVelocity, input, deltaTime, movementSpeedModifier);
        return velocity;
    }

    // ===============================================================================================================================

    public void SetToLobbyCamera()
    {
        if (!IsLocal) return;

        playerCamera.SetZoom(LOBBY_ZOOM_LEVEL);
        playerCamera.SetZoomSpeed(LOBBY_ZOOM_SPEED);
        playerCamera.SetTarget(Entity);
    }

    public void SetToSpectatorCamera()
    {
        if (!IsLocal) return;

        playerCamera.SetZoom(SPECTATOR_ZOOM_LEVEL);
        playerCamera.SetZoomSpeed(SPECTATOR_ZOOM_SPEED);
        if (spectatorCameraTarget != null)
        {
            playerCamera.SetTarget(spectatorCameraTarget);
            playerCamera.SetOffset(new Vector2(0, 0));
        }
    }

    public void SetToDeathSpectatorCamera()
    {
        if (!IsLocal) return;

        playerCamera.SetZoom(SPECTATOR_ZOOM_LEVEL);
        playerCamera.SetZoomSpeed(DEATH_SPECTATOR_ZOOM_SPEED);
        if (spectatorCameraTarget != null)
        {
            playerCamera.SetTarget(spectatorCameraTarget);
            playerCamera.SetOffset(new Vector2(0, 0));
        }
    }

    public void SetToGameCamera()
    {
        if (!IsLocal) return;

        playerCamera.SetZoom(GAME_ZOOM_LEVEL);
        playerCamera.SetZoomSpeed(GAME_ZOOM_SPEED);
        playerCamera.SetTarget(Entity);
    }

    // ===============================================================================================================================

    public void PlayerDeath()
    {
        // TODO: Add death animation and ragdoll
        // Random rng = new Random();
        // deathDirection = new Vector2(rng.NextFloat(), rng.NextFloat());

        GameManagerBomberman.instance.RemovePlayerFromMatch(this);

        if (Network.IsServer)
        {
            isDead.Set(true);
        }
        LockMovement();

        SpineAnimator.SpineInstance.StateMachine.SetTrigger("death");
        Coroutine.Start(Entity, PlayerRevivalDelay());
    }

    public IEnumerator PlayerRevivalDelay()
    {
        SetToDeathSpectatorCamera();
        LockMovement();

        yield return new WaitForSeconds(DEATH_LENGTH);
        PlayerRevival();
    }

    public void PlayerRevival()
    {
        if (Network.IsServer)
        {
            isDead.Set(false);
        }
        UnlockMovement();

        // TODO: Investigate why I am calling both, probably a duplicate call if it is both server and client?
        // Spawn back in the spectator rafters
        if (Network.IsServer)
        {
            Teleport(spectatorRespawnTarget.Position);
        }
        else
        {
            RequestSetPosition(this, spectatorRespawnTarget.Position);
        }

        SetToSpectatorCamera();
        SpineAnimator.SpineInstance.StateMachine.SetTrigger("idle");
    }

    public void RequestSetPosition(Player player, Vector2 position)
    {
        CallServer_SetPosition(player, position);
    }

    [ServerRpc]
    public void SetPosition(Player player, Vector2 position)
    {
        if (!player.Entity.Alive() || Network.IsClient) return;

        Log.Warn($"ZACH LOG - SetPosition");
        player.Teleport(position);
    }

    // ===============================================================================================================================

    public void UnlockMovement()
    {
        // TODO: Figure out why the freeze is not responding all the time, a server / client issue? 
        movementSpeedModifier = 1f;
        //RemoveFreezeReason("dead");
    }

    public void LockMovement()
    {
        // TODO: Figure out why the freeze is not responding all the time, a server / client issue? 
        movementSpeedModifier = 0f;
        //AddFreezeReason("dead");
    }

    // ===============================================================================================================================
}
