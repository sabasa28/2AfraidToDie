using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeBuilder : MonoBehaviour
{
    public enum Shape3D
    {
        cube,
        cylinder,
        pyramid,
        hex_prism,
        oct_prism,
    }
    public List<Color> posibleColors;
    public List<Material> posibleSymbols;


}
