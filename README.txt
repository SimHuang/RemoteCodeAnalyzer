--------------------------------------------------------------------------------------
HOW TO RUN IN VISUAL STUDIO
--------------------------------------------------------------------------------------
1) Load entire RemoteCodeAnalyzer project in visual studio
2) Launch both server and client project

--------------------------------------------------------------------------------------
HOW TO RUN IN COMMAND LINE
--------------------------------------------------------------------------------------
1) Open command line as administrator

2) Traverse to RemoteCodeAnalyzer directory which contains compile.bat

3) Execute command 'compile.bat' which will compile server and client project

4) Traverse to RemoteCodeAnalyzer\RemoteCodeAnalyzer\bin\Debug which contains the
server .exe.  execute command 'RemoteCodeAnalyzer.exe' which will launch the server service.
(Must be admin in console). You must traverse to the debug directory in console first,
or it may cause some server/client connection problem

5) In a new console, traverse to RemoteCodeAnalyzer root directory, which
contains the runClient.bat. Execute command 'runClient.bat' which will launch the client side application.

--------------------------------------------------------------------------------------
Software Information
--------------------------------------------------------------------------------------
This project consists on a WCF server and a WPF client. A user can perform action such as download, upload,
authenticate, retrieve files, and grant user. Everytime the user request a service it will send a 
message object to the server. 

--------------------------------------------------------------------------------------
HOW TO USE
--------------------------------------------------------------------------------------

Authentication Feature
----------------------
	Under the Authentication tab, you can enter username and password to authentication. The list of authenticated users are
	located in RemoteCodeAnalyzer/Authentication/user.xml. You must authenticate before accessing the other features.
	
	Avaliable users:
	username - samsmith, password -123456
	username - john, password -123456
	username - jane, password -123456

Retrieve List of File Names
----------------------------
	After you have authenticated, you can go to the Directories Tab. This is where you can retrieve the files for a directory. The directory 
	concept is base on the same name as the user. The 'samsmith', 'john', 'jane' are all directories base on the username. If you enter 
	a directory and click retrieve, the list of file names will be returned to you. 
	
Grant File Access to other users
--------------------------------
	After you have authenticated, you can go to the Files tab on WPF. A list of files the logged in user owns will be displayed. You must enter a 
	valid username (base on the user.xml) and click grant permission to grant the other user permission to downlad the file. If the username is
	invalid permission will not be granted. This will update the user.xml with new permission information
	
Upload File
-----------
	All files used for upload must be stored in \RemoteCodeAnalyzer/RemoteCodeAnalyzerClient\ToSend directory. One you have stored you file 
	for upload in that directory, go to the Up/Down tab in WPF. Enter the exact file name from the ToSend directory. Click the 'Upload File'. The 
	file will be uploaded and stored in RemoteCodeAnalyzer/RemoteCodeAnalyzer\FileStorage which is the server side.
	
Download File
-------------
	All files that can be downloaded from the server must be stored on the server side under RemoteCodeAnalyzer/RemoteCodeAnalyzer\FileStorage. Once you have 
	authenticated, you can go to the Up/Down tab and and enter a existing filename located in FileStorage, and click the Download button. This will save the file in 
	RemoteCodeAnalyzer\RemoteCodeAnalyzerClient\Download.