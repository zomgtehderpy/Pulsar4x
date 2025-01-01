using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Pulsar4X.Engine.Auth;
using Pulsar4X.Modding;
using Pulsar4X.DataStructures;
using Pulsar4X.Blueprints;
using Pulsar4X.Interfaces;
using Pulsar4X.Engine.Orders;
using System.Runtime.CompilerServices;
using Pulsar4X.Events;
using Pulsar4X.Factions;
using Pulsar4X.Galaxy;
using Pulsar4X.Energy;
using Pulsar4X.Sensors;
[assembly: InternalsVisibleTo("Pulsar4X.Tests")]

namespace Pulsar4X.Engine
{
    public class Game
    {
        /// <summary>
        /// Entities like Suns, Planets, Asteroids etc are considered Neutral
        /// and should have their FactionOwnerID set equal to NeutralFactionID
        /// </summary>
        public static readonly int NeutralFactionId = -99;

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string CreatedOnGitHash { get; set; }

        [JsonProperty]
        public string LastSaveGitHash { get; set; }

        [JsonProperty]
        public MasterTimePulse TimePulse { get; internal set; }

        [JsonProperty]
        public SafeDictionary<string, ThemeBlueprint> Themes { get; internal set; }

        [JsonProperty]
        public SafeDictionary<string, GasBlueprint> AtmosphericGases { get; internal set; }

        [JsonProperty]
        public SafeDictionary<string, TechCategoryBlueprint> TechCategories { get; internal set; }

        [JsonProperty]
        public SystemGenSettingsBlueprint SystemGenSettings { get; internal set; }
        /// <summary>
        /// List of StarSystems currently in the game.
        /// </summary>
        [JsonProperty]
        public List<StarSystem> Systems { get; internal set; } = new ();

        [JsonProperty]
        public EntityManager GlobalManager { get; internal set; }

        [JsonProperty]
        internal readonly SafeDictionary<string, EntityManager> GlobalManagerDictionary = new ();

        [JsonIgnore]
        internal ProcessorManager ProcessorManager { get; private set; }

        [JsonProperty]
        public Player SpaceMaster { get; internal set; } = new Player("Space Master", "");

        [JsonProperty]
        public List<Player> Players { get; internal set; }= new List<Player>();

        [JsonIgnore]
        public IOrderHandler OrderHandler { get; internal set; }

        [JsonProperty]
        public Entity GameMasterFaction { get; internal set; }

        [JsonProperty]
        public GameSettings Settings { get; internal set; }

        [JsonProperty]
        public ModDataStore StartingGameData { get; private set; }

        [JsonProperty]
        internal GalaxyFactory GalaxyGen { get; private set; }

        [JsonProperty]
        public Dictionary<int, Entity> Factions { get; } = new ();

        // This is horribly named, it generates the ID's for the ICargoables NOT Entities
        [JsonProperty]
        private int EntityIDCounterValue => EntityIDCounter;

        [JsonProperty]
        internal int NextEntityID => EntityIDGenerator.NextId;

        private static int EntityIDCounter = 0;

        internal event EventHandler PostLoad;

        internal Random RNG => GlobalManager.RNG;

        public Game() { }

        public Game(NewGameSettings settings, ModDataStore modDataStore)
        {
            Name = settings.GameName;

            ApplyModData(modDataStore);
            ApplySettings(settings);

            if(SystemGenSettings == null) throw new ArgumentNullException("SystemGenSettings cannot be null");
            if(Settings == null) throw new ArgumentNullException("Settings cannot be null");

            TimePulse = new (this);
            TimePulse.Initialize(this);
            ProcessorManager = new ProcessorManager(this);
            OrderHandler = new StandAloneOrderHandler(this);
            GlobalManager = new EntityManager();
            GlobalManager.Initialize(this, settings.MasterSeed);
            GameMasterFaction = FactionFactory.CreateSpaceMasterFaction(this, SpaceMaster, "SpaceMaster Faction");
            GalaxyGen = new GalaxyFactory(SystemGenSettings);
        }

        public void ApplySettings(NewGameSettings settings)
        {
            Settings = settings;
        }

        public void ApplyModData(ModDataStore modDataStore)
        {
            StartingGameData = modDataStore;
            Themes = new SafeDictionary<string, ThemeBlueprint>(modDataStore.Themes);
            AtmosphericGases = new SafeDictionary<string, GasBlueprint>(modDataStore.AtmosphericGas);
            TechCategories = new SafeDictionary<string, TechCategoryBlueprint>(modDataStore.TechCategories);
            SystemGenSettings = modDataStore.SystemGenSettings["default-system-gen-settings"];
        }

        [CanBeNull]
        public Player? GetPlayerForToken(AuthenticationToken authToken)
        {
            if (SpaceMaster.IsTokenValid(authToken))
            {
                return SpaceMaster;
            }

            Player? foundPlayer = Players.Find(player => player.ID == authToken?.PlayerID);
            return foundPlayer?.IsTokenValid(authToken) != null ? foundPlayer : null;
        }

        public GasBlueprint GetGasBySymbol(string symbol)
        {
            return AtmosphericGases.Values.Where(g => g.ChemicalSymbol.Equals(symbol)).First();
        }

        public static int GetEntityID() => EntityIDCounter++;

        public static string Save(Game game)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings() {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new NonPublicResolver()
            };

            return JsonConvert.SerializeObject(game, settings);
        }

        public static Game Load(string json)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings() {
                Formatting = Formatting.Indented,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new NonPublicResolver(),
            };
            var loadedGame = JsonConvert.DeserializeObject<Game>(json, settings);

            loadedGame.TimePulse.Initialize(loadedGame);
            loadedGame.ProcessorManager = new ProcessorManager(loadedGame);
            loadedGame.OrderHandler = new StandAloneOrderHandler(loadedGame);
            loadedGame.GlobalManager.Initialize(loadedGame);

            foreach (var mgr in loadedGame.GlobalManagerDictionary)
            {
                mgr.Value.Initialize(loadedGame);
                mgr.Value.ManagerSubpulses.Initialize(mgr.Value, loadedGame.ProcessorManager);
            }

            // Hook up the event logs
            EventManager.Instance.Clear();
            foreach(var (id, faction) in loadedGame.Factions)
            {
                faction.GetDataBlob<FactionInfoDB>().EventLog.Subscribe();
            }
            loadedGame.GameMasterFaction.GetDataBlob<FactionInfoDB>().EventLog.Subscribe();

            // settings.Context = new StreamingContext(StreamingContextStates.All, loadedGame);
            // loadedGame.TimePulse = JsonConvert.DeserializeObject<MasterTimePulse>(JObject.Parse(json)["GameInfo"]["TimePulse"].ToString(), settings);
            // loadedGame.ProcessorManager = new ProcessorManager(loadedGame);
            // //loadedGame.ProcessorManager = JsonConvert.DeserializeObject<ProcessorManager>(JObject.Parse(json)["GameInfo"]["ProcessManager"].ToString(), settings);
            // loadedGame.GlobalManager = JsonConvert.DeserializeObject<EntityManager>(JObject.Parse(json)["GameInfo"]["GlobalManager"].ToString(), settings);
            // loadedGame.GlobalManager.Initialize(loadedGame);
            // loadedGame.GameMasterFaction = JsonConvert.DeserializeObject<Entity>(JObject.Parse(json)["GameInfo"]["GameMasterFaction"].ToString(), settings);
            // StandAloneOrderHandler currently doesn't need to be serialized
            // loadedGame.OrderHandler = JsonConvert.DeserializeObject<StandAloneOrderHandler>(JObject.Parse(json)["OrderHandler"].ToString(), settings);

            return loadedGame;
        }

        public void PostNewGameInitialization()
        {
            // There are few DB's that need to run the processor when the game begins
            foreach(var system in Systems)
            {
                var entitiesWithEnergyGen = system.GetAllEntitiesWithDataBlob<EnergyGenAbilityDB>();
                foreach (var entity in entitiesWithEnergyGen)
                {
                    ProcessorManager.GetInstanceProcessor(nameof(EnergyGenProcessor)).ProcessEntity(entity, TimePulse.GameGlobalDateTime);
                }

                var entitiesWithSensors = system.GetAllEntitiesWithDataBlob<SensorAbilityDB>();
                foreach (var entity in entitiesWithSensors)
                {
                    ProcessorManager.GetInstanceProcessor(nameof(SensorScan)).ProcessEntity(entity, TimePulse.GameGlobalDateTime);
                }
            }
        }
    }

    // public class GameConverter : JsonConverter
    // {
    //     public override bool CanConvert(Type objectType)
    //     {
    //         return objectType == typeof(Game);
    //     }

    //     public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //     {
    //         var jsonObject = JToken.Load(reader);

    //         List<ModManifest> modManifests = jsonObject["ModInfo"].ToObject<List<ModManifest>>(serializer);

    //         ModLoader modLoader = new ModLoader();
    //         ModDataStore modDataStore = new ModDataStore();

    //         foreach(var manifest in modManifests)
    //         {
    //             var modInfoFilePath = Path.Combine(manifest.ModDirectory, "modInfo.json");

    //             modLoader.LoadModManifest(modInfoFilePath, modDataStore);
    //         }

    //         var settings = jsonObject["Settings"].ToObject<NewGameSettings>(serializer);
    //         //var settings = new NewGameSettings();

    //         var game = jsonObject["GameInfo"].ToObject<Game>(serializer);
    //         game.ApplySettings(settings);
    //         game.ApplyModData(modDataStore);
    //         game.Initialize();

    //         return game;
    //     }

    //     public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //     {
    //         var game = (Game)value;
    //         var jsonObject = new JObject();

    //         var modInfo = JToken.FromObject(game.StartingGameData.ModManifests, serializer);
    //         var settings = JToken.FromObject(game.Settings, serializer);
    //         var gameInfo = new JObject();

    //         // For each property in the Game class, serialize it using the serializer.
    //         gameInfo["TimePulse"] = JToken.FromObject(game.TimePulse, serializer);

    //         // ProcessManager currently doesn't need to be serialized
    //         //gameInfo["ProcessManager"] = JToken.FromObject(game.ProcessorManager, serializer);

    //         // StandAloneOrderHandler currently doesn't need to be serialized
    //         // gameInfo["OrderHandler"] = JToken.FromObject(game.OrderHandler, serializer);

    //         gameInfo["GlobalManager"] = JToken.FromObject(game.GlobalManager, serializer);
    //         gameInfo["GameMasterFaction"] = JToken.FromObject(game.GameMasterFaction, serializer);
    //         gameInfo["Systems"] = JToken.FromObject(game.Systems, serializer);
    //         // TODO: Serialize the other properties.

    //         jsonObject["ModInfo"] = modInfo;
    //         jsonObject["Settings"] = settings;
    //         jsonObject["GameInfo"] = gameInfo;

    //         jsonObject.WriteTo(writer);
    //     }
    // }
}