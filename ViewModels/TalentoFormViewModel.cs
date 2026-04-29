using Microsoft.AspNetCore.Mvc.Rendering;

namespace TalentosIT.Web.ViewModels
{
    public class TalentoFormViewModel
    {
        public List<SelectListItem> Utilizadores { get; set; } = [];
        public List<string> Categorias { get; set; } = [];
    }
}