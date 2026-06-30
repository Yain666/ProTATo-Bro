using UnityEngine;

public sealed class LootPickupBurstEffect : MonoBehaviour
{
    private const float Lifetime = 0.45f;
    private const int ParticleCount = 10;

    private SpriteRenderer[] _particles;
    private Vector3[] _velocities;
    private float _elapsed;

    public static void Play(Vector3 worldPosition, Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        GameObject root = new GameObject("LootPickupBurstEffect");
        root.transform.position = worldPosition;

        LootPickupBurstEffect effect = root.AddComponent<LootPickupBurstEffect>();
        effect.Initialize(sprite);
    }

    private void Initialize(Sprite sprite)
    {
        _particles = new SpriteRenderer[ParticleCount];
        _velocities = new Vector3[ParticleCount];

        for (int i = 0; i < ParticleCount; i++)
        {
            GameObject particle = new GameObject($"Burst_{i}");
            particle.transform.SetParent(transform, false);
            particle.transform.localScale = Vector3.one * Random.Range(0.32f, 0.46f);

            SpriteRenderer renderer = particle.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerID = 0;
            renderer.sortingOrder = 20;
            renderer.color = new Color(1f, 0.92f, 0.42f, 1f);

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            particleTransformOffset(particle.transform, i);

            float speed = Random.Range(2.2f, 3.6f);
            _velocities[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * speed;
            _particles[i] = renderer;
        }
    }

    private static void particleTransformOffset(Transform particleTransform, int index)
    {
        float radius = Random.Range(0.03f, 0.08f);
        float seedAngle = (360f / ParticleCount) * index * Mathf.Deg2Rad;
        particleTransform.localPosition = new Vector3(Mathf.Cos(seedAngle), Mathf.Sin(seedAngle), 0f) * radius;
    }

    private void Update()
    {
        if (_particles == null)
        {
            Destroy(gameObject);
            return;
        }

        _elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(_elapsed / Lifetime);
        float alpha = 1f - progress;

        for (int i = 0; i < _particles.Length; i++)
        {
            SpriteRenderer particle = _particles[i];
            if (particle == null)
            {
                continue;
            }

            Transform particleTransform = particle.transform;
            particleTransform.localPosition += _velocities[i] * Time.deltaTime;
            particleTransform.localScale *= 0.992f;

            Color color = particle.color;
            color.a = alpha;
            particle.color = color;
        }

        if (_elapsed >= Lifetime)
        {
            Destroy(gameObject);
        }
    }
}
