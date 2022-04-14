using DevInterface;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheVolatile.PlacedObs
{
    public static class PlacedObs
    {
        public static void Apply()
        {
            On.Room.Loaded += Room_Loaded;
            //On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData;
            On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRep;
            On.DevInterface.ObjectsPage.RemoveObject += ObjectsPage_RemoveObject;
        }


        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            orig(self);

            if (self.game == null) return;

            // Necessary, adds the screen if a placed object exists when the room loads
            var pObjs = self.roomSettings.placedObjects;
            for (int i = pObjs.Count - 1; i >= 0; i--)
                TryAddCustomObject(pObjs[i], self);
        }
        private static void TryAddCustomObject(PlacedObject obj, Room room) 
        { 
            if (obj.type == EnumExt_Volatile.MountainShrine) room.AddObject(new MountainShrine(obj));
        }

        //private static void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self)
        //{
        //    throw new NotImplementedException();
        //}

        private static void ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, DevInterface.ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj)
        {
            var rep = TryAddCustomObjectRep(self, tp, pObj);
            if (rep == null)
                orig(self, tp, pObj);

            // Not necessary, adds the screen immediately when the placed object is created
            if (pObj == null) {
                pObj = self.RoomSettings.placedObjects[self.RoomSettings.placedObjects.Count - 1];
                if (pObj.type == tp)
                    TryAddCustomObject(pObj, self.owner.room);
            }
        }

        private static PlacedObjectRepresentation TryAddCustomObjectRep(ObjectsPage page, PlacedObject.Type type, PlacedObject pObj)
        {
            void MakePlacedObject()
            {
                if (pObj == null) {
                    pObj = new PlacedObject(type, null);
                    pObj.pos = page.owner.room.game.cameras[0].pos + Vector2.Lerp(page.owner.mousePos, new Vector2(-683f, 384f), 0.25f) + Custom.DegToVec(UnityEngine.Random.value * 360f) * 0.2f;
                    page.RoomSettings.placedObjects.Add(pObj);
                }
            }

            PlacedObjectRepresentation objRep = null;

            // Shrine
            if (type == EnumExt_Volatile.MountainShrine) {
                MakePlacedObject();
                objRep = new PlacedObjectRepresentation(page.owner, type.ToString() + "_Rep", page, pObj, type.ToString());
            }

            if (objRep != null) {
                page.tempNodes.Add(objRep);
                page.subNodes.Add(objRep);
            }

            return objRep;
        }

        private static void ObjectsPage_RemoveObject(On.DevInterface.ObjectsPage.orig_RemoveObject orig, DevInterface.ObjectsPage self, DevInterface.PlacedObjectRepresentation objRep)
        {
            foreach (MountainShrine sh in self.owner.room.updateList.Where(obj => obj is MountainShrine shr && shr.pObj == objRep.pObj))
                sh.Destroy();
            orig(self, objRep);
        }
    }
}
