using NineLetter.Web.Models.NineLetter;

namespace NineLetter.Web.Models
{
    public class BasePage
    {
        public BasePage()
        {
            Result = new Result();
            Message = string.Empty;
        }

        public Result Result { get; set; }
        public string Message { get; set; }
    }
}