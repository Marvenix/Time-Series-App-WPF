using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Time_Series_App_WPF.Model
{
    public class MadeAnnotation
    {
        public Guid Id {  get; set; }
        public required Annotation Annotation { get; set; }
        public float Start { get; set; }
        public float End { get; set; }
        public float Duration { get; set; }
        public required string Channel { get; set; }
        public required string ChannelId { get; set; }
    }
}
