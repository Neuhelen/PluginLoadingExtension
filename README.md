# PluginLoadingExtension
This plugin loading extension allows plugins to be imported from DLL files securely. 

All plugins that implement a known interface or inherit from a known base class can be imported, making the code reusable across projects. 

DLL validity and security were ensured using whitelisting, cryptograhic hashing, digital signatures and folder permission restrictions.

This project was made in .NET Framework as my final project. The programming language was requested by the collaborating company. 

# How to use the Application

Should the code be run as it is, it will present errors due to the digital signature files not being included. This was intentional, as the DLL files must be validated before they are loaded. 

As such, any user must create or receive one or more appropriate digital certificates on the device used to run the application, place it in their "Trusted Root" digital certificate folder, use the digital certificates to make the digital signature files for the plugins, and place the digital signatures in the appropriate folder(s) of the plugin loading extension. 

If the digital certificate(s) is placed in the "Trusted Root" and digital signatures are made using the certifates and the DLL files to be loaded into the application, then all that has to be done is to place the DLL files and digital signature files in "..\PluginLoadingExtension\Presentation\bin\Debug\PluginFolder" and "..\PluginLoadingExtension\Presentation\bin\Debug\SignatureFolder", respectively. If this is done, the application will be able to load the DLL files successfully. 
