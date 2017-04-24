using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashCodeVideoStreaming
{
    class Program
    {
        static int numberOfVideos, nEndPoint, nRequests, nCacheServers, capacityOfEachCache = 0;
        static string path = "C:/Users/Akin/Desktop/videos_worth_spreading.in";
        //static string path = "C:/Users/Akin/Desktop/Sample Input.txt";
        static string inputText = ReadTextFIle(path);
        static string[] inputArray = inputText.Split('\n');
        static int lineIndex = 2;

        static List<Video> Videos = new List<Video>();
        static List<EndPoint> EndPoints = new List<EndPoint>();
        static List<Request> Requests = new List<Request>();
        static List<CacheServer> CacheServers = new List<CacheServer>();
        static void Main(string[] args)
        {
           
            //Console.WriteLine(inputText);

            
            //Read Line 1
            string lineOne = getLineOne(inputArray);
            string[] lineOneArray =  lineOne.Split(' ');
            numberOfVideos = int.Parse(lineOneArray[0].ToString());
            nEndPoint = int.Parse(lineOneArray[1].ToString());
            nRequests  = int.Parse(lineOneArray[2].ToString());
            nCacheServers  = int.Parse(lineOneArray[3].ToString());
            capacityOfEachCache  = int.Parse(lineOneArray[4].ToString());

            Videos  = GetVideosWithSizes();
            EndPoints = GetEndPoints();
            Requests = GetVideoRequests();
            CacheServers = new List<CacheServer>();



            //SOLVING THE ALGORITHM
            //create the list of caches
            for (int i = 0; i < nCacheServers; i++)
            {
                var cacheServer = new CacheServer { ID = i, MaxSize = nCacheServers };
                CacheServers.Add(cacheServer);
            }

            //Take a request and compute the VideoSize * Number of request
            foreach (var request in Requests)
            {
                //get the endpoint the video is coming from
                EndPoint ep = GetEndPointById(request.EndPointID);
                int totalVideoRequestSizeInMB = request.NumberOfRequests * GetVideoById(request.VideoId).Size;
                //check the Caches the EP is connected to, and get a list of those not yet filled up and check the one with the lowest latency
                IEnumerable<int> cacheIdsConnectedToThisEp = GetIdsOfCachesThisEndPointIsConnectedTo(ep);// 
               //List<CacheServer> theseCacheServers = GetTheCachesServers(cacheIdsConnectedToThisEp);

                //var cacheServersWithEnoughSpace = new List<CacheServer>();
                var cacheServersWithEnoughSpace = GetTheCachesServers(cacheIdsConnectedToThisEp, request.Video.Size);
                /*
                int testCount = 0;
                foreach (var cacheServer in theseCacheServers)
                {
                    
                    if (cacheServer.UsedSize + request.Video.Size <= capacityOfEachCache)    //check if this cache still have enough space to contain this Video request
                    {
                        cacheServersWithEnoughSpace.Add(cacheServer);
                    }
                    testCount++;
                }*/


                //get the one with the least latency

                CacheServer selectedCache = null;
                int latency = ep.LatencyToDataCenter;
                foreach (var cache in cacheServersWithEnoughSpace)
                {
                    
                    int latencyToThisServer = 0;
                    if (ep.CacheToEndPointConnections.TryGetValue(cache.ID, out latencyToThisServer))
                    {
                        if (latencyToThisServer < latency)
                        {
                            selectedCache = cache;
                        }
                    }
                    latency = latencyToThisServer;
                }

                //allocate video to cache
                if (selectedCache != null && !selectedCache.VideoIDs.Contains(request.VideoId))
                {
                    selectedCache.VideoIDs.Add(request.VideoId);
                    selectedCache.UsedSize += request.Video.Size;
                }
              
            }//end foreach request

            //get all used servers
            var usedCacheServers = CacheServers.Where(c => c.VideoIDs.Count() > 0);
            StringBuilder outputString = new StringBuilder();
            //string outputString = "";
            //outputString += usedCacheServers.Count().ToString() +"\n";
            outputString.AppendLine(usedCacheServers.Count().ToString());
            foreach (var item in usedCacheServers)
            {
                //outputString += item.ID + " " + GetString(item.VideoIDs) + "\n";
                outputString.AppendLine(item.ID + " " + GetString(item.VideoIDs));
            }           
            WriteOutputToFile(outputString.ToString());
            Console.WriteLine("Output::\n" + outputString.ToString());
                     

            Console.Read();
        }

        private static void WriteOutputToFile(string v)
        {
            string outputPath = @"C:/Users/Akin/Desktop/Videos Worth Sharing.txt";
            System.IO.File.WriteAllText(outputPath, v);
        }

        private static string GetString(List<int> list)
        {
            //var nums = new List<int> { 1, 2, 3 };
            //var result = string.Join(", ", nums)
            return String.Join(" ", list);
        }

        private static List<CacheServer> GetTheCachesServers(IEnumerable<int> cacheIdsConnectedToThisEp, int size)
        {
            var result = new List<CacheServer>();
            foreach (var id in cacheIdsConnectedToThisEp)
            {
                var cache =  GetCacheServerById(id);
                if (cache.UsedSize + size <= capacityOfEachCache)
                {
                    result.Add(cache);
                }
            }
            
            return result;
        }

        private static IEnumerable<int> GetIdsOfCachesThisEndPointIsConnectedTo(EndPoint ep)
        {
            return ep.CacheToEndPointConnections.Select(i => i.Key);
        }

        private static EndPoint GetEndPointById(int endPointID)
        {
            return EndPoints.Where(e => e.ID == endPointID).SingleOrDefault();
        }
        private static Video GetVideoById(int videoId)
        {
            return Videos.Where(v => v.ID == videoId).SingleOrDefault();
        }
        private static CacheServer GetCacheServerById(int cacheId)
        {
            return CacheServers.Where(c => c.ID == cacheId).SingleOrDefault();
        }

        private static List<Request> GetVideoRequests()
        {
            List<Request> results = new List<Request>();

            string currentLine = "";
            string[] currentLineArray = currentLine.Split(' ');
            //lineIndex++;        //increment the line index from what it was after reading the endpoints
            for (int i = 0; i < nRequests; i++)
            {
                var request = new Request();
                currentLine = inputArray[lineIndex].Trim();
                currentLineArray = currentLine.Split(' ');
                int videoId = int.Parse(currentLineArray[0].ToString());
                int endPointId = int.Parse(currentLineArray[1].ToString());
                int numberOfRequests = int.Parse(currentLineArray[2].ToString());

                request.ID = i;
                request.VideoId = videoId;
                request.EndPointID = endPointId;
                request.NumberOfRequests = numberOfRequests;
                request.Video = GetVideoById(videoId);

                results.Add(request);
                lineIndex++;
            }
            return results;
        }

        private static List<EndPoint> GetEndPoints()
        {
            //from line 3 downwards
            List<EndPoint> result = new List<EndPoint>();
            //            string lineThree = inputArray[2].Trim();
            //string[] lineThreeArray = lineThree.Split(' ');
            //int Ld = int.Parse(lineThreeArray[0].ToString());       //Latency toDC
            //int nCaches = int.Parse(lineThreeArray[1].ToString());        //no of caches

            string currentLine = "";
            string[] currentLineArray = currentLine.Split(' ');
            
            for (int i = 0; i < nEndPoint; i++)
            {
                EndPoint endPoint = new EndPoint();
                currentLine = inputArray[lineIndex].Trim();
                currentLineArray = currentLine.Split(' ');
                int Ld = int.Parse(currentLineArray[0].ToString());       //Latency toDC
                int nCaches = int.Parse(currentLineArray[1].ToString());        //no of caches                                                        //i
                endPoint.ID = i;
                endPoint.LatencyToDataCenter = Ld;
                endPoint.NumberOfCaches = nCaches;

                //List<IDictionary<int, int>> conns = new List<IDictionary<int, int>>();
                IDictionary<int, int> conns = new Dictionary<int, int>();
                //connection to Caches
                for (int c = 0; c < nCaches; c++)
                {
                    lineIndex++;
                    currentLine = inputArray[lineIndex].Trim();
                    currentLineArray = currentLine.Split(' ');
                    //IDictionary<int, int> connToCache = new Dictionary<int, int>();
                    conns.Add(int.Parse(currentLineArray[0].ToString()), int.Parse(currentLineArray[1].ToString()));    //key: cacheId, Value: Latency to the cache
                    //conns.Add(connToCache);
                    
                }
                lineIndex++;
                endPoint.CacheToEndPointConnections = conns;
                result.Add(endPoint);
            }
            return result;
            
        }

        private static List<Video> GetVideosWithSizes()
        {
            //Reading second line
            string secondLine = inputArray[1].Trim();
            string[] lineTwoArray = secondLine.Split(' ');
            List<Video> result = new List<Video>();
            for (int i = 0; i < lineTwoArray.Count(); i++)
            {
                int size = int.Parse(lineTwoArray[i].ToString());
                Video video = new Video { ID = i, Size = size };
                result.Add(video);
            }
            return result;
        }

        private static string getLineOne(string[] text)
        {
            return text[0].Trim();
        }

        private static string ReadTextFIle(string path)
        {
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(path))
                {
                    // Read the stream to a string, and write the string to the console.
                    String line = sr.ReadToEnd();
                    return line;
                    //Console.WriteLine(line);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);

                return String.Empty;
            }
        }
    }

    //ENTITIES

    class CacheServer
    {
        public int ID { get; set; }
        public int MaxSize { get; set; }
        public int UsedSize { get; set; }
        public List<int> VideoIDs { get; set; } = new List<int>();
    }

    class EndPoint
    {
        public int ID { get; set; }
        public int LatencyToDataCenter { get; set; }
        public int NumberOfCaches { get; set; }
        //latency to each cache

        public List<CacheServer> CacheServers { get; set; }
        //public List<IDictionary<int, int>> CacheToEndPointConnections { get; set; } //key = CacheID, Value = Latency to Cache
        public IDictionary<int, int> CacheToEndPointConnections { get; set; } //key = CacheID, Value = Latency to Cache
    }

    class Request
    {
        public int ID { get; set; }
        public int VideoId { get; set; }
        public int EndPointID { get; set; }
        public int NumberOfRequests { get; set; }

        public Video Video { get; set; }
        public EndPoint EndPoint { get; set; }
    }

    public class Video
    {
        public int ID { get; set; }
        public int Size { get; set; }   //MB
    }

    public class CacheServerDescription
    {
        public int ID { get; set; }
        public int CacheServerId { get; set; }
        public List<int> VideoIDs { get; set; }
    }
}
