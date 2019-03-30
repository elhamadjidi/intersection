using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace traffic_light
{
    class Program
    {

       
        static void Main(string[] args)
        {
            
            
            
            Console.WriteLine("enter intersection X");
            int X=int.Parse(Console.ReadLine());
            Console.WriteLine("enter intersection Y");
            int Y = int.Parse(Console.ReadLine());
            
            light L1 = new light(1,30,15,"green");
            light NL1 = new light(3,0,0,"unknown");// cheraqe hamsaye dar 4 rahe kenari
            
            light L2 = new light(2,30,15,"red");
            light NL2 = new light(4,0,0, "unknown");// cheraqe hamsaye dar 4 rahe kenari
            
            light L3 = new light(3,60,15,"red");
            light NL3 = new light(1,0,0, "unknown");// cheraqe hamsaye dar 4 rahe kenari
            
            light L4 = new light(4,90,15,"red");
            light NL4 = new light(2,0,0, "unknown");// cheraqe hamsaye dar 4 rahe kenari
            
            intersection inter = new intersection(X,Y,L1, L2, L3, L4, NL1, NL2, NL3, NL4);
            
            Timer timer_updateNLs = new Timer(updateNLs, inter, 0, 30000);

            Timer timer_pushInfo = new Timer(pushInfo, inter, 0, 10000);

            Console.ReadLine();




        }
        /// <summary>
        /// چراغ های همسایه را بروز میکند
        /// </summary>
        /// <param name="o">شی چهار راه مورد نظر</param>
        public static void updateNLs(object o)
        {
            Console.Clear();
            intersection inter = (intersection)o;
            Console.WriteLine("position: {0},{1}", inter.X.ToString(), inter.Y.ToString());
            
            updateNL(inter.NL1, inter.X, inter.Y);
            updateNL(inter.NL2, inter.X, inter.Y);
            updateNL(inter.NL3, inter.X, inter.Y);
            updateNL(inter.NL4, inter.X, inter.Y);
        }


        /// <summary>
        /// اطلاعات چراغ همسایه را از صف مربوط با آن چراغ برمیدارد
        /// </summary>
        /// <param name="L">چراغ مورد نظر برای بروز رسانی</param>
        /// <param name="X">مختصات چراغ فعلی X پارامتر</param>
        /// <param name="Y">مختصات چراغ فعلی Y پارامتر</param>
        public static void updateNL(light L,int X, int Y)
        {
            
            string QName = "";

            switch (L.id)
            {
                case 1:
                    QName = "intersection" + (X).ToString() + (Y + 1).ToString();

                    break;
                case 2:
                    QName = "intersection" + (X - 1).ToString() + (Y).ToString();
                    break;
                case 3:
                    QName = "intersection" + (X).ToString() + (Y - 1).ToString();
                    break;
                case 4:
                    QName = "intersection" + (X + 1).ToString() + (Y).ToString();
                    break;

                default:
                    break;
            }
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var consumerNL = new QueueingBasicConsumer(channel);
                  
                  

                    try
                    {
                        Console.WriteLine("Looking for L{0} in {1}", L.id.ToString(), QName);
                        
                        var msg = channel.BasicGet(QName, true);
                        
                        var body = msg.Body;
                        string task = Encoding.UTF8.GetString(body);
                        string[] parts = task.Split(',');
                        
                        L.traffic = int.Parse(parts[L.id-1]);
                        L.time = int.Parse(parts[L.id-1+4]);
                        L.state = parts[L.id - 1 + 8];
                        L.ANT= (int.Parse(parts[0])+ int.Parse(parts[1])+ int.Parse(parts[2])+ int.Parse(parts[3]))/4;
                        Console.WriteLine("L{0} in {1} : traffic={2} , time={3}s , state={4} ", L.id.ToString(), QName,L.traffic.ToString(), L.time.ToString(), L.state);

                        
                        
                    }
                    catch (Exception)
                    {

                        Console.WriteLine(QName + " not exists!");
                       
                        
                    }


                }
            }
            
           
        }
        public static void pushInfo(object o)
        {
            
            intersection inter = (intersection)o;

            updateSelfInfo(inter, 10);

            light L1 = inter.L1;
            light L2 = inter.L2;
            light L3 = inter.L3;
            light L4 = inter.L4;
            int X = inter.X;
            int Y = inter.Y;
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    string QName = "intersection" + X.ToString() + Y.ToString();
                    var inter_queue = channel.QueueDeclare(QName, false, false, false, null);
                    channel.QueuePurge(QName);

                    string info = L1.traffic + "," + L2.traffic + "," + L3.traffic + "," + L4.traffic + "," +
                                  L1.time + "," + L2.time + "," + L3.time + "," + L4.time + "," +
                                  L1.state + "," + L2.state + "," + L3.state + "," + L4.state;
                    var re = Encoding.UTF8.GetBytes(info);

                    Console.WriteLine(info);
                    channel.BasicPublish("", QName, null, re);
                    channel.BasicPublish("", QName, null, re);
                    channel.BasicPublish("", QName, null, re);
                    channel.BasicPublish("", QName, null, re);


                }
            }
        }
        /// <summary>
        /// وضعیت چراغ جاری را بروز میکند
        /// </summary>
        /// <param name="inter">چراغ جاری</param>
        /// <param name="timeStep">گام زمانی کاهش زمان</param>
        public static void updateSelfInfo(intersection inter,int timeStep)
        {
            inter.L1.time -= timeStep;
            inter.L2.time -= timeStep;
            inter.L3.time -= timeStep;
            inter.L4.time -= timeStep;
            int min_threshold = 5;
            int max_threshold = 40;
            Random r = new Random();


            //generate traffic
            //l1
            if (inter.L1.state == "red")
            {
                inter.L1.traffic += r.Next(0, 3);
            }
            else if (inter.L1.state == "green")
            {
                inter.L1.traffic = Math.Max(0, inter.L1.traffic - r.Next(0, 12) + r.Next(0, 3));
            }
            else
            {
                inter.L1.traffic = Math.Max(0, inter.L1.traffic - r.Next(0, 3) + r.Next(0, 6));
            }

            ///l2
            if (inter.L2.state == "red")
            {
                inter.L2.traffic += r.Next(0, 3);
            }
            else if (inter.L2.state == "green")
            {
                inter.L2.traffic = Math.Max(0, inter.L2.traffic - r.Next(0, 12) + r.Next(0, 3));
            }
            else
            {
                inter.L2.traffic = Math.Max(0, inter.L2.traffic - r.Next(0, 3) + r.Next(0, 6));
            }

            //l3
            if (inter.L3.state == "red")
            {
                inter.L3.traffic += r.Next(0, 3);
            }
            else if (inter.L3.state == "green")
            {
                inter.L3.traffic = Math.Max(0, inter.L3.traffic - r.Next(0, 12) + r.Next(0, 3));
            }
            else
            {
                inter.L3.traffic = Math.Max(0, inter.L3.traffic - r.Next(0, 3) + r.Next(0, 6));
            }

            //l4
            if (inter.L4.state == "red")
            {
                inter.L4.traffic +=   r.Next(0, 3);
            }
            else if (inter.L4.state == "green")
            {
                inter.L4.traffic = Math.Max(0, inter.L4.traffic - r.Next(0, 12) + r.Next(0, 3));
            }
            else
            {
                inter.L4.traffic = Math.Max(0, inter.L4.traffic - r.Next(0, 3) + r.Next(0, 6));
            }


            /*
            inter.L1.traffic = inter.L1.state == "red" ? inter.L1.traffic + r.Next(0, 3) : Math.Max(0, inter.L1.traffic - r.Next(0, 6) + r.Next(0, 3));
            inter.L2.traffic = inter.L2.state == "red" ? inter.L2.traffic + r.Next(0, 3) : Math.Max(0, inter.L2.traffic - r.Next(0, 6) + r.Next(0, 3));
            inter.L3.traffic = inter.L3.state == "red" ? inter.L3.traffic + r.Next(0, 3) : Math.Max(0, inter.L3.traffic - r.Next(0, 6) + r.Next(0, 3));
            inter.L4.traffic = inter.L4.state == "red" ? inter.L4.traffic + r.Next(0, 3) : Math.Max(0, inter.L4.traffic - r.Next(0, 6) + r.Next(0, 3));
            */




            // terafice poshte cheraqe sabz kam ya 0 ast
            bool rule1_1 = inter.L1.state == "green" && ((inter.L1.traffic < min_threshold && inter.L2.traffic > max_threshold) || (inter.L1.traffic == 0));
            bool rule1_2 = inter.L2.state == "green" && ((inter.L2.traffic < min_threshold && inter.L3.traffic > max_threshold) || (inter.L2.traffic == 0));
            bool rule1_3 = inter.L2.state == "green" && ((inter.L3.traffic < min_threshold && inter.L4.traffic > max_threshold) || (inter.L3.traffic == 0));
            bool rule1_4 = inter.L2.state == "green" && ((inter.L4.traffic < min_threshold && inter.L1.traffic > max_threshold) || (inter.L4.traffic == 0));

            if (rule1_1 || rule1_2 || rule1_3 || rule1_4 )
            {
                Console.Write("rule1 : decrease all times");
                inter.L1.time -= 5;
                inter.L2.time -= 5;
                inter.L3.time -= 5;
                inter.L4.time -= 5;

            }



            //terafice poshte cheraqe sabz ziyad ast va 4 rahe hamsaye khalvat ast
            bool rule2_1 = inter.L1.state == "green" && inter.L1.traffic > max_threshold && inter.NL1.ANT < (min_threshold + max_threshold) / 2 && inter.L1.allowIncrease;
            if (rule2_1) inter.L1.allowIncrease = false;
            bool rule2_2 = inter.L2.state == "green" && inter.L2.traffic > max_threshold && inter.NL2.ANT < (min_threshold + max_threshold) / 2 && inter.L2.allowIncrease;
            if (rule2_2) inter.L2.allowIncrease = false;
            bool rule2_3 = inter.L3.state == "green" && inter.L3.traffic > max_threshold && inter.NL3.ANT < (min_threshold + max_threshold) / 2 && inter.L3.allowIncrease;
            if (rule2_3) inter.L3.allowIncrease = false;
            bool rule2_4 = inter.L4.state == "green" && inter.L4.traffic > max_threshold && inter.NL4.ANT < (min_threshold + max_threshold) / 2 && inter.L4.allowIncrease;
            if (rule2_4) inter.L4.allowIncrease = false;

            if (rule2_1 || rule2_2 || rule2_3 || rule2_4 )
            {
                Console.Write("rule2 : increase all times");
                inter.L1.time += 5;
                inter.L2.time += 5;
                inter.L3.time += 5;
                inter.L4.time += 5;

            }


            // ترافیک این 4 راه و همسایه های آن کم است
            bool rule3 = ((inter.L1.traffic + inter.L2.traffic + inter.L3.traffic + inter.L4.traffic) / 4 +
                           inter.L1.ANT + inter.L2.ANT + inter.L3.ANT + inter.L4.ANT) / 5 < (max_threshold+min_threshold)/5;

            if (rule3 && inter.L1.state != "blink")// start blink mode
            {
                Console.WriteLine("start blink mode");
                inter.L1.state = "blink";
                inter.L2.state = "blink";
                inter.L3.state = "blink";
                inter.L4.state = "blink";
            }


            if(!rule3 && inter.L1.state == "blink")//exit blink mode
            {
                Console.WriteLine("end blink mode");
                inter.L1.state = "green";
                inter.L2.state = "red";
                inter.L3.state = "red";
                inter.L4.state = "red";
                inter.L1.time = 30;
                inter.L2.time = 30;
                inter.L3.time = 60;
                inter.L4.time = 90;
            }
            



            if (inter.L1.time < 0 && inter.L1.state=="green" )
            {
                inter.L1.time = 90;
                inter.L2.time = 30;
                inter.L1.state = "red";
                inter.L1.allowIncrease = true;
                inter.L2.state = "green";
           
            }
            if (inter.L2.time < 0 && inter.L2.state == "green")
            {
                inter.L2.time = 90;
                inter.L3.time = 30;
                inter.L2.state = "red";
                inter.L2.allowIncrease = true;
                inter.L3.state = "green";
            }
            if (inter.L3.time <0 && inter.L3.state == "green")
            {
                inter.L3.time = 90;
                inter.L4.time = 30;
                inter.L3.state = "red";
                inter.L3.allowIncrease = true;
                inter.L4.state = "green";
            }
            if (inter.L4.time < 0 && inter.L4.state == "green")
            {
                inter.L4.time = 90;
                inter.L1.time = 30;
                inter.L4.state = "red";
                inter.L4.allowIncrease = true;
                inter.L1.state = "green";
            }
           


        }
    }
}
