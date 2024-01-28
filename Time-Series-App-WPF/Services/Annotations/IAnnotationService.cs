using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Time_Series_App_WPF.Model;

namespace Time_Series_App_WPF.Services.Annotations
{
    public interface IAnnotationService
    {
        public Task<IEnumerable<Annotation>> GetAnnotationsAsync();
        public Task CreateAsync(Annotation annotation);
        public Task<Annotation?> UpdateAsync(Guid id, Annotation annotation);
        public Task<Annotation?> DeleteAsync(Guid id);

    }
}
