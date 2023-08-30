using System.Xml.Serialization;

namespace FracturedState.Game.Data
{
    [XmlRoot("game")]
    public class GameConfig
    {
        /// <summary>
        /// How close is considered "close enough" when navigating to a destination in the world
        /// </summary>
        [XmlElement("closeEnough")]
        public float CloseEnoughThreshold { get; set; }

        [XmlElement("transportEnterDistance")]
        public float TransportEnterDistance { get; set; }

        /// <summary>
        /// How close to a fire or cover point the player must place the mouse cursor before a micro path will snap to that point
        /// </summary>
        [XmlElement("cursorPointSnapDistance")]
        public float CursorPointSnapDistance { get; set; }

        /// <summary>
        /// The distance a unit should check around him for points of cover
        /// </summary>
        [XmlElement("coverCheckDistance")]
        public float CoverCheckDistance { get; set; }

        /// <summary>
        /// The maximum difference in facing between a unit's approach vector and a cover point's facing vector that will be considered viable when evaluating cover positions.
        /// This is calculated by taking the dot product of the two normalize vectors and comparing if the result is less than this value
        /// </summary>
        [XmlElement("coverPointDirectionThreshold")]
        public float CoverPointDirectionThreshold { get; set; }

        /// <summary>
        /// The maximum difference in facing between a unit's forward vector and an enemy unit's position that will allow the garrisoned unit to attack the enemy
        /// </summary>
        [XmlElement("firePointVisionThreshold")]
        public float FirePointVisionThreshold { get; set; }

        [XmlElement("squadPopCap")]
        public int SquadPopulationCap { get; set; }

        /// <summary>
        /// The radius out from a designated starting position that the game will attempt to place starting units
        /// </summary>
        [XmlElement("startUnitSpawnRadius")]
        public float StartingUnitSpawnRadius { get; set; }

        /// <summary>
        /// The maximum number of times the game will attempt to place a starting unit.  After this is reached the game will place units
        /// on top of each other
        /// </summary>
        [XmlElement("startUnitPlacementMaxTries")]
        public int StartingUnitSpawnMaxTries { get; set; }

        /// <summary>
        /// The max radius a point in the navigation grid can have. This is used as the max radius to check for nearby obstacles around each point
        /// </summary>
        [XmlElement("navPointMaxRadius")]
        public float NavPointMaxRadius { get; set; }

        /// <summary>
        /// The initial size of the selection projector pool
        /// </summary>
        [XmlElement("initialSelectProjectors")]
        public int InitialSelectionProjectorCount { get; set; }

        /// <summary>
        /// How fast the camera moves in the XYZ axis
        /// </summary>
        [XmlElement("cameraMoveSpeed")]
        public float CameraMoveSpeed { get; set; }

        /// <summary>
        /// How fast the camera spins on its up axis
        /// </summary>
        [XmlElement("cameraRotateSpeed")]
        public float CameraRotateSpeed { get; set; }

        /// <summary>
        /// A sensitivity multiplier for camera movement
        /// </summary>
        [XmlElement("cameraSensitivity")]
        public float CameraSensitivity { get; set; }

        /// <summary>
        /// The height of the camera when no obstacles are underneath it
        /// </summary>
        [XmlElement("cameraDefaultHeight")]
        public float CameraDefaultHeight { get; set; }

        /// <summary>
        /// The height of the camera when switched into tactical overhead mode
        /// </summary>
        [XmlElement("cameraTacticalHeight")]
        public float CameraTacticalHeight { get; set; }

        /// <summary>
        /// The down angle of the camera when no obstacles are underneath it
        /// </summary>
        [XmlElement("cameraDefaultAngle")]
        public float CameraDefaultAngle { get; set; }

        /// <summary>
        /// How much vertical space the camera will put between it and the obstacle underneath it
        /// </summary>
        [XmlElement("cameraBufferHeight")]
        public float CameraBufferHeight { get; set; }

        /// <summary>
        /// The down angle the camera will transition to when an obstacle is underneath it
        /// </summary>
        [XmlElement("cameraBufferAngle")]
        public float CameraBufferAngle { get; set; }

        /// <summary>
        /// How fast in units per second the camera will move towards its buffer height
        /// </summary>
        [XmlElement("cameraHeightUnitAdjustment")]
        public float CameraHeightUnitAdjustment { get; set; }

        /// <summary>
        /// How fast in radians per second the camera will rotate towards its buffer angle
        /// </summary>
        [XmlElement("cameraAngleUnitAdjustment")]
        public float CameraAngleUnitAdjustment { get; set; }

        /// <summary>
        /// The maximum penalty an attacking unit can incur for attacking a movement target if that target
        /// is moving in a direction perpendicular to the attacker - this value is compounded by the speed of the target
        /// </summary>
        [XmlElement("movementAccuracyPenalty")]
        public float MovementAccuracyPenalty { get; set; }

        /// <summary>
        /// The percentage bonus to rate of fire that units receive while occupying a fire point in a structure
        /// </summary>
        [XmlElement("garrisonROFBonus")]
        public float GarrisonROFBonus { get; set; }

        /// <summary>
        /// The amount of time, in seconds, that gibs will remain on the field
        /// </summary>
        [XmlElement("gibLifetime")]
        public float GibLifetime { get; set; }

        /// <summary>
        /// The amount of vertical space between nav points that is considered too high for them to be connected
        /// </summary>
        [XmlElement("navOffsetThreshold")]
        public float NavOffsetThreshold { get; set; }
    }
}