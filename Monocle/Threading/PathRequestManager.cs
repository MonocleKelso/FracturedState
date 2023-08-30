using System.Collections.Generic;
using System.Threading;
using FracturedState.Game.Nav;

namespace Monocle.Threading
{
    /// <summary>
    /// A singleton responsible for queueing and processing requests to calculate A* paths in a separate thread
    /// </summary>
    public sealed class PathRequestManager
    {
        private PathRequestManager() { }

        private static PathRequestManager instance;
        public static PathRequestManager Instance => instance ?? (instance = new PathRequestManager());

        private List<PathRequest> requests;
        private Thread thread;
        private bool running;
        
        /// <summary>
        /// Queues a path request for processing by the pathing thread
        /// </summary>
        public void RequestPath(PathRequest request)
        {
            if (requests == null)
            {
                requests = new List<PathRequest>();
                thread = new Thread(ProcessRequests) {IsBackground = true};
                running = true;
                thread.Start();
            }
            lock (requests)
            {
                requests.Add(request);
            }
        }

        private void ProcessRequests()
        {
            while (running)
            {
                if (requests.Count <= 0) continue;
                
                PathRequest[] reqCopy;
                lock (requests)
                {
                    reqCopy = requests.ToArray();
                    requests.Clear();
                }
                foreach (var r in reqCopy)
                {
                    if (!r.IsStateActive) continue;
                        
                    List<UnityEngine.Vector3> p = null;
                    try
                    {
                        if (r.InteriorGrid != null)
                        {
                            p = AStarPather.Instance.PlanInteriorPath(r.InteriorGrid, r.Start, r.End, r.UnitRadius);
                        }
                        else
                        {
                            p = AStarPather.Instance.PlanExteriorPath(r.Start, r.End, r.UnitRadius, r.StartCacheId, r.EndCacheId);
                        }
                    }
                    catch (System.Exception e)
                    {
                        r.Error = e;
                    }
                    finally
                    {
                        r.CompleteRequest(p);
                    }
                }
            }
        }

        /// <summary>
        /// Causes the processing thread to stop waiting for requests once its current set of requests has finished processing.
        /// This also causes the processing thread to complete once it has finished all requests.
        /// </summary>
        public void Cleanup()
        {
            running = false;
        }
    }
}