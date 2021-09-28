using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaruScript : MonoBehaviour
{
    private Renderer _Renderer;

    void Start()
    {
        _Renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (IsVisible())
        {
            Debug.Log("maru表示中");
        }
        else
        {
            Debug.Log("maru非表示中");
        }
    }

    public bool IsVisible()
    {
        return _Renderer.isVisible;
    }
}
