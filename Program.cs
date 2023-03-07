class C
{
    public string Name { get; set; }
} 

struct S {}


class Base
{
}

interface IInterface
{
}

interface IInterface2
{
}

abstract class AbstractBase
{
}

class Derived : Base, IInterface, IInterface2
{
}

record R(string Name, int Age);

class Program
{
    public static void Main(string[] args)
    {
        C c1 = new C()
        {
            Name = "John"
        };
        
        C c2 = new C()
        {
            Name = "John"
        };
        
        R r1 = new R("John", 20);
        R r2 = new R("John", 20);
        
        Console.WriteLine(r1 == r2);
        Console.WriteLine(c1 == c2);
        Console.WriteLine(r1);
        Console.WriteLine(c1);
    }
}
