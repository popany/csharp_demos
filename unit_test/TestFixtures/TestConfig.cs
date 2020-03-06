using NUnit.Framework;
using unit_test.Utils;
using System.Linq;
using System.Configuration;
using System.IO;

namespace unit_test.TestFixtures
{
    // Define a custom section. The CustomSection type allows to define a custom section programmatically.
    public sealed class CustomSection1 : ConfigurationSection
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
                "  </configSections>\n" +
                "  <appSettings>\n" +
                "    <add key=\"Setting1\" value=\"May 5, 2014\"/>\n" +
                "    <add key=\"Setting2\" value=\"May 6, 2014\"/>\n" +
                "  </appSettings>\n" +
                "  <CustomSection1 fileName=\"default.txt\" maxUsers=\"1000\" maxIdleTime=\"00:15:00\" />\n" + 
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

            Assert.AreEqual(appSettings.Settings["Setting1"].Value, "May 5, 2014");
            Assert.AreEqual(appSettings.Settings["Setting2"].Value, "May 6, 2014");

            CustomSection1 customSection1 = configuration.GetSection("CustomSection1") as CustomSection1;
            Assert.IsNotNull(customSection1);
            Assert.AreEqual(customSection1.FileName, "default.txt");
            Assert.AreEqual(customSection1.MaxUsers, 1000);
            Assert.AreEqual(customSection1.MaxIdleTime, System.TimeSpan.Parse("00:15:00"));
        }
    }
}
