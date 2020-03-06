using NUnit.Framework;
using unit_test.Utils;
using System.Linq;
using System.Configuration;
using System.IO;

namespace unit_test.TestFixtures
{
    // Define a custom section. The CustomSection type allows to define a custom section programmatically.
    public class CustomSection1 : ConfigurationSection
    {
        // The collection (property bag) that contains the section properties.
        private static ConfigurationPropertyCollection properties;

        // Internal flag to disable property setting.
        private static bool readOnly;

        // The FileName property.
        private static readonly ConfigurationProperty fileName = new ConfigurationProperty(
            "fileName",
            typeof(string),
            "default.txt",
            ConfigurationPropertyOptions.IsRequired);

        // The MaxUsers property.
        private static readonly ConfigurationProperty maxUsers = new ConfigurationProperty(
            "maxUsers",
            typeof(long),
            (long)1000,
            ConfigurationPropertyOptions.None);

        // The MaxIdleTime property.
        private static readonly ConfigurationProperty maxIdleTime = new ConfigurationProperty(
            "maxIdleTime",
            typeof(System.TimeSpan),
            System.TimeSpan.FromMinutes(5),
            ConfigurationPropertyOptions.IsRequired);

        // CustomSection constructor.
        public CustomSection1()
        {
            // Property initialization
            properties = new ConfigurationPropertyCollection();

            properties.Add(fileName);
            properties.Add(maxUsers);
            properties.Add(maxIdleTime);
        }

        // This is a key customization. It returns the initialized property bag.
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return properties;
            }
        }

        private new bool IsReadOnly
        {
            get
            {
                return readOnly;
            }
        }

        // Use this to disable property setting.
        private void ThrowIfReadOnly(string propertyName)
        {
            if (IsReadOnly)
            {
                throw new ConfigurationErrorsException("The property " + propertyName + " is read only.");
            }
        }

        // Customizes the use of CustomSection by setting readOnly to false. Remember you must use it along with ThrowIfReadOnly.
        protected override object GetRuntimeObject()
        {
            // To enable property setting just assign true to the following flag.
            readOnly = true;
            return base.GetRuntimeObject();
        }


        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
        public string FileName
        {
            get
            {
                return (string)this["fileName"];
            }
            set
            {
                // With this you disable the setting. Remember that the readOnly flag must be set to true in the GetRuntimeObject.
                ThrowIfReadOnly("FileName");
                this["fileName"] = value;
            }
        }

        [LongValidator(MinValue = 1, MaxValue = 1000000, ExcludeRange = false)]
        public long MaxUsers
        {
            get
            {
                return (long)this["maxUsers"];
            }
            set
            {
                this["maxUsers"] = value;
            }
        }

        [TimeSpanValidator(MinValueString = "0:0:30", MaxValueString = "5:00:0", ExcludeRange = false)]
        public System.TimeSpan MaxIdleTime
        {
            get
            {
                return (System.TimeSpan)this["maxIdleTime"];
            }
            set
            {
                this["maxIdleTime"] = value;
            }
        }
    }

    public class CustomConfigElement1 : ConfigurationElement
    {
        // Constructor allowing property1, property2, and property3 to be specified.
        public CustomConfigElement1(string property1, string property2, int property3)
        {
            Property1 = property1;
            Property2 = property2;
            Property3 = property3;
        }

        // Default constructor, will use default values as defined below.
        public CustomConfigElement1()
        {
        }

        // Constructor allowing property1 to be specified, will take the default values for property2 and property3.
        public CustomConfigElement1(string property1)
        {
            Property1 = property1;
        }

        [ConfigurationProperty("e1property1", DefaultValue = "default1", IsRequired = true, IsKey = true)]
        public string Property1
        {
            get
            {
                return (string)this["e1property1"];
            }
            set
            {
                this["e1property1"] = value;
            }
        }

        [ConfigurationProperty("e1property2", DefaultValue = "http://www.microsoft.com", IsRequired = true)]
        [RegexStringValidator(@"\w+:\/\/[\w.]+\S*")]
        public string Property2 
        {
            get
            {
                return (string)this["e1property2"];
            }
            set
            {
                this["e1property2"] = value;
            }
        }

        [ConfigurationProperty("e1property3", DefaultValue = (int)3, IsRequired = false)]
        [IntegerValidator(MinValue = 0, MaxValue = 10000, ExcludeRange = false)]
        public int Property3 
        {
            get
            {
                return (int)this["e1property3"];
            }
            set
            {
                this["e1property3"] = value;
            }
        }
    }

    public class CustomConfigElement1Collection1 : ConfigurationElementCollection
    {
        public CustomConfigElement1Collection1()
        {
            // Add one element to the collection.  This is
            // not necessary; could leave the collection 
            // empty until items are added to it outside
            // the constructor.
            CustomConfigElement1 tmp = (CustomConfigElement1)CreateNewElement();
            Add(tmp);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CustomConfigElement1();
        }

        protected override ConfigurationElement CreateNewElement(string property1)
        {
            return new CustomConfigElement1(property1);
        }

        protected override System.Object GetElementKey(ConfigurationElement element)
        {
            return ((CustomConfigElement1)element).Property1;
        }


        public CustomConfigElement1 this[int index]
        {
            get
            {
                return (CustomConfigElement1)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public CustomConfigElement1 this[string k]
        {
            get
            {
                return (CustomConfigElement1)BaseGet(k);
            }
        }

        public void Add(CustomConfigElement1 e)
        {
            BaseAdd(e);
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }
    }

    public class CustomConfigElement1Collection2 : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap; // different with CustomConfigElement1Collection1
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CustomConfigElement1();
        }

        protected override System.Object GetElementKey(ConfigurationElement element)
        {
            return ((CustomConfigElement1)element).Property1;
        }

        public CustomConfigElement1 this[int index]
        {
            get
            {
                return (CustomConfigElement1)BaseGet(index);
            }
        }

        protected override string ElementName
        {
            get
            {
                return "element1name";
            }
        }
    }

    // Implement and use a custom section implemented using the attributed model.
    // Define a custom section containing an individual element and a collection of elements.
    public class CustomSection2 : ConfigurationSection
    {
        [ConfigurationProperty("property1", DefaultValue = "default1", IsRequired = true, IsKey = false)]
        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
        public string Property1
        {
            get
            {
                return (string)this["property1"];
            }
            set
            {
                this["property1"] = value;
            }
        }

        [ConfigurationProperty("property2", DefaultValue = "default2", IsRequired = true, IsKey = false)]
        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
        public string Property2
        {
            get
            {
                return (string)this["property2"];
            }
            set
            {
                this["property2"] = value;
            }
        }

        // Declare an element (not in a collection) of the type
        // CustomConfigElement1. In the configuration
        // file it corresponds to <element1 .... />.
        [ConfigurationProperty("element1")]
        public CustomConfigElement1 Element1 
        {
            get
            {
                CustomConfigElement1 element1 = (CustomConfigElement1)base["element1"];
                return element1;
            }
        }


        // Declare a collection element represented 
        // in the configuration file by the sub-section
        // <element1Collection> <add .../> </element1Collection> 
        // Note: the "IsDefaultCollection = false" 
        // instructs the .NET Framework to build a nested 
        // section like <element1Collection1> ...</element1Collection1>.
        [ConfigurationProperty("element1Collection1", IsDefaultCollection = false)]
        public CustomConfigElement1Collection1 Element1Collection1
        {
            get
            {
                CustomConfigElement1Collection1 collection = (CustomConfigElement1Collection1)base["element1Collection1"];
                return collection;
            }
        }

        [ConfigurationProperty("element1Collection2", IsDefaultCollection = false)]
        public CustomConfigElement1Collection2 Element1Collection2
        {
            get
            {
                CustomConfigElement1Collection2 collection = (CustomConfigElement1Collection2)base["element1Collection2"];
                return collection;
            }
        }
    }

    public class CustomConfigElement2 : ConfigurationElement
    {

        [ConfigurationProperty("e2property1", DefaultValue = "default1", IsRequired = true, IsKey = true)]
        public string Property1
        {
            get
            {
                return (string)this["e2property1"];
            }
        }

        [ConfigurationProperty("e2element1")]
        public CustomConfigElement1 Element1
        {
            get
            {
                CustomConfigElement1 element1 = (CustomConfigElement1)base["e2element1"];
                return element1;
            }
        }
    }

    public class CustomConfigElement3Collection1 : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CustomConfigElement2();
        }

        protected override System.Object GetElementKey(ConfigurationElement element)
        {
            return ((CustomConfigElement2)element).Property1;
        }

        public CustomConfigElement2 this[int index]
        {
            get
            {
                return (CustomConfigElement2)BaseGet(index);
            }
        }

        protected override string ElementName
        {
            get
            {
                return "e3c1element2name";
            }
        }
    }

    public class CustomConfigElement3 : ConfigurationElement
    {

        [ConfigurationProperty("e3property1", DefaultValue = "default1", IsRequired = true, IsKey = true)]
        public string Property1
        {
            get
            {
                return (string)this["e3property1"];
            }
        }

        [ConfigurationProperty("e3element1", IsRequired = false)]
        public CustomConfigElement1 Element1
        {
            get
            {
                CustomConfigElement1 element1 = (CustomConfigElement1)base["e3element1"];
                return element1;
            }
        }

        [ConfigurationProperty("e3Collection1", IsDefaultCollection = false, IsRequired = false)]
        public CustomConfigElement3Collection1 Element3Collection1
        {
            get
            {
                CustomConfigElement3Collection1 collection = (CustomConfigElement3Collection1)base["e3Collection1"];
                return collection;
            }
        }
    }

    public class CustomSection3 : ConfigurationSection
    {
        [ConfigurationProperty("s3property1", DefaultValue = "default1", IsRequired = true, IsKey = false)]
        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
        public string Property1
        {
            get
            {
                return (string)this["s3property1"];
            }
        }

        [ConfigurationProperty("s3element3")]
        public CustomConfigElement3 Element
        {
            get
            {
                CustomConfigElement3 element = (CustomConfigElement3)base["s3element3"];
                return element;
            }
        }
    }

    class TestConfig
    {
        string configFilePath;

        void GenTestConfigFile()
        {

            configFilePath = Directory.GetCurrentDirectory();
            while (Path.GetFileName(configFilePath) != "unit_test")
            {
                configFilePath = Directory.GetParent(configFilePath).FullName;
            }
            configFilePath = Path.Combine(Directory.GetParent(configFilePath).FullName, "tmp");
            Directory.CreateDirectory(configFilePath);
            
            configFilePath = Path.Combine(configFilePath, "testconfig.xml");

            string text =
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n" +
                "<configuration>\n" +
                "  <configSections>\n" +
                "    <section name=\"CustomSection1\" type=\"unit_test.TestFixtures.CustomSection1, unit_test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\" allowDefinition=\"Everywhere\" allowExeDefinition=\"MachineToApplication\" restartOnExternalChanges=\"true\" />\n" +
                "    <section name=\"CustomSection2\" type=\"unit_test.TestFixtures.CustomSection2, unit_test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\" allowDefinition=\"Everywhere\" allowExeDefinition=\"MachineToApplication\" restartOnExternalChanges=\"true\" />\n" +
                "    <section name=\"CustomSection3\" type=\"unit_test.TestFixtures.CustomSection3, unit_test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\" allowDefinition=\"Everywhere\" allowExeDefinition=\"MachineToApplication\" restartOnExternalChanges=\"true\" />\n" +
                "  </configSections>\n" +
                "  <appSettings>\n" +
                "    <add key=\"Setting1\" value=\"May 5, 2014\"/>\n" +
                "    <add key=\"Setting2\" value=\"May 6, 2014\"/>\n" +
                "  </appSettings>\n" +
                "  <CustomSection1 fileName=\"default.txt\" maxUsers=\"1000\" maxIdleTime=\"00:15:00\" />\n" + 
                "  <CustomSection2 property1=\"a\" property2=\"b\">\n" + 
                "    <element1 e1property1=\"abc\" e1property2=\"http://www.xxx.com\" e1property3=\"300\" />\n" +
                "    <element1Collection1>\n" +
                "      <add e1property1=\"k1\" e1property2=\"http://www.xxx.com\" e1property3=\"300\" />\n" +
                "      <add e1property1=\"k2\" e1property2=\"http://www.xxx.com\" e1property3=\"400\" />\n" +
                "    </element1Collection1>\n" +
                "    <element1Collection2>\n" +
                "      <element1name e1property1=\"k1\" e1property2=\"http://www.xxx.com\" e1property3=\"300\" />\n" +
                "      <element1name e1property1=\"k2\" e1property2=\"http://www.xxx.com\" e1property3=\"400\" />\n" +
                "    </element1Collection2>\n" +
                "  </CustomSection2>\n" +
                "  <CustomSection3 s3property1=\"sss\">\n" +
                "    <s3element3 e3property1=\"ttt\">\n" +
                "      <e3element1 e1property1=\"kkk\" e1property2=\"http://www.xxx.com\" e1property3=\"300\" />\n" +
                "      <e3Collection1>\n" +
                "        <e3c1element2name e2property1=\"hhh111\">\n" +
                "          <e2element1 e1property1=\"qqq111\" e1property2=\"http://www.xxx.com\" e1property3=\"300\" />\n" +
                "        </e3c1element2name>\n" +
                "        <e3c1element2name e2property1=\"hhh222\">\n" +
                "          <e2element1 e1property1=\"qqq222\" e1property2=\"http://www.xxx.com\" e1property3=\"300\" />\n" +
                "        </e3c1element2name>\n" +
                "      </e3Collection1>\n" +
                "    </s3element3>\n" +
                "  </CustomSection3>\n" +
                "</configuration>\n";
            System.IO.File.WriteAllText(configFilePath, text);
        }

        [SetUp]
        public void Setup()
        {
            GenTestConfigFile();
        }

        [Test]
        public void TestConfigurationManager()
        {
            // Map the new configuration file.
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = Path.GetFileName(configFilePath);
            Directory.SetCurrentDirectory(Path.GetDirectoryName(configFilePath));

            // Get the mapped configuration file
            Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(
                 configFileMap,
                 ConfigurationUserLevel.None);

            var appSettings = configuration.AppSettings;

            Assert.AreEqual("May 5, 2014", appSettings.Settings["Setting1"].Value);
            Assert.AreEqual("May 6, 2014", appSettings.Settings["Setting2"].Value);

            CustomSection1 customSection1 = configuration.GetSection("CustomSection1") as CustomSection1;
            Assert.IsNotNull(customSection1);
            Assert.AreEqual("default.txt", customSection1.FileName);
            Assert.AreEqual(1000, customSection1.MaxUsers);
            Assert.AreEqual(System.TimeSpan.Parse("00:15:00"), customSection1.MaxIdleTime);

            var customSection2 = configuration.GetSection("CustomSection2") as CustomSection2;
            Assert.IsNotNull(customSection2);
            Assert.AreEqual("a", customSection2.Property1);
            Assert.AreEqual("b", customSection2.Property2);

            Assert.AreEqual("abc", customSection2.Element1.Property1);
            Assert.AreEqual("http://www.xxx.com", customSection2.Element1.Property2);
            Assert.AreEqual(300, customSection2.Element1.Property3);

            Assert.AreEqual(3, customSection2.Element1Collection1.Count);
            Assert.AreEqual("default1", customSection2.Element1Collection1[0].Property1);
            Assert.AreEqual("default1", customSection2.Element1Collection1["default1"].Property1);
            Assert.AreEqual("k1", customSection2.Element1Collection1[1].Property1);
            Assert.AreEqual("k1", customSection2.Element1Collection1["k1"].Property1);
            Assert.AreEqual("k2", customSection2.Element1Collection1[2].Property1);
            Assert.AreEqual("k2", customSection2.Element1Collection1["k2"].Property1);

            Assert.AreEqual(2, customSection2.Element1Collection2.Count);

            var customSection3 = configuration.GetSection("CustomSection3") as CustomSection3;
            Assert.IsNotNull(customSection3);
            Assert.AreEqual("sss", customSection3.Property1);
            Assert.AreEqual("ttt", customSection3.Element.Property1);
            Assert.AreEqual("kkk", customSection3.Element.Element1.Property1);

            Assert.AreEqual(2, customSection3.Element.Element3Collection1.Count);
            Assert.AreEqual("hhh111", customSection3.Element.Element3Collection1[0].Property1);
            Assert.AreEqual("qqq111", customSection3.Element.Element3Collection1[0].Element1.Property1);
            Assert.AreEqual("hhh222", customSection3.Element.Element3Collection1[1].Property1);
            Assert.AreEqual("qqq222", customSection3.Element.Element3Collection1[1].Element1.Property1);
        }
    }
}

