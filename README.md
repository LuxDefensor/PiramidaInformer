# PiramidaInformer
This informer reads info from database according to xml tasks file, draws plots from data and then sends those plots vie email.

The database contains results from electric meters. Every morning an external scheduler launches this program.
The program reads tasks from xml file and starts a cycle through them
For each task it reads certain data from the database and draws plots on bitmaps which then saves as jpegs on a hard drive
After all tasks are done it sends all jpegs with plots to designated emails.
