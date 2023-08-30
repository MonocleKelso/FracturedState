using System.Collections.Generic;
using UnityEngine;

namespace FracturedState.Game
{
    public sealed class ObjectUIDLookUp
    {
        private static ObjectUIDLookUp instance;
        public static ObjectUIDLookUp Instance
        {
            get
            {
                if (instance == null)
                    instance = new ObjectUIDLookUp();

                return instance;
            }
        }

        private Dictionary<int, CoverManager> coverLookup;
        private Dictionary<int, StructureManager> structureLookup;

        private ObjectUIDLookUp()
        {
            coverLookup = new Dictionary<int, CoverManager>();
            structureLookup = new Dictionary<int, StructureManager>();
        }

        public void AddCoverManager(int uid, CoverManager coverManager)
        {
            coverLookup[uid] = coverManager;
        }

        public void AddStructure(int uid, StructureManager structureManager)
        {
            structureLookup[uid] = structureManager;
        }

        public CoverManager GetCoverManager(int uid)
        {
            CoverManager cm;
            if (coverLookup.TryGetValue(uid, out cm))
            {
                return cm;
            }

            throw new FracturedStateException("Unable to get CoverManager.  No object on this map that provides cover has a UID of: " + uid);
        }

        public StructureManager GetStructureManager(int uid)
        {
            StructureManager sm;
            if (structureLookup.TryGetValue(uid, out sm))
            {
                return sm;
            }

            throw new FracturedStateException("Unable to get StructureManager.  No object on this map that is enterable has a UID of: " + uid);
        }
    }
}