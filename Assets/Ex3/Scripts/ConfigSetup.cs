using UnityEngine;
using Unity.Entities;

public class ConfigSetup : MonoBehaviour
{
    public Ex3Config config;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;

        Entity configEntity = entityManager.CreateEntity(typeof(Ex3ConfigComponent));

        entityManager.SetComponentData(configEntity, new Ex3ConfigComponent
        {
            plantCount = config.plantCount,
            preyCount  = config.preyCount,
            predatorCount = config.predatorCount,
            gridSize = config.gridSize,

            PreySpeed = Ex3Config.PreySpeed,
            PredatorSpeed = Ex3Config.PredatorSpeed,
            TouchingDistance = Ex3Config.TouchingDistance
        });
    }
}
