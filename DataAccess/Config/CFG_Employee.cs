using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using DataAccess.Assorting;
using DataAccess.CartFinding;

namespace DataAccess.Config
{
    /// <summary>
    /// 操作员。
    /// </summary>
    public class CFG_Employee
    {
        /// <summary>
        /// 初始化操作员。
        /// </summary>
        public CFG_Employee()
        {
            this.AST_CartResults = new HashSet<AST_CartResult>();
            this.AST_CartTaskItems = new HashSet<AST_CartTaskItem>();
            this.AST_PalletResults = new HashSet<AST_PalletResult>();
            this.AST_PalletTaskItems = new HashSet<AST_PalletTaskItem>();
            this.FND_DeliveryResults = new HashSet<FND_DeliveryResult>();
            this.FND_Tasks = new HashSet<FND_Task>();
        }

        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 获取或设置工牌。
        /// </summary>
        [Index(IsUnique = true)]
        [Required]
        [MaxLength(100)]
        public string Code { get; set; }

        /// <summary>
        /// 获取或设置姓名。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置登录名。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string LoginName { get; set; }

        /// <summary>
        /// 获取或设置登录密码。
        /// </summary>
        [MaxLength(100)]
        public string Password { get; set; }

        /// <summary>
        /// 获取或设置是否在职。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 获取单车拣选结果的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<AST_CartResult> AST_CartResults { get; set; }

        /// <summary>
        /// 获取单车明细的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<AST_CartTaskItem> AST_CartTaskItems { get; set; }

        /// <summary>
        /// 获取单托拣选结果的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<AST_PalletResult> AST_PalletResults { get; set; }

        /// <summary>
        /// 获取按托明细的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<AST_PalletTaskItem> AST_PalletTaskItems { get; set; }

        /// <summary>
        /// 获取单车发车结果的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<FND_DeliveryResult> FND_DeliveryResults { get; set; }

        /// <summary>
        /// 获取单车寻车任务的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<FND_Task> FND_Tasks { get; set; }
    }
}