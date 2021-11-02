using System.Collections;
using UnityEngine;
using System;

public class DeliveryMachine : MonoBehaviour
{
    int shapeCorrect3dShape;
    int shapeCorrectColor;
    int shapeCorrectSymbol;
    [SerializeField] CodeBar codebarPrefab;
    int correctCode;

    [SerializeField] Drone dronePrefab;

    public bool test;
    [SerializeField] CreatedShape shapeTest;

    private void Update()
    {
        if (test)
        {
            InsertShape(shapeTest);
            test = false;
        }
    }

    public void SetShapePuzzleVars(int newShapeCorrect3dShape, int newShapeCorrectColor, int newShapeCorrectSymbol, out int code)
    {
        shapeCorrect3dShape = newShapeCorrect3dShape;
        shapeCorrectColor = newShapeCorrectColor;
        shapeCorrectSymbol = newShapeCorrectSymbol;
        correctCode = GenerateCode(shapeCorrect3dShape,shapeCorrectColor,shapeCorrectSymbol);
        code = correctCode;
    }

    
    public void InsertShape(CreatedShape shapeInserted)
    {
        if (shapeInserted.colorUsed == shapeCorrectColor &&
            shapeInserted.shapeUsed == shapeCorrect3dShape &&
            shapeInserted.symbolUsed == shapeCorrectSymbol)
        {
            InstantiateCodeBarAndSendDrone(shapeInserted);
        }
    }

    void InstantiateCodeBarAndSendDrone(CreatedShape shape)
    {
        CodeBar codeBar = Instantiate(codebarPrefab, transform.position, Quaternion.identity, transform);
        codeBar.SetCode(GenerateCode(shape.shapeUsed, shape.colorUsed, shape.symbolUsed));

        Drone drone = Instantiate(dronePrefab, transform.position, Quaternion.identity, transform);
        drone.SetStartingMovement(Drone.MovementToDo.riseAndLeave);

        Destroy(shape.gameObject);
    }

    int GenerateCode(int shape3D, int shapeColor, int shapeSymbol)
    {
        int arbitraryConst = 420; //si tuviesemos las diferentes seeds de escenarios usariamos esa seed en vez de este numero, para que el numero cambie cada partida
                                  //siempre tiene que ser un numero de 3 digitos preferentemente menor a 900

        shape3D = shape3D * 3 + arbitraryConst;
        shapeColor = shapeColor * 3 + arbitraryConst;
        shapeSymbol = shapeSymbol * 3 + arbitraryConst;

        string asString = shape3D.ToString() + shapeColor.ToString() + shapeSymbol.ToString();

        Debug.Log(asString);

        char[] asCharArray = asString.ToCharArray();

        for (int i = 0; i < Mathf.Floor(asCharArray.Length/2.0f); i++)
        {
            char temp = asCharArray[i];
            asCharArray[i] = asCharArray[asCharArray.Length - 1 -i];
            asCharArray[asCharArray.Length - 1 - i] = temp;
        }
        asString = new string(asCharArray);
        Debug.Log(asString);

        for (int i = 0; i < Mathf.Floor(asCharArray.Length-1); i+=2)
        {
            char temp = asCharArray[i];
            asCharArray[i] = asCharArray[i+1];
            asCharArray[i+1] = temp;
        }

        asString = new string(asCharArray);
        int resultantCode = int.Parse(asString);
        Debug.Log(resultantCode);
        return resultantCode;
    }
}
