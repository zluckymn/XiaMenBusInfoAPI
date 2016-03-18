using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 多线程测试
{
    class Program
    {
        static void Main(string[] args)
        {

            Task parent = new Task(() =>
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                var tf = new TaskFactory<Int32>(cts.Token, TaskCreationOptions.AttachedToParent, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

                //创建并启动3个子任务
                var childTasks = new[] {
                tf.StartNew(() => StartCode(cts.Token, 10000)),
                tf.StartNew(() => StartCode(cts.Token, 20000)),
                tf.StartNew(() => StartCode(cts.Token, Int32.MaxValue))  // 这个会抛异常
           };

                // 任何子任务抛出异常就取消其余子任务
                for (Int32 task = 0; task < childTasks.Length; task++)
                    childTasks[task].ContinueWith(t => cts.Cancel(), TaskContinuationOptions.OnlyOnFaulted);

                // 所有子任务完成后，从未出错/未取消的任务获取返回的最大值
                // 然后将最大值传给另一个任务来显示最大结果
                tf.ContinueWhenAll(childTasks,
                   completedTasks => completedTasks.Where(t => !t.IsFaulted && !t.IsCanceled).Max(t => t.Result),
                   CancellationToken.None)
                   .ContinueWith(t => Console.WriteLine("The maxinum is: " + t.Result),
                      TaskContinuationOptions.ExecuteSynchronously).Wait(); // Wait用于测试
            });

            // 子任务完成后，也显示任何未处理的异常
            parent.ContinueWith(p =>
            {
                // 用StringBuilder输出所有

                StringBuilder sb = new StringBuilder("The following exception(s) occurred:" + Environment.NewLine);
                foreach (var e in p.Exception.Flatten().InnerExceptions)
                    sb.AppendLine("   " + e.GetType().ToString());
                Console.WriteLine(sb.ToString());
            }, TaskContinuationOptions.OnlyOnFaulted);

            // 启动父任务
            parent.Start();
            Console.Read();
            try
            {
                parent.Wait(); //显示结果
            }
            catch (AggregateException)
            {
            }




        }
        private static int StartCode(CancellationToken ct, int n)
        {
            Int32 sum = 0;
            for (; n > 0; n--)
            {
                ct.ThrowIfCancellationRequested();
                checked { sum += n; }
            }
            return sum;

        }

        

        private void test1()
        {
            Console.WriteLine("主线程启动");
            //ThreadPool.QueueUserWorkItem(StartCode,5);
            List<Task> AllTask = new List<Task>();
            for (var i = 0; i <= 5; i++)
            {
                var task = new Task<int>(StartCode, i, TaskCreationOptions.AttachedToParent);

                AllTask.Add(task);
                task.Start();
                Task cwt = task.ContinueWith(_task => Console.WriteLine("thread index 真正完成 is:{0}", _task.Result), TaskContinuationOptions.AttachedToParent);


            }

            Console.WriteLine("主线程运行到此！");
            //Task.WaitAll(AllTask.ToArray());
            Console.WriteLine("等待完毕！");
            Console.ReadLine();
        }


        private static int StartCode(object i)
        {
            Console.WriteLine("开始执行子线程...{0}", i);
           
            var task = new Task(_i=> { Console.WriteLine("开始执行{0}的子线程...", _i);if ((int)i <= 1) { Thread.Sleep(5000); } },i,TaskCreationOptions.AttachedToParent);
            task.Start();
            Thread.Sleep((int)i * 1000);//模拟代码操作    
            Console.WriteLine("结束执行子线程...{0}", i);
            return (int)i;
        }
    }
}
