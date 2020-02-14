using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using DataAccess.Assorting;

namespace DataAccess.Config
{
    /// <summary>
    /// 分拣巷道、转台。
    /// </summary>
    public class CFG_Channel
    {
        /// <summary>
        /// 初始化分拣巷道。
        /// </summary>
        public CFG_Channel()
        {
            this.CFG_ChannelPtlDevices = new HashSet<CFG_ChannelPtlDevice>();
            this.CFG_CartCurrentMaterials = new HashSet<CFG_CartCurrentMaterial>();
            this.CFG_ChannelCurrentCarts = new HashSet<CFG_ChannelCurrentCart>();
            this.AST_CartResults = new HashSet<AST_CartResult>();
            this.AST_CartTasks = new HashSet<AST_CartTask>();
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
        /// 获取或设置编码。
        /// </summary>
        [Index(IsUnique = true)]
        [Required]
        [MaxLength(100)]
        public string Code { get; set; }

        /// <summary>
        /// 获取或设置名称。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// 获取分拣转台上 PTL 设备的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<CFG_ChannelPtlDevice> CFG_ChannelPtlDevices { get; set; }

        /// <summary>
        /// 获取在此巷道分拣的小车当前物料的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<CFG_CartCurrentMaterial> CFG_CartCurrentMaterials { get; set; }

        /// <summary>
        /// 获取或设置当前停靠托盘信息。
        /// </summary>
        public virtual CFG_ChannelCurrentPallet CFG_ChannelCurrentPallet { get; set; }

        /// <summary>
        /// 获取当前停靠的小车信息的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<CFG_ChannelCurrentCart> CFG_ChannelCurrentCarts { get; set; }

        /// <summary>
        /// 获取单车拣选结果的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<AST_CartResult> AST_CartResults { get; set; }

        /// <summary>
        /// 获取单车播种任务的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<AST_CartTask> AST_CartTasks { get; set; }

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