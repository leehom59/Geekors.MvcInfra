using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ModelHelper.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class WebApiCrudAttribute : Attribute
    {
        public WebApiCrudAttribute()
        {
        }

        public WebApiCrudAttribute(string routeName, string controllName)
        {
            DestroyControllerMap = UpdateControllerMap = CreateControllerMap = ReadControllerMap = new[] {routeName, controllName};
        }

        public string[] ReadControllerMap { get; set; }
        public string[] CreateControllerMap { get; set; }
        public string[] UpdateControllerMap { get; set; }
        public string[] DestroyControllerMap { get; set; }

        public string ReadUrl
        {
            get {
                return ReadControllerMap != null ? RcControlerMap(ReadControllerMap) : "";
            }
            set
            {
                if (ReadControllerMap == null)
                    ReadControllerMap = new string[1];
                ReadControllerMap[0] = value;
            }
        }

        public string CreateUrl
        {
            get
            {
                return CreateControllerMap != null ? RcControlerMap(CreateControllerMap) : "";
            }
            set
            {
                if (CreateControllerMap == null)
                    CreateControllerMap = new string[1];
                CreateControllerMap[0] = value;
            }
        }

        public string UpdateUrl
        {
            get
            {
                return UpdateControllerMap != null ? UdControlerMap(UpdateControllerMap) : "";
            }
            set
            {
                if (UpdateControllerMap == null)
                    UpdateControllerMap = new string[1];
                UpdateControllerMap[0] = value;
            }
        }

        public string DestroyUrl
        {
            get
            {
                return DestroyControllerMap != null ? UdControlerMap(DestroyControllerMap) : "";
            }
            set
            {
                if (DestroyControllerMap == null)
                    DestroyControllerMap = new string[1];
                DestroyControllerMap[0] = value;
            }
        }

        static string UdControlerMap(string[] mapPath)
        {
            if (mapPath.Length > 3)
                if (mapPath[3].LastIndexOf("#") >= 0)
                    return RcControlerMap(mapPath);
            if (mapPath.Length == 1)
                return mapPath[0].LastIndexOf("#") >= 0 ? mapPath[0] : string.Format("{0}/{{0}}", mapPath[0]);
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            object obj;
            if (mapPath.Length > 2)
                obj = new { controller = mapPath[1], action = mapPath[2], id = "{0}" };
            else
                obj = new { controller = mapPath[1], id = "{0}" };
            var strUrl = url.HttpRouteUrl(mapPath[0], obj);
            if (mapPath.Length > 3)
                strUrl += mapPath[3];
            return strUrl;
        }

        private static string RcControlerMap(string[] mapPath)
        {
            if (mapPath.Length == 1)
                return mapPath[0];
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            object obj;
            if (mapPath.Length > 2)
                obj = new { controller = mapPath[1], action = mapPath[2] };
            else
                obj = new { controller = mapPath[1] };
            var strUrl = url.HttpRouteUrl(mapPath[0], obj);
            if (mapPath.Length > 3)
                strUrl += mapPath[3];
            return strUrl;
        }
        public static WebApiCrudAttribute Get<TViewModel>()
        {
            Type type = typeof (TViewModel);

            var webApiCrudAttribute = (WebApiCrudAttribute) type.GetCustomAttributes(typeof (WebApiCrudAttribute), false).FirstOrDefault();
            string viewModelName = type.Name;
            if (viewModelName.EndsWith("ViewModel"))
            {
                var defaultControlerName = string.Format("{0}Service", viewModelName.Substring(0, viewModelName.Length - 9));
                if (webApiCrudAttribute != null)
                {
                    if (webApiCrudAttribute.ReadControllerMap == null)
                        webApiCrudAttribute.ReadControllerMap = new [] {"DefaultApi", defaultControlerName};
                    if (webApiCrudAttribute.CreateControllerMap == null)
                        webApiCrudAttribute.CreateControllerMap = new[] { "DefaultApi", defaultControlerName };
                    if (webApiCrudAttribute.UpdateControllerMap == null)
                        webApiCrudAttribute.UpdateControllerMap = new[] { "DefaultApi", defaultControlerName };
                    if (webApiCrudAttribute.DestroyControllerMap == null)
                        webApiCrudAttribute.DestroyControllerMap = new[] { "DefaultApi", defaultControlerName };
                }
                else
                    webApiCrudAttribute = new WebApiCrudAttribute("DefaultApi", defaultControlerName);
            }
            return webApiCrudAttribute;
        }
    }
}