using System;
using System.IO;
using Pulsar4X.Client.Interface.Widgets;
using Pulsar4X.Engine;
using Pulsar4X.SDL2UI;
using Pulsar4X.SDL2UI.ModFileEditing;

namespace Pulsar4X.Client.Interface.Menus;

public class SaveGame : PulsarGuiWindow
{
    private string _filePath = "Saves";
    private string _fileName = "savegame";

    private SaveGame() {}
    
    internal static SaveGame GetInstance()
    {
        if (!_uiState.LoadedWindows.ContainsKey(typeof(SaveGame)))
        {
            if(!Directory.Exists("Saves"))
            {Directory.CreateDirectory("Saves");}
            return new SaveGame();
        }
        return (SaveGame)_uiState.LoadedWindows[typeof(SaveGame)];
    }
    
    internal override void Display()
    {
        if (IsActive && FileDialog.DisplaySave(ref _filePath, ref _fileName, ref IsActive))
        {
            if (String.IsNullOrEmpty(_fileName) || String.IsNullOrEmpty(_filePath))
            {
                IsActive = false;
                return;
            }
            string gameJson = Game.Save(_uiState.Game);
            File.WriteAllText(Path.Combine(_filePath, _fileName), gameJson);
        }
    }
}