using System.Numerics;
using AO;

public partial class InteractableStatue : Component
{
    private Interactable interactable;

    // ===============================================================================================================================

    public override void Awake()
    {
        interactable = Entity.GetComponent<Interactable>();
        interactable.Awaken();

        interactable.CanUseCallback += (Player p) =>
        {
            BomberPlayer player = (BomberPlayer)p;
            if (player == null) return false;

            if (Network.IsClient && player.IsLocal)
            {
                UpdateInteractableText(player);
            }

            return (!player.isPlayerInGame);
        };

        interactable.OnInteract = (Player p) =>
        {
            BomberPlayer player = (BomberPlayer)p;
            if (player == null) return;

            if (!player.isPlayerInQueue)
            {
                JoinQueue(player);
            }
            else
            {
                LeaveQueue(player);
            }
        };
    }

    public void JoinQueue(BomberPlayer player)
    {
        GameManagerBomberman.instance.AddPlayerToQueue(player);
        if (Network.IsClient && player.IsLocal)
        {
            UpdateInteractableText(player);
        }
    }

    public void LeaveQueue(BomberPlayer player)
    {
        GameManagerBomberman.instance.RemovePlayerFromQueue(player);
        if (Network.IsClient && player.IsLocal)
        {
            UpdateInteractableText(player);
        }
    }

    private void UpdateInteractableText(BomberPlayer player)
    {
        if (player.isPlayerInQueue)
        {
            interactable.Text = "Leave Game";
            interactable.HoldText = "Leaving!";
        }
        else
        {
            interactable.Text = "Join Game";
            interactable.HoldText = "Joining!";
        }
    }

    // ===============================================================================================================================
}
