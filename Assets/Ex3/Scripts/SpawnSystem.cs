using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Random = UnityEngine.Random;
//take note of how Unity.Mathematics.Random works first...


public partial struct SpawnSystem : ISystem
{
    public float minLife;
    public float maxLife;
    public float halfHeight;
    public float halfWidth;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Ex3ConfigComponent>();

        var config = SystemAPI.GetSingleton<Ex3ConfigComponent>();

        float minLife = 5.0f;
        float maxLife = 15.0f;

        //Screen size calculations
        {
            float size = (float)config.gridSize;
            halfHeight = size / 2;
            halfWidth = size / 2;
        }

        var entityManager = state.EntityManager;

        var plantArch = entityManager.CreateArchetype(
            typeof(Position),
            typeof(Size),
            typeof(Timer),
            typeof(PlantTag)
            );

        var preyArch = entityManager.CreateArchetype(
            typeof(Position),
            typeof(Velocity),
            typeof(Timer),
            typeof(PreyTag)
            );

        var predatorArch = entityManager.CreateArchetype(
            typeof(Position),
            typeof(Velocity),
            typeof(Timer),
            typeof(PredatorTag)
            );

        NativeArray<Entity> plants = new NativeArray<Entity>(config.plantCount, Allocator.Temp);
        NativeArray<Entity> preys = new NativeArray<Entity>(config.preyCount, Allocator.Temp);
        NativeArray<Entity> predators = new NativeArray<Entity>(config.predatorCount, Allocator.Temp);

        entityManager.CreateEntity(plantArch, plants);
        entityManager.CreateEntity(preyArch, preys);
        entityManager.CreateEntity(predatorArch, predators);


        for (int i = 0; i < config.plantCount; i++)
        {
            // Set up Components
            entityManager.SetComponentData(plants[i], new Position
            {
                Value = new float2(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight))
            });
            entityManager.SetComponentData(plants[i], new Size { Value = Random.Range(0.2f, 3.0f) });
            entityManager.SetComponentData(plants[i], new Timer { Value = Random.Range(minLife, maxLife), DecaySpeed = 1.0f, Exponent = 0 });
        }

        for (int i = 0; i < config.preyCount; i++)
        {
            // Set up Components
            entityManager.SetComponentData(preys[i], new Position
            {
                Value = new float2(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight))
            });

            entityManager.SetComponentData(preys[i], new Velocity { Value = float2.zero });
            entityManager.SetComponentData(preys[i], new Timer { Value = Random.Range(minLife, maxLife), DecaySpeed = 1.0f, Exponent = 0 });
        }

        for (int i = 0; i < config.predatorCount; i++)
        {
            // Set up Components
            entityManager.SetComponentData(predators[i], new Position
            {
                Value = new float2(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight))
            });

            entityManager.SetComponentData(predators[i], new Velocity { Value = float2.zero });
            entityManager.SetComponentData(predators[i], new Timer { Value = Random.Range(minLife, maxLife), DecaySpeed = 1.0f, Exponent = 0 });
        }

        plants.Dispose();
        preys.Dispose();
        predators.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        var respawn = new RespawnJob
        {
            minLife = minLife,
            maxLife = maxLife,
            halfWidth = halfWidth,
            halfHeight = halfHeight,
            ecb = ecb
        }.Schedule(state.Dependency);

        state.Dependency = new DeathJob
        {
            ecb = ecb
        }.Schedule(respawn);
    }
}