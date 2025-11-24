namespace WebServer.Monitoring
{
    public static class ServerMetrics
    {
        private static int _requestCount = 0;
        private static int _onlineUsers = 0;

        public static int RequestCount => _requestCount;
        public static int OnlineUserCount => _onlineUsers;

        public static void IncrementRequest() => Interlocked.Increment(ref _requestCount);
        public static void DecrementOnlineUsers() => Interlocked.Decrement(ref _onlineUsers);
        public static void IncrementOnlineUsers() => Interlocked.Increment(ref _onlineUsers);
    }
}
