using System.Collections;
using System.Drawing;
using System.Numerics;
using AO;

public class Bomb : Component
{
    public Sprite_Renderer spriteRenderer;
    public InteractableBombSpot bombSpot;

    // ===============================================================================================================================

    public override void Awake()
    {
        spriteRenderer = Entity.GetComponent<Sprite_Renderer>();
    }

    public virtual IEnumerator Pulse()
    {
        yield return null;
    }

    public virtual IEnumerator Explode()
    {
        yield return null;
    }

    public void Setup(InteractableBombSpot interactableBombSpot)
    {
        bombSpot = interactableBombSpot;
        Coroutine.Start(Entity, Pulse());
    }

    // ===============================================================================================================================
}
