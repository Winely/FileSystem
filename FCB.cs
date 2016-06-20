using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace FileSystem
{
    /// <summary>
    /// FCB记录，记录文件/文件夹各项信息
    /// </summary>
    [Serializable]
    class FCB
    {
        public char[] name = new char[60];
        public char[] owner = new char[60];
        public List<int> SubFile;
        int contentBlock;
        int size = 0;
        public bool isFolder { get; set; }
        DateTime createTime;
        DateTime lastUpdate;
        /// <summary>
        /// 初始化FCB
        /// </summary>
        /// <param name="block"></param>
        /// <param name="Name"></param>
        /// <param name="Content"></param>
        /// <param name="Isfolder"></param>
        /// <param name="Owner"></param>
        public FCB(int block, string Name, int Content, int father, bool Isfolder, string Owner)
        {
            name = new char[60];
            owner = new char[60];
            Name.CopyTo(0, name, 0, Name.Length);
            Owner.CopyTo(0, owner, 0, Owner.Length);
            contentBlock = Content;
            isFolder = Isfolder;
            createTime = DateTime.Now;
            lastUpdate = DateTime.Now;
            size = 0;

            //第一个节点为上级目录
            SubFile.Add(father);
        }

        public FCB()
        {
            // TODO: Complete member initialization
        }

        /// <summary>
        /// 输出目录下所有文件夹/文件
        /// </summary>
        /// <remarks>要在确认是文件夹时调用</remarks>
        public void ls() { }

        /// <summary>
        /// 序列化输出保存到content
        /// </summary>
        /// <param name="block">目标块</param>
        public void update(Block block)
        {
            
        }

        public static FCB read(byte[] content)
        {
            FCB fcb;
            MemoryStream stream = new MemoryStream(content);
            BinaryFormatter b = new BinaryFormatter();
            fcb = b.Deserialize(stream) as FCB;
            stream.Close();
            return fcb;
        }
        /// <summary>
        /// 将子文件/文件夹的fcb块号添加到sub列表
        /// </summary>
        /// <param name="fcb"></param>
        public void addSub(int fcb)
        {
            SubFile.Add(fcb);
        }
    }

    class FCBList : List<FCB>
    {

    }
}
