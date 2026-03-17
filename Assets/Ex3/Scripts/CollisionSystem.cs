using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


public partial struct CollisionSystem : ISystem
{
    private NativeParallelMultiHashMap<int, Entity> grid;
    private int cellCountX;
    private int cellCountY;
    private float cellSize;

    private ComponentLookup<PreyTag> preys;
    private ComponentLookup<PredatorTag> predators;
    private ComponentLookup<PlantTag> plants;
    private ComponentLookup<Timer> timers;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Ex3ConfigComponent>();

        var config = SystemAPI.GetSingleton<Ex3ConfigComponent>();

        cellSize = 2 * config.TouchingDistance;
        cellCountX = (int)(config.gridSize / config.TouchingDistance);
        cellCountY = (int)(config.gridSize / config.TouchingDistance);

        grid = new NativeParallelMultiHashMap<int, Entity>(cellCountX * cellCountY, Allocator.Persistent);
    }

    public void OnUpdate(ref SystemState state)
    {
        if (grid.IsCreated)
            grid.Clear();

        //Build the Grid
        var gridJob = new BuildSpatialGridJob
        {
            grid = grid.AsParallelWriter(),
            cellSize = cellSize,
            cellCountX = cellCountX
        }.Schedule(state.Dependency);

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        // Update the ComponentLookup so it reflects the current world
        preys = SystemAPI.GetComponentLookup<PreyTag>(true);      // true = read-only
        predators = SystemAPI.GetComponentLookup<PredatorTag>(true);
        plants = SystemAPI.GetComponentLookup<PlantTag>(true);
        timers = SystemAPI.GetComponentLookup<Timer>(false);      // false = writable

        var collisionJob = new CollisionJob
        {
            grid = grid,
            ecb = ecb,
            preys = preys,
            predators = predators,
            plants = plants,
            timers = timers
        }.Schedule(gridJob);
    }
}