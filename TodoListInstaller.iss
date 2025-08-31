[Setup]
; App Information
AppId={{A1B2C3D4-E5F6-7890-ABCD-1234567890AB}
AppName=Ranjan Todo List Application                 
AppVersion=1.0.0
AppPublisher=Ranjan                                     
AppPublisherURL=https://github.com/abhishekranjan6759         
AppSupportURL=https://github.com/abhishekranjan6759    

; Installation Settings
DefaultDirName={autopf}\RanjanTodoApp                  
DefaultGroupName=Ranjan Todo App                       
DisableProgramGroupPage=yes
OutputDir=installer
OutputBaseFilename=RanjanTodoAppSetup           

; Icons for installer and uninstaller
SetupIconFile=todoicon.ico
UninstallDisplayIcon={app}\todoicon.ico

; Installer Settings
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

; Language
[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

; Optional Tasks
[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

; Files to Install
[Files]
Source: "publish\TodoListApp.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "todoicon.ico"; DestDir: "{app}"; Flags: ignoreversion

; Shortcuts with Custom Icons
[Icons]
Name: "{group}\Ranjan Todo List App"; Filename: "{app}\TodoListApp.exe"; IconFilename: "{app}\todoicon.ico"    
Name: "{autodesktop}\Ranjan Todo List App"; Filename: "{app}\TodoListApp.exe"; IconFilename: "{app}\todoicon.ico"; Tasks: desktopicon  
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\Ranjan Todo List App"; Filename: "{app}\TodoListApp.exe"; IconFilename: "{app}\todoicon.ico"; Tasks: quicklaunchicon

; Run App After Installation
[Run]
Filename: "{app}\TodoListApp.exe"; Description: "{cm:LaunchProgram,Ranjan Todo List App}"; Flags: nowait postinstall skipifsilent
