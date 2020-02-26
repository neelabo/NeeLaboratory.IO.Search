using System;

namespace NeeLaboratory.IO.Search
{
    /// <summary>
    /// NodeContent属性
    /// </summary>
    [Flags]
    public enum NodeContentFlag
    {
        None = 0,

        /// <summary>
        /// ディレクトリ
        /// </summary>
        Directory = (1 << 0),

        /// <summary>
        /// 追加された
        /// </summary>
        Added = (1 << 1),

        /// <summary>
        /// 削除された
        /// </summary>
        Removed = (1 << 2),

        /// <summary>
        /// ピン留め
        /// </summary>
        PushPin = (1 << 3),
    }

    /// <summary>
    /// 拡張メソッド
    /// </summary>
    public static class NodeContentFlagExtensions
    {
        /// <summary>
        /// 属性判定
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static bool IsFlag(this NodeContentFlag self, NodeContentFlag flag)
        {
            return (self & flag) == flag;
        }

        /// <summary>
        /// 属性設定
        /// </summary>
        /// <param name="flag"></param>
        public static NodeContentFlag SetFlag(this NodeContentFlag self, NodeContentFlag flag, bool state)
        {
            return state ? (self | flag) : (self & ~flag);
        }

        /// <summary>
        /// 属性ON
        /// </summary>
        /// <param name="flag"></param>
        public static NodeContentFlag SetFlag(this NodeContentFlag self, NodeContentFlag flag)
        {
            return self | flag;
        }

        /// <summary>
        /// 属性OFF
        /// </summary>
        /// <param name="self"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static NodeContentFlag ResetFlag(this NodeContentFlag self, NodeContentFlag flag)
        {
            return self & ~flag;
        }
    }
}
