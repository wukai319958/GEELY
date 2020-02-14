using System;
using System.Runtime.Serialization;

namespace Interfaces
{
    /// <summary>
    /// 接口数据有效性异常。
    /// </summary>
    [Serializable]
    public class InterfaceDataException : Exception
    {
        /// <summary>
        /// 初始化接口数据有效性异常。
        /// </summary>
        public InterfaceDataException()
        { }

        /// <summary>
        /// 初始化接口数据有效性异常。
        /// </summary>
        /// <param name="message">描述错误的消息。</param>
        public InterfaceDataException(string message)
            : base(message)
        { }

        /// <summary>
        /// 初始化接口数据有效性异常。
        /// </summary>
        /// <param name="message">解释异常原因的错误消息。</param>
        /// <param name="inner">导致当前异常的异常。</param>
        public InterfaceDataException(string message, Exception inner)
            : base(message, inner)
        { }

        /// <summary>
        /// 初始化接口数据有效性异常。
        /// </summary>
        /// <param name="info">它存有有关所引发异常的序列化的对象数据。</param>
        /// <param name="context">它包含有关源或目标的上下文信息。</param>
        protected InterfaceDataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}