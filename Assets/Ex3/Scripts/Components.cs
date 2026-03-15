using Unity.Entities;
using Unity.Mathematics;

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
}

//Flags pour distinguer les proies des prédateurs (index 0) et la reproduction (index 1)
//
public struct Flags : IComponentData
{
    public byte Value;
}
