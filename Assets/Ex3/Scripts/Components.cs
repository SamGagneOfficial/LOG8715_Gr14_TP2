using Unity.Entities;
using Unity.Mathematics;

public struct Ex3ConfigComponent : IComponentData
{
    public int plantCount;
    public int preyCount;
    public int predatorCount;
    public int gridSize;

    public float PreySpeed;
    public float PredatorSpeed;
    public float TouchingDistance;
}

//Position 2D
public struct Position : IComponentData
{
    public float2 Value;
}

//Vitesse de déplacement 2D pour les proies et prédateurs
public struct Velocity : IComponentData
{
    public float2 Value;
}

//Taille des plantes
public struct Size : IComponentData
{
    public float Value;
}

//Temps de vie est dégradation pour chaque entité
public struct Timer : IComponentData
{
    public float Value;
    public float DecaySpeed;
    public int Exponent;
}

public struct PlantTag : IComponentData {}
public struct PreyTag : IComponentData {}
public struct PredatorTag : IComponentData {}
public struct ReproductionTag : IComponentData {}
