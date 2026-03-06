using System;
using UnityEngine;

public class Clear : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("게임 클리어");
        }
    }
}
