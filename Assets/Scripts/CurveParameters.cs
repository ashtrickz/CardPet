using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AnimationCurveParams")]
public class CurveParameters : ScriptableObject
{
    public AnimationCurve positioning;
    public float positioningInfluence = .1f;
    public AnimationCurve rotation;
    public float rotationInfluence = 10f;
}
