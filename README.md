# SwarmForge
Swarm forge is a windows application designed to ease the testing of different particle swarm parameter value combinations and setups in order to explore result differences in facility location problems (with p-medain and p-center solution options).

Some of the options included are:
-dataset reconstruction
-particle swarm movement limitations adjustments
-particle swarm orientation and tendency adjustments
-different algorithm stop options
-display of multiple resulted metrics

It is supposed to help in narrowing down parameter option for different problems trough testing on different datasetes (40 of them) which are included in the project and are acquired from OR-Library [Copyright (c) (2010) (J E Beasley}](http://people.brunel.ac.uk/~mastjjb/jeb/orlib/legal.html).

## Basic information on solution shape and origin
This git contains complete fileset of a Visual Studio solution (.sln) project (x64, visual C#, windows forms application).
Starting and editing of the project in this form requires Micorsoft Windows, suitable version of [Visual Studio](https://www.visualstudio.com/) (VS2015 and VS 2017 certany work), suitable x64 machine, and a suitable version of Microsoft .NET framework (4.5 or higher).

## Folders and files

"SwarmForge.sln" - Visual studio solution project file

"SwarmForge" folder - main folder containing the project (containg 3 types of files/folders) >>>>>>
  Folders required for the application installation to run properly, and must be contained in the distribution >>>
      "data" folder - contains 40 datasets (.txt files) from the OR library;
      "opt" folder - contains 2 .txt files with solutions of 40 problems from datasets above, also from theOR library;
      "log" folder - will contain potential log files in the application installation directory;  
    
  Files/Folders required for the VS solution project to run properly >> >>>
      "Properties" folder - folder containig some visual studio project properties files;
      "obj" folder - contains files for debug and release of the current application;
      "App.config" file that contains the app configuration;
      "SwarmForge.csproj" file that contains the project information;
      "SwarmForge.resx" file;
   
   Files that contain author code >>>
      "Program.cs" file - is the starting program file with whole purpose to start the SwarForge application;
      "SwarmForge.cs" file - is the main application file containing author code;
      "SwarmForge.Designer.cs" file - is the file containing definition, outline and properties of the complete application GUI.

## Contributing and versioning
Currently the code has only one version (this one) and only one contributor (me), but others are welcome to join in.

## Licence
MIT License

Copyright (c) [2017] [Mihailo Škorić]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
