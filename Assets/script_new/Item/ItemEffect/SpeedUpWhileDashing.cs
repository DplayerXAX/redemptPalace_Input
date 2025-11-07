using UnityEngine;

public class SpeedUpWhileDashing : ItemEffect
{
    private float bonusSpeed = 3f; 

    public SpeedUpWhileDashing(Player player)
    {
        this.player = player;
    }
   
    public override void OnUpdate()
    {
        if (Input.GetKey(KeyCode.Space) && player.energy > 0)
        {
            player.rushingSpeed = player.rushingSpeed + bonusSpeed * Time.deltaTime;
        }
        else 
        {
            player.rushingSpeed = player._rushSpeed * player.rushMulti;
        }
    }

}
