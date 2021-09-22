using System.Collections;
using UnityEngine;

public class DeliveryMachine : MonoBehaviour
{
    int shapeCorrect3dShape;
    int shapeCorrectColor;
    int shapeCorrectSymbol;
    [SerializeField] CodeBar codebarPrefab;

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

    public void UpdateShapeCorrectFeatures(int newShapeCorrectColor, int newShapeCorrect3dShape, int newShapeCorrectSymbol)
    {
        shapeCorrect3dShape = newShapeCorrect3dShape;
        shapeCorrectColor = newShapeCorrectColor;
        shapeCorrectSymbol = newShapeCorrectSymbol;
    }

    
    public void InsertShape(CreatedShape shapeInserted)
    {
        if (shapeInserted.colorUsed == shapeCorrectColor &&
            shapeInserted.shapeUsed == shapeCorrect3dShape &&
            shapeInserted.symbolUsed == shapeCorrectSymbol)
        {
            InstanciateCodeBarAndSendDrone();
        }
    }

    void InstanciateCodeBarAndSendDrone()
    {
        CodeBar codeBar = Instantiate<CodeBar>(codebarPrefab, transform.position, Quaternion.identity, transform);
        codeBar.code = GenerateCode(shapeCorrectColor, shapeCorrect3dShape, shapeCorrectSymbol);

        Drone drone = Instantiate<Drone>(dronePrefab, transform.position, Quaternion.identity, transform);
        drone.SetStartingMovement(Drone.MovementToDo.riseAndLeave);
    }

    int GenerateCode(int a, int b, int c)
    {
        int arbitraryConst = 420; //si tuviesemos las diferentes seeds de escenarios usariamos esa seed en vez de este numero, para que el numero cambie cada partida
                                  //siempre tiene que ser un numero de 3 digitos preferentemente menor a 900

        a = a * 3 + arbitraryConst;
        b = b * 3 + arbitraryConst;
        c = c * 3 + arbitraryConst;

        string asString = a.ToString() + b.ToString() + c.ToString();

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
