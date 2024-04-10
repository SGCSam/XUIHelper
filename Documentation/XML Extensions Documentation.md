# XML Extensions Documentation

## Overview

XML Extensions are XUI's method of allowing UI authors to extend XUI with their own additional custom UI controls. 
Each control is considered an XUIClass where each XUIClass has a name and a collection of Property Definitions. Classes can also inherit as children from other base classes.
Each property definition contains a name and a data type.

For example, the XuiText class contains property definitions for "Text" and "TextColor" which are of type "string" and "color" respectively.
XuiText itself inherits from XuiElement which is the base class of all controls. This allows XuiText to have a "Width", "Height", "Position, etc

XUIHelper provides a collection of XML extensions that can be used to parse XUI/XUR files. Not all controls have been reversed however. In fact, missing extensions are the most common reason an XUI/XUR file will fail to parse. In such cases, this is clearly indicated as such in the log files.

## Extension Groups

Throughout the 360's life cycle, chiefly between NXE (9199) and Kinect NXE (12611), there have been changes to the property definitions of the core XUI controls used. For instance, XuiControl implements a "UseNuiAsMouse" element for Kinect functionality. These changes are one of the primary reasons as to why XUR v8 files won't open in XuiTool.

However, since XUIHelper's goal has been to support as many XUR versions as possible, the concept of "Extension Groups" have been added to the library. This allows a collection of XML extensions to be paired together in a group, where the active group to use is specified at convert time.
This modularity allows you to easily swap between groups "V5" and "V8" when converting XUR v5 and XUR v8 respectively, since XUR v5 used in dashboard 9199 and earlier do not implement any of the Kinect Nui property definitions.

## Ignore Properties

For some unknown reason, there are certain property definitions that prevent XuiTool from opening with a "The property was not found" error if they are included in an XUI file. 
XUIHelper implements an IgnoreProperties section of its extensions format, so you can specify property definitions of specific classes that should be ignored at write time to allow these files to open successfully. The most common ones are already included.

## Registering Extensions

To register extensions in XUIHelper.CLI and XUIHelper.GUI:
1) Create a folder named "Extensions" in the same directory as the program .exe
2) Create a folder inside "Extensions" named as your group name
3) Inside your group name folder, place as many .xml extension files as you'd like

XUIHelper.GUI also implements a graphical XML Extensions Manager system that you can use.
