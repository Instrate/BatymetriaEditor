using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SceneEditor.editor
{
    public class MultiThreader
    {
        Thread[] workers;

        ThreadStart funcToProcced;

        private int activeTs = 0;

        public MultiThreader(int amount_of_threads = 4, ThreadStart func = default)
        {
            workers = new Thread[amount_of_threads];
            funcToProcced = func;
        }

        [MTAThread]
        public void activateFreeThread()
        {
            for(int i = 0; i < workers.Length; i++)
            {
                if(workers[i].ThreadState == ThreadState.Unstarted)
                {
                    workers[i].Start();
                    activeTs++;
                }
                else
                {
                    if (!workers[i].IsAlive)
                    {
                        if (workers[i] == null)
                        {
                            workers[i] = new Thread(funcToProcced);
                        }
                        else
                        {
                            activeTs--;
                        }
                        
                    }          
                }
            }
        }
    }
}
