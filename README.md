# ParquetViewerPlus
## Summary
Forked version of https://github.com/mukunku/ParquetViewer with some additional features and updates:
- Excel file (\*.xlsx) export using ClosedXML package (Shortcut: Ctrl + Shift + E).
- Toggle to export short date format for CSV files (Edit > Date Format > Export Time with CSV).
- Updated Nuget packages.

Special thanks to the original developer for creating this very useful tool!

Please refer to the original repo for more information.

# ParquetViewer

Simple Windows desktop application for viewing & querying Apache Parquet files. 

![Main UI](https://github.com/mukunku/ParquetViewer/blob/master/wiki_images/main_screenshot3.png)

Please also checkout the Wiki for a detailed user guide: https://github.com/mukunku/ParquetViewer/wiki

# Summary
This is a quick and dirty utility that I created to easily view Apache Parquet files on Windows desktop machines. 

If you'd like to add any new features feel free to send a pull request.

Some Key Features:
* Run simple sql-like queries on chunks of the file
* Generate ansi sql schema for opened files
* View parquet file metadata

# Limitations
This application can only open Parquet files located on the Windows machine the app is running on. It cannot connect to HDFS to read parquet data. 

Complex types such as structs, arrays and maps are not supported at this time.

# Download
Pre-compiled releases can be found here: https://github.com/mukunku/ParquetViewer/releases

Visit the Wiki for details on how to use the utility: https://github.com/mukunku/ParquetViewer/wiki

# Technical Details
The latest version of this project was written in C# using Visual Studio 2022 and .NET 4.7.2

# Acknowledgements
This utility would not be possible without: https://github.com/elastacloud/parquet-dotnet
