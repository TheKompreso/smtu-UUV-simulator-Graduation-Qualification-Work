using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveEngine : MonoBehaviour
{
    [SerializeField] private GameObject Vehicle;
    public static MoveEngine Instance;
    private void Awake()
    {
        Instance = this;
    }

    public Vector2 VerticalSpeedLimit;
    public Vector2 ForwardSpeedLimit;
    public Vector2 SideSpeedLimit;
    public static float immersionDepth;

    private void FixedUpdate()
    {
        if (Vehicle.transform.position.y <= -30.0f)
        {
            if (Vehicle.transform.position.y <= immersionDepth)
                VerticalSpeedLimit = new Vector2(0.05f, 1.0f);
            else if (Vehicle.transform.position.y - 7 <= immersionDepth)
                VerticalSpeedLimit = new Vector2(-0.05f, 1.0f);
            else if (Vehicle.transform.position.y - 14 <= immersionDepth)
                VerticalSpeedLimit = new Vector2(-0.35f, 1.0f);
            else if (Vehicle.transform.position.y - 20 <= immersionDepth)
                VerticalSpeedLimit = new Vector2(-0.65f, 1.0f);
            else
                VerticalSpeedLimit = new Vector2(-1.00f, 1.00f);
        }
        else if (Vehicle.transform.position.y > -13.0f)  
            VerticalSpeedLimit = new Vector2(-1.00f, -0.05f); 
        else if (Vehicle.transform.position.y > -14.0f)  
            VerticalSpeedLimit = new Vector2(-1.00f, 0.0f); 
        else if (Vehicle.transform.position.y > -18.0f)  
            VerticalSpeedLimit = new Vector2(-1.00f, 0.15f); 
        else if (Vehicle.transform.position.y > -23.0f)  
            VerticalSpeedLimit = new Vector2(-1.00f, 0.35f); 
        else if (Vehicle.transform.position.y > -27.0f)  
            VerticalSpeedLimit = new Vector2(-1.00f, 0.65f);
        else
            VerticalSpeedLimit = new Vector2(-1.00f, 1.00f);

        ForwardSpeedLimit = new Vector2(-1.00f, 1.00f);
        SideSpeedLimit = new Vector2(-1.00f, 1.00f);
    }
}
