using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Random = UnityEngine.Random;

[BurstCompile]
public partial struct RespawnJob : IJobEntity
{
    public float minLife;
    public float maxLife;

    public float halfWidth;
    public float halfHeight;

    public EntityCommandBuffer.ParallelWriter ecb;

    public void Execute([EntityIndexInQuery] int index, Entity entity, ref Position pos, ref Timer timer, in ReproductionTag rep)
    {
        if(timer.Value <= 0f)
        {
            pos.Value = new float2(
            Random.Range(-halfWidth, halfWidth),
            Random.Range(-halfHeight, halfHeight)
            );

            timer.Value = Random.Range(minLife, maxLife);

            //Removes reproduction tag
            ecb.RemoveComponent<ReproductionTag>(index, entity);
        }
    }
}

[BurstCompile]
public partial struct DeathJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecb;

    public void Execute([EntityIndexInQuery] int index, Entity entity, ref Timer timer)
    {
        if (timer.Value <= 0f)
        {
            ecb.DestroyEntity(index, entity);
        }
    }
}

//This job creates a grid of base cellSize by cellSize and hashes the index of the struct
[BurstCompile]
public partial struct BuildSpatialGridJob : IJobEntity
{
    public NativeParallelMultiHashMap<int, Entity>.ParallelWriter grid;
    public float cellSize;
    public int cellCountX;

    public void Execute(Entity entity, in Position pos)
    {
        int cellX = (int)math.floor(pos.Value.x / cellSize);
        int cellY = (int)math.floor(pos.Value.y / cellSize);
        int key = cellX + cellY * cellCountX;

        grid.Add(key, entity);
    }
}

[BurstCompile]
public partial struct CollisionJob : IJobEntity
{
    [ReadOnly] public NativeParallelMultiHashMap<int, Entity> grid;

    public EntityCommandBuffer.ParallelWriter ecb;

    [ReadOnly] public ComponentLookup<PreyTag> preys;
    [ReadOnly] public ComponentLookup<PredatorTag> predators;
    [ReadOnly] public ComponentLookup<PlantTag> plants;

    [NativeDisableParallelForRestriction]
    public ComponentLookup<Timer> timers;

    [NativeDisableParallelForRestriction]
    public ComponentLookup<Velocity> velocities;

    [ReadOnly] public ComponentLookup<Position> positions;

    public float cellSize;
    public int cellCountX;

    public void Execute(
        [EntityIndexInQuery] int index,
        Entity entity,
        in Position pos)
    {
        int cellX = (int)math.floor(pos.Value.x / cellSize);
        int cellY = (int)math.floor(pos.Value.y / cellSize);

        int currentKey = cellX + cellY * cellCountX;
        Entity currentTarget = entity;

        if (grid.TryGetFirstValue(currentKey, out var other, out var it))
        {
            do
            {
                if (other == entity)
                    continue;

                HandleInteraction(index, entity, other);
                if (Target(entity, other))
                    currentTarget = other;

            } while (grid.TryGetNextValue(out other, ref it));
        }

        if (currentTarget != entity)
            return;

        // check 3x3 neighborhood for target
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                int key = (cellX + x) + (cellY + y) * cellCountX;

                if (grid.TryGetFirstValue(key, out var other2, out var it2))
                {
                    do
                    {
                        if (other2 == entity)
                            continue;

                        float dist = math.lengthsq(pos.Value - positions[other2].Value);

                    } while (grid.TryGetNextValue(out other2, ref it2));
                }
            }
        }
    }

    void HandleInteraction(int index, Entity a, Entity b)
    {
        if ((plants.HasComponent(a) && preys.HasComponent(b)) ||
            (preys.HasComponent(a) && predators.HasComponent(b)))
        {
            var ta = timers[a];
            var tb = timers[b];

            ta.Exponent--;
            tb.Exponent++;

            timers[a] = ta;
            timers[b] = tb;
        } else if ((preys.HasComponent(a) && plants.HasComponent(b)) ||
            (predators.HasComponent(a) && preys.HasComponent(b)))
        {
            var ta = timers[a];
            var tb = timers[b];

            ta.Exponent++;
            tb.Exponent--;

            timers[a] = ta;
            timers[b] = tb;
        }

        if ((preys.HasComponent(a) && preys.HasComponent(b)) ||
            (predators.HasComponent(a) && predators.HasComponent(b)))
        {
            ecb.AddComponent<ReproductionTag>(index, a);
            ecb.AddComponent<ReproductionTag>(index, b);

            var ta = timers[a];
            var tb = timers[b];

            ta.Exponent++;
            tb.Exponent--;

            timers[a] = ta;
            timers[b] = tb;
        }
    }

    bool Target(Entity a, Entity b)
    {
        return (plants.HasComponent(a) && preys.HasComponent(b)) || (preys.HasComponent(a) && predators.HasComponent(b));
    }
}

[BurstCompile]
public partial struct PlantSizeJob : IJobEntity
{
    public float dt;

    public void Execute(ref Size size, in Timer timer)
    {
        float delta = timer.DecaySpeed * dt * math.exp2(timer.Exponent);
        size.Value *= timer.Value / (timer.Value + delta);
    }
}

[BurstCompile]
public partial struct DecayJob : IJobEntity
{
    public float dt;

    public void Execute(ref Timer timer)
    {
        timer.Value -= timer.DecaySpeed * dt * math.exp2(timer.Exponent);
        timer.Exponent = 0; //reset
    }
}

[BurstCompile]
public partial struct MovementJob : IJobEntity
{
    public float dt;

    public void Execute(ref Position pos, in Velocity speed)
    {
        pos.Value += speed.Value * dt;
    }
}