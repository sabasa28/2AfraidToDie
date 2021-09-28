using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIPhone : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI[] phoneNumberText;
    static int cantNumsInPhoneNum = 9;
    int[] phoneNumber = new int[cantNumsInPhoneNum];
    int currentNumberIndex = 0;
    [SerializeField] Color selectedNumColor;
    [SerializeField] Color notSelectedNumColor;
    public Phone phone;
    
    void Start()
    {
        foreach (int num in phoneNumber)
        {
            phoneNumber[num] = 0;
        }
        for (int i = 0; i < phoneNumberText.Length && i < phoneNumber.Length; i++)
        {
            phoneNumberText[i].text = phoneNumber[i].ToString();
        }
        SetCurrentNumberIndex(0);
    }

    public void SetCurrentNumberAndAdvance(TextMeshProUGUI buttonText)
    {
        if (currentNumberIndex == cantNumsInPhoneNum) return;
        int num;
        bool parsed = int.TryParse(buttonText.text, out num);

        if (parsed) SetCurrentNumber(num);
        else Debug.Log("Cant be parsed");

        SetCurrentNumberIndex(currentNumberIndex + 1);
    }
    void SetCurrentNumber(int num)
    {
        if (currentNumberIndex == cantNumsInPhoneNum) return;
        phoneNumber[currentNumberIndex] = num;
        phoneNumberText[currentNumberIndex].text = num.ToString();
    }

    void SetCurrentNumberIndex(int num)
    {
        SetCurrentPhoneNumberTextColor(false);
        currentNumberIndex = num;
        if (currentNumberIndex < 0) currentNumberIndex = 0;
        if (currentNumberIndex > cantNumsInPhoneNum) currentNumberIndex = cantNumsInPhoneNum;
        SetCurrentPhoneNumberTextColor(true);
    }

    void SetCurrentPhoneNumberTextColor(bool selected)
    {
        if (currentNumberIndex >= cantNumsInPhoneNum || currentNumberIndex < 0) return;
        if (selected) phoneNumberText[currentNumberIndex].color = selectedNumColor;
        else phoneNumberText[currentNumberIndex].color = notSelectedNumColor;
    }

    public void ReturnAndEraseNumber()
    {
        SetCurrentNumberIndex(currentNumberIndex - 1);
        SetCurrentNumber(0);
    }

    public void CheckPhoneNumAndCloseUI()
    {
        //check num
        phone.OnStopUsingPhone();
    }
}
