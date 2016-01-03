using System;
using System.Web.Mvc;

namespace ModelHelper.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class EnumAttribute : Attribute, IMetadataAware
    {
        public EnumAttribute(Type enumType)
        {
            EnumType = enumType;
        }

        private Type EnumType { get; set; }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.AdditionalValues["EnumType"] = EnumType;
            metadata.TemplateHint = "Enum";
        }
    }
}