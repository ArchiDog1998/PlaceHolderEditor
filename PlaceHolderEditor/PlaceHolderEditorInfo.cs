using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace PlaceHolderEditor
{
    public class PlaceHolderEditorInfo : GH_AssemblyInfo
    {
        public override string Name => "Place Holder Component Editor";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => Properties.Resources.PlaceHolderComponentEditorIcon_24;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "Make place holder componennt editable!";

        public override Guid Id => new Guid("5C788E3B-E279-43E6-A7C0-B88CF226C298");

        //Return a string identifying you or your company.
        public override string AuthorName => "秋水";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "1123993881@qq.com";

        public override string Version => "0.9.0";
    }
}