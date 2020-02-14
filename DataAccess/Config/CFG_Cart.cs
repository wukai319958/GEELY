using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using DataAccess.AssemblyIndicating;
using DataAccess.Assorting;
using DataAccess.CartFinding;

namespace DataAccess.Config
{
    /// <summary>
    /// PTL 小车。
    /// </summary>
    public class CFG_Cart
    {
        /// <summary>
        /// 初始化 PTL 小车。
        /// </summary>
        public CFG_Cart()
        {
            this.CFG_CartPtlDevices = new HashSet<CFG_CartPtlDevice>();
            this.CFG_ChannelCurrentCarts = new HashSet<CFG_ChannelCurrentCart>();
            this.CFG_CartCurrentMaterials = new HashSet<CFG_CartCurrentMaterial>();
            this.CFG_WorkStationCurrentCarts = new HashSet<CFG_WorkStationCurrentCart>();
            this.AST_CartResults = new HashSet<AST_CartResult>();
            this.AST_PalletResultItems = new HashSet<AST_PalletResultItem>();
            this.AST_CartTasks = new HashSet<AST_CartTask>();
            this.FND_DeliveryResults = new HashSet<FND_DeliveryResult>();
            this.FND_Tasks = new HashSet<FND_Task>();
            this.ASM_TaskItems = new HashSet<ASM_TaskItem>();
            this.ASM_AssembleResultItems = new HashSet<ASM_AssembleResultItem>();
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
        /// 获取或设置 RFID 卡号一。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Rfid1 { get; set; }

        /// <summary>
        /// 获取或设置 RFID 卡号二。
        /// </summary>
        [MaxLength(100)]
        public string Rfid2 { get; set; }

        /// <summary>
        /// 获取或设置 XGate 的 MAC 地址。
        /// </summary>
        [MaxLength(100)]
        public string XGateMAC { get; set; }

        /// <summary>
        /// 获取或设置 XGate 的 IP 地址。
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string XGateIP { get; set; }

        /// <summary>
        /// 获取或设置小车状态。
        /// </summary>
        public CartStatus CartStatus { get; set; }

        /// <summary>
        /// 获取或设置是否在线。
        /// </summary>
        public bool OnLine { get; set; }

        /// <summary>
        /// 获取停靠在工位的小车的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<CFG_WorkStationCurrentCart> CFG_WorkStationCurrentCarts { get; set; }

        /// <summary>
        /// 获取其上设备的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<CFG_CartPtlDevice> CFG_CartPtlDevices { get; set; }

        /// <summary>
        /// 获取停靠在的分拣巷道信息的集合，0 或 1 个元素。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<CFG_ChannelCurrentCart> CFG_ChannelCurrentCarts { get; set; }

        /// <summary>
        /// 获取小车当前存放物料的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<CFG_CartCurrentMaterial> CFG_CartCurrentMaterials { get; set; }

        /// <summary>
        /// 获取单车拣选结果的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<AST_CartResult> AST_CartResults { get; set; }

        /// <summary>
        /// 获取按托拣选结果明细的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<AST_PalletResultItem> AST_PalletResultItems { get; set; }

        /// <summary>
        /// 获取单车播种任务的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<AST_CartTask> AST_CartTasks { get; set; }

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

        /// <summary>
        /// 获取装配明细的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<ASM_TaskItem> ASM_TaskItems { get; set; }

        /// <summary>
        /// 获取装配结果明细的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<ASM_AssembleResultItem> ASM_AssembleResultItems { get; set; }
    }
}