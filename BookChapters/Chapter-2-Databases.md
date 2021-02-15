# Chapter 2 Databases

Almost all applications interface with data stores of one type or another.  In this chapter we'll put aside file stores, and concentrate on databases.  While we have seen the growth of none SQL databases in recent years, I'm going to concentrate here on SQL databases.  That being said, I'm not dismissing non SQL databases.  The data layer design advocated in this book can be modified to fit with any data store.  You change out the database layer or a database layer that fits the underlying data store.

In the ideal word you would design your database from scratch, using a table design and naming convention of your choosing.  That is often not the case.  You have to work with existing databases that someone else designed and controls, where you wil only have designed limited access.  For this and several other reasons, I'm not a great advocate of handing over full control of my database layer operations to Entity Framework.  The other reasons are based in my past.  I'm a grizzy old programmer, database integrity and keeping applications away from your tables was hammered in hard at an informative age.  I'm a readonly views and stored procedures man to the core.
 
For the above stated reasons, the database access layer design used in this book uses Views and Stored Procedures.  I don't throw out Entity Framework, just don't use it in it's entirety.  More later.

As this book is about Blazor, I'll not dive too deeply on the SQL database side.  There's a SQL script in the Github Repo that will build out the database tables, views, and stored procedures, and populate the tables with data.

Once you run the script you should have a database that looks like this:

![view of database](../images/SQL-Database-View.png)

## Security

Need to work on database security section here