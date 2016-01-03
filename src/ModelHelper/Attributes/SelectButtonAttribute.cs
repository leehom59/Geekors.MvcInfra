using System;
using System.Web.Mvc;

namespace ModelHelper.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SelectButtonAttribute : Attribute, IMetadataAware
    {
        public string Click { get; set; }

        public SelectButtonAttribute(string onclick)
        {
            Click = onclick;
        }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.AdditionalValues["Click"] = Click;
            metadata.TemplateHint = "ForeignKeyRemote";
        }
    }
}
