#Door Access Control System

This WPF application allows managing door access records by storing person information and access times. Users can add, update, delete, list, and search access entries. The application provides a clean and user-friendly interface for easy management of door access logs.

Features
Add new access records with Person ID, Name, and Access Time

Update and delete existing records

List all access records in a grid view

Search records by Person ID

Display all records in a read-only DataGrid for quick overview

Intuitive UI with colored and styled buttons for better usability

Technologies Used
C#

WPF (Windows Presentation Foundation)

.NET Framework

SQL Server (for storing access records)

How to Use
Add a Record: Enter Person ID, Name, and select Access Date, then click "Add".

Update a Record: Select a record from the list, modify the details, and click "Update".

Delete a Record: Select a record and click "Delete".

List All Records: Click "List All" to refresh and display all records.

Search Records: Enter Person ID and click "Search" to find specific entries.

Database
The application connects to an SQL Server database named DoorScan. The main table used is [ACCESSRECORD], which stores:

RecordID (Primary Key)

PersonID

PersonName

AccessTime
