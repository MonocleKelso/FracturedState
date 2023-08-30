
using UnityEngine.Networking;

namespace FracturedState.Game
{
    public static class GameConstants
    {
        // the current version of this build
        public const string Version = "v0.4.3";

        // layer definitions
        public const int UiLayer = 5;
        public const int GizmoLayer = 11;
        public const int DecalLayer = 19;
        public const int WeaponBlockerLayer = 20;
        public const int SightBlockerLayer = 21;
        public const int RagdollLayer = 22;
        public const int InteriorEnemyLayer = 23;
        public const int ExteriorEnemyLayer = 24;
        public const int InteriorUnitLayer = 25;
        public const int ExteriorUnitLayer = 26;
        public const int NavMeshIntLayer = 27;
        public const int NavMeshExtLayer = 28;
        public const int TerrainLayer = 29;
        public const int InteriorLayer = 30;
        public const int ExteriorLayer = 31;

        // layer masks
        public const int InteriorEnemyMask = 1 << InteriorEnemyLayer;   // exterior enemy units
        public const int ExteriorEnemyMask = 1 << ExteriorEnemyLayer;   // interior enemy units
        public const int ExteriorMask = 1 << ExteriorLayer; // exterior objects (props, buildings)
        public const int InteriorMask = 1 << InteriorLayer; // interior objects (props, etc)
        public const int ExteriorSightMask = 1 << ExteriorLayer | 1 << SightBlockerLayer; // exterior LOS eval (exterior objects and sight blockers)
        public const int InteriorSightMask = 1 << InteriorLayer | 1 << SightBlockerLayer; // interior LOS eval (interior objects and sight blocks)
        public const int ExteriorNavMask = 1 << ExteriorLayer | 1 << NavMeshExtLayer;   // exterior mask for navigation
        public const int WorldMask = 1 << ExteriorLayer | 1 << InteriorLayer;   // all props and buildings
        public const int SightBlockerMask = 1 << SightBlockerLayer; // sight blockers
        public const int ExteriorMoveMask = 1 << NavMeshExtLayer;  // exterior navigation mesh
        public const int InteriorMoveMask = 1 << NavMeshIntLayer;   // interior navigation mesh
        public const int NavMeshAllMask = 1 << NavMeshExtLayer | 1 << NavMeshIntLayer;  // interior and exterior navigation mesh
        public const int FriendlyUnitMask = 1 << ExteriorUnitLayer | 1 << InteriorUnitLayer;    // interior and exterior friendly units
        public const int EnemyUnitMask = 1 << ExteriorEnemyLayer | 1 << InteriorEnemyLayer; // interior and exterior enemy units
        public const int ExteriorUnitMask = 1 << ExteriorUnitLayer; //exterior units
        public const int InteriorUnitMask = 1 << InteriorUnitLayer; // interior units
        public const int ExteriorObjectMask = 1 << ExteriorLayer | 1 << ExteriorEnemyLayer;  // exterior all (enemies and objects)
        public const int InteriorObjectMask = 1 << InteriorLayer | 1 << ExteriorEnemyLayer;  // interior all (enemies and objects)
        public const int ExteriorUnitAllMask = 1 << ExteriorUnitLayer | 1 << ExteriorEnemyLayer;    // all exterior units (enemies and friendly)
        public const int InteriorUnitAllMask = 1 << InteriorUnitLayer | 1 << InteriorEnemyLayer;    // all interior units (enemies and friendly)
        public const int AllUnitMask = 1 << ExteriorUnitLayer | 1 << ExteriorEnemyLayer | 1 << InteriorUnitLayer | 1 << InteriorEnemyLayer;
        public const int TerrainMask = 1 << TerrainLayer;   // terrain only
        public const int WeaponBlockMask = 1 << WeaponBlockerLayer; // colliders that intercept attacks and projectiles
        public const int DecalPlaceMask = 1 << ExteriorLayer | 1 << TerrainLayer;
        public const int DecalMask = 1 << DecalLayer;
        public const int UiMask = 1 << UiLayer;
        
        // Mod Tool Gizmo mask
        public const int GizmoMask = 1 << GizmoLayer;

        // camera render masks
        public const int EditorExteriorCameraMask = 1 << ExteriorLayer | 1 << ExteriorUnitLayer |1 << TerrainLayer |
                                                    1 << ExteriorEnemyLayer | 1 << GizmoLayer | 1 << WeaponBlockerLayer |
                                                    1 << DecalLayer;
        public const int EditorInteriorCameraMask = 1 << InteriorLayer | 1 << InteriorUnitLayer | 1 << TerrainLayer |
                                                    1 << InteriorEnemyLayer | 1 << GizmoLayer | 1 << WeaponBlockerLayer |
                                                    1 << DecalLayer;

        // reflection probe render mask
        public const int ReflectionMask = 1 << ExteriorLayer | 1 << TerrainLayer;

        // tags
        public const string UnitTag = "Unit";
        public const string BuildingShroudTag = "BuildingShroud";
        public const string PropTag = "Prop";
        public const string TerritoryHelperName = "TerritoryHelpers";

        /// <summary>
        /// The amount of time in seconds between line-of-sight checks
        /// </summary>
        public const float SightEvalTime = 0.2f;

        /// <summary>
        /// The name of the high level object that contains all other mesh-related objects
        /// </summary>
        public const string ModelContainerName = "ModelInfo";

        /// <summary>
        /// The adjustment along the Y axis for structures to guarantee that the floor draws above the terrain
        /// </summary>
        public const float ObjectYAdjustment = 0.01f;

        // Mod Tool custom style names
        public const string ModuleTabStyle = "ModuleTab";
        public const string ModuleTabActiveStyle = "ModuleTabActive";
        
        #if UNITY_EDITOR
        public const float EliminationWaitTime = 10;
        #else
        public const float EliminationWaitTime = 60;
        #endif

        public static class MessageType
        {
            public const short LobbySetup = MsgType.Highest + 1;
            public const short SpawnSquad = LobbySetup + 1;
            public const short MapTransfer = SpawnSquad + 1;
        }
    }
}