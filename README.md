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

What will it ask of you?
------------------------
All changes to your database should happen through TSQL scripts. Either you have to generate them using SSMS or some other tool, or you should write them by hand. For the most part, we have found that writing them by hand works the best for alterations to existing DB objects, such as adding columns or changing constraints or indexes, etc.
The only time we generate scripts is for new objects, as when we add one or more tables. In that case we usually design them in a diagram, and generate create scripts for them afterward.
Either way, you need to end up with a script.
You cannot mix DML and DDL in the same batch, so DbScriptomate expects only one of the 2 in a given script file.
You will copy the content of your scripts into the script files that DbScriptomate generates for you off predefined templates.

The templates are written in such a way that each script file is transactional, and has the same semantics whether it is executed manually through SSMS, or automatically by DbScriptomate. Either way, it will rollback if the script file could not be fully applied, and if successful, it will log the fact that the script was applied to the DB so that the same script will not ever be attempted again on the same DB. (That's cool, in case you didn't realise)


How does it work?
-----------------
* There is *DbScriptomate.NextSequenceNumber.Service.exe*
This is the central "web" service that you can smack on to a server somewhere on your network. It's sole purpose in life is to dish out new sequence numbers in a concurrency safe way as devs require new DB scripts. So you can just ask it, give me the next number in the sequence for the key "YourVeryCoolDbName", and you will get it.
DbScriptomate.NextSequenceNumber.Service.exe can run both as a console app or be installed as a Windows service. (It uses topshelf - http://topshelf-project.com/)
We use a central service to hand out script numbers so they are unique across different source control branches. This, together with SQL DB Snapshots, allows you to version your database with your code in quite a simple way)

* Then there is *DbScriptomate.exe*
This has two functions. It is the client you use to ask the NextSequenceNumber service for, well, the next sequence number, and it will generate a script off a script template for you with that sequence number baked in.
And it can also check a specified DB for scripts that have not yet been applied to it, and optionally apply missing ones.
This is what each developer will run when he or she wants to apply scripts from a source control branch to a specific database or databases.
And it is also what will be run by your CI server during an automated deployment.
It has both an interactive and commandline mode.

* We also have a table we create in your DB (dbo.DbScripts). This stores the information of each script that has been applied to that specific DB. Not the actual content of the script, but just the meta information. Script number, date applied, author initials, login used, etc.


Why does it exist?
------------------
We were running in an environment where we make extensive use of SQL Merge Replication. For those who have tried this, you will know that replication puts a number of additional constraints on what you can do to a database.
We haven't found an existing tool that fit our needs. (I'm not going to recap all the reasons here or tools we investigated)

What we want to add
-------------------
* Generally get the code cleaner.
* Make commandline arg parsing more flexible and robust.
* Adding the option to take a DB snapshot before scripts are applied on SQL Server editions that support snapshots, plus a snapshot restore option if not all the scripts are applied successfully.
