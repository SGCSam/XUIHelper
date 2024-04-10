# XUIHelper.GUI Documentation

## Overview

XUIHelper.GUI is a graphical user interface wrapper around the functionality provided in the XUIHelper.Core library.

XUIHelper.GUI is a WPF application using NXEControls, a custom control library that emulates the visual styling of the New Xbox Experience, the dashboard present on the Xbox 360 from 2008 - 2010.

## Registering Extensions

To register extensions in XUIHelper.GUI, you can use the "XML Extensions Manager" option via the main menu.

Alternatively, if you wish to perform this manually:
1) Create a folder named "Extensions" in the same directory as the program .exe
2) Create a folder inside "Extensions" named as your group name
3) Inside your group name folder, place as many .xml extension files as you'd like

## Single Conversion

To convert a single file:
1) Navigate to "Single Conversion" from the main menu.
2) Use the "Source File Path" browse button on the right to browse to a source file path.
3) Use the "Destination File Path" browse button on the right to browse to a destination file path.
4) (OPTIONAL) Disable ignore properties if you would like to ensure the properties are NOT ignored
5) Use the Output File Type dropdown to select the file type you'd like to convert to
6) Select your XML extensions group that you'd like to use for the conversion using the Extensions Group dropdown
7) (OPTIONAL) Set your logging level with the Log Verbosity dropdown. Any value other than "None" will place a log file in the same directory as the destination file path.
8) Press the Convert button. After a few moments, a message box will indicate the status of the conversion.

## Mass Conversion

To mass convert an entire directory:
1) Navigate to "Mass Conversion" from the main menu.
2) Use the "Source Directory" browse button on the right to browse to a source directory.
3) Use the "Destination Directory" browse button on the right to browse to a destination directory.
4) (OPTIONAL) Disable ignore properties if you would like to ensure the properties are NOT ignored
5) Use the Output File Type dropdown to select the file type you'd like to convert to
6) Select your XML extensions group that you'd like to use for the conversion using the Extensions Group dropdown
7) (OPTIONAL) Set your logging level with the Log Verbosity dropdown. Any value other than "None" will place a log file in the the destination directory.
8) Press the Convert button. A progress page will pop up indicating the overall progress of the mass conversion, with a sucess rate message displayed once completed.
