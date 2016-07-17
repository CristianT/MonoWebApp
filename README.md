# Mono stand alone Web Server with .Net and [dotVVM](https://www.dotvvm.com/)
## Introduction
The initial objective behind creating a stand alone web server for mono is to have a simple, low cost, low power consumption internet of things server running in a Raspberry Pi, but as it just needs mono it is applicable to other linux running devices. The main concern that I wanted to address with this is to avoid having dependencies like and internet connection or a PC server to manage the small devices inside my network, the web server in the Raspberry can not only handle the device itself but can potentially be a communication hub for other smaller network enabled devices, like Arduino or NodeMCU running around the house.

The functional project can be found in the repository, but as this is just a sample project here is a small list of steps to create it from the scratch.

NOTE: Other features like authentication, database access... should be possible following standard Owin/ASP.MVC/dotVVM tutorials with minor adaptations.

## Creating the project
* Create a new C# Console Application
* Add Nuget support in your project and add the next lines to your packages config: [File](https://github.com/CristianT/MonoWebApp/blob/master/MonoWebApp/packages.config)
* In the nuget console of the project type:
```
Update-Package -reinstall
```
* Add the startup class: [File](https://github.com/CristianT/MonoWebApp/blob/master/MonoWebApp/StartUp.cs)
* Modify your program like this one: [File](https://github.com/CristianT/MonoWebApp/blob/master/MonoWebApp/Program.cs)
* Add the ViewModels folder: [Folder](https://github.com/CristianT/MonoWebApp/tree/master/MonoWebApp/ViewModels)
* Add the Views folder: [Folder](https://github.com/CristianT/MonoWebApp/tree/master/MonoWebApp/Views)
* In the 'Copy to Output Directory' property of the files inside the folders 'Fonts', 'Scripts', 'Views' and 'Content' select 'Copy if newer'
* Compile the project and copy the output to your Linux device
* Run the application
```
mono MonoWebApp.exe
```
* Visit the page http://[linux device ip]:5000/Calculator

## Code highlights
* All paths should be constructed with the separator ‘/’ instead of the typical windows separator ‘\’:

In the views:

```
@masterPage Views/Master/Master.dotmaster
```

In the routes:

```C#
appBuilder.UseFileServer(new FileServerOptions()
{
  RequestPath = new PathString("/fonts"),
  FileSystem = new PhysicalFileSystem(@"./fonts"),
});

config.RouteTable.Add("Calculator", "Calculator", "Views/Calculator.dothtml", new { });
```
* Use at least Mono version 3.10, the default version in the Raspberry Pi repositories is 3.2.8 which gives an error when there is a request. [Link] (http://www.htpcguides.com/install-sonarr-raspberry-pi-mono-310/)
* DpapiDataProtector requires some native Win32 functions, that is why there is a custom implementation in the startup file. More information here: [1](https://github.com/IdentityServer/IdentityServer3/issues/1573), [2](http://beginor.github.io/2015/04/08/using-microsoft-owin-auth-middleware-with-mono.html), [3](http://www.ryanmelena.com/2014/10/31/thinktecture-identityserver-v3-on-mono/)
* If you want to access the page from outside the system you need to set the baseUrl in the main program to http://*:5000. This gives an error on Windows if you don’t run the application (or Visual Studio when debugging) with administrator privileges.
