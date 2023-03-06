namespace Lab1;

using System;

class A
{
    public void Foo()
    {
        Console.WriteLine("Hello from Foo()");
    }
}

struct S
{
    public void Boo()
    {
        Console.WriteLine("Hello from Boo()");
    }
}

static class Program
{
    public static void Main(string[] args)
    {
        int i = 3;
        bool b = false;
        float f = 3.14f;
        double d = 2.71;
        char c = 'a';

        string str = "abc";
        var str2 = str + "def";
        
        A a = new();
        a.Foo();

        {
            S s = new();
            s.Boo();
        }

        Console.WriteLine("Hello, World!");
    }
}