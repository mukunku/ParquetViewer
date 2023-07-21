---
name: Bug report
about: Create a report to help us improve
title: "[BUG] "
labels: bug
assignees: ''

---

**Parquet Viewer Version**
What version of Parquet Viewer are you experiencing the issue with?

**Where was the parquet file created?**
Apache Spark, Hive, Java, C#, pyarrow, etc.

**Sample File**
Upload a sample file so the issue can be debugged!

**Describe the bug**
A clear and concise description of what the bug is.

**Screenshots**
If applicable, add screenshots to help explain your problem.

**Additional context**
Add any other context about the problem here.

Note: This tool relies on the [parquet-dotnet](https://github.com/aloneguid/parquet-dotnet) library for all the actual Parquet processing. So any issues where that library cannot process a parquet file will not be addressed by us. Please open a ticket on that library's repo to address such issues.
