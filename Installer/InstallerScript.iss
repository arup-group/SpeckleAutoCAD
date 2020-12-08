#define AppName "SpeckleAutoCAD"
#define AppPublisher "Speckle"
#define AppURL "https://github.com/arup-group/SpeckleAutoCAD"
#define AppVersion "0.0.1"
#define ErrorMessage "AutoCAD/Civil3D is not installed on this machine. Setup will now exit."


[Setup]
AppId={{FFE541D3-60E4-4A3E-8224-D0C51F238690}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName="C:\ProgramData\SpeckleAutoCAD"
DisableDirPage=yes
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
DisableWelcomePage=no
OutputDir="Build\"
OutputBaseFilename=SpeckleAutoCADInstaller
SetupIconFile="Assets\icon.ico"
Compression=lzma
SolidCompression=yes
WizardImageFile="Assets\installer.bmp"
ChangesAssociations=yes
; PrivilegesRequired must be "none" to access ProgramData
PrivilegesRequired=none
VersionInfoVersion={#AppVersion}
SignTool=signtool
SignedUninstaller=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Components]
Name: autocad20; Description: Speckle for AutoCAD/Civil3D 2020 WIP;
Name: autocad21; Description: Speckle for AutoCAD/Civil3D 2021 WIP;

[Files]
Source: "..\Release\SpeckleAutoCAD2020\*"; Excludes: "SpeckleAutoCAD*.dll"; DestDir: "{userappdata}\Autodesk\ApplicationPlugins\SpeckleAutoCAD2020.bundle\"; Flags: ignoreversion recursesubdirs; Components: autocad20
Source: "..\Release\SpeckleAutoCAD2020\Contents\Win64\2020\SpeckleAutoCAD*.dll"; DestDir: "{userappdata}\Autodesk\ApplicationPlugins\SpeckleAutoCAD2020.bundle\"; Flags: ignoreversion recursesubdirs sign; Components: autocad20
Source: "..\Release\SpeckleAutoCAD2021\*"; Excludes: "SpeckleAutoCAD*.dll"; DestDir: "{userappdata}\Autodesk\ApplicationPlugins\SpeckleAutoCAD2021.bundle\"; Flags: ignoreversion recursesubdirs; Components: autocad21
Source: "..\Release\SpeckleAutoCAD2021\Contents\Win64\2021\SpeckleAutoCAD*.dll"; DestDir: "{userappdata}\Autodesk\ApplicationPlugins\SpeckleAutoCAD2021.bundle\"; Flags: ignoreversion recursesubdirs sign; Components: autocad21
