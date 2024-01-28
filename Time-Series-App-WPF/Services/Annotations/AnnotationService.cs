using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Time_Series_App_WPF.Context;
using Time_Series_App_WPF.Model;

namespace Time_Series_App_WPF.Services.Annotations
{
    public class AnnotationService : IAnnotationService
    {
        private readonly ApplicationDbContext _context;
        public AnnotationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Annotation>> GetAnnotationsAsync()
        {
            var annotationList = await _context.Annotations.ToListAsync();
            return annotationList;
        }

        public async Task CreateAsync(Annotation annotation)
        {
            _context.Annotations.Add(annotation);
            await _context.SaveChangesAsync();
        }

        public async Task<Annotation?> UpdateAsync(Guid id, Annotation annotation)
        {
            var annotationToEdit = await _context.Annotations.FirstOrDefaultAsync(x => x.Id == id);
            
            if (annotationToEdit != null)
            {
                annotation.Id = id;
                _context.Annotations.Entry(annotationToEdit).CurrentValues.SetValues(annotation);
                await _context.SaveChangesAsync();
            }
            return annotationToEdit;
        }

        public async Task<Annotation?> DeleteAsync(Guid id)
        {
            var annotation = await _context.Annotations.FirstOrDefaultAsync(x => x.Id == id);
            if (annotation != null)
            {
                _context.Annotations.Remove(annotation);
                await _context.SaveChangesAsync();
            }
            return annotation;
        }
    }
}
