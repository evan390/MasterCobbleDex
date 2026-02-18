using System.Configuration;
using System.Data;
using System.Windows;

namespace MasterCobbleDexWorkingNew
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public class Pokemon
        {
            public string Name { get; set; }
            public string Form { get; set; }
            public string Type1 { get; set; }
            public string Type2 { get; set; }
            public double MaleRatio { get; set; }
            public List<Ability> Abilities { get; set; }
            public List<Move> Moves { get; set; }
            public List<string> EggGroups { get; set; }
            public int HP { get; set; }
            public int Attack { get; set; }
            public int Defense { get; set; }
            public int SpecialAttack { get; set; }
            public int SpecialDefense { get; set; }
            public int Speed { get; set; }
            public EVYield EVS { get; set; }
            public List<Drop> Drops { get; set; }
            public PreEvolution PreEvo { get; set; }
            public List<Evolution> Evolutions { get; set; }
            public List<Pokemon> Forms { get; set; }
            public int FormSearch { get; set; }
            public List<Spawn> Spawns { get; set; }

        }
        public class PreEvolution
        {
            public string PreEvolutionName { get; set; }
            public string PreEvolutionForm { get; set; }
        }
        public class Ability
        {
            public string AbilityName { get; set; }
            public bool Hidden { get; set; }
        }
        public class Move
        {
            public string MoveName { get; set; }
            public string MoveType { get; set; }
        }
        public class EVYield
        {
            public int HP { get; set; }
            public int Attack { get; set; }
            public int Defense { get; set; }
            public int SpecialAttack { get; set; }
            public int SpecialDefense { get; set; }
            public int Speed { get; set; }
        }
        public class Drop
        {
            public string Item { get; set; }
            public string Location { get; set; }
            public string QuantityRange { get; set; }
            public double Percent { get; set; }
        }
        //requirements and required-context
        public class Evolution
        {
            public string EvolveInto { get; set; }
            public int Level { get; set; }
            public List<EvoItem> Items { get; set; }//List because of Gimmighoul
            public bool Trade { get; set; }
            public int Friendship { get; set; }
            public string? Gender { get; set; } //properties
            public string? Time { get; set; }
            public List<string> learnedMoveType { get; set; } //sylveon
            public List<string> learnedMove { get; set; } //piloswine
            public string? Biome { get; set; }//regionals
            public string? HeldItem { get; set; }
            public int BlocksTravelled { get; set; }//Pawmo
            public List<EvoStatCompare> StatCompare { get; set; }//Tyrogue
            public List<EvoUsedMove> UsedMove { get; set; }//Primeape
            public List<EvoDefeatRequirement> DefeatRequirement { get; set; }//Bisharp
            public bool isShedinja { get; set; }//Shedinja
            public bool isWurmple { get; set; }//Cascoon & Silcoon
            public int Damaged { get; set; } //Galarian Yamask
            public int Recoil { get; set; } //White Striped Basculin
            public int Crits { get; set; } //Galarian Farfetch'd
            public List<string> PartyMember { get; set; }//Mantyke
            public List<string> PartyMemberType { get; set; }//Pancham
            public string? Weather { get; set; }//Sliggoo
        }
        public class EvoItem
        {
            public string Item { get; set; }
            public int ItemMin { get; set; }
            public int ItemMax { get; set; }
        }
        public class EvoDefeatRequirement
        {
            public string Pokemon { get; set; }
            public int Amount { get; set; }
        }
        public class EvoStatCompare
        {
            public string StatOne { get; set; }
            public string StatOperator { get; set; }
            public string StatTwo { get; set; }
        }
        public class EvoUsedMove
        {
            public string UsedMove { get; set; }
            public int UsedMoveTimes { get; set; }
        }
        public class Spawn
        {
            public string ID { get; set; }
            public string Pokemon { get; set; }
            public List<string> Presets { get; set; }
            public string SpawnablePositionType { get; set; }
            public string Bucket { get; set; }
            public Condition Condition { get; set; }
            public Condition AntiCondition { get; set; }
        }
        public class Condition
        {
            public bool? CanSeeSky { get; set; }
            public int? MinSkyLight { get; set; }
            public int? MaxSkyLight { get; set; }
            public int? MinY{ get; set; }
            public int? MaxY { get; set; }
            public string TimeRange { get; set; }
            public string MoonPhase { get; set; }
            public bool? IsRaining { get; set; }
            public List<string> NeededNearbyBlocks { get; set; }
            public List<string> NeededBaseBlocks { get; set; }
            public List<string> Biomes { get; set; }
            public List<string> Structures { get; set; }
            public bool? IsSlimeChunk { get; set; }
            public int? MinLureLevel { get; set; }
            public int? MaxLureLevel { get; set; }
            public string RodType { get; set; }
            public string Bait { get; set; }
        }

    }

}
