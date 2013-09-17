Real-time Filter Demo
=====================

A Nokia example application demonstrating the use of the Nokia Imaging SDK for
real-time image effects. The effects are applied to the stream received from the
camera and shown in the viewfinder. This app does not support capturing photos. 

This example application is hosted in GitHub:
https://github.com/nokia-developer/real-time-filter-demo

For more information on implementation visit Nokia Lumia
Developer's Library:
http://developer.nokia.com/Resources/Library/Lumia/#!nokia-imaging-sdk/sample-projects/real-time-filter-demo.html


1. Prerequisites
-------------------------------------------------------------------------------

* C# basics
* Windows 8
* Microsoft Visual Studio Express for Windows Phone 2012


2. Project structure and implementation
-------------------------------------------------------------------------------

The example consists basically of three key classes. The main page is your
typical phone application page implemented by a XAML file and a C# counterpart.
The main page implements the application UI including the `MediaElement` which
displays the camera viewfinder with an effect. The `MainPage` class also owns
the instances of the two other key classes: `CameraStreamSource` and
`NokiaImageEditingSDKEffects`. The `CameraStreamSource`, derived from
`MediaStreamSource`, provides the camera data. The `NokiaImageEditingSDKEffects`
implements all the effects of the application. 

2.1 Used APIs
-------------

* System.Linq
* System.Runtime.InteropServices.WindowsRuntime
* System.Threading.Tasks
* System.Windows.Media
* System.Windows.Threading
* Windows.Phone.Media.Capture
* Windows.Storage.Streams
* Nokia.Graphics
* Nokia.Graphics.Imaging
* Nokia.InteropServices.WindowsRuntime


3. Compatibility
-------------------------------------------------------------------------------

Windows Phone 8. Tested to work on Nokia Lumia 920 and Nokia Lumia 520.
Developed with Microsoft Visual Studio Express for Windows Phone 2012.

3.1 Required Capabilities
-------------------------

* `ID_CAP_ISV_CAMERA`
* `ID_CAP_MEDIALIB_AUDIO`
* `ID_CAP_MEDIALIB_PLAYBACK`

3.2 Known Issues
----------------

None.


4. Building, installing, and running the application
-------------------------------------------------------------------------------

4.1 Preparations
----------------

Make sure you have the following installed:

* Windows 8
* Windows Phone SDK 8.0

4.2 Using the WINDOWS PHONE 8 SDK
---------------------------------

1. Open the SLN file:
   File > Open Project, select the solution (.sln postfix) file
2. Select the target 'Emulator WXGA'.
3. Press F5 to build the project and run it on the Windows Phone Emulator.

4.3 Deploying to Windows Phone 8
--------------------------------

Please see official documentation for deploying and testing applications on
Windows Phone devices:
http://msdn.microsoft.com/en-us/library/gg588378%28v=vs.92%29.aspx


5. License
-------------------------------------------------------------------------------

See the license text file delivered with this project. The license file is also
available online at
https://github.com/nokia-developer/real-time-filter-demo/blob/master/Licence.txt


6. Version history
-------------------------------------------------------------------------------

* 1.0.1 Bug fix: MediaElement did not release the camera handle
* 1.0 First public release
* 0.8 First release candidate
