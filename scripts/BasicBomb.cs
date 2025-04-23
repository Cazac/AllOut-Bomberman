using System.Collections;
using AO;

public class BasicBomb : Bomb
{
    // Serialized on a List seems to cause a crash! Avoid for now!
    //[Serialized] public List<Entity> bombExplosionRadiuses;
    //[Serialized] public List<Entity> bombBlastActivations = new List<Entity>();

    public const float bombBlastActivationSpeed = 0.5f;
    public const float bombBlastRaduisOutwardSpeed = 0.15f;
    public const float bombBlastRaduisLifetimeLength = 0.5f;

    [Serialized] public Vector4 defaultTint;
    [Serialized] public Vector4 pulseTint;

    // TODO: refference these strings is probably poor, use a list of Serialized prefabs list instead when possible
    private readonly string[] explosionSetPrefabsStrings = new string[]
    {
        "BasicBomb-ExplosionSet1.prefab",
        "BasicBomb-ExplosionSet2.prefab",
        "BasicBomb-ExplosionSet3.prefab",
    };

    // ===============================================================================================================================

    public override void Awake()
    {
        base.Awake();
    }

    public override IEnumerator Pulse()
    {
        spriteRenderer.Tint = defaultTint;

        // TODO: Rewrite this as a nested loop for more dynamic bomb options
        float elapsedTime = 0f;
        while (elapsedTime < bombBlastActivationSpeed)
        {
            float time = elapsedTime / bombBlastActivationSpeed;
            spriteRenderer.Tint = Vector4.Lerp(defaultTint, pulseTint, time);

            elapsedTime += Time.DeltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < bombBlastActivationSpeed)
        {
            float time = elapsedTime / bombBlastActivationSpeed;
            spriteRenderer.Tint = Vector4.Lerp(pulseTint, defaultTint, time);

            elapsedTime += Time.DeltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < bombBlastActivationSpeed)
        {
            float time = elapsedTime / bombBlastActivationSpeed;
            spriteRenderer.Tint = Vector4.Lerp(defaultTint, pulseTint, time);

            elapsedTime += Time.DeltaTime;
            yield return null;
        }

        spriteRenderer.Tint = pulseTint;
        Coroutine.Start(Entity, Explode());
    }

    public override IEnumerator Explode()
    {
        // Hide the bomb Sprite
        Entity.GetComponent<Sprite_Renderer>().Tint = new Vector4(1f, 1f, 1f, 0f);

        // Spawn each of the raduis in a slowmotion style so players can react
        for (int i = 0; i < explosionSetPrefabsStrings.Length; i++)
        {
            Entity explosionRadius = Entity.Instantiate(Assets.GetAsset<Prefab>(explosionSetPrefabsStrings[i]));
            explosionRadius.Position = Entity.Position;
            explosionRadius.GetComponent<BombDamageTrigger>().Setup(bombBlastRaduisLifetimeLength);

            yield return new WaitForSeconds(bombBlastRaduisOutwardSpeed);
        }

        // TODO: Use a object pool instead of spawning/destroying
        bombSpot.RemoveBomb();
        Entity.Destroy();
    }

    // ===============================================================================================================================
}
