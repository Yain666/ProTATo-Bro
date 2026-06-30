using UnityEngine;

public sealed class ExplosionFramePlayer : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Sprite[] _frames;
    private float _frameDuration;
    private float _timer;
    private int _index;

    public static void Spawn(Vector3 worldPosition, float size, Sprite[] frames, float frameDuration)
    {
        if (frames == null || frames.Length == 0)
        {
            return;
        }

        GameObject go = new GameObject("Fx_ExplosionFrames");
        go.transform.position = worldPosition;
        go.transform.localScale = new Vector3(size, size, 1f);
        ExplosionFramePlayer player = go.AddComponent<ExplosionFramePlayer>();
        player.Initialize(frames, frameDuration);
    }

    public void Initialize(Sprite[] frames, float frameDuration)
    {
        _frames = frames;
        _frameDuration = Mathf.Max(0.01f, frameDuration);
        _renderer = gameObject.AddComponent<SpriteRenderer>();
        _renderer.sortingOrder = 12;
        _renderer.sprite = _frames[0];
        _renderer.color = Color.white;
    }

    private void Update()
    {
        if (_frames == null || _frames.Length == 0 || _renderer == null)
        {
            Destroy(gameObject);
            return;
        }

        _timer += Time.deltaTime;
        if (_timer < _frameDuration)
        {
            return;
        }

        _timer = 0f;
        _index++;
        if (_index >= _frames.Length)
        {
            Destroy(gameObject);
            return;
        }

        _renderer.sprite = _frames[_index];
    }
}
