// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using scrub_lang;
using scrub_lang.VirtualMachine;

//copied old code out of the scrub project in order to clean it up.
//need to re-write the whole dang thing here to use actual c# testing.
Stopwatch sw = new Stopwatch();
sw.Start();
Console.WriteLine("Parsing Tests...");
Tests.TestParse();
Console.WriteLine("Compiler Tests...");
Tests.TestCompile();
//VM tests...
VMTests.RunTests();


TimeSpan ts = sw.Elapsed;
var elapsedTime = $"{ts.Seconds}s; {ts.Microseconds} Microseconds. {ts.Ticks} ticks.";
Console.WriteLine($"---\nCompleted in {elapsedTime}");