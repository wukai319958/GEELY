using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using DataAccess.Assorting;

namespace DataAccess.Config
{
    /// <summary>
    /// 托盘。
    /// </summary>
    public class CFG_Pallet
    {
        /// <summary>
        /// 初始化托盘。
        /// </summary>
        public CFG_Pallet()
        {
            this.CFG_CartCurrentMaterials = new HashSet<CFG_CartCurrentMaterial>();
            this.CFG_ChannelCurrentPallets = new HashSet<CFG_ChannelCurrentPallet>();
            this.AST_LesTasks = new HashSet<AST_LesTask>();
            this.AST_PalletArriveds = new HashSet<AST_PalletArrived>();
            this.AST_PalletResults = new HashSet<AST_PalletResult>();
            this.AST_PalletTasks = new HashSet<AST_PalletTask>();
        }

        /// <summary>
        /// 获取或设置自增主键。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 获取或设置托盘编码。
        /// </summary>
        [Index(IsUnique = true)]
        [Required]
        [MaxLength(100)]
        public string Code { get; set; }

        /// <summary>
        /// 箱型，01-五料盒组，02-围板箱，03-大件。
        /// </summary>
        [MaxLength(100)]
        public string PalletType { get; set; }

        /// <summary>
        /// 获取或设置托盘旋转状态。
        /// </summary>
        public PalletRotationStatus PalletRotationStatus { get; set; }

        /// <summary>
        /// 获取从此托盘分拣的小车当前物料的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<CFG_CartCurrentMaterial> CFG_CartCurrentMaterials { get; set; }

        /// <summary>
        /// 获取停靠在的分拣巷道信息，0 或 1 个元素。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<CFG_ChannelCurrentPallet> CFG_ChannelCurrentPallets { get; set; }

        /// <summary>
        /// 获取 LES 原始任务的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<AST_LesTask> AST_LesTasks { get; set; }

        /// <summary>
        /// 获取托盘抵达分拣口通知的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<AST_PalletArrived> AST_PalletArriveds { get; set; }

        /// <summary>
        /// 获取单托拣选结果的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<AST_PalletResult> AST_PalletResults { get; set; }

        /// <summary>
        /// 获取按托合并后的分拣任务的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<AST_PalletTask> AST_PalletTasks { get; set; }
    }
}