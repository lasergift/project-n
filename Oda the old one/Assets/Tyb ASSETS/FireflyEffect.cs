using UnityEngine;

public class FireflyEffect : MonoBehaviour
{
    [Header("Firefly Settings")]
    [SerializeField] private int maxParticles = 20;
    [SerializeField] private float spawnRadius = 2f;
    [SerializeField] private Color fireflyColor = new Color(1f, 0.9f, 0.5f, 1f);
    [SerializeField] private float minSize = 0.05f;
    [SerializeField] private float maxSize = 0.15f;
    [SerializeField] private float glowIntensity = 2f;

    private ParticleSystem particleSystem;

    private void Awake()
    {
        SetupParticleSystem();
    }

    private void SetupParticleSystem()
    {
        GameObject particleObject = new GameObject("FireflyParticles");
        particleObject.transform.SetParent(transform);
        particleObject.transform.localPosition = Vector3.zero;

        particleSystem = particleObject.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = particleSystem.main;
        main.startLifetime = 3f;
        main.startSpeed = 0.5f;
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor = new ParticleSystem.MinMaxGradient(fireflyColor);
        main.maxParticles = maxParticles;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;
        main.playOnAwake = true;

        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.rateOverTime = maxParticles / 3f;

        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = spawnRadius;
        shape.radiusThickness = 0.5f;

        ParticleSystem.VelocityOverLifetimeModule velocity = particleSystem.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.2f, 0.4f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(fireflyColor, 0f),
                new GradientColorKey(fireflyColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.2f),
                new GradientAlphaKey(1f, 0.8f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.3f);
        sizeCurve.AddKey(0.5f, 1f);
        sizeCurve.AddKey(1f, 0.3f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        ParticleSystemRenderer renderer = particleObject.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateFireflyMaterial();
        renderer.sortingOrder = 10;
    }

    private Material CreateFireflyMaterial()
    {
        Material material = new Material(Shader.Find("Sprites/Default"));
        
        Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[32 * 32];
        Vector2 center = new Vector2(16, 16);
        
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float normalizedDistance = distance / 16f;
                float alpha = Mathf.Clamp01(1f - normalizedDistance);
                alpha = Mathf.Pow(alpha, 2f);
                
                Color color = fireflyColor;
                color.a = alpha;
                pixels[y * 32 + x] = color * glowIntensity;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;
        
        material.mainTexture = texture;
        material.SetFloat("_Mode", 3);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        
        return material;
    }
}
