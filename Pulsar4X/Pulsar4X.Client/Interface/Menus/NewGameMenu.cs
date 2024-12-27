using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using ImGuiSDL2CS;
using Pulsar4X.Client.Interface.Widgets;
using Pulsar4X.Client.State;
using Pulsar4X.Engine;
using Pulsar4X.Modding;

namespace Pulsar4X.SDL2UI;

enum Page
{
    SelectMods,
    SelectDetails
}

static class Helper
{
    public static byte[] ToByteArray(this string str)
    {
        return System.Text.Encoding.UTF8.GetBytes(str);
    }
}

public class NewGameMenu : PulsarGuiWindow
{
    Page _currentPage = Page.SelectMods;
    ModLoader _modLoader = new ModLoader();
    ModDataStore _modDataStore = new ModDataStore();
    string _selectedSpeciesId = "";
    string _selectedThemeId = "";
    string _selectedSystemId = "";
    string _selectedBodyId = "";
    string _selectedColonyId = "";

    enum gameType { Nethost, Standalone }
    int _gameTypeButtonGrp = 0;
    gameType _selectedGameType = gameType.Standalone;
    byte[] _netPortInputBuffer = new byte[8];
    string _netPortString { get { return System.Text.Encoding.UTF8.GetString(_netPortInputBuffer); } }
    int _maxSystems = 5;


    byte[] _nameInputBuffer = ImGuiSDL2CSHelper.BytesFromString("Test Game", 16);
    byte[] _factionInputBuffer = ImGuiSDL2CSHelper.BytesFromString("UEF", 16);
    byte[] _passInputBuffer = ImGuiSDL2CSHelper.BytesFromString("", 16);

    byte[] _smPassInputbuffer = ImGuiSDL2CSHelper.BytesFromString("", 16);

    int _masterSeed = 12345678;
    private NewGameMenu()
    {

    }
    internal static NewGameMenu GetInstance()
    {
        if (!_uiState.LoadedWindows.ContainsKey(typeof(NewGameMenu)))
        {
            return new NewGameMenu();
        }
        return (NewGameMenu)_uiState.LoadedWindows[typeof(NewGameMenu)];
    }

    NewGameSettings gameSettings = new NewGameSettings();

    internal override void Display()
    {
        if(!IsActive) return;

        if (Window.Begin("New Game Setup", ref IsActive, _flags))
        {

            switch(_currentPage)
            {
                case Page.SelectMods:
                    DisplayModsPage();
                    break;
                case Page.SelectDetails:
                    DisplayDetailsPage();
                    break;
            }
            Window.End();
        }
    }

    private void DisplayModsPage()
    {
        ImGui.InputText("Game Name", _nameInputBuffer, 16);
        ImGui.InputText("SM Pass", _smPassInputbuffer, 16);
        ImGui.InputText("Password", _passInputBuffer, 16);

        //ImGui.InputInt("Max Systems", ref _maxSystems);
        ImGui.InputInt("Master Seed:", ref _masterSeed);

        if(ImGui.CollapsingHeader("Mod List", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.SpanAvailWidth))
        {
            if(ImGui.BeginTable("ModsList", 3))
            {
                ImGui.TableNextColumn();
                ImGui.TableHeader("Mod Name");
                ImGui.TableNextColumn();
                ImGui.TableHeader("Version");
                ImGui.TableNextColumn();
                ImGui.TableHeader("Enable?");

                foreach(var modMetadata in ModsState.AvailableMods)
                {
                    ImGui.TableNextColumn();
                    ImGui.Text(modMetadata.Mod.ModName);
                    ImGui.TableNextColumn();
                    ImGui.Text(modMetadata.Mod.Version);
                    var isEnabled = ModsState.IsModEnabled[modMetadata.Mod.ModName];
                    ImGui.TableNextColumn();
                    if(ImGui.Checkbox("###" + modMetadata.Mod.ModName + "-checkbox", ref isEnabled))
                    {
                        ModsState.IsModEnabled[modMetadata.Mod.ModName] = !ModsState.IsModEnabled[modMetadata.Mod.ModName];
                    }
                }

                ImGui.EndTable();
            }
        }



        // if (ImGui.RadioButton("Host Network Game", ref _gameTypeButtonGrp, 1))
        //     _selectedGameType = gameType.Nethost;
        // if (ImGui.RadioButton("Start Standalone Game", ref _gameTypeButtonGrp, 0))
        //     _selectedGameType = gameType.Standalone;
        // if (_selectedGameType == gameType.Nethost)
        //     ImGui.InputText("Network Port", _netPortInputBuffer, 8);
        ImGui.Separator();
        if (ImGui.Button("Next") || _uiState.debugnewgame)
        {
            _uiState.debugnewgame = false;
            LoadEnabledMods();
            _selectedSpeciesId = _modDataStore.Species.First().Key;
            _selectedThemeId = _modDataStore.Themes.First().Key;
            _selectedSystemId = _modDataStore.Systems.First().Key;
            _selectedColonyId = _modDataStore.Colonies.First().Key;
            ResetSelectedBodyId();

            _currentPage = Page.SelectDetails;
        }
    }

    private void DisplayDetailsPage()
    {
        ImGui.Text("Game Options:");
        ImGui.InputText("Faction Name", _factionInputBuffer, 16);

        var display = _modDataStore.Species.TryGetValue(_selectedSpeciesId, out var speciesBlueprint) ? speciesBlueprint.Name : "";
        if(ImGui.BeginCombo("Select Species", display))
        {
            foreach(var (id, species) in _modDataStore.Species)
            {
                if(!species.Playable) continue;

                if(ImGui.Selectable(species.Name, _selectedSpeciesId.Equals(id)))
                {
                    _selectedSpeciesId = id;
                }
            }
            ImGui.EndCombo();
        }

        display = _modDataStore.Themes.TryGetValue(_selectedThemeId, out var themeBlueprint) ? themeBlueprint.Name : "";
        if(ImGui.BeginCombo("Select Theme", display))
        {
            foreach(var (id, theme) in _modDataStore.Themes)
            {
                if(ImGui.Selectable(theme.Name, _selectedThemeId.Equals(id)))
                {
                    _selectedThemeId = id;
                }
            }
            ImGui.EndCombo();
        }

        display = _modDataStore.Colonies.TryGetValue(_selectedColonyId, out var colonyBlueprint) ? colonyBlueprint.Name : "";
        if(ImGui.BeginCombo("Starting Colony Configuration", display))
        {
            foreach(var (id, colony) in _modDataStore.Colonies)
            {
                if(ImGui.Selectable(colony.Name, _selectedColonyId.Equals(id)))
                {
                    _selectedColonyId = id;
                }
            }
            ImGui.EndCombo();
        }

        display = _modDataStore.Systems.TryGetValue(_selectedSystemId, out var systemBlueprint) ? systemBlueprint.Name : _selectedSystemId.Equals("random") ? "Randomly Generated" : "";
        if(ImGui.BeginCombo("Select Starting System", display))
        {
            foreach(var (id, system) in _modDataStore.Systems)
            {
                if(ImGui.Selectable(system.Name, _selectedSystemId.Equals(id)))
                {
                    _selectedSystemId = id;
                    ResetSelectedBodyId();
                }
            }
            ImGui.Separator();
            if(ImGui.Selectable("Randomly Generated", _selectedSystemId.Equals("random")))
            {
                _selectedSystemId = "random";
            }
            ImGui.EndCombo();
        }

        if(!_selectedSystemId.Equals("random"))
        {
            display = _modDataStore.SystemBodies.TryGetValue(_selectedBodyId, out var bodyBlueprint) ? bodyBlueprint.Name : "";
            if(ImGui.BeginCombo("Select Starting Location", display))
            {
                foreach(var (id, body) in _modDataStore.SystemBodies.Where(kvp => _modDataStore.Systems[_selectedSystemId].Bodies.Contains(kvp.Key)))
                {
                    if(!body.CanStartHere) continue;
                    if(ImGui.Selectable(body.Name, _selectedBodyId.Equals(id)))
                    {
                        _selectedBodyId = id;
                    }
                }
                ImGui.EndCombo();
            }
        }

        ImGui.Separator();
        if (ImGui.Button("Back"))
        {
            _currentPage = Page.SelectMods;
        }
        ImGui.SameLine();
        if (ImGui.Button("Create Game!"))
        {
            CreateNewGame();
        }
    }

    private void LoadEnabledMods()
    {
        List<string> enabledMods = new ();

        foreach(var modMetadata in ModsState.AvailableMods)
        {
            if(ModsState.IsModEnabled[modMetadata.Mod.ModName])
            {
                enabledMods.Add(modMetadata.Path);
            }
        }

        // FIXME: this is show some error in the UI if no mods are selected
        if(enabledMods.Count == 0)
            return;

        _modLoader.LoadedMods.Clear();
        _modDataStore = new ModDataStore();
        foreach (var mod in enabledMods)
        {
            _modLoader.LoadModManifest(mod, _modDataStore);
        }
    }

    void CreateNewGame()
    {
        gameSettings = new NewGameSettings
        {
            GameName = ImGuiSDL2CSHelper.StringFromBytes(_nameInputBuffer),
            MaxSystems = _maxSystems,
            SMPassword = ImGuiSDL2CSHelper.StringFromBytes(_smPassInputbuffer),
            CreatePlayerFaction = true,
            DefaultFactionName = ImGuiSDL2CSHelper.StringFromBytes(_factionInputBuffer),
            DefaultPlayerPassword = ImGuiSDL2CSHelper.StringFromBytes(_passInputBuffer),
            DefaultSolStart = true,
            MasterSeed = _masterSeed
        };

        Pulsar4X.Engine.Game game = GameFactory.CreateGame(_modDataStore, gameSettings);

        // TODO: need to add the implementation for a random start
        // TODO: need to find a way to handle this via the mods instead of loading it here
        var (newGameFaction, systemId) = Pulsar4X.Engine.DefaultStartFactory.LoadFromJson(game, "Data/basemod/defaultStart.json");

        if(newGameFaction == null) return;

        _uiState.Game = game;
        _uiState.SetFaction(newGameFaction, true);
        _uiState.SetActiveSystem(systemId);

        DebugWindow.GetInstance().SetGameEvents();
        IsActive = false;
        _currentPage = Page.SelectMods; // reset the page
        //we initialize window instances so that they get always displayed and automatically open after new game is created.
        TimeControl.GetInstance().SetActive();
        ToolBarWindow.GetInstance().SetActive();
        Selector.GetInstance().SetActive();
        //EntityUIWindowSelector.GetInstance().SetActive();
        //EntityInfoPanel.GetInstance().SetActive();
    }

    private void ResetSelectedBodyId()
    {
        if(_modDataStore.Systems.TryGetValue(_selectedSystemId, out var systemBlueprint))
        {
            var candidates = _modDataStore.SystemBodies.Where(kvp => kvp.Value.CanStartHere && systemBlueprint.Bodies.Contains(kvp.Key));
            _selectedBodyId = candidates.Any() ? candidates.First().Key : "";
        }
        else
        {
            _selectedBodyId = "";
        }


    }
}