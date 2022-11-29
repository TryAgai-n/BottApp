// See https://aka.ms/new-console-template for more information

// Console.WriteLine("Hello, World!");

var intCollection = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

foreach (var val in intCollection.Skip(8).Take(2).Where(x => x == 8))
{
    Console.WriteLine(val);
}

var a = 0;
a += 1;
a += -1;
Console.WriteLine(a);