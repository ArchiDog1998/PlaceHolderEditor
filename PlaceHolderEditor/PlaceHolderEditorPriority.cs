using Eto.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PlaceHolderEditor
{
    public class PlaceHolderEditorPriority : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Grasshopper.Instances.CanvasCreated += Instances_CanvasCreated;
            return GH_LoadingInstruction.Proceed;
        }

        private void Instances_CanvasCreated(Grasshopper.GUI.Canvas.GH_Canvas canvas)
        {
            Grasshopper.Instances.CanvasCreated -= Instances_CanvasCreated;
            canvas.DocumentChanged += Canvas_DocumentChanged;

            ExchangeMethod(
                typeof(GH_Document).GetRuntimeMethods().Where(m => m.Name.Contains("RelevantObjectAtPoint") && m.GetParameters().Length == 2).First(),
                typeof(DrawingHelper).GetRuntimeMethods().Where(m => m.Name.Contains("RelevantObjectAtPoint")).First()
            );

        }


        private void Canvas_DocumentChanged(Grasshopper.GUI.Canvas.GH_Canvas sender, Grasshopper.GUI.Canvas.GH_CanvasDocumentChangedEventArgs e)
        {
            //MessageBox.Show("changed!");
            if (e.NewDocument == null) return;
            foreach (var item in e.NewDocument.Objects)
            {
                if (item.GetType().FullName.Contains("Grasshopper.Kernel.Components.GH_PlaceholderComponent") && item is GH_Component compnent)
                {
                    compnent.Attributes = new GH_EditablePlaceholderComponentAttributes(compnent);
                }
                else if (item.GetType().FullName.Contains("Grasshopper.Kernel.Components.GH_PlaceholderParameter") && item is GH_Param<IGH_Goo> param)
                {
                    param.Attributes = new GH_EditablePlaceholderFloatingAttributes(param);
                }
            }
        }

        internal static bool ExchangeMethod(MethodInfo targetMethod, MethodInfo injectMethod)
        {
            if (targetMethod == null || injectMethod == null)
            {
                return false;
            }
            RuntimeHelpers.PrepareMethod(targetMethod.MethodHandle);
            RuntimeHelpers.PrepareMethod(injectMethod.MethodHandle);
            unsafe
            {
                if (IntPtr.Size == 4)
                {
                    int* tar = (int*)targetMethod.MethodHandle.Value.ToPointer() + 2;
                    int* inj = (int*)injectMethod.MethodHandle.Value.ToPointer() + 2;
                    var relay = *tar;
                    *tar = *inj;
                    *inj = relay;
                }
                else
                {
                    long* tar = (long*)targetMethod.MethodHandle.Value.ToPointer() + 1;
                    long* inj = (long*)injectMethod.MethodHandle.Value.ToPointer() + 1;
                    var relay = *tar;
                    *tar = *inj;
                    *inj = relay;
                }
            }
            return true;
        }

    }
}
