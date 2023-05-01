using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

namespace Curs6;

class Program
{
    class MyClass
    {
        public int ANumber;
        public int AnotherNumber { get; set; }

        public MyClass()
        {
            ANumber = 7;
            AnotherNumber = 3;
        }
    }
    
    static void Main()
    {
        // var myInstance = new MyClass();
        // Console.WriteLine($"MyClass has ANumber = {myInstance.ANumber}");

        Type myClassType = typeof(MyClass);
        var myInstance = myClassType.GetConstructor(Type.EmptyTypes)?.Invoke(null);
        if (myInstance == null)
        {
            Console.WriteLine("Bad constructor");
            return;
        }

        // MemberInfo aNumberMember = myClassType.GetMember("ANumber").First();
        FieldInfo? aNumberField = myClassType.GetField("ANumber");
        Console.WriteLine($"MyClass has ANumber = {(int)aNumberField.GetValue(myInstance)}");

        PropertyInfo? anotherNumberProperty = myClassType.GetProperty("AnotherNumber");
        MethodInfo anotherPropertyGetter = anotherNumberProperty.GetGetMethod();
        var anotherPropertyValue = (int)anotherPropertyGetter.Invoke(myInstance, null);
        Console.WriteLine($"MyClass has AnotherNumber = {anotherPropertyValue}");

    }
}

