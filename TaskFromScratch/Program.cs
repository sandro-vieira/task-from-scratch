// See https://aka.ms/new-console-template for more information
using TaskFromScratch;

Console.WriteLine($"Starting Thread Id: {Environment.CurrentManagedThreadId}");

ScratchTask.Run(() => Console.WriteLine($"First Scratch Thread Id: {Environment.CurrentManagedThreadId}"))
    .Wait();

ScratchTask.Delay(TimeSpan.FromSeconds(3))
    .Wait();

Console.WriteLine($"Second Scratch Thread Id: {Environment.CurrentManagedThreadId}");

ScratchTask.Delay(TimeSpan.FromSeconds(3))
    .Wait();

ScratchTask.Run(() => Console.WriteLine($"Third Scratch Thread Id: {Environment.CurrentManagedThreadId}"))
    .Wait();
