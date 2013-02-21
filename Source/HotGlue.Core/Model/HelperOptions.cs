using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HotGlue.Model
{
    /// <summary>
    /// Used to configure the behavior of HotGlue.Script.Reference
    /// </summary>
    public class HelperOptions
    {
        /// <summary>
        /// When true, <script> tags and other surrounding HTML will be rendered.
        /// When false, this markup must be included on the page.
        /// Set this false when you have a need to control this markup yourself.
        /// Default value is true.
        /// </summary>
        public bool GenerateHeaderAndFooter { get; set; }
    }
}
