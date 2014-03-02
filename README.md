DbScriptomate
=============

Automated SQL Server database change deployments and versioning.

Our awesome company, Inivit Systems, let us share this project with the world in case it might help other dev teams do things better.

What does it do?
----------------
In short, it lets you:
* Manage DB alterations an a controlled and repeatable way.
* Version databases with your source code.
* Propagate these DB versions to development team members running local databases.
* Automate testing, staging and production deployments with your Continuous Integration or Deployment server.

How does it work?
-----------------
There is
*DbScriptomate.NextSequenceNumber.Service.exe*
This is the central "web" service that you can smack on to a server somewhere on your network. It's sole purpose in life is to dish out new sequence numbers in a concurrency safe way as devs require new DB scripts. So you can just ask it, give me the next number in the sequence for the key "YourVeryCoolDbName", and you will get it.
DbScriptomate.NextSequenceNumber.Service.exe can run both as a console app or be installed as a Windows service. (It uses topshelf - http://topshelf-project.com/)

Then there is
*DbScriptomate.exe*
This has two functions. It is the client you use to ask the NextSequenceNumber service for, well, the next sequence number, and it will generate a script off a script template for you with that sequence number baked in.
And it can also check a specified DB for scripts that have not yet been applied to it, and optionally apply missing ones.
This is what each developer will run when he or she wants to apply scripts from a source control branch to a specific database or databases.
And it is also what will be run by your CI server during an automated deployment.
It has both an interactive and commandline mode.

Why does it exist?
------------------
We were running in an environment where we make extensive use of SQL Merge Replication. For those who have tried this, you will know that replication puts a number of additional constraints on what you can do to a database.
We haven't found an existing tool that fit our needs. (I'm not going to recap all the reasons here or tools we investigated)

What we want to add
-------------------
* Generally get the code cleaner.
* Make commandline arg parsing more flexible and robust.
* Adding the option to take a DB snapshot before scripts are applied on SQL Server editions that support snapshots, plus a snapshot restore option if not all the scripts are applied successfully.
