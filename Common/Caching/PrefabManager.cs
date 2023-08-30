using UnityEngine;

namespace FracturedState.Game
{
	public static class PrefabManager
	{
		private static GameObject inertUIElement;
		/// <summary>
		/// A prefab representing an XML-based UI element that does not receive user input such as backgrounds or static graphics
		/// </summary>
		public static GameObject InertUIElement
		{
			get
			{
				if (inertUIElement == null)
					inertUIElement = (GameObject)Resources.Load(DataLocationConstants.InertUIGameObject, typeof(GameObject));
					
				return inertUIElement;
			}
		}
		
		private static GameObject clickableUIElement;
		/// <summary>
		/// A prefab representing an XML-based UI element that receives user input in the form of a left mouse click
		/// </summary>
		public static GameObject ClickableUIElement
		{
			get
			{
				if (clickableUIElement == null)
					clickableUIElement = (GameObject)Resources.Load(DataLocationConstants.ClickableUIGameObject, typeof(GameObject));
					
				return clickableUIElement;
			}
		}

        private static GameObject textElement;
        public static GameObject TextElement
        {
            get
            {
                if (textElement == null)
                    textElement = (GameObject)Resources.Load(DataLocationConstants.TextUIGameObject, typeof(GameObject));

                return textElement;
            }
        }

		private static GameObject modelContainer;
		/// <summary>
		/// A prefab representing a GameObject that is meant to render a Mesh with a Material.  No other functionality is attached so this is used
		/// mainly for non-interactive props and the Model Viewer tool in the Editor.
		/// </summary>
		public static GameObject ModelContainer
		{
			get
			{
				if (modelContainer == null)
					modelContainer = (GameObject)Resources.Load(DataLocationConstants.ModelContainerGameObject, typeof(GameObject));
					
				return modelContainer;
			}
		}
		
		private static GameObject unitContainer;
		/// <summary>
		/// A prefab representing the top-most object of a unit.
		/// </summary>
		public static GameObject UnitContainer
		{
			get
			{
				if (unitContainer == null)
					unitContainer = (GameObject)Resources.Load(DataLocationConstants.UnitContainer, typeof(GameObject));
					
				return unitContainer;
			}
		}
		
		private static GameObject networkUnitContainer;
		/// <summary>
		/// A prefab representing a unit in a multiplayer game.
		/// </summary>
		public static GameObject NetworkUnitContainer
		{
			get
			{
				if (networkUnitContainer == null)
					networkUnitContainer = (GameObject)Resources.Load(DataLocationConstants.NetworkUnitContainer, typeof(GameObject));
					
				return networkUnitContainer;
			}
		}

		private static GameObject damageModuleContainer;
		/// <summary>
		/// A prefab representing a non-unit that is capable of taking damage
		/// </summary>
		public static GameObject DamageModuleContainer
		{
			get
			{
				if (damageModuleContainer == null)
					damageModuleContainer = Resources.Load<GameObject>(DataLocationConstants.NetworkDamageModule);

				return damageModuleContainer;
			}
		}

		private static GameObject startingPoint;
        /// <summary>
        /// A prefab representing the prop used to display a team's starting point in the Map Editor.
        /// </summary>
        public static GameObject StartingPoint
        {
            get
            {
                if (startingPoint == null)
                    startingPoint = (GameObject)Resources.Load(DataLocationConstants.StartingPoint, typeof(GameObject));

                return startingPoint;
            }
        }

        private static GameObject netManager;
        /// <summary>
        /// A prefab representing the main NetworkView component for multiplayer
        /// </summary>
        public static GameObject NetManager
        {
            get
            {
                if (netManager == null)
                    netManager = (GameObject)Resources.Load(DataLocationConstants.NetworkManager, typeof(GameObject));

                return netManager;
            }
        }

        private static GameObject navMeshPointHelper;
        /// <summary>
        /// A prefab used as a helper object for visualizing vertex locations when creating a navigation mesh in the Map Editor
        /// </summary>
        public static GameObject NavMeshPointHelper
        {
            get
            {
                if (navMeshPointHelper == null)
                    navMeshPointHelper = (GameObject)Resources.Load(DataLocationConstants.NavMeshPointHelper, typeof(GameObject));

                return navMeshPointHelper;
            }
        }

        private static GameObject selectionProjectorPrefab;

        public static GameObject SelectionProjectorPrefab
        {
            get
            {
                if (selectionProjectorPrefab == null)
                    selectionProjectorPrefab = (GameObject)Resources.Load(DataLocationConstants.SelectionProjectorPrefab, typeof(GameObject));

                return selectionProjectorPrefab;
            }
        }
	}
}