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


        endOfShapes
    }
    public List<Color> posibleColors;
    public List<Material> posibleSymbols;
    int shape3d;
    int shapeColor;
    int shapeSymbol;

    public void GetRandomShape(out int randShape3d, out int randShapeColor, out int randShapeSymbol)
    {
        int randomNumber = Random.Range(0, (int)Shape3D.endOfShapes);
        randShape3d = randomNumber;
        randomNumber = Random.Range(0, posibleColors.Count);
        randShapeColor = randomNumber;
        randomNumber = Random.Range(0, posibleSymbols.Count);
        randShapeSymbol = randomNumber;
    }

}
