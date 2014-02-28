DbScriptomate
=============

Automated SQL Server database change deployments and versioning.

What does it do?
----------------
In short, it lets you:
1. Manage DB alterations an a controlled and repeatable way.
2. Version databases with your source code.
3. Propagate these DB versions to development team members running local databases.
4. Automate testing, staging and production deployments with your Continuous Integration or Deployment server.

Why does it exist?
------------------
We were running in an environment where we make extensive use of SQL Merge Replication. For those who have tried this, you will know that replication puts a number of additional constraints on what you can do to a database.
We haven't found an existing tool that fit our needs. (I'm not going to recap all the reasons here or tools we investigated)

