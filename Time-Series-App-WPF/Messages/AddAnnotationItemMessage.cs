using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Time_Series_App_WPF.Model;

namespace Time_Series_App_WPF.Messages
{
    public class AddAnnotationItemMessage : ValueChangedMessage<Annotation>
    {
        public AddAnnotationItemMessage(Annotation annotation) : base(annotation)
        {

        }
    }
}
