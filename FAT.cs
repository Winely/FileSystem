using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;


namespace FileSystem
{
    /// <summary>
    /// FAT表格，存储了1024个数据块的FAT记录
    /// </summary>
    class FAT
    {
        public FATRecord[] fatRecord;
        public FAT()
        {
            fatRecord = new FATRecord[1024];
            for(int i=0;i<1024;i++)
            {
                fatRecord[i] = new FATRecord();
            }
        }
        public FATRecord this[int i]
        {
            get { return fatRecord[i]; }
            set { fatRecord[i] = value; }
        }

        /// <summary>
        /// 得到下一块。调用前确认next不为-1
        /// </summary>
        /// <param name="r">r的next不能为-1</param>
        /// <returns>r的连续下一块block的FAT记录</returns>
        public FATRecord next(FATRecord r) { return this[r.next]; }

        /// <summary>
        /// 申请空区块。申请只是返回空块号，需要后续操作正式占用
        /// </summary>
        /// <param name="offset">搜索开始索引号</param>
        /// <returns>空余块号</returns>
        /// <see cref="isFCB"/><seealso cref="use"/>
        public int getFreeBlock(int offset)
        {
            for(int i=offset;i<1024;i++)
            {
                if (fatRecord[i].length_used == -1) return i;
            }
            return -1;
        }

        /// <summary>
        /// 初始化特定FAT记录
        /// </summary>
        /// <param name="block">待格式化块号</param>
        public void clear(int block)
        {
            this[block].initialize();
        }

        /// <summary>
        /// 初始化全部FAT记录
        /// </summary>
        public void clear()
        {
            for (int i = 0; i < 1024; i++) fatRecord[i].initialize();
        }

        /// <summary>
        /// 将内存本地化
        /// </summary>
        /// <param name="stream"></param>
        public void save(ref FileStream stream)
        {
            BinaryWriter b = new BinaryWriter(stream);
            for (int i = 0; i < 1024; i++)
            {
                b.Write(fatRecord[i].length_used);
                b.Write(fatRecord[i].next);
                b.Write(fatRecord[i].IsFCB);
            }
        }
    }

    /// <summary>
    /// FAT记录表
    /// </summary>
    class FATRecord
    {
        bool isFCB = false;
        /// <summary>
        /// 区块已用长度
        /// </summary>
        /// <value>值为-1代表未使用</value>
        public int length_used { get;set;} 
        /// <summary>
        /// 下一块快号
        /// </summary>
        public int next { get; set; }
        public bool IsFCB
        {
            get
            {
                return isFCB;
            }
            set 
            { 
                if (value) use();
                isFCB = value;
            }
        }

        /// <summary>
        /// 初始化记录
        /// </summary>
        public void initialize()
        {
            length_used = -1;
            next = -1;
            isFCB = false;
        }

        /// <summary>
        /// 标记占用块号
        /// </summary>
        public void use() { length_used = 0; }
        public FATRecord() { initialize(); }
    }
}
