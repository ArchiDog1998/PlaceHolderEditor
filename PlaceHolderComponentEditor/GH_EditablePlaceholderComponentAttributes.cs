using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Components;
using Grasshopper.GUI.Canvas;
using System.Drawing.Drawing2D;
using Grasshopper.GUI;
using Eto.Forms;

namespace PlaceHolderComponentEditor
{
    public class GH_EditablePlaceholderComponentAttributes : GH_ComponentAttributes
    {
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

        public override void ExpireLayout()
        {
			//MessageBox.Show("Expired!");
            base.ExpireLayout();
        }


        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
		{
			switch (channel)
			{
				case GH_CanvasChannel.Wires:
					base.Render(canvas, graphics, channel);
					break;
				case GH_CanvasChannel.Objects:
					{
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
