using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Vector3 mousePos;
    private Vector3 objectPos;
    public GameObject yourPrefab;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            mousePos = Input.mousePosition;
            mousePos.z = 2.0f;
            objectPos = Camera.main.ScreenToWorldPoint(mousePos);
            Instantiate(yourPrefab, objectPos, Quaternion.identity);
        }
    }
}
