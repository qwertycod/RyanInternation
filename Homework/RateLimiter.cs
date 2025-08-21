namespace Homework
{
    public class RateLimiter
    {
        private readonly int _limit;
        private int _count = 0;
        private readonly TimeSpan _interval;
        private DateTime _resetTime;

        public RateLimiter(int limit, TimeSpan interval)
        {
            _limit = limit;
            _interval = interval;
            _resetTime = DateTime.UtcNow.Add(_interval);
        }

        public bool TryExecute()
        {
            if (DateTime.UtcNow > _resetTime)
            {
                _count = 0;
                _resetTime = DateTime.UtcNow.Add(_interval);
            }

            if (_count >= _limit)
            {
                return false;
            }

            _count++;
            return true;
        }
    }
}
