using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using ImGuiSDL2CS;
using Pulsar4X.ECSLib;
using Pulsar4X.ECSLib.ComponentFeatureSets.Missiles;

namespace Pulsar4X.SDL2UI
{
    public class OrdinanceDesignUI : PulsarGuiWindow
    {
        private byte[] _designName =  ImGuiSDL2CSHelper.BytesFromString("foo", 32);
        
        private OrdnanceDesign[] _currentDesigns;
        private string[] _currentDesignNames;
        private int _currentDesignsSelectedIndex = 0;

        private float _missileSize = 1;
        
        private string[] _payload;
        private int _payloadSelectedIndex = 0;
        private float _payloadPercent;
        
        private string[] _electronicsPackage;
        private int _electronicsSelectedIndex = 0;
        
        private float _fuelPercent;       
        
        private string[] _engineDesigns;
        private int _engineSelectedIndex = 0;
        private int _engineCount;

        private OrdinanceDesignUI()
        {
            HardRefresh();
        }

        public static OrdinanceDesignUI GetInstance()
        {
            OrdinanceDesignUI thisitem;
            if (!_state.LoadedWindows.ContainsKey(typeof(OrdinanceDesignUI)))
            {
                thisitem = new OrdinanceDesignUI();
            }
            else
                thisitem = (OrdinanceDesignUI)_state.LoadedWindows[typeof(OrdinanceDesignUI)];
            
            return thisitem;
            
        }

        public void HardRefresh()
        {
            var designs = _state.Faction.GetDataBlob<FactionInfoDB>().MissileDesigns;
            var componentDesigns = _state.Faction.GetDataBlob<FactionInfoDB>().ComponentDesigns;

            foreach (var des in componentDesigns)
            {

            }
            
            _currentDesigns = designs.Values.ToArray();
            _currentDesignNames = new string[_currentDesigns.Length];
            int i = 0;
            foreach (var mdesign in _currentDesigns)
            {
                _currentDesignNames[i] = mdesign.Name;
                i++;
            }
            


            List<ComponentDesign> missilePayload = new List<ComponentDesign>();
            List<ComponentDesign> missileSensors = new List<ComponentDesign>();
            List<ComponentDesign> missileEngines = new List<ComponentDesign>();
            foreach (ComponentDesign cdes in componentDesigns.Values)
            {
                if ((cdes.ComponentMountType & ComponentMountType.Missile) == ComponentMountType.Missile)
                {
                    if (cdes.AttributesByType.ContainsKey(typeof(OrdnancePayloadAtb)))
                    {
                        missilePayload.Add(cdes);
                    }
                    if (cdes.AttributesByType.ContainsKey(typeof(SensorReceverAtbDB)))
                    {
                        missileSensors.Add(cdes);
                    }

                    if (cdes.AttributesByType.ContainsKey(typeof(NewtonionThrustAtb)))
                    {
                        missileEngines.Add(cdes);
                    }

                }
            }
            
            _payload = new string[missilePayload.Count];
            i = 0;
            foreach (var des in missilePayload)
            {
                _payload[i] = des.Name;
            }
            _electronicsPackage = new string[missileSensors.Count];
            i = 0;
            foreach (var des in missileSensors)
            {
                _electronicsPackage[i] = des.Name;
            }
            _engineDesigns = new string[missileEngines.Count];
            i = 0;
            foreach (var des in missileEngines)
            {
                _engineDesigns[i] = des.Name;
            }

            

        }

        internal override void Display()
        {
            if (IsActive && ImGui.Begin("Ordinace Design"))
            {
                ImGui.Combo("Current Designs", ref _payloadSelectedIndex, _currentDesignNames, _currentDesignNames.Length);

                ImGui.SliderFloat("Missile Size", ref _missileSize, 1, 256);
                
                ImGui.Combo("Payload type", ref _payloadSelectedIndex, _payload, _payload.Length);
                ImGui.SliderFloat("Payload", ref _payloadPercent, 0, 100);
                
                ImGui.Combo("ElectronicsSuite", ref _electronicsSelectedIndex, _electronicsPackage, _electronicsPackage.Length);
                ImGui.Text("Size");             
                
                ImGui.SliderFloat("Fuel", ref _fuelPercent, 0, 100);

                ImGui.Combo("Engine Designs", ref _engineSelectedIndex, _engineDesigns, _engineDesigns.Length);
                ImGui.SliderInt("Engine", ref _engineCount, 0, 256);
                
                ImGui.InputText("Design Name", _designName, (uint)_designName.Length);
                NewDesignButton();
                
            }
        }
        
        internal void NewDesignButton()
        {
            if (ImGui.Button("Create Design"))
            {
                int version = 0;
                var strName = ImGuiSDL2CSHelper.StringFromBytes(_designName);
                foreach (var design in _currentDesigns)
                {
                    if (design.Name == strName)
                    {
                        if (design.DesignVersion >= version)
                            version = design.DesignVersion + 1;
                    }
                }
                //OrdnanceDesign missileDesign = new OrdnanceDesign(_state.Faction.GetDataBlob<FactionInfoDB>(), strName, _shipComponents, (_armor, _armorThickness));
                //missileDesign.DesignVersion = version;

            }
        }
    }
}