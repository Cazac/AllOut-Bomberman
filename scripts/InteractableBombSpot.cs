using AO;

public class InteractableBombSpot : Component
{
    private Interactable interactable;
    public Sprite_Renderer spriteRenderer;

    public bool hasBomb = false;
    public Entity currentBomb;

    // ===============================================================================================================================

    public override void Awake()
    {
        interactable = Entity.GetComponent<Interactable>();
        spriteRenderer = Entity.GetComponent<Sprite_Renderer>();
        interactable.Awaken();

        interactable.CanUseCallback += (Player p) =>
        {
            BomberPlayer player = (BomberPlayer) p;
            if (player == null) return false;

            if (player.isPlayerInGame && !hasBomb)
            {
                //spriteRenderer.Tint = new Vector4(1f, 1f, 1f, 1f);
                return true;
            }
            else
            {
                //spriteRenderer.Tint = new Vector4(1f, 1f, 1f, 0.5f);
                return false;
            }
        };

        interactable.OnInteract = (Player p) =>
        {
            BomberPlayer player = (BomberPlayer)p;
            if (player == null) return;

            PlaceBomb(player);
        };
    }

    public void PlaceBomb(BomberPlayer player)
    {
        hasBomb = true;

        // Spawn Bomb and Prime it to explode after a delay
        // TODO: Use a object pool instead of spawning/destroying
        currentBomb = Entity.Instantiate(Assets.GetAsset<Prefab>("Basic Bomb.prefab"));
        currentBomb.Position = Entity.Position;
        currentBomb.GetComponent<Bomb>().Setup(this);
    }

    public void RemoveBomb()
    {
        hasBomb = false;
        currentBomb = null;
    }

    // ===============================================================================================================================
}
