using Dqh.Domain.Encounters;
using Dqh.Domain.Strategies;

namespace Dqh.Domain.Tests.Encounters;

public class RandomEncounterGeneratorTests
{
    [Fact]
    public void Generate_ReturnsBetweenOneAndThreeMonsters()
    {
        var generator = new RandomEncounterGenerator(new RandomTargetStrategy());

        for (var i = 0; i < 20; i++)
        {
            var encounter = generator.Generate();
            Assert.InRange(encounter.Monsters.Count, 1, 3);
        }
    }
}
