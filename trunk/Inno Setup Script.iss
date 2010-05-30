;    Copyright (C) 2009-2010  Ryan Skeldon <psykad@gmail.com>
;
;    This program is free software; you can redistribute it and/or modify
;    it under the terms of the GNU General Public License as published by
;    the Free Software Foundation; either version 2 of the License, or
;    (at your option) any later version.
;
;    This program is distributed in the hope that it will be useful,
;    but WITHOUT ANY WARRANTY; without even the implied warranty of
;    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
;    GNU General Public License for more details.
;
;    You should have received a copy of the GNU General Public License
;    along with this program; if not, write to the Free Software
;    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
[Setup]
AppId={{C43FE8E8-57AF-4CFF-88E1-D1A821C97CB5}
AppName=Blitz
AppVerName=Blitz
AppPublisher=Ryan Skeldon
AppPublisherURL=http://code.google.com/p/blitz31/
AppSupportURL=http://code.google.com/p/blitz31/issues/list
AppUpdatesURL=http://code.google.com/p/blitz31/downloads/list
DefaultDirName={commondocs}\Blitz
DefaultGroupName=Blitz
OutputBaseFilename=setup
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: ".\Blitz\bin\Release\Blitz.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\README.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\COPYING.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Blitz"; Filename: "{app}\Blitz.exe"
Name: "{commondesktop}\Blitz"; Filename: "{app}\Blitz.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Blitz.exe"; Description: "{cm:LaunchProgram,Blitz}"; Flags: nowait postinstall skipifsilent
