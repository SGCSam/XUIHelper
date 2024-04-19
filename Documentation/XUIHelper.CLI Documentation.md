# XUIHelper.CLI Documentation

## Overview

XUIHelper.CLI is a command line interface wrapper around the functionality provided in the XUIHelper.Core library.

## Registering Extensions

To register extensions in XUIHelper.CLI:
1) Create a folder named "Extensions" in the same directory as the program .exe
2) Create a folder inside "Extensions" named as your group name
3) Inside your group name folder, place as many .xml extension files as you'd lik

## Commands

`about` - Display information about XUIHelper.CLI.

`conv` - Convert an individual file to one of the XU supported formats  
&emsp;`-f <xurv5/xurv8/xuiv12>` - The format to convert to. Must be either "xurv5", "xurv8" or "xuiv12"  
&emsp;`-o <Path>` - The output file path  
&emsp;`-g <Group Name>` - The name of the XML extensions group to use for the conversion  
&emsp;`-s <Path>` - The input source file path  
&emsp;`(Optional) -i` - Used to disable the ignore properties for the conversion (the properties will NOT be ignored)  
&emsp;`(Optional) -l <Path>` - The path to output a log file to  
&emsp;`(Optional) -v <info/verbose>` - The logging level to use for the log file. Must be either "info" or "verbose"  

`massconv` - Convert an entire directory of files to one of the XU supported formats  
&emsp;`-s <Path>` - The input source directory  
&emsp;`-f <xurv5/xurv8/xuiv12>` - The format to convert to. Must be either "xurv5", "xurv8" or "xuiv12"  
&emsp;`-o <Path>` - The output directory  
&emsp;`-g <Group Name>` - The name of the XML extensions group to use for the conversion  
&emsp;`(Optional) -i` - Used to disable the ignore properties for the conversion (the properties will NOT be ignored)  
&emsp;`(Optional) -l <Path>` - The path to output a log file to  
&emsp;`(Optional) -v <info/verbose>` - The logging level to use for the log file. Must be either "info" or "verbose". Verbose is NOT recommended, as it has the potential to create huge log files.  
