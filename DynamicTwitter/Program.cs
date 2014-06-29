using System;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicTwitter
{
 

    class Program
    {
        static void Main(string[] args)
        {
            dynamic dynamicTwitter = new DynamiTwitterClient("yourKey","yourSecret");

            var tweet = dynamicTwitter.statuses.show.id("483247449549864961");
            Console.WriteLine(tweet);
            
            var timeLine = dynamicTwitter.statuses.user_timeline.screen_name("ibezuglyi");
            Console.WriteLine(timeLine);
            Console.ReadKey();

        }
    }
}
