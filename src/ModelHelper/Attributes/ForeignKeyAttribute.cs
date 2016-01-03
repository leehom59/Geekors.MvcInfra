using System;
using System.Web;
using System.Web.Mvc;

namespace Kendo.Mvc.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : Attribute, IMetadataAware
    {
        public string Url { get; set; }
        public string AddonPreams { get; set; }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            metadata.AdditionalValues["Url"] = urlHelper.Content(Url);
            if (AddonPreams != null)
                metadata.AdditionalValues["AddonPreams"] = AddonPreams;
            metadata.TemplateHint = "ForeignKeyRemote";
        }
    }
}