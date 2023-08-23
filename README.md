
# TT Project

## Framework/libraries used

Developed using WPF, EF Core and Bootstrap Icons.

## Project structure overview

The `Database` folder contains 3 files that represent adequate models (`Symbol.cs`, `Exchange.cs` and `Type.cs`) and one main (`DB.cs`) where the connection with the SQLite database is initialized.

`Models` folder has one file, and that is `DataGridItems.cs`, a model for data displayed on screen.

`Styles` contains 2 style files that are forked from the main styles and modified for this design.

The 3 windows (`MainWindow`, `DataDialog` and `DeleteDialog`) contain all the logic, especially `MainWindow`, in which all the heavy lifting is done.
