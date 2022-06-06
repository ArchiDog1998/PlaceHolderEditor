using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaceHolderComponentEditor
{
    internal class GH_EditablePlaceholderFloatingAttributes : GH_Attributes<GH_Param<IGH_Goo>>
	{
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
		}

		protected override void Layout()
		{
			PointF dir = Pivot.Subtract(lastPivot);
			lastPivot = Pivot;

			Bounds = Bounds.MoveRectangleF(dir);
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
						graphics.FillPath(Selected ? new SolidBrush(GH_Skin.wire_selected_a) : Brushes.White, graphicsPath);
						graphics.DrawPath(pen, graphicsPath);
						graphicsPath.Dispose();
						pen.Dispose();
						break;
					}
			}
		}
	}
}
