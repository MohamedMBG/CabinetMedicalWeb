using System.Threading.Tasks;

namespace CabinetMedicalWeb.Services
{
    public interface IViewRenderService
    {
        Task<string> RenderToStringAsync(string viewName, object model);
    }
}
