# DbLocalizer

# Introduction 
This project is for developers and organizations looking for a simple way to export localizable data from, and import localized data to a SQL Server database, via a Translation Management System (TMS). The first release ships with an implementation of the Smartling plugin, which will act as a starting point for your own implementation. Over time I hope to be able to add more plugins for different TMS applications.

This is a .NET Core Web API application, with the option to add a UI of your choice, either through MVC or a static UI in the wwwroot. The advantage of making the app a service is that it can then be integrated into business processes, allowing user interaction and extensibility. It will also support automation through scheduled jobs. 

# Getting Started
The example setup makes some assumptions about the schema design of the target SQL Server Database. It assumes that there will be seperate tables for localizable data, specificaslly, for any "base" table there will be a corresponding "localized" table. e.g [dbo].[Products] and [dbo].[ProductsLocalized] that share a common key relationship.
1.	Installation process
2.	Software dependencies
3.	Latest releases
4.	API references

# Build and Test
TODO: Describe and show how to build your code and run the tests. 

# Contribute
TODO: Explain how other users and developers can contribute to make your code better. 
