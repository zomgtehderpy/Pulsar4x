using System.Collections.Generic;

namespace Pulsar4X.Blueprints;

public class ColonyBlueprint
{
    public struct StartingItemBlueprint
    {
        public string Id { get; set; }
        public uint Amount { get; set; }
        public string? Type { get; set; }
    }

    public List<StartingItemBlueprint> Installations { get; set; }
    public List<StartingItemBlueprint> Cargo { get; set; }
}