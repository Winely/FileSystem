using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace FileSystem
{
    /// <summary>
    /// FCB记录，记录文件/文件夹各项信息
    /// </summary>
    [Serializable]
    class FCB : ISerializable
    {
        /// <summary>
        /// 文件/文件夹名
        /// </summary>
        public char[] name = new char[60];

        /// <summary>
        /// 创建者用户名
        /// </summary>
        public char[] owner = new char[60];
        /// <summary>
        /// 子文件列表 保存了子文件fcb所在块号
        /// </summary>
        public List<int> SubFile = new List<int>();

        /// <summary>
        /// 实际内容块号
        /// </summary>
        public int contentBlock { get; set; }

        /// <summary>
        /// 文件/文件夹大小
        /// </summary>
        public int size { get; set; }

        /// <summary>
        /// 是否文件夹
        /// </summary>
        public bool isFolder { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createTime { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime lastUpdate { get; set; }

        /// <summary>
        /// 初始化FCB
        /// </summary>
        /// <param name="Name">文件/文件夹名</param>
        /// <param name="Content">内容块号</param>
        /// <param name="Isfolder">是否文件夹</param>
        /// <param name="Owner">创建者</param>
        public FCB(string Name, int Content, int father, bool Isfolder, string Owner)
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
        /// 序列化输出保存到content
        /// </summary>
        /// <param name="block">目标块</param>
        public void update(ref Block block)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            BinaryWriter r = new BinaryWriter(stream);
            b.Serialize(stream, this);
            b.Serialize(stream, name);
            b.Serialize(stream, owner);
            b.Serialize(stream, SubFile);
            r.Write(size);
            stream.GetBuffer().CopyTo(block.content, 0);
            stream.Close();
        }

        /// <summary>
        /// 利用反序列化从内存中读取fcb
        /// </summary>
        /// <param name="content">目标内存块</param>
        /// <returns>fcb数据</returns>
        public static FCB read(byte[] content)
        {
            FCB fcb;
            MemoryStream stream = new MemoryStream(content);
            BinaryFormatter b = new BinaryFormatter();
            BinaryReader r = new BinaryReader(stream);
            fcb = b.Deserialize(stream) as FCB;
            fcb.name=b.Deserialize(stream) as char[];
            fcb.owner = b.Deserialize(stream) as char[];
            fcb.SubFile = b.Deserialize(stream) as List<int>;
            fcb.size = r.ReadInt32();
            stream.Close();
            return fcb;
        }

        /// <summary>
        /// 将子文件/文件夹的fcb块号添加到sub列表
        /// 不确定子文件夹的类型所以再封装一层
        /// </summary>
        /// <param name="fcb">被添加的fcb所在块号</param>
        public void addSub(int fcb)
        {
            SubFile.Add(fcb);
        }

        /// <summary>
        /// 实现序列化的函数
        /// </summary>
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("contentblock", contentBlock);
            info.AddValue("isfolder", isFolder);
            info.AddValue("lastupdate", lastUpdate);
            info.AddValue("createtime", createTime);
        }

        /// <summary>
        /// 实现序列化的函数
        /// </summary>
        /// <see cref="ISerializable.GetObjectData"/>
        protected FCB(SerializationInfo info, StreamingContext context)
        {
            contentBlock=info.GetInt32("contentblock");
            isFolder=info.GetBoolean("isfolder");
            lastUpdate=info.GetDateTime("lastupdate");
            createTime=info.GetDateTime("createtime");
        }

    }
}
