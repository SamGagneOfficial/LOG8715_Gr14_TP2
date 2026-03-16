using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Random = UnityEngine.Random;


public partial struct SpawnSystem : ISystem
{
    private Ex3Config config;

    private float halfHeight;
    private float halfWidth;

    private float minLife;
    private float maxLife;
    private float minDecay;
    private float maxDecay;

    public void OnCreate(ref SystemState state)
    {
        config = SystemAPI.GetSingleton<Ex3Config>();

        float minLife = 5.0f;
        float maxLife = 15.0f;
        float minDecay = 1.0f;
        float maxDecay = 3.0f;

        //Screen size calculations
        {
            float size = (float)config.gridSize;
            var ratio = Camera.main.aspect;
            halfHeight = math.round(math.sqrt(size / ratio)) / 2;
            halfWidth = math.round(size / (2 * halfHeight)) / 2;
        }

        var entityManager = state.EntityManager;

        var plantArch = entityManager.CreateArchetype(
            typeof(Position),
            typeof(Size),
            typeof(Timer),
            typeof(Flags)
            );

        //Les proies et prédateurs ont les même composantes
        var actorArch = entityManager.CreateArchetype(
            typeof(Position),
            typeof(Velocity),
            typeof(Timer),
            typeof(Flags)
            );

        NativeArray<Entity> plants = new NativeArray<Entity>(config.plantCount, Allocator.Temp);
        NativeArray<Entity> actors = new NativeArray<Entity>(config.preyCount + config.predatorCount, Allocator.Temp);

        entityManager.CreateEntity(plantArch, plants);
        entityManager.CreateEntity(actorArch, actors);


        for (int i = 0; i < config.plantCount; i++)
        {
            // Set up Components
            entityManager.SetComponentData(plants[i], new Position
            {
                Value = new float2(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight))
            });
            entityManager.SetComponentData(plants[i], new Size { Value = Random.Range(0.2f, 3.0f) });
            entityManager.SetComponentData(plants[i], new Timer { Value = Random.Range(minLife, maxLife), DecaySpeed = 1.0f, Exponent = 0 });
            entityManager.SetComponentData(actors[i], new Flags { Value = 0 });
        }

        for (int i = 0; i < config.preyCount + config.predatorCount; i++)
        {
            // Set up Components
            entityManager.SetComponentData(actors[i], new Position
            {
                Value = new float2(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight))
            });

            entityManager.SetComponentData(actors[i], new Velocity { Value = float2.zero });
            entityManager.SetComponentData(actors[i], new Timer { Value = Random.Range(minLife, maxLife), DecaySpeed = 1.0f, Exponent = 0 });

            if (i >= config.preyCount)
            {
                entityManager.SetComponentData(actors[i], new Flags { Value = 2 });
            }
            else
            {
                entityManager.SetComponentData(actors[i], new Flags { Value = 1 });
            }
        }
        plants.Dispose();
        actors.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        var respawn = new RespawnJob
        {
            minLife = minLife,
            maxLife = maxLife,
            halfWidth = halfWidth,
            halfHeight = halfHeight
        };

        state.Dependency = respawn.Schedule(state.Dependency);

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        var death = new TimerDeathJob
        {
            ecb = ecb
        }.Schedule(state.Dependency);
    }
}