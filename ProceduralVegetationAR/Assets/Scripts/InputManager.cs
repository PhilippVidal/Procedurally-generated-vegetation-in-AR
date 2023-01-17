using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

public class InputManager : MonoBehaviour, IMixedRealityGestureHandler
{
    GameManager gameManager;
    public bool isDesktop;

    private void Start()
    {
        gameManager = GameManager.INSTANCE;
    }

    private void Update()
    {
        if (isDesktop)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 direction = Camera.main.ScreenPointToRay(Input.mousePosition).direction;
                gameManager.InputRegistered(direction);
            }
        }
    }
    private void OnEnable()
    {
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityGestureHandler>(this);
    }

    public void OnGestureCompleted(InputEventData eventData)
    {
        Vector3 direction = Camera.main.transform.forward;
        gameManager.InputRegistered(direction);
    }

    public void OnGestureCanceled(InputEventData eventData)
    {
        //not implemented
    }

    public void OnGestureStarted(InputEventData eventData)
    {
        //not implemented
    }

    public void OnGestureUpdated(InputEventData eventData)
    {
        //not implemented
    }
}
