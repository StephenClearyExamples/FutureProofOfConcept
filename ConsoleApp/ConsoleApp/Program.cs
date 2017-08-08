using System;
using System.Threading.Tasks;
using FuturePlayground;

class Program
{
    static void Main()
    {
        MainAsync().Wait();
        Console.WriteLine("Done");
        Console.ReadKey();
    }

    // IFuture constructed by async state machine
    static async IFuture WaitAsync()
    {
        await Task.Delay(1000);
    }

    // IFuture<T> constructed by async state machine
    static async IFuture<int> WaitAndReturnAsync()
    {
        await Task.Delay(1000);
        return 7;
    }

    // Inheritance
    class Parent { }
    class Child : Parent { }
    static async IFuture<Child> CreateChild()
    {
        await Task.Delay(100);
        return new Child();
    }

    static async Task MainAsync()
    {
        // IFuture awaited
        await WaitAsync();

        // IFuture<T> awaited
        Console.WriteLine(await WaitAndReturnAsync());

        // IFuture<Parent> (that actually implements IFuture<Child>) awaited
        var childFuture = CreateChild();
        IFuture<Parent> parentFuture = childFuture; // Implicit conversion from IFuture<Child> to IFuture<Parent>
        var result = await parentFuture;
        Console.WriteLine(result.GetType().Name);

        // IFuture configured and awaited
        await WaitAsync().ConfigureAwait(false);

        // IFuture<T> configured and awaited
        Console.WriteLine(await WaitAndReturnAsync().ConfigureAwait(false));

        // IFuture<Parent> (that actually implements IFuture<Child>) configured and awaited
        var childFuture2 = CreateChild();
        IFuture<Parent> parentFuture2 = childFuture2; // Implicit conversion from IFuture<Child> to IFuture<Parent>
        var result2 = await parentFuture2.ConfigureAwait(false);
        Console.WriteLine(result2.GetType().Name);
    }
}
