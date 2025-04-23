using System.Collections;
using System.Drawing;
using System.Numerics;
using AO;

public class BombDamageTrigger : Component
{
    public List<Collider> boxCollidersList;

    // ===============================================================================================================================

    public override void Awake()
    {
        List<Component> collidersList = new();
        Entity.GetAllComponents(collidersList);
        boxCollidersList = collidersList.OfType<Collider>().ToList();

        for (int i = 0; i < boxCollidersList.Count; i++)
        {
            Collider collider = boxCollidersList[i];
            collider.OnCollisionEnter += otherCollider =>
            {
                BomberPlayer player = otherCollider.GetComponent<BomberPlayer>();
                if (player.Alive())
                {
                    player.PlayerDeath();
                }
            };
        }
    }

    public void Setup(float lifeLength)
    {
        Coroutine.Start(Entity, DeathTimer(lifeLength));
    }

    public IEnumerator DeathTimer(float lifeLength)
    {
        // TODO: Use a object pool instead of spawning/destroying
        yield return new WaitForSeconds(lifeLength);
        Entity.Destroy();
    }

    // ===============================================================================================================================
}
