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
    public int Exponent;
}

//Flags pour distinguer les proies des prédateurs et plantes (index 0-1) et la reproduction (index 7)
//0 : plantes, 1 : proies, 2:prédateurs
//index 6 is death flag?
public struct Flags : IComponentData
{
    public byte Value;
}
