using System.Collections.Generic;

namespace Model.Licenses
{
    public partial class License 
    {
        public List<LicensePluginLoader> Plugins { get; set; }

        public License() 
        {
            Plugins = new List<LicensePluginLoader>();
            LicensePluginLoader licensePluginLoader = new LicensePluginLoader();

            //FileName is the absolute file path of the DLL or Zip file containing the plugin. 
            //Thumbprint is the SHA-256 hash value of the digital certificate. 
            //Identifier is the SHA-256 hash value of the DLL or Zip file containing the plugin. 

            // Actual Plugins
            licensePluginLoader.FileName = @"C:\Users\patri\source\repos\PluginLoadingExtension\Presentation\bin\Debug\PluginFolder\IPlugin2\Plugin2TestDLL.zip";
            licensePluginLoader.Thumbprint = "6A03C66626CEADA31B698463CC6E804CBF66782FA27EBC9C3AD86A7F2A18E9DC";
            licensePluginLoader.Identifier = "33C1A6CD5F40CBFBFEB36722D3D8ED2403A0C3E678C513C5F50C495C5E47F635";

            Plugins.Add(licensePluginLoader);

            // Test PLugins
            licensePluginLoader = new LicensePluginLoader();
            licensePluginLoader.FileName = @"C:\Users\patri\source\repos\PluginLoadingExtension\Plugin.Test\bin\Debug\PluginFolder\IPlugin\PluginTestDLL2.dll";
            licensePluginLoader.Thumbprint = "6A03C66626CEADA31B698463CC6E804CBF66782FA27EBC9C3AD86A7F2A18E9DC";
            licensePluginLoader.Identifier = "30B1E09E157C473B79AAE07B2B1C21FE59B5B0D52A8668881278C1C613738157";

            Plugins.Add(licensePluginLoader);

            licensePluginLoader = new LicensePluginLoader();
            licensePluginLoader.FileName = @"C:\Users\patri\source\repos\PluginLoadingExtension\Plugin.Test\bin\Debug\PluginFolder\IPlugin\PluginTestDLL.zip";
            licensePluginLoader.Thumbprint = "6A03C66626CEADA31B698463CC6E804CBF66782FA27EBC9C3AD86A7F2A18E9DC";
            licensePluginLoader.Identifier = "B799D1847A865C3319F8DE2EF3CA3D40D38E9FAF4705E844F94F91422BE42773";

            Plugins.Add(licensePluginLoader);
        }
    }
    public class LicensePluginLoader
    {
        private string fileName;
        private string thumbprint;
        private string identifier;

        public string FileName
        {
            get
            {
                return this.fileName;
            }
            set
            {
                this.fileName = value;
            }
        }

        public string Thumbprint
        {
            get
            {
                return this.thumbprint;
            }
            set
            {
                this.thumbprint = value;
            }
        }

        public string Identifier 
        {
            get
            {
                return this.identifier;
            }
            set
            {
                this.identifier = value;
            }
        }
    }
}
