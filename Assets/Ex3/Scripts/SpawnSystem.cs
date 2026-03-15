using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public partial struct SpawnSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {    
        Ex3Config config;

        float halfHeight;
        float halfWidth;

        float maxLife = 10.0f;
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
            typeof(Timer)
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
            entityManager.SetComponentData(plants[i], new Position { 
                    Value = float2(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight))
                });
            entityManager.SetComponentData(plants[i], new Size { Value = Random.Range(0.2f, 3.0f) });
            entityManager.SetComponentData(plants[i], new Timer { Value = Random.Range(0, maxLife), DecaySpeed = Random.Range(minDecay, maxDecay) });
        }

        for (int i = 0; i < config.preyCount + config.predatorCount; i++)
        {
            // Set up Components
            entityManager.SetComponentData(actors[i], new Position {
                    Value = float2(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight))
                });
            entityManager.SetComponentData(actors[i], new Velocity { Value = float2.zero });
            entityManager.SetComponentData(actors[i], new Timer { Value = Random.Range(0, maxLife), DecaySpeed = Random.Range(minDecay, maxDecay) });
            entityManager.SetComponentData(actors[i], new Flags { Value = (i >= config.preyCount) });
        }

        plants.Dispose();
        actors.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {

    }
}