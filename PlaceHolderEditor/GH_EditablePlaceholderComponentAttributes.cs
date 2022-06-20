using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Grasshopper.Kernel.Components;
using Grasshopper.GUI.Canvas;
using System.Drawing.Drawing2D;
using Grasshopper.GUI;
using Eto.Forms;

namespace PlaceHolderEditor
{
    public class GH_EditablePlaceholderComponentAttributes : GH_ComponentAttributes
    {
		private static PropertyInfo _getPluginInfo = null;
		private static FieldInfo _getName = null;
		private static FieldInfo _getAuthor = null;
		private static FieldInfo _getVersion = null;
		private static FieldInfo _getAssembly = null;
		private static FieldInfo _getAssemblyVersion = null;

		private string showText;
		private string name;
		public override bool AllowMessageBalloon => false;
		public override RectangleF Bounds { get; set; }
		public override PointF Pivot { get; set; }

        private PointF lastPivot;

		public GH_EditablePlaceholderComponentAttributes(GH_Component owner)
			: base(owner)
		{
			lastPivot = Pivot = owner.Attributes.Pivot;
			Bounds = owner.Attributes.Bounds;
			Selected = owner.Attributes.Selected;

			if(_getPluginInfo == null)
				_getPluginInfo = owner.GetType().GetRuntimeProperties().First(p => p.Name.Contains("PluginInfo"));

			var pluginInfo = _getPluginInfo.GetValue(owner);

			if (_getName == null)
				_getName = pluginInfo.GetType().GetRuntimeFields().First(p => p.Name.Contains("Name"));

			if (_getAuthor == null)
				_getAuthor = pluginInfo.GetType().GetRuntimeFields().First(p => p.Name.Contains("Author"));

			if (_getVersion == null)
				_getVersion = pluginInfo.GetType().GetRuntimeFields().First(p => p.Name.Contains("Version"));

			if (_getAssembly == null)
				_getAssembly = pluginInfo.GetType().GetRuntimeFields().First(p => p.Name.Contains("AssemblyFullName"));

			if (_getAssemblyVersion == null)
				_getAssemblyVersion = pluginInfo.GetType().GetRuntimeFields().First(p => p.Name.Contains("AssemblyVersion"));

			name = _getName.GetValue(pluginInfo).ToString();
			showText = "Name: " + name;
			showText += "\nVersion: " + _getVersion.GetValue(pluginInfo).ToString();
			showText += "\nAuthor: " + _getAuthor.GetValue(pluginInfo).ToString();
			showText += "\nAssembly: " + _getAssembly.GetValue(pluginInfo).ToString().Replace(" ", "\n--");

		}

		protected override void Layout()
		{
			PointF dir = Pivot.Subtract(lastPivot);
			lastPivot = Pivot;

			Bounds = Bounds.MoveRectangleF(dir);

            foreach (var item in Owner.Params)
            {
				item.Attributes.Bounds = item.Attributes.Bounds.MoveRectangleF(dir);
			}
		}

        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
			System.Diagnostics.Process.Start($"https://www.food4rhino.com/en/browse?searchText={name.Replace(' ', '+')}&sort_by=score");
			return base.RespondToMouseDoubleClick(sender, e);
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
		{
			switch (channel)
			{
				case GH_CanvasChannel.Wires:
					base.Render(canvas, graphics, channel);
					break;
				case GH_CanvasChannel.Objects:
                    int num = base.Owner.Params.Input.Count - 1;
                    for (int i = 0; i <= num; i++)
                    {
                        PointF inputGrip = base.Owner.Params.Input[i].Attributes.InputGrip;
                        graphics.FillEllipse(rect: new RectangleF(inputGrip.X - 4f, inputGrip.Y - 4f, 8f, 8f), brush: Brushes.DimGray);
                    }
                    int num2 = base.Owner.Params.Output.Count - 1;
                    for (int j = 0; j <= num2; j++)
                    {
                        PointF outputGrip = base.Owner.Params.Output[j].Attributes.OutputGrip;
                        graphics.FillEllipse(rect: new RectangleF(outputGrip.X - 4f, outputGrip.Y - 4f, 8f, 8f), brush: Brushes.DimGray);
                    }
                    GraphicsPath graphicsPath = GH_CapsuleRenderEngine.CreateRoundedRectangle(GH_Convert.ToRectangle(Bounds), 3);
                    Pen pen = new Pen(Color.DimGray, 1f)
                    {
                        DashStyle = DashStyle.Dash
                    };
                    graphics.FillPath(Selected ? new SolidBrush(Color.PaleGreen) : Brushes.White, graphicsPath);
                    graphics.DrawPath(pen, graphicsPath);
                    graphicsPath.Dispose();
                    pen.Dispose();
                    break;

                case GH_CanvasChannel.Last:
					if (!this.Selected) break;

					graphics.DrawString(showText, GH_FontServer.Small, new SolidBrush(Color.Black),
						new PointF(this.Bounds.Left + 1, this.Bounds.Top + 1));

					break;

			}
		}
	}
}
