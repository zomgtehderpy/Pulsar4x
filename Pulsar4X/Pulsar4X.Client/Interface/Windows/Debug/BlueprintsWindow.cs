using ImGuiNET;
using Pulsar4X.Client.Interface.Widgets;
using Pulsar4X.SDL2UI;

namespace Pulsar4X.Client.Interface.Windows;

public class BlueprintsWindow : PulsarGuiWindow
{
    public static BlueprintsWindow GetInstance()
    {
        BlueprintsWindow instance;
        if (!_uiState.LoadedWindows.ContainsKey(typeof(BlueprintsWindow)))
        {
            instance = new BlueprintsWindow();
        }
        else
        {
            instance = (BlueprintsWindow)_uiState.LoadedWindows[typeof(BlueprintsWindow)];
        }

        return instance;
    }

    internal override void Display()
    {
        if(!IsActive) return;

        if(Window.Begin("Blueprints Window"))
        {
            if(ImGui.BeginTabBar("Blueprint Tabs"))
            {
                if(ImGui.BeginTabItem("Armor"))
                {
                    foreach(var template in _uiState.Game.StartingGameData.Armor)
                    {
                        ImGui.Text(template.Key);
                    }
                    ImGui.EndTabItem();
                }

                if(ImGui.BeginTabItem("Cargo Type"))
                {
                    foreach(var template in _uiState.Game.StartingGameData.CargoTypes)
                    {
                        ImGui.Text(template.Key);
                    }
                    ImGui.EndTabItem();
                }

                if(ImGui.BeginTabItem("Component Templates"))
                {
                    foreach(var template in _uiState.Game.StartingGameData.ComponentTemplates)
                    {
                        ImGui.Text(template.Key);
                    }
                    ImGui.EndTabItem();
                }

                if(ImGui.BeginTabItem("Default Items"))
                {
                    foreach(var template in _uiState.Game.StartingGameData.DefaultItems)
                    {
                        ImGui.Text(template.Key);
                    }
                    ImGui.EndTabItem();
                }

                if(ImGui.BeginTabItem("Gas"))
                {
                    foreach(var template in _uiState.Game.StartingGameData.AtmosphericGas)
                    {
                        ImGui.Text(template.Key);
                    }
                    ImGui.EndTabItem();
                }

                if(ImGui.BeginTabItem("Industry Type"))
                {
                    foreach(var template in _uiState.Game.StartingGameData.IndustryTypes)
                    {
                        ImGui.Text(template.Key);
                    }
                    ImGui.EndTabItem();
                }

                if(ImGui.BeginTabItem("Minerals"))
                {
                    foreach(var template in _uiState.Game.StartingGameData.Minerals)
                    {
                        ImGui.Text(template.Key);
                    }
                    ImGui.EndTabItem();
                }

                if(ImGui.BeginTabItem("Processed Materials"))
                {
                    foreach(var template in _uiState.Game.StartingGameData.ProcessedMaterials)
                    {
                        ImGui.Text(template.Key);
                    }
                    ImGui.EndTabItem();
                }

                if(ImGui.BeginTabItem("System Gen Settings"))
                {
                    foreach(var template in _uiState.Game.StartingGameData.SystemGenSettings)
                    {
                        ImGui.Text(template.Key);
                    }
                    ImGui.EndTabItem();
                }

                if(ImGui.BeginTabItem("Techs"))
                {
                    foreach(var template in _uiState.Game.StartingGameData.Techs)
                    {
                        ImGui.Text(template.Key);
                    }
                    ImGui.EndTabItem();
                }

                if(ImGui.BeginTabItem("Tech Categories"))
                {
                    foreach(var template in _uiState.Game.StartingGameData.TechCategories)
                    {
                        ImGui.Text(template.Key);
                    }
                    ImGui.EndTabItem();
                }

                if(ImGui.BeginTabItem("Themes"))
                {
                    foreach(var template in _uiState.Game.StartingGameData.Themes)
                    {
                        ImGui.Text(template.Key);
                    }
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            Window.End();
        }
    }
}