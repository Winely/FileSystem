using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <returns>空余块号</returns>
        /// <see cref="isFCB"/><seealso cref="use"/>
        public int getFreeBlock()
        {
            for(int i=0;i<1024;i++)
            {
                if (fatRecord[i].length_used == -1) return i;
            }
            return -1;
        }
    }

    /// <summary>
    /// FAT记录表
    /// </summary>
    class FATRecord
    {
        /// <summary>
        /// 区块已用长度
        /// </summary>
        /// <value>值为-1代表未使用</value>
        public int length_used { get;set;} 

        /// <summary>
        /// 下一块快号
        /// </summary>
        public int next { get; set; }
        public bool isFCB
        {
            get;
            set 
            { 
                if (value) use();
                isFCB = value;
            }
        }

        public void use() { length_used = 0; }
        public FATRecord() { length_used = 0; next = -1; isFCB = false; }
    }
}
