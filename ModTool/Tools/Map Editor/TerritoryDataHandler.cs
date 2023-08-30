using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.ModTools
{
    public class TerritoryDataHandler
    {
        TerritoryTool owner;
        public TerritoryTool Owner { get { return owner; } }
        private GameObject ui;
        private TerritoryData territory;

        public TerritoryDataHandler(TerritoryData territory, TerritoryTool owner)
        {
            this.territory = territory;
            this.owner = owner;
            ui = new GameObject("Territory Window");
            TerritoryDataUI t = ui.AddComponent<TerritoryDataUI>();
            t.Init(this, territory);
        }

        public void Close()
        {
            GameObject.Destroy(ui);
            owner.ClosePropertyWindow(territory);
        }
    }
}