using Dqh.Domain.Battles;
using Dqh.Domain.Combatants.Adventurers;
using Dqh.Domain.Encounters;
using Dqh.Domain.Party;
using Dqh.Domain.Strategies;
using Dqh.Game;
using Dqh.Game.World;

var party = new Party();
party.Register(new Hero("Hero"));
party.Register(new Warrior("Warrior"));
party.Register(new Mage("Mage"));
party.Register(new Priest("Priest"));
party.Register(new Ranger("Ranger"));

var world = new GameWorld("overworld", party, new RandomEncounterGenerator(new RandomTargetStrategy()), new StandardBattleResolver());
var player = new PlayerMarker(world.Map, world.PlayerSpawn.Column, world.PlayerSpawn.Row);

var headless = args.Any(a => a.Equals("--headless", StringComparison.OrdinalIgnoreCase));

if (headless)
{
    Bootstrap.RunHeadless(world, player);
}
else
{
    Bootstrap.RunWindowed(world, player);
}
