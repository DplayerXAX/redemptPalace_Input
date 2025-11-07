using UnityEngine;

public class BurnTrail : ItemEffect
{
    public GameObject burningFloor;


    public override void OnMove(Vector3 p) 
    {
        if (burningFloor != null) 
        {
            GameObject.Instantiate(burningFloor,p,Quaternion.identity);
        }
    }
    
}
