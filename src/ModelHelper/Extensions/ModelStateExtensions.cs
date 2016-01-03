using System.Text;
using System.Web.Http;

namespace ModelHelper.Extensions
{
    public static class ModelStateExtensions
    {
        public static string ExpendErrors(this ApiController controller)
        {
            var errors = new StringBuilder();
            foreach (var item in controller.ModelState.Values)
            {
                if (item.Errors.Count > 0)
                {
                    for (var i = item.Errors.Count - 1; i >= 0; i--)
                    {
                        errors.Append(item.Errors[i].ErrorMessage);
                        errors.Append("<br/>");
                    }
                }
            }
            return errors.ToString();
        }
    }
}