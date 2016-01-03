using System;
using System.Web.Http.Filters;
using NLog;

namespace ModelHelper.Attributes
{
    public class NLogExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public override void OnException(HttpActionExecutedContext context)
        {
            var ex = context.Exception;
            var message = "發生錯誤的方法:{0}錯誤訊息:{1}堆疊內容:{2}";
            message = string.Format(message, context.Request.Method + Environment.NewLine,
                ex.GetBaseException().Message + Environment.NewLine, Environment.NewLine + ex.StackTrace);
            Log.Error(message);
        }
    }
}