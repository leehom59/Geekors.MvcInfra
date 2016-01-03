using System;
using System.Web.Mvc;

namespace ModelHelper.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : Attribute, IMetadataAware
    {
        public string Url { get; set; }
        public string Filter { get; set; }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.AdditionalValues["Url"] = Url;
            if (Filter != null)
                metadata.AdditionalValues["Filter"] = Filter;
            metadata.TemplateHint = "ForeignKeyRemote";
        }
    }
}