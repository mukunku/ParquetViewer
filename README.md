# ParquetViewer
Simple Windows desktop application for viewing & querying Apache Parquet files. 

![Main UI](https://github.com/mukunku/ParquetViewer/blob/master/wiki_images/main_screenshot3.png)

Please also checkout the Wiki for a detailed user guide: https://github.com/mukunku/ParquetViewer/wiki

# Summary
This is a quick and dirty utility that I created to easily view Apache Parquet files on Windows desktop machines. 
The Parquet.NET library that does all the actual work is not heavily optimized so performance within this application
is far from ideal.

This application can only open Parquet files located on the Windows machine the app is running on. It cannot connect to HDFS to read parquet data. If you would like to add this feature please feel free to do so..

# Download
If you'd like to use a pre-compiled EXE instead of compiling the project yourself, please see the release folder: https://github.com/mukunku/ParquetViewer/tree/master/release

Visit the Wiki for details on how to use the utility: https://github.com/mukunku/ParquetViewer/wiki

# Technical Details
The project was written in C# using Visual Studio 2013 and .NET 4.5

If you'd like to build the project yourself you only need to download the src folder.

# Acknowledgements
This utility would not be possible without: https://github.com/elastacloud/parquet-dotnet
