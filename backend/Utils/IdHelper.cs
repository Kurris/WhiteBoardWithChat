using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WhiteBoard.Utils
{
    /// <summary>
    /// ID生成帮助类
    /// </summary>
    public class IdHelper
    {

        private Snowflake _snowflake;

        private IdHelper()
        {
            _snowflake = new Snowflake(2, 0, 0);
        }

        public static IdHelper Instance { get; } = new IdHelper();

        /// <summary>
        /// 获取分布式唯一Id
        /// </summary>
        /// <returns></returns>
        public long GetLongId()
        {
            return _snowflake.NextId();
        }

        /// <summary>
        /// 获取分布式唯一Id
        /// </summary>
        /// <returns></returns>
        public string GetStrId() => _snowflake.NextId().ToString();

        /// <summary>
        /// 唯一ID
        /// </summary>
        /// <returns></returns>
        public int GetIntId()
        {
            return (int)(GetLongId() - 100000);
        }
    }

    /// <summary>
    /// Twitter的snowflake分布式算法
    /// </summary>
    public class Snowflake
    {
        private const long TwEpoch = 1546272000000L;//2019-01-01 00:00:00

        private const int _workerIdBits = 5;
        private const int _dataCenterIdBits = 5;
        private const int _sequenceBits = 12;
        private const long _maxWorkerId = -1L ^ (-1L << _workerIdBits);
        private const long _maxDatacenterId = -1L ^ (-1L << _dataCenterIdBits);

        private const int _workerIdShift = _sequenceBits;
        private const int _dataCenterIdShift = _sequenceBits + _workerIdBits;
        private const int _timestampLeftShift = _sequenceBits + _workerIdBits + _dataCenterIdBits;
        private const long _sequenceMask = -1L ^ (-1L << _sequenceBits);

        private long _sequence = 0L;
        private long _lastTimestamp = -1L;
        /// <summary>
        ///10位的数据机器位中的高位
        /// </summary>
        private long _workerId;

        /// <summary>
        /// 10位的数据机器位中的低位
        /// </summary>
        private long _dataCenterId;

        private readonly object _lock = new object();

        /// <summary>
        /// Twitter的snowflake分布式算法
        /// </summary>
        /// <param name="workerId">10位的数据机器位中的高位，默认不应该超过5位(5byte)</param>
        /// <param name="datacenterId"> 10位的数据机器位中的低位，默认不应该超过5位(5byte)</param>
        /// <param name="sequence">初始序列</param>
        public Snowflake(long workerId, long datacenterId, long sequence = 0L)
        {
            if (workerId > _maxWorkerId || workerId < 0)
            {
                throw new ArgumentException($"worker Id can't be greater than {_maxWorkerId} or less than 0");
            }

            if (datacenterId > _maxDatacenterId || datacenterId < 0)
            {
                throw new ArgumentException($"datacenter Id can't be greater than {_maxDatacenterId} or less than 0");
            }

            _workerId = workerId;
            _dataCenterId = datacenterId;
            _sequence = sequence;
        }

        /// <summary>
        /// 获取下一个Id，该方法线程安全
        /// </summary>
        /// <returns></returns>
        public long NextId()
        {
            lock (_lock)
            {
                var timestamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds();
                if (timestamp < _lastTimestamp)
                {

                    throw new Exception(
                        $"Clock moved backwards or wrapped around. Refusing to generate id for {_lastTimestamp - timestamp} ticks");
                }

                if (_lastTimestamp == timestamp)
                {
                    _sequence = (_sequence + 1) & _sequenceMask;
                    if (_sequence == 0)
                    {
                        timestamp = TilNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 0;
                }
                _lastTimestamp = timestamp;
                return ((timestamp - TwEpoch) << _timestampLeftShift) |
                         (_dataCenterId << _dataCenterIdShift) |
                         (_workerId << _workerIdShift) | _sequence;
            }
        }

        private long TilNextMillis(long lastTimestamp)
        {
            var timestamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds();
            while (timestamp <= lastTimestamp)
            {
                timestamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds();
            }
            return timestamp;
        }
    }
}
