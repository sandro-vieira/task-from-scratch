// See https://aka.ms/new-console-template for more information
using TaskFromScratch;

Console.WriteLine($"Current Thread Id: {Thread.CurrentThread.ManagedThreadId}");

ScratchTask.Run(() => Console.WriteLine($"Current Scratch Thread Id: {Thread.CurrentThread.ManagedThreadId}"));

Console.ReadLine();