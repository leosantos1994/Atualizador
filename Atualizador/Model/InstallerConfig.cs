namespace UpdaterService.Model
{
    public class InstallerConfig
    {
        public root GetConfigFile(string site, string siteUser, string sitePass, string releasePath, bool adjustIndexes = false)
        {
            var root = new root();
            root.param = new rootParam[]
            {
                new rootParam()
                {
                    key = "ACTION",
                    value = "IMPORTRELEASE"
                },
                new rootParam()
                {
                    key = "BASEPATH",
                    value = site
                },
                new rootParam()
                {
                    key = "USERNAME",
                    value = siteUser
                },
                new rootParam()
                {
                    key = "PASSWORD",
                    value = sitePass
                },
                new rootParam()
                {
                    key = "FILEPATH",
                    value = releasePath
                },
                new rootParam()
                {
                    key = "INTERFACEDETAIL",
                    value = "false"
                },
                new rootParam()
                {
                    key = "ADJUSTINDEXES",
                    value = adjustIndexes ? "true" : "false"
                }
            };
            return root;
        }

        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public class root
        {

            private rootParam[]? paramField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("param")]
            public rootParam[] param
            {
                get
                {
                    return this.paramField;
                }
                set
                {
                    this.paramField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class rootParam
        {

            private string? keyField;

            private string? valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string key
            {
                get
                {
                    return this.keyField;
                }
                set
                {
                    this.keyField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string value
            {
                get
                {
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }
        }


    }
}
