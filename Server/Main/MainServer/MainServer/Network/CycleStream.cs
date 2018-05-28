using System;

namespace MyNetwork
{
    class CycleStream
    {
        private int m_headIndex = 0;
        private int m_tailIndex = -1;//因为每次从tail+1开始写数据，tail==-1是区分full和empty的标记
        private byte[] m_byteStream = null;
        private int m_maxSize = 0;
        public CycleStream(int size)
        {
            m_byteStream = new byte[size];
            m_headIndex = 0;
            m_tailIndex = -1;
            m_maxSize = size;
        }
        public void Reset()
        {
            m_headIndex = 0;
            m_tailIndex = -1;
        }
        public int GetMaxSize()
        {
            return m_maxSize;
        }
        public bool HasData()
        {
            return GetLeftSizeToRead() > 0;
        }
        public bool IsFull()
        {
            return GetLeftSizeToWrite() <= 0;
        }
        public int GetWriteIndex()
        {
            return (m_tailIndex + 1) % GetMaxSize();
        }
        public void SetWriteIndex(int count)
        {
            if (count > 0)
            {
                m_tailIndex = (m_tailIndex + count) % GetMaxSize();
            }
        }
        public int GetLeftSizeToWrite()
        {//返回剩余可写入的size
            int leftSizeToWrite = 0;
            if (m_tailIndex == -1)
            {
                return GetMaxSize();
            }
            if (m_tailIndex < m_headIndex)
            {//tail在head前面，从tail+1写到head - 1
                leftSizeToWrite = m_headIndex - m_tailIndex - 1;
            }
            else
            {//tail在head后面或者重合
                leftSizeToWrite = GetMaxSize() - m_tailIndex - 1 + m_headIndex;
            }
            return leftSizeToWrite;
        }
        public int GetReadIndex()
        {
            return m_headIndex;
        }
        public void SetReadIndex(int count)
        {
            if (count > 0)
            {
                m_headIndex = (m_headIndex + count) % GetMaxSize();
                if ((m_headIndex - m_tailIndex) == 1)
                {
                    Reset();//读完了
                }
                else if (m_tailIndex == (GetMaxSize() - 1) && m_headIndex == 0)
                {//tail和head都在边界上
                    Reset();//读完了
                }
            }
        }
        public int GetLeftSizeToRead()
        {//返回剩余可读取的size
            int leftSizeToRead = 0;
            if (m_tailIndex == -1)
            {
                return 0;
            }
            if (m_tailIndex >= m_headIndex)
            {
                leftSizeToRead = m_tailIndex - m_headIndex + 1;
            }
            else
            {//TailIndex < HeadIndex
                leftSizeToRead = GetMaxSize() - m_headIndex + m_tailIndex + 1;
            }
            return leftSizeToRead;
        }
        public int TryReadBuffer(byte[] buffer)
        {
            if (buffer == null)
            {
                return 0;
            }
            int sizeToRead = Math.Min(GetLeftSizeToRead(), buffer.Length);
            if (sizeToRead > 0)
            {
                for (int i = 0; i < sizeToRead; i++)
                {
                    int index = (GetReadIndex() + i) % GetMaxSize();
                    buffer[i] = m_byteStream[index];
                }
                //这里只是read，不能保证buffer发送出去了，所以不能在这里调用，应该根据send的返回值来set
                //SetReadIndex(sizeToRead); 
                return sizeToRead;
            }
            return 0;
        }
        public int WriteBuffer(byte[] buffer, int size)
        {
            if (buffer == null || buffer.Length < size || size <= 0)
            {
                return 0;
            }
            int sizeToWrite = Math.Min(GetLeftSizeToWrite(), size);
            if (sizeToWrite > 0)
            {
                for (int i = 0; i < sizeToWrite; i++)
                {
                    int index = (GetWriteIndex() + i) % GetMaxSize();
                    m_byteStream[index] = buffer[i];
                }
                SetWriteIndex(sizeToWrite);
                return sizeToWrite;
            }
            return 0;
        }
    }
}
