$note(jefflill):

This folder holds the decompiled source files from the HyperV PowerShell cmdlets.
Here's how I did tnis:

1. Use **ILSpy** to save the DLL source and project files.
2. Rename each project folder to match the namespace for the project source files.
3. Add **using** statements to resolve ambiguous type references.
4. Convert all **public** types to **internal**.
