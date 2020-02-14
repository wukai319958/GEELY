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
    /// 装配工位。
    /// </summary>
    public class CFG_WorkStation
    {
        /// <summary>
        /// 初始化装配工位。
        /// </summary>
        public CFG_WorkStation()
        {
            this.CFG_MarketWorkStationCurrentCarts = new HashSet<CFG_MarketWorkStationCurrentCart>();
            this.CFG_WorkStationCurrentCarts = new HashSet<CFG_WorkStationCurrentCart>();
            this.CFG_CartCurrentMaterials = new HashSet<CFG_CartCurrentMaterial>();
            this.AST_CartResults = new HashSet<AST_CartResult>();
            this.AST_CartTasks = new HashSet<AST_CartTask>();
            this.AST_LesTasks = new HashSet<AST_LesTask>();
            this.AST_PalletResultItems = new HashSet<AST_PalletResultItem>();
            this.AST_PalletTaskItems = new HashSet<AST_PalletTaskItem>();
            this.FND_DeliveryResults = new HashSet<FND_DeliveryResult>();
            this.FND_Tasks = new HashSet<FND_Task>();
            this.ASM_AssembleIndications = new HashSet<ASM_AssembleIndication>();
            this.ASM_AssembleResults = new HashSet<ASM_AssembleResult>();
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
        /// 获取物料超市工位所停靠的小车的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<CFG_MarketWorkStationCurrentCart> CFG_MarketWorkStationCurrentCarts { get; set; }

        /// <summary>
        /// 获取工位所停靠的小车的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<CFG_WorkStationCurrentCart> CFG_WorkStationCurrentCarts { get; set; }

        /// <summary>
        /// 获取待配送到此工位的小车当前物料的集合。
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
        /// 获取单托拣选结果明细的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<AST_PalletResultItem> AST_PalletResultItems { get; set; }

        /// <summary>
        /// 获取单托拣选任务明细的集合。
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

        /// <summary>
        /// 获取装配指示的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<ASM_AssembleIndication> ASM_AssembleIndications { get; set; }

        /// <summary>
        /// 获取装配结果的集合。
        /// </summary>
        [SoapIgnore]
        [XmlIgnore]
        public virtual ICollection<ASM_AssembleResult> ASM_AssembleResults { get; set; }
    }
}