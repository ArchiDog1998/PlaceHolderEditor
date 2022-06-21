using Grasshopper.Kernel;
using Grasshopper.Kernel.Undo.Actions;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Types;
using System.Drawing;

namespace PlaceHolderEditor
{
    public class GH_RemovePlaceHolderAction : GH_RemoveObjectAction
    {
        private static FieldInfo _componentGuid;
        private static FieldInfo _componentName;
        private static PropertyInfo _componentInfo;
        private static FieldInfo _paramGuid;
        private static FieldInfo _paramName;
        private static PropertyInfo _paramInfo;

        private static FieldInfo _wires;
        private static FieldInfo _index;


        private object _objectGuid;
        private object _objectName;
        private object _gluginInfo;

        private RectangleF[] inputRects = new RectangleF[0];
        private RectangleF[] outputRect = new RectangleF[0];
        private RectangleF lastBounds;

        private Type _type;
        public GH_RemovePlaceHolderAction(IGH_DocumentObject obj) :
            base(obj)
        {
            _type = obj.GetType();
            if (_type.FullName.Contains("Grasshopper.Kernel.Components.GH_PlaceholderComponent"))
            {
                if (_componentGuid == null || _componentInfo == null || _componentName == null)
                {
                    var fields = _type.GetRuntimeFields();
                    _componentGuid = fields.First(p => p.Name.Contains("_objectGuid"));
                    _componentName = fields.First(p => p.Name.Contains("_objectName"));
                    _componentInfo = _type.GetRuntimeProperties().First(p => p.Name.Contains("PluginInfo"));
                }
                _objectGuid = _componentGuid.GetValue(obj);
                _objectName = _componentName.GetValue(obj);
                _gluginInfo = _componentInfo.GetValue(obj);

                if(obj is GH_Component com)
                {
                    if(com.Params.Input.Count > 0) inputRects = com.Params.Input.Select(p => p.Attributes.Bounds).ToArray();
                    if(com.Params.Output.Count > 0) outputRect = com.Params.Output.Select(p => p.Attributes.Bounds).ToArray();
                }
            }

            else if (_type.FullName.Contains("Grasshopper.Kernel.Components.GH_PlaceholderParameter"))
            {
                if (_paramGuid == null || _paramInfo == null || _paramName == null)
                {
                    var fields = _type.GetRuntimeFields();
                    _paramGuid = fields.First(p => p.Name.Contains("_objectGuid"));
                    _paramName = fields.First(p => p.Name.Contains("_objectName"));
                    _paramInfo = _type.GetRuntimeProperties().First(p => p.Name.Contains("PluginInfo"));
                }
                _objectGuid = _componentGuid.GetValue(obj);
                _objectName = _componentName.GetValue(obj);
                _gluginInfo = _componentInfo.GetValue(obj);
            }

            lastBounds = obj.Attributes.Bounds;
        }

        protected override void Internal_Undo(GH_Document doc)
        {
            IGH_DocumentObject iGH_DocumentObject = (IGH_DocumentObject)Activator.CreateInstance(_type, new object[] { _objectGuid, _objectName, _gluginInfo });
            if (iGH_DocumentObject == null)
            {
                throw new NullReferenceException("Object type could not be reconstructed");
            }

            iGH_DocumentObject.CreateAttributes();
            Deserialize(iGH_DocumentObject);

            if (iGH_DocumentObject.GetType().FullName.Contains("Grasshopper.Kernel.Components.GH_PlaceholderComponent") && iGH_DocumentObject is GH_Component compnent)
            {
                compnent.Attributes = new GH_EditablePlaceholderComponentAttributes(compnent);
                for (int i = 0; i < inputRects.Length; i++)
                {
                    compnent.Params.Input[i].Attributes.Bounds = inputRects[i];
                }
                for (int i = 0; i < outputRect.Length; i++)
                {
                    compnent.Params.Output[i].Attributes.Bounds = outputRect[i];
                }
            }
            else if (iGH_DocumentObject.GetType().FullName.Contains("Grasshopper.Kernel.Components.GH_PlaceholderParameter") && iGH_DocumentObject is GH_Param<IGH_Goo> param)
            {
                param.Attributes = new GH_EditablePlaceholderFloatingAttributes(param);
            }
            iGH_DocumentObject.Attributes.Bounds = lastBounds;

            if (_index == null) _index = typeof(GH_RemoveObjectAction).GetRuntimeFields().First(f => f.Name.Contains("m_object_index"));
            doc.AddObject(iGH_DocumentObject, update: false, ((int)_index.GetValue(this)));

            List<IGH_Attributes> list = new List<IGH_Attributes>();
            iGH_DocumentObject.Attributes.AppendToAttributeTree(list);
            foreach (IGH_Attributes item in list)
            {
                if (item.DocObject is IGH_Param)
                {
                    item.Pivot = iGH_DocumentObject.Attributes.Pivot;
                    ((IGH_Param)item.DocObject).ClearProxySources();
                }
            }

            if (_wires == null) _wires = typeof(GH_RemoveObjectAction).GetRuntimeFields().First(f => f.Name.Contains("m_wires"));
            ((GH_WireTopologyDiagram)_wires.GetValue(this)).EnsureConnections(doc, throw_exceptions: false);

            iGH_DocumentObject.Attributes.ExpireLayout();

            iGH_DocumentObject.ExpireSolution(recompute: false);
        }
    }
}
