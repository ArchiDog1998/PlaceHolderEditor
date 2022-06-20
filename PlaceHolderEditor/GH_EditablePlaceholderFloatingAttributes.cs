using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlaceHolderEditor
{
    internal class GH_EditablePlaceholderFloatingAttributes : GH_Attributes<GH_Param<IGH_Goo>>
	{
		private static PropertyInfo _getPluginInfo = null;
		private static FieldInfo _getName = null;
		private static FieldInfo _getAuthor = null;
		private static FieldInfo _getVersion = null;
		private static FieldInfo _getAssembly = null;
		private static FieldInfo _getAssemblyVersion = null;

		private string showText;
		private string name;
		public override bool HasInputGrip => true;

		public override bool HasOutputGrip => true;

		public override RectangleF Bounds { get; set; }

		public override PointF Pivot { get; set; }
		private PointF lastPivot;


		public override bool AllowMessageBalloon => false;

		public GH_EditablePlaceholderFloatingAttributes(GH_Param<IGH_Goo> owner)
			: base(owner)
		{
			lastPivot = Pivot = owner.Attributes.Pivot;
			Bounds = owner.Attributes.Bounds;
			Selected = owner.Attributes.Selected;

			if (_getPluginInfo == null)
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
					RenderIncomingWires(canvas.Painter, base.Owner.Sources, GH_ParamWireDisplay.@default);
					break;
				case GH_CanvasChannel.Objects:
					{
						RectangleF rect = new RectangleF(InputGrip.X - 4f, InputGrip.Y - 4f, 8f, 8f);
						graphics.FillEllipse(Brushes.DimGray, rect);
						rect = new RectangleF(OutputGrip.X - 4f, OutputGrip.Y - 4f, 8f, 8f);
						graphics.FillEllipse(Brushes.DimGray, rect);
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
					}


				case GH_CanvasChannel.Last:
					if (!this.Selected) break;

					graphics.DrawString(showText, GH_FontServer.Small, new SolidBrush(Color.Black),
						new PointF(this.Bounds.Left + 1, this.Bounds.Top + 1));

					break;
			}
		}
	}
}
