# PluginLoadingExtension
This plugin loading extension allows plugins to be imported from DLL files securely. 

All plugins that implement a known interface or inherit from a known base class can be imported, making the code reusable across projects. 

DLL validity and security were ensured using whitelisting, cryptograhic hashing, digital signatures and folder permission restrictions.

This project was made in .NET Framework as my final project. The programming language was requested by the collaborating company. 

Should the code be run, it will present errors due to the digital signature files not being included. 

This was done on purpose, as any user must create or receive one or more appropriate digital certificates, place it in their "Trusted Root" digital certificate folder, use the digital certificates to make the digital signature files for the plugins, and place the digital signatures in the appropriate folder(s) of the plugin loading extension. 

If the digital certificate(s) is placed in the "Trusted Root", and the digital signatures have been placed in the appropriate folder(s), the application will run successfully. 
