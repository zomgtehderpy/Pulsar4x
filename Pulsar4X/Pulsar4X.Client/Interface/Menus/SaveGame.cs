using System;
using System.IO;
using Pulsar4X.Engine;
using Pulsar4X.SDL2UI;
using Pulsar4X.SDL2UI.ModFileEditing;

namespace Pulsar4X.Client.Interface.Menus;

public class SaveGame : PulsarGuiWindow
{
    private string _filePath = "";
    private string _fileName = "";

    private SaveGame() {}
    
    internal static SaveGame GetInstance()
    {
        if (!_uiState.LoadedWindows.ContainsKey(typeof(SaveGame)))
        {
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