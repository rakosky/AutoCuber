using AutoCuber.Cubing;
using AutoCuber.Flaming;

namespace AutoCuber
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var autoCuber = new Cuber();
            var autoFlamer = new Flamer();

            await autoCuber.Run();
            //await autoFlamer.Run();
        }
    }
}
