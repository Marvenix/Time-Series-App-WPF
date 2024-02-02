using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Time_Series_App_WPF.Model
{
    public sealed class MadeAnnotationMap : ClassMap<MadeAnnotation>
    {
        public MadeAnnotationMap()
        {
            Map(m => m.Id).Name("Id").NameIndex(0);
            Map(m => m.Annotation.Id).Name("Id").NameIndex(1);
            Map(m => m.Annotation.Name);
            Map(m => m.Annotation.Color);
            Map(m => m.Start);
            Map(m => m.End);
            Map(m => m.Duration);
            Map(m => m.Channel);
            Map(m => m.ChannelId);
        }
    }
}
