using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ChatterBotAPI;
using IrcDotNet;

namespace ConsoleApplication2
{
    class Program
    {
        static Dictionary<string,ChatterBotSession> sessions;
        static IrcClient irc;
        static ChatterBot bot1;
        static string name = "SomebodyElse";
        public static void Main(string[] args) {
            sessions = new Dictionary<string, ChatterBotSession>();
            ChatterBotFactory factory = new ChatterBotFactory();

            bot1 = factory.Create(ChatterBotType.CLEVERBOT);
                        
            //ChatterBot bot2 = factory.Create(ChatterBotType.PANDORABOTS, "b0dafd24ee35a477");
            //ChatterBotSession bot2session = bot2.CreateSession();
                        
            //string s = "Hi";
            irc = new IrcClient();
            irc.Connected += irc_Connected;
            irc.Registered += irc_Registered;
            irc.Error += irc_Error;
            irc.ConnectFailed += irc_ConnectFailed;
            irc.Connect("chat.freenode.net", false, new IrcUserRegistrationInfo() { NickName=name, RealName=name, UserName=name  });
            while (true) {
                Thread.Sleep(1000);
            }
        }

        static void irc_Error(object sender, IrcErrorEventArgs e)
        {
            Console.WriteLine("{0}", e.Error.ToString());
        }

        static void irc_Registered(object sender, EventArgs e)
        {
            Console.WriteLine("IRC Registered");
            irc.LocalUser.JoinedChannel += LocalUser_JoinedChannel;
            irc.LocalUser.LeftChannel += LocalUser_LeftChannel;
            irc.Channels.Join("#udoo");
        }

        static bool kicked = false;
        static void LocalUser_LeftChannel(object sender, IrcChannelEventArgs e)
        {
            kicked = true;
            irc.Channels.Join("#udoo");
        }

        static void irc_ConnectFailed(object sender, IrcErrorEventArgs e)
        {
            Console.WriteLine("{0}", e.Error.ToString());
        }

        static void irc_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("IRC Connected");
            //while (irc.Channels.Count == 0)
            //    Thread.Sleep(400);
            //irc.Channels[0].MessageReceived += Program_MessageReceived;
        }

        static void LocalUser_JoinedChannel(object sender, IrcChannelEventArgs e)
        {
            Console.WriteLine("Joined channel {0}", e.Channel);
            e.Channel.MessageReceived += Channel_MessageReceived;
            if (kicked)
            {
                kicked = false;
                irc.LocalUser.SendMessage("#udoo", "That was rude!");
            }
            else
            {
                irc.LocalUser.SendMessage("#udoo", "Hello");
            }
        }

        static void Channel_MessageReceived(object sender, IrcMessageEventArgs e)
        {

            if(e.Text.ToLower().Contains(name.ToLower()) && e.Text.ToLower().IndexOf(name.ToLower()) < name.Length+2)
            {
                string txt = e.Text.Remove(0, name.Length+1).Trim();
                Console.WriteLine("{0}: {1}", e.Source.Name, txt);
                if (!sessions.ContainsKey(e.Source.Name))
                    sessions.Add(e.Source.Name, bot1.CreateSession());
                string resp = string.Format("{0}: {1}",
                    e.Source.Name,
                    sessions[e.Source.Name].Think(txt)
                );
                irc.LocalUser.SendMessage("#udoo", resp);
                Console.WriteLine("Bot: {0}", resp);
            }
        }

    }
}
