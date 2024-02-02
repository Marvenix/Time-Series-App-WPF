using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Time_Series_App_WPF.Model
{
    public class MadeAnnotation
    {
        public Guid Id {  get; set; }
        public Annotation? Annotation { get; set; }
        public float Start { get; set; }
        public float End { get; set; }
        public float Duration { get; set; }
        public string? Channel { get; set; }
        public string? ChannelId { get; set; }
    }
}