using AO;

public class SpectatorTrigger : Component
{
    public Box_Collider Collider;

    // ===============================================================================================================================

    public override void Awake()
    {
        Collider = Entity.GetComponent<Box_Collider>();
        Collider.OnCollisionEnter += otherCollider =>
        {
            BomberPlayer player = otherCollider.GetComponent<BomberPlayer>();
            if (player.Alive())
            {
                player.SetToSpectatorCamera();
            }
        };

        Collider.OnCollisionExit += otherCollider =>
        {
            BomberPlayer player = otherCollider.GetComponent<BomberPlayer>();
            if (player.Alive())
            {
                player.SetToLobbyCamera();
            }
        };
    }

    // ===============================================================================================================================
}
