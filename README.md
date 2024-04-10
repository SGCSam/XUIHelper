# XUIHelper

## A suite of tools designed for reading, writing and converting XUI/XUR files, the Xbox 360's UI file format.

XUIHelper is a suite of tools written in C# that provides interfaces for reading, writing and converting XUI/XUR files that the Xbox 360 uses as its UI file format. This includes:
* Read/write support for XUR v5 (Used between 1888 - 9199)
* Near full read/write support for XUR v8 (Used between 12611 - 17559)
* Read/write support for XUI v12
* Full interoperability between XUR v5, XUR v8 and XUI v12
* Support for XUIs XML extensions so custom classes can be registered for reading and writing
* Support for ignorable properties at write time to ensure support with XuiTool
* A core functionality library (XUIHelper.Core), with both a CLI and GUI interface wrapper provided.

## Quick Start Guide (Convert XURv8 to XUI)

1) Clone the repository
2) Build XUIHelper.CLI in Visual Studio.
3) Run XUIHelper.CLI via command line with the following arguments:
  conv -s <Source XUR Path> -f "xuiv12" -o <Output XUI Path> -g "V8"

## Wiki

For documentation on using XUIHelper.CLI, see: TODO
For documentation on using XUIHelper.GUI, see: TODO
For documentation on the XML extensions system, see: TODO

## Contributors

Contributions are more than welcome! Tackling any TODOs in the "Known Issues/TODOs" section would be especially appreciated. I won't be too strict here with any sort of explicit requirements but:

  * Please try to prefer many small and consise commits in a PR than one huge commit
  * Please try to stick to the styling of the existing code base, noting the styling of indentation and spaces, prefixing private variables, etc

Thank you!

## Known Issues/TODOs

1) XUR v8's KEYD section hasn't been fully reversed. There's 6 bits of a flag byte I've been unable to reverse.
2) XUR v5's logic for whether the extended count header should be written isn't fully correct - there's a few false positives.
3) Not all XML extensions have been added for custom dash controls for both 9199 and 17559 - reversing these will allow greater compatibility.
4) Currently, the read/write property functions for XURs and XUIs only do checks for indexed properties inside the TryWriteObject functions, since the only XUI control that uses indexed properties is XuiFigureFillGradient. While this works in practice, this is *technically* wrong, and the read/write functions should be refactored to check for indexed properties in the root TryReadProperty and TryWriteProperty functions.
5) Support for ignore properties should be added for XUR files rather than just XUI files for completeness.

## Shoutouts

Huge thank you to MaesterRowen and Wondro! Your original work on XuiWorkshop was invaluable.
