using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Undo;
using Grasshopper.Kernel.Undo.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaceHolderEditor
{
    internal class UndoServer
    {
        private static bool IsPlaceHolder(IGH_DocumentObject obj)
        {
            if (obj.GetType().FullName.Contains("Grasshopper.Kernel.Components.GH_PlaceholderComponent")) return true;

            if (obj.GetType().FullName.Contains("Grasshopper.Kernel.Components.GH_PlaceholderParameter")) return true;

            return false;
        }

        public GH_UndoRecord CreateRemoveObjectEvent(string name, IGH_DocumentObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            return new GH_UndoRecord(name, IsPlaceHolder(obj)? new GH_RemovePlaceHolderAction(obj) : new GH_RemoveObjectAction(obj));
        }

		public GH_UndoRecord CreateRemoveObjectEvents(string name, IEnumerable<IGH_DocumentObject> objs)
		{
			if (objs == null)
			{
				throw new ArgumentNullException("objs");
			}
			List<IGH_UndoAction> list = new List<IGH_UndoAction>();
			foreach (IGH_DocumentObject obj in objs)
			{
				GH_Relay gH_Relay = obj as GH_Relay;
				if (gH_Relay != null)
				{
					list.AddRange(gH_Relay.SafeDisconnect());
				}
			}
			foreach (IGH_DocumentObject obj2 in objs)
			{
				list.Add(IsPlaceHolder(obj2) ? new GH_RemovePlaceHolderAction(obj2) : new GH_RemoveObjectAction(obj2));
			}
			return new GH_UndoRecord(name, list);
		}
	}
}
