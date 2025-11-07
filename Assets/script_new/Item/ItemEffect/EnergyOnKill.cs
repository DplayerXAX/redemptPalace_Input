public class EnergyOnKill : ItemEffect
{

    private int energyRecover = 10;

    public EnergyOnKill(Player player)
    {
        this.player = player;
    }
    public override void OnKill()
    {
        player.energy += energyRecover;

    }
}

