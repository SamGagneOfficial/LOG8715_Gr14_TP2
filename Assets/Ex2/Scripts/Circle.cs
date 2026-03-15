using UnityEngine;
using UnityEngine.Serialization;

public class Circle : MonoBehaviour
{
    [FormerlySerializedAs("I")] [HideInInspector]
    public int i;

    [FormerlySerializedAs("J")] [HideInInspector]
    public int j;

    public float Health { get; private set; }

    private const float BaseHealth = 1000;

    private const float HealingPerSecond = 1;
    private const float HealingRange = 3;


    private GridShape _grid;
    private SpriteRenderer _spriteRenderer;

    // Start is called before the first frame update
    private void Start()
    {
        Health = BaseHealth;
        _grid = FindFirstObjectByType<GridShape>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    // Update is called once per frame
    private void Update()
    {
        UpdateColor();
        HealNearbyShapes();
    }

    private void UpdateColor()
    {
        _spriteRenderer.color = _grid.Colors[i, j] * Health / BaseHealth;
    }

    private static Collider2D[] _results = new Collider2D[20];

    private void HealNearbyShapes()
    {
        for (int di = -1; di <= 1; di++)
        {
            for (int dj = -1; dj <= 1; dj++)
            {
                if (di == 0 && dj == 0) continue;

                int ni = i + di;
                int nj = j + dj;

                if (ni < 0 || nj < 0 || ni >= _grid._width || nj >= _grid._height)
                    continue;

                var neighbor = _grid.Circles[ni, nj];
                neighbor.ReceiveHp(HealingPerSecond * Time.deltaTime);
            }
        }
    }

    public void ReceiveHp(float hpReceived)
    {
        Health += hpReceived;
        Health = Mathf.Clamp(Health, 0, BaseHealth);
    }
}
