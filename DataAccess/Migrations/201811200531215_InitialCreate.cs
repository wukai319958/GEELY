namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ASM_AssembleIndicationItem",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ASM_AssembleIndicationId = c.Long(nullable: false),
                        Gzz = c.String(nullable: false, maxLength: 100),
                        MaterialCode = c.String(nullable: false, maxLength: 100),
                        MaterialName = c.String(nullable: false, maxLength: 100),
                        AssembleSequence = c.Int(nullable: false),
                        ToAssembleQuantity = c.Int(nullable: false),
                        Qtxbs = c.String(maxLength: 100),
                        ProjectCode = c.String(nullable: false, maxLength: 100),
                        ProjectStep = c.String(nullable: false, maxLength: 100),
                        BatchCode = c.String(maxLength: 100),
                        AssembleStatus = c.Int(nullable: false),
                        AssembledQuantity = c.Int(),
                        AssembledTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ASM_AssembleIndication", t => t.ASM_AssembleIndicationId)
                .Index(t => t.ASM_AssembleIndicationId);
            
            CreateTable(
                "dbo.ASM_AssembleIndication",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        FactoryCode = c.String(nullable: false, maxLength: 100),
                        ProductionLineCode = c.String(nullable: false, maxLength: 100),
                        CFG_WorkStationId = c.Int(nullable: false),
                        GzzList = c.String(nullable: false, maxLength: 1000),
                        MONumber = c.String(nullable: false, maxLength: 100),
                        ProductSequence = c.String(nullable: false, maxLength: 100),
                        PlanBatch = c.String(maxLength: 100),
                        CarBatch = c.String(maxLength: 100),
                        CarArrivedTime = c.DateTime(nullable: false),
                        AssembleStatus = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_WorkStation", t => t.CFG_WorkStationId)
                .Index(t => new { t.CFG_WorkStationId, t.GzzList, t.ProductSequence }, unique: true, name: "UK_ASM_AssembleIndication")
                .Index(t => t.CarArrivedTime)
                .Index(t => t.AssembleStatus);
            
            CreateTable(
                "dbo.ASM_AssembleIndicationMessage",
                c => new
                    {
                        ASM_AssembleIndicationId = c.Long(nullable: false),
                        ReceivedXml = c.String(nullable: false),
                        ReceivedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ASM_AssembleIndicationId)
                .ForeignKey("dbo.ASM_AssembleIndication", t => t.ASM_AssembleIndicationId)
                .Index(t => t.ASM_AssembleIndicationId);
            
            CreateTable(
                "dbo.ASM_AssembleResult",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ASM_AssembleIndicationId = c.Long(nullable: false),
                        FactoryCode = c.String(nullable: false, maxLength: 100),
                        ProductionLineCode = c.String(nullable: false, maxLength: 100),
                        CFG_WorkStationId = c.Int(nullable: false),
                        GzzList = c.String(nullable: false, maxLength: 100),
                        MONumber = c.String(nullable: false, maxLength: 100),
                        ProductSequence = c.String(nullable: false, maxLength: 100),
                        BeginTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ASM_AssembleIndication", t => t.ASM_AssembleIndicationId)
                .ForeignKey("dbo.CFG_WorkStation", t => t.CFG_WorkStationId)
                .Index(t => t.ASM_AssembleIndicationId, unique: true)
                .Index(t => t.CFG_WorkStationId);
            
            CreateTable(
                "dbo.ASM_AssembleResultItem",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ASM_AssembleResultId = c.Long(nullable: false),
                        CFG_CartId = c.Int(nullable: false),
                        CartPosition = c.Int(nullable: false),
                        Gzz = c.String(nullable: false, maxLength: 100),
                        MaterialCode = c.String(nullable: false, maxLength: 100),
                        MaterialName = c.String(nullable: false, maxLength: 100),
                        AssembleSequence = c.Int(nullable: false),
                        ToAssembleQuantity = c.Int(nullable: false),
                        AssembledQuantity = c.Int(nullable: false),
                        PickedTime = c.DateTime(nullable: false),
                        ProjectCode = c.String(nullable: false, maxLength: 100),
                        ProjectStep = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ASM_AssembleResult", t => t.ASM_AssembleResultId)
                .ForeignKey("dbo.CFG_Cart", t => t.CFG_CartId)
                .Index(t => t.ASM_AssembleResultId)
                .Index(t => t.CFG_CartId);
            
            CreateTable(
                "dbo.CFG_Cart",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 100),
                        Name = c.String(nullable: false, maxLength: 100),
                        Rfid1 = c.String(nullable: false, maxLength: 100),
                        Rfid2 = c.String(maxLength: 100),
                        XGateMAC = c.String(maxLength: 100),
                        XGateIP = c.String(nullable: false, maxLength: 100),
                        CartStatus = c.Int(nullable: false),
                        OnLine = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code, unique: true);
            
            CreateTable(
                "dbo.ASM_TaskItem",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ASM_TaskId = c.Long(nullable: false),
                        ASM_AssembleIndicationItemId = c.Long(nullable: false),
                        Gzz = c.String(nullable: false, maxLength: 100),
                        CFG_CartId = c.Int(nullable: false),
                        CartPosition = c.Int(nullable: false),
                        AssembleSequence = c.Int(nullable: false),
                        ToAssembleQuantity = c.Int(nullable: false),
                        Qtxbs = c.String(maxLength: 100),
                        AssembleStatus = c.Int(nullable: false),
                        AssembledQuantity = c.Int(),
                        AssembledTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ASM_AssembleIndicationItem", t => t.ASM_AssembleIndicationItemId)
                .ForeignKey("dbo.ASM_Task", t => t.ASM_TaskId)
                .ForeignKey("dbo.CFG_Cart", t => t.CFG_CartId)
                .Index(t => t.ASM_TaskId)
                .Index(t => t.ASM_AssembleIndicationItemId)
                .Index(t => t.CFG_CartId)
                .Index(t => t.AssembleStatus);
            
            CreateTable(
                "dbo.ASM_Task",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ASM_AssembleIndicationId = c.Long(nullable: false),
                        AssembleStatus = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ASM_AssembleIndication", t => t.ASM_AssembleIndicationId)
                .Index(t => t.ASM_AssembleIndicationId, unique: true)
                .Index(t => t.AssembleStatus);
            
            CreateTable(
                "dbo.AST_CartResult",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ProjectCode = c.String(nullable: false, maxLength: 100),
                        WbsId = c.String(maxLength: 100),
                        ProjectStep = c.String(nullable: false, maxLength: 100),
                        CFG_WorkStationId = c.Int(nullable: false),
                        BatchCode = c.String(nullable: false, maxLength: 100),
                        CFG_ChannelId = c.Int(nullable: false),
                        CFG_CartId = c.Int(nullable: false),
                        BeginPickTime = c.DateTime(nullable: false),
                        EndPickTime = c.DateTime(nullable: false),
                        CFG_EmployeeId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_Cart", t => t.CFG_CartId)
                .ForeignKey("dbo.CFG_Channel", t => t.CFG_ChannelId)
                .ForeignKey("dbo.CFG_WorkStation", t => t.CFG_WorkStationId)
                .ForeignKey("dbo.CFG_Employee", t => t.CFG_EmployeeId)
                .Index(t => t.CFG_WorkStationId)
                .Index(t => t.CFG_ChannelId)
                .Index(t => t.CFG_CartId)
                .Index(t => t.EndPickTime)
                .Index(t => t.CFG_EmployeeId);
            
            CreateTable(
                "dbo.AST_CartResultItem",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AST_CartResultId = c.Long(nullable: false),
                        CartPosition = c.Int(nullable: false),
                        MaterialCode = c.String(nullable: false, maxLength: 100),
                        MaterialName = c.String(nullable: false, maxLength: 100),
                        MaterialBarcode = c.String(maxLength: 100),
                        Quantity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AST_CartResult", t => t.AST_CartResultId)
                .Index(t => t.AST_CartResultId);
            
            CreateTable(
                "dbo.AST_CartResultMessage",
                c => new
                    {
                        AST_CartResultId = c.Long(nullable: false),
                        SentXml = c.String(),
                        LastSentTime = c.DateTime(),
                        SentSuccessful = c.Boolean(nullable: false),
                        ReceivedXml = c.String(),
                    })
                .PrimaryKey(t => t.AST_CartResultId)
                .ForeignKey("dbo.AST_CartResult", t => t.AST_CartResultId)
                .Index(t => t.AST_CartResultId);
            
            CreateTable(
                "dbo.CFG_Channel",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 100),
                        Name = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code, unique: true);
            
            CreateTable(
                "dbo.AST_CartTask",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        CFG_CartId = c.Int(nullable: false),
                        CFG_WorkStationId = c.Int(nullable: false),
                        BatchCode = c.String(nullable: false, maxLength: 100),
                        ProjectCode = c.String(nullable: false, maxLength: 100),
                        WbsId = c.String(maxLength: 100),
                        ProjectStep = c.String(nullable: false, maxLength: 100),
                        CFG_ChannelId = c.Int(nullable: false),
                        AssortingStatus = c.Int(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_WorkStation", t => t.CFG_WorkStationId)
                .ForeignKey("dbo.CFG_Cart", t => t.CFG_CartId)
                .ForeignKey("dbo.CFG_Channel", t => t.CFG_ChannelId)
                .Index(t => new { t.CFG_CartId, t.CFG_WorkStationId, t.BatchCode }, unique: true, name: "UK_AST_CartTask")
                .Index(t => t.CFG_ChannelId)
                .Index(t => t.AssortingStatus)
                .Index(t => t.CreateTime);
            
            CreateTable(
                "dbo.AST_CartTaskItem",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AST_CartTaskId = c.Long(nullable: false),
                        CartPosition = c.Int(nullable: false),
                        AST_PalletTaskItemId = c.Long(nullable: false),
                        AssortingStatus = c.Int(nullable: false),
                        AssortedQuantity = c.Int(),
                        AssortedTime = c.DateTime(),
                        CFG_EmployeeId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AST_CartTask", t => t.AST_CartTaskId)
                .ForeignKey("dbo.AST_PalletTaskItem", t => t.AST_PalletTaskItemId)
                .ForeignKey("dbo.CFG_Employee", t => t.CFG_EmployeeId)
                .Index(t => t.AST_CartTaskId)
                .Index(t => t.AST_PalletTaskItemId)
                .Index(t => t.AssortingStatus)
                .Index(t => t.CFG_EmployeeId);
            
            CreateTable(
                "dbo.AST_PalletTaskItem",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AST_PalletTaskId = c.Long(nullable: false),
                        CFG_WorkStationId = c.Int(nullable: false),
                        BoxCode = c.String(maxLength: 100),
                        FromPalletPosition = c.Int(nullable: false),
                        MaterialCode = c.String(nullable: false, maxLength: 100),
                        MaterialName = c.String(nullable: false, maxLength: 100),
                        MaterialBarcode = c.String(maxLength: 100),
                        ToPickQuantity = c.Int(nullable: false),
                        MaxQuantitySinglePosition = c.Int(nullable: false),
                        IsSpecial = c.Boolean(nullable: false),
                        IsBig = c.Boolean(nullable: false),
                        PickStatus = c.Int(nullable: false),
                        PickedQuantity = c.Int(),
                        PickedTime = c.DateTime(),
                        CFG_EmployeeId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_WorkStation", t => t.CFG_WorkStationId)
                .ForeignKey("dbo.CFG_Employee", t => t.CFG_EmployeeId)
                .ForeignKey("dbo.AST_PalletTask", t => t.AST_PalletTaskId)
                .Index(t => t.AST_PalletTaskId)
                .Index(t => t.CFG_WorkStationId)
                .Index(t => t.PickStatus)
                .Index(t => t.CFG_EmployeeId);
            
            CreateTable(
                "dbo.AST_LesTaskItem",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AST_LesTaskId = c.Long(nullable: false),
                        BillDetailId = c.String(maxLength: 100),
                        MaterialCode = c.String(nullable: false, maxLength: 100),
                        MaterialName = c.String(nullable: false, maxLength: 100),
                        MaterialBarcode = c.String(maxLength: 100),
                        ToPickQuantity = c.Int(nullable: false),
                        MaxQuantitySinglePosition = c.Int(nullable: false),
                        IsSpecial = c.Boolean(nullable: false),
                        IsBig = c.Boolean(nullable: false),
                        AST_PalletTaskItemId = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AST_LesTask", t => t.AST_LesTaskId)
                .ForeignKey("dbo.AST_PalletTaskItem", t => t.AST_PalletTaskItemId)
                .Index(t => t.AST_LesTaskId)
                .Index(t => t.AST_PalletTaskItemId);
            
            CreateTable(
                "dbo.AST_LesTask",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ProjectCode = c.String(nullable: false, maxLength: 100),
                        WbsId = c.String(maxLength: 100),
                        ProjectStep = c.String(nullable: false, maxLength: 100),
                        BillCode = c.String(nullable: false, maxLength: 100),
                        BillDate = c.DateTime(nullable: false),
                        CFG_WorkStationId = c.Int(nullable: false),
                        GzzList = c.String(maxLength: 100),
                        BatchCode = c.String(nullable: false, maxLength: 100),
                        CFG_ChannelId = c.Int(nullable: false),
                        CFG_PalletId = c.Int(nullable: false),
                        BoxCode = c.String(maxLength: 100),
                        FromPalletPosition = c.Int(nullable: false),
                        RequestTime = c.DateTime(nullable: false),
                        TaskGenerated = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_Channel", t => t.CFG_ChannelId)
                .ForeignKey("dbo.CFG_Pallet", t => t.CFG_PalletId)
                .ForeignKey("dbo.CFG_WorkStation", t => t.CFG_WorkStationId)
                .Index(t => t.BillCode, unique: true)
                .Index(t => t.CFG_WorkStationId)
                .Index(t => t.BatchCode)
                .Index(t => t.CFG_ChannelId)
                .Index(t => t.CFG_PalletId)
                .Index(t => t.RequestTime);
            
            CreateTable(
                "dbo.AST_LesTaskMessage",
                c => new
                    {
                        AST_LesTaskId = c.Long(nullable: false),
                        ReceivedXml = c.String(nullable: false),
                        ReceivedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.AST_LesTaskId)
                .ForeignKey("dbo.AST_LesTask", t => t.AST_LesTaskId)
                .Index(t => t.AST_LesTaskId);
            
            CreateTable(
                "dbo.CFG_Pallet",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 100),
                        PalletType = c.String(maxLength: 100),
                        PalletRotationStatus = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code, unique: true);
            
            CreateTable(
                "dbo.AST_PalletArrived",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ProjectCode = c.String(nullable: false, maxLength: 100),
                        WbsId = c.String(maxLength: 100),
                        ProjectStep = c.String(nullable: false, maxLength: 100),
                        BatchCode = c.String(nullable: false, maxLength: 100),
                        CFG_ChannelId = c.Int(nullable: false),
                        CFG_PalletId = c.Int(nullable: false),
                        PickBillIds = c.String(maxLength: 4000),
                        ArrivedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_Channel", t => t.CFG_ChannelId)
                .ForeignKey("dbo.CFG_Pallet", t => t.CFG_PalletId)
                .Index(t => new { t.BatchCode, t.CFG_PalletId, t.PickBillIds }, unique: true, name: "UK_AST_PalletArrived")
                .Index(t => t.CFG_ChannelId)
                .Index(t => t.ArrivedTime);
            
            CreateTable(
                "dbo.AST_PalletArrivedMessage",
                c => new
                    {
                        AST_PalletArrivedId = c.Long(nullable: false),
                        ReceivedXml = c.String(nullable: false),
                        ReceivedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.AST_PalletArrivedId)
                .ForeignKey("dbo.AST_PalletArrived", t => t.AST_PalletArrivedId)
                .Index(t => t.AST_PalletArrivedId);
            
            CreateTable(
                "dbo.AST_PalletResult",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ProjectCode = c.String(nullable: false, maxLength: 100),
                        WbsId = c.String(maxLength: 100),
                        ProjectStep = c.String(nullable: false, maxLength: 100),
                        BatchCode = c.String(nullable: false, maxLength: 100),
                        CFG_ChannelId = c.Int(nullable: false),
                        CFG_PalletId = c.Int(nullable: false),
                        BeginPickTime = c.DateTime(nullable: false),
                        EndPickTime = c.DateTime(nullable: false),
                        CFG_EmployeeId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_Employee", t => t.CFG_EmployeeId)
                .ForeignKey("dbo.CFG_Channel", t => t.CFG_ChannelId)
                .ForeignKey("dbo.CFG_Pallet", t => t.CFG_PalletId)
                .Index(t => t.BatchCode)
                .Index(t => t.CFG_ChannelId)
                .Index(t => t.CFG_PalletId)
                .Index(t => t.EndPickTime)
                .Index(t => t.CFG_EmployeeId);
            
            CreateTable(
                "dbo.AST_PalletResultItem",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AST_PalletResultId = c.Long(nullable: false),
                        CFG_WorkStationId = c.Int(nullable: false),
                        GzzList = c.String(maxLength: 100),
                        BillDetailId = c.String(maxLength: 100),
                        BoxCode = c.String(nullable: false, maxLength: 100),
                        PalletPosition = c.Int(nullable: false),
                        MaterialCode = c.String(nullable: false, maxLength: 100),
                        MaterialName = c.String(nullable: false, maxLength: 100),
                        MaterialBarcode = c.String(maxLength: 100),
                        ToPickQuantity = c.Int(nullable: false),
                        PickedQuantity = c.Int(nullable: false),
                        CFG_CartId = c.Int(),
                        CartPosition = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AST_PalletResult", t => t.AST_PalletResultId)
                .ForeignKey("dbo.CFG_Cart", t => t.CFG_CartId)
                .ForeignKey("dbo.CFG_WorkStation", t => t.CFG_WorkStationId)
                .Index(t => t.AST_PalletResultId)
                .Index(t => t.CFG_WorkStationId)
                .Index(t => t.CFG_CartId);
            
            CreateTable(
                "dbo.CFG_WorkStation",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 100),
                        Name = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code, unique: true);
            
            CreateTable(
                "dbo.CFG_CartCurrentMaterial",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CFG_CartId = c.Int(nullable: false),
                        Position = c.Int(nullable: false),
                        AST_CartTaskItemId = c.Long(),
                        ProjectCode = c.String(maxLength: 100),
                        WbsId = c.String(maxLength: 100),
                        ProjectStep = c.String(maxLength: 100),
                        CFG_WorkStationId = c.Int(),
                        BatchCode = c.String(maxLength: 100),
                        CFG_ChannelId = c.Int(),
                        CFG_PalletId = c.Int(),
                        BoxCode = c.String(maxLength: 100),
                        FromPalletPosition = c.Int(),
                        MaterialCode = c.String(maxLength: 100),
                        MaterialName = c.String(maxLength: 100),
                        MaterialBarcode = c.String(maxLength: 100),
                        Quantity = c.Int(),
                        AssortedTime = c.DateTime(),
                        CFG_EmployeeId = c.Int(),
                        Usability = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AST_CartTaskItem", t => t.AST_CartTaskItemId)
                .ForeignKey("dbo.CFG_Cart", t => t.CFG_CartId)
                .ForeignKey("dbo.CFG_Channel", t => t.CFG_ChannelId)
                .ForeignKey("dbo.CFG_Pallet", t => t.CFG_PalletId)
                .ForeignKey("dbo.CFG_WorkStation", t => t.CFG_WorkStationId)
                .Index(t => new { t.CFG_CartId, t.Position }, unique: true, name: "UK_CFG_CartCurrentMaterial")
                .Index(t => t.AST_CartTaskItemId)
                .Index(t => t.CFG_WorkStationId)
                .Index(t => t.CFG_ChannelId)
                .Index(t => t.CFG_PalletId);
            
            CreateTable(
                "dbo.CFG_MarketWorkStationCurrentCart",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        CFG_WorkStationId = c.Int(nullable: false),
                        CFG_CartId = c.Int(),
                        DockedTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_Cart", t => t.CFG_CartId)
                .ForeignKey("dbo.CFG_WorkStation", t => t.CFG_WorkStationId)
                .Index(t => t.CFG_WorkStationId)
                .Index(t => t.CFG_CartId);
            
            CreateTable(
                "dbo.CFG_WorkStationCurrentCart",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CFG_WorkStationId = c.Int(nullable: false),
                        Position = c.Int(nullable: false),
                        CFG_CartId = c.Int(),
                        DockedTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_Cart", t => t.CFG_CartId)
                .ForeignKey("dbo.CFG_WorkStation", t => t.CFG_WorkStationId)
                .Index(t => new { t.CFG_WorkStationId, t.Position }, unique: true, name: "UK_CFG_WorkStationCurrentCart")
                .Index(t => t.CFG_CartId);
            
            CreateTable(
                "dbo.FND_DeliveryResult",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        FND_TaskId = c.Long(nullable: false),
                        ProjectCode = c.String(nullable: false, maxLength: 100),
                        ProjectStep = c.String(nullable: false, maxLength: 100),
                        CFG_WorkStationId = c.Int(nullable: false),
                        BatchCode = c.String(nullable: false, maxLength: 100),
                        MaxNeedArrivedTime = c.DateTime(nullable: false),
                        CFG_CartId = c.Int(nullable: false),
                        DepartedTime = c.DateTime(nullable: false),
                        CFG_EmployeeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_Cart", t => t.CFG_CartId)
                .ForeignKey("dbo.CFG_Employee", t => t.CFG_EmployeeId)
                .ForeignKey("dbo.FND_Task", t => t.FND_TaskId)
                .ForeignKey("dbo.CFG_WorkStation", t => t.CFG_WorkStationId)
                .Index(t => t.FND_TaskId)
                .Index(t => t.CFG_WorkStationId)
                .Index(t => t.CFG_CartId)
                .Index(t => t.CFG_EmployeeId);
            
            CreateTable(
                "dbo.CFG_Employee",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 100),
                        Name = c.String(nullable: false, maxLength: 100),
                        LoginName = c.String(nullable: false, maxLength: 100),
                        Password = c.String(maxLength: 100),
                        IsEnabled = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Code, unique: true);
            
            CreateTable(
                "dbo.FND_Task",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ProjectCode = c.String(nullable: false, maxLength: 100),
                        ProjectStep = c.String(nullable: false, maxLength: 100),
                        BatchCode = c.String(nullable: false, maxLength: 100),
                        MaxNeedArrivedTime = c.DateTime(nullable: false),
                        RequestTime = c.DateTime(nullable: false),
                        CFG_WorkStationId = c.Int(nullable: false),
                        CFG_CartId = c.Int(nullable: false),
                        LightColor = c.Byte(nullable: false),
                        FindingStatus = c.Int(nullable: false),
                        DisplayTime = c.DateTime(),
                        DepartedTime = c.DateTime(),
                        CFG_EmployeeId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_Cart", t => t.CFG_CartId)
                .ForeignKey("dbo.CFG_Employee", t => t.CFG_EmployeeId)
                .ForeignKey("dbo.CFG_WorkStation", t => t.CFG_WorkStationId)
                .Index(t => t.RequestTime)
                .Index(t => t.CFG_WorkStationId)
                .Index(t => t.CFG_CartId)
                .Index(t => t.FindingStatus)
                .Index(t => t.CFG_EmployeeId);
            
            CreateTable(
                "dbo.FND_DeliveryResultItem",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        FND_DeliveryResultId = c.Long(nullable: false),
                        CartPosition = c.Int(nullable: false),
                        MaterialCode = c.String(nullable: false, maxLength: 100),
                        MaterialName = c.String(nullable: false, maxLength: 100),
                        MaterialBarcode = c.String(maxLength: 100),
                        Quantity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FND_DeliveryResult", t => t.FND_DeliveryResultId)
                .Index(t => t.FND_DeliveryResultId);
            
            CreateTable(
                "dbo.FND_DeliveryResultMessage",
                c => new
                    {
                        FND_DeliveryResultId = c.Long(nullable: false),
                        SentXml = c.String(),
                        LastSentTime = c.DateTime(),
                        SentSuccessful = c.Boolean(nullable: false),
                        ReceivedXml = c.String(),
                    })
                .PrimaryKey(t => t.FND_DeliveryResultId)
                .ForeignKey("dbo.FND_DeliveryResult", t => t.FND_DeliveryResultId)
                .Index(t => t.FND_DeliveryResultId);
            
            CreateTable(
                "dbo.AST_PalletResultMessage",
                c => new
                    {
                        AST_PalletResultId = c.Long(nullable: false),
                        SentXml = c.String(),
                        LastSentTime = c.DateTime(),
                        SentSuccessful = c.Boolean(nullable: false),
                        ReceivedXml = c.String(),
                    })
                .PrimaryKey(t => t.AST_PalletResultId)
                .ForeignKey("dbo.AST_PalletResult", t => t.AST_PalletResultId)
                .Index(t => t.AST_PalletResultId);
            
            CreateTable(
                "dbo.AST_PalletTask",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        CFG_PalletId = c.Int(nullable: false),
                        BatchCode = c.String(nullable: false, maxLength: 100),
                        PickBillIds = c.String(maxLength: 4000),
                        ProjectCode = c.String(nullable: false, maxLength: 100),
                        WbsId = c.String(maxLength: 100),
                        ProjectStep = c.String(nullable: false, maxLength: 100),
                        CFG_ChannelId = c.Int(nullable: false),
                        PickStatus = c.Int(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_Channel", t => t.CFG_ChannelId)
                .ForeignKey("dbo.CFG_Pallet", t => t.CFG_PalletId)
                .Index(t => t.CFG_PalletId)
                .Index(t => t.BatchCode)
                .Index(t => t.CFG_ChannelId)
                .Index(t => t.PickStatus)
                .Index(t => t.CreateTime);
            
            CreateTable(
                "dbo.CFG_ChannelCurrentPallet",
                c => new
                    {
                        CFG_ChannelId = c.Int(nullable: false),
                        CFG_PalletId = c.Int(),
                        ArrivedTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.CFG_ChannelId)
                .ForeignKey("dbo.CFG_Channel", t => t.CFG_ChannelId)
                .ForeignKey("dbo.CFG_Pallet", t => t.CFG_PalletId)
                .Index(t => t.CFG_ChannelId)
                .Index(t => t.CFG_PalletId);
            
            CreateTable(
                "dbo.CFG_ChannelCurrentCart",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CFG_ChannelId = c.Int(nullable: false),
                        Position = c.Int(nullable: false),
                        CFG_CartId = c.Int(),
                        DockedTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_Cart", t => t.CFG_CartId)
                .ForeignKey("dbo.CFG_Channel", t => t.CFG_ChannelId)
                .Index(t => new { t.CFG_ChannelId, t.Position }, unique: true, name: "UK_CFG_ChannelCurrentCart")
                .Index(t => t.CFG_CartId);
            
            CreateTable(
                "dbo.CFG_ChannelPtlDevice",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CFG_ChannelId = c.Int(nullable: false),
                        Position = c.Int(nullable: false),
                        XGateIP = c.String(nullable: false, maxLength: 100),
                        RS485BusIndex = c.Byte(nullable: false),
                        Ptl900UAddress = c.Byte(nullable: false),
                        OnLine = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_Channel", t => t.CFG_ChannelId)
                .Index(t => new { t.CFG_ChannelId, t.Position }, unique: true, name: "UK_CFG_ChannelPtlDevice");
            
            CreateTable(
                "dbo.CFG_CartPtlDevice",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CFG_CartId = c.Int(nullable: false),
                        DeviceAddress = c.Byte(nullable: false),
                        OnLine = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_Cart", t => t.CFG_CartId)
                .Index(t => new { t.CFG_CartId, t.DeviceAddress }, unique: true, name: "UK_CFG_CartPtlDevice");
            
            CreateTable(
                "dbo.ASM_AssembleResultMessage",
                c => new
                    {
                        ASM_AssembleResultId = c.Long(nullable: false),
                        SentXml = c.String(),
                        LastSentTime = c.DateTime(),
                        SentSuccessful = c.Boolean(nullable: false),
                        ReceivedXml = c.String(),
                    })
                .PrimaryKey(t => t.ASM_AssembleResultId)
                .ForeignKey("dbo.ASM_AssembleResult", t => t.ASM_AssembleResultId)
                .Index(t => t.ASM_AssembleResultId);
            
            CreateTable(
                "dbo.AST_LesTask_PDA",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ProjectCode = c.String(nullable: false, maxLength: 100),
                        WbsId = c.String(maxLength: 100),
                        ProjectStep = c.String(nullable: false, maxLength: 100),
                        BillCode = c.String(nullable: false, maxLength: 100),
                        BillDate = c.DateTime(nullable: false),
                        CFG_WorkStationId = c.Int(nullable: false),
                        GzzList = c.String(maxLength: 100),
                        BatchCode = c.String(nullable: false, maxLength: 100),
                        CFG_ChannelId = c.Int(nullable: false),
                        CFG_PalletId = c.Int(nullable: false),
                        BoxCode = c.String(maxLength: 100),
                        FromPalletPosition = c.Int(nullable: false),
                        RequestTime = c.DateTime(nullable: false),
                        TaskGenerated = c.Boolean(nullable: false),
                        IsFinished = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_Channel", t => t.CFG_ChannelId)
                .ForeignKey("dbo.CFG_Pallet", t => t.CFG_PalletId)
                .ForeignKey("dbo.CFG_WorkStation", t => t.CFG_WorkStationId)
                .Index(t => t.BillCode, unique: true)
                .Index(t => t.CFG_WorkStationId)
                .Index(t => t.BatchCode)
                .Index(t => t.CFG_ChannelId)
                .Index(t => t.CFG_PalletId)
                .Index(t => t.RequestTime);
            
            CreateTable(
                "dbo.AST_LesTaskItem_PDA",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AST_LesTaskId = c.Long(nullable: false),
                        BillDetailId = c.String(maxLength: 100),
                        MaterialCode = c.String(nullable: false, maxLength: 100),
                        MaterialName = c.String(nullable: false, maxLength: 100),
                        MaterialBarcode = c.String(maxLength: 100),
                        ToPickQuantity = c.Int(nullable: false),
                        MaxQuantitySinglePosition = c.Int(nullable: false),
                        IsSpecial = c.Boolean(nullable: false),
                        IsBig = c.Boolean(nullable: false),
                        AST_PalletTaskItemId = c.Long(),
                        AST_LesTask_PDA_Id = c.Long(),
                        AST_PalletTaskItem_PDA_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AST_LesTask_PDA", t => t.AST_LesTask_PDA_Id)
                .ForeignKey("dbo.AST_PalletTaskItem_PDA", t => t.AST_PalletTaskItem_PDA_Id)
                .Index(t => t.AST_LesTask_PDA_Id)
                .Index(t => t.AST_PalletTaskItem_PDA_Id);
            
            CreateTable(
                "dbo.AST_PalletTaskItem_PDA",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AST_PalletTaskId = c.Long(nullable: false),
                        CFG_WorkStationId = c.Int(nullable: false),
                        BoxCode = c.String(maxLength: 100),
                        FromPalletPosition = c.Int(nullable: false),
                        MaterialCode = c.String(nullable: false, maxLength: 100),
                        MaterialName = c.String(nullable: false, maxLength: 100),
                        MaterialBarcode = c.String(maxLength: 100),
                        ToPickQuantity = c.Int(nullable: false),
                        MaxQuantitySinglePosition = c.Int(nullable: false),
                        IsSpecial = c.Boolean(nullable: false),
                        IsBig = c.Boolean(nullable: false),
                        PickStatus = c.Int(nullable: false),
                        PickedQuantity = c.Int(),
                        PickedTime = c.DateTime(),
                        CFG_EmployeeId = c.Int(),
                        AST_PalletTask_PDA_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AST_PalletTask_PDA", t => t.AST_PalletTask_PDA_Id)
                .ForeignKey("dbo.CFG_Employee", t => t.CFG_EmployeeId)
                .ForeignKey("dbo.CFG_WorkStation", t => t.CFG_WorkStationId)
                .Index(t => t.CFG_WorkStationId)
                .Index(t => t.PickStatus)
                .Index(t => t.CFG_EmployeeId)
                .Index(t => t.AST_PalletTask_PDA_Id);
            
            CreateTable(
                "dbo.AST_PalletTask_PDA",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        CFG_PalletId = c.Int(nullable: false),
                        BatchCode = c.String(nullable: false, maxLength: 100),
                        PickBillIds = c.String(maxLength: 4000),
                        ProjectCode = c.String(nullable: false, maxLength: 100),
                        WbsId = c.String(maxLength: 100),
                        ProjectStep = c.String(nullable: false, maxLength: 100),
                        CFG_ChannelId = c.Int(nullable: false),
                        PickStatus = c.Int(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_Channel", t => t.CFG_ChannelId)
                .ForeignKey("dbo.CFG_Pallet", t => t.CFG_PalletId)
                .Index(t => t.CFG_PalletId)
                .Index(t => t.BatchCode)
                .Index(t => t.CFG_ChannelId)
                .Index(t => t.PickStatus)
                .Index(t => t.CreateTime);
            
            CreateTable(
                "dbo.AST_LesTaskMessage_PDA",
                c => new
                    {
                        AST_LesTaskId = c.Long(nullable: false),
                        ReceivedXml = c.String(nullable: false),
                        ReceivedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.AST_LesTaskId)
                .ForeignKey("dbo.AST_LesTask_PDA", t => t.AST_LesTaskId)
                .Index(t => t.AST_LesTaskId);
            
            CreateTable(
                "dbo.AST_PalletArrived_PDA",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ProjectCode = c.String(nullable: false, maxLength: 100),
                        WbsId = c.String(maxLength: 100),
                        ProjectStep = c.String(nullable: false, maxLength: 100),
                        BatchCode = c.String(nullable: false, maxLength: 100),
                        CFG_ChannelId = c.Int(nullable: false),
                        CFG_PalletId = c.Int(nullable: false),
                        PickBillIds = c.String(maxLength: 4000),
                        ArrivedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_Channel", t => t.CFG_ChannelId)
                .ForeignKey("dbo.CFG_Pallet", t => t.CFG_PalletId)
                .Index(t => new { t.BatchCode, t.CFG_PalletId, t.PickBillIds }, unique: true, name: "UK_AST_PalletArrived_PDA")
                .Index(t => t.CFG_ChannelId)
                .Index(t => t.ArrivedTime);
            
            CreateTable(
                "dbo.AST_PalletArrivedMessage_PDA",
                c => new
                    {
                        AST_PalletArrivedId = c.Long(nullable: false),
                        ReceivedXml = c.String(nullable: false),
                        ReceivedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.AST_PalletArrivedId)
                .ForeignKey("dbo.AST_PalletArrived_PDA", t => t.AST_PalletArrivedId)
                .Index(t => t.AST_PalletArrivedId);
            
            CreateTable(
                "dbo.AST_PalletPickResult_PDA",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        BatchCode = c.String(nullable: false, maxLength: 100),
                        CFG_PalletId = c.Int(nullable: false),
                        PickBillIds = c.String(maxLength: 4000),
                        BoxCode = c.String(maxLength: 100),
                        Status = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                        ReceivedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.BatchCode, t.CFG_PalletId, t.PickBillIds }, unique: true, name: "UK_AST_PalletPickResult_PDA");
            
            CreateTable(
                "dbo.AST_PalletPickResultMessage_PDA",
                c => new
                    {
                        AST_PalletPickResultId = c.Long(nullable: false),
                        ReceivedXml = c.String(nullable: false),
                        ReceivedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.AST_PalletPickResultId)
                .ForeignKey("dbo.AST_PalletPickResult_PDA", t => t.AST_PalletPickResultId)
                .Index(t => t.AST_PalletPickResultId);
            
            CreateTable(
                "dbo.DST_AgvSwitch",
                c => new
                    {
                        ID = c.Long(nullable: false, identity: true),
                        isOpen = c.Boolean(nullable: false),
                        lastOpenTime = c.DateTime(),
                        lastCloseTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.DST_DistributeArriveResult",
                c => new
                    {
                        ID = c.Long(nullable: false, identity: true),
                        reqCode = c.String(nullable: false, maxLength: 50),
                        arriveTime = c.DateTime(nullable: false),
                        startPosition = c.String(maxLength: 100),
                        endPosition = c.String(maxLength: 100),
                        podCode = c.String(maxLength: 100),
                        currentPointCode = c.String(),
                        DistributeReqTypes = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .Index(t => t.reqCode);
            
            CreateTable(
                "dbo.DST_DistributeArriveTaskResult",
                c => new
                    {
                        ID = c.Long(nullable: false, identity: true),
                        code = c.String(maxLength: 10),
                        message = c.String(maxLength: 1000),
                        reqCode = c.String(maxLength: 50),
                        data = c.String(maxLength: 1000),
                        sendTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .Index(t => t.reqCode);
            
            CreateTable(
                "dbo.DST_DistributeArriveTask",
                c => new
                    {
                        ID = c.Long(nullable: false, identity: true),
                        data = c.String(maxLength: 1000),
                        method = c.String(maxLength: 100),
                        taskType = c.String(maxLength: 100),
                        reqCode = c.String(nullable: false, maxLength: 50),
                        reqTime = c.DateTime(nullable: false),
                        robotCode = c.String(maxLength: 100),
                        taskCode = c.String(nullable: false, maxLength: 50),
                        receiveTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .Index(t => t.reqCode)
                .Index(t => t.taskCode);
            
            CreateTable(
                "dbo.DST_DistributeTaskResult",
                c => new
                    {
                        ID = c.Long(nullable: false, identity: true),
                        code = c.String(maxLength: 10),
                        message = c.String(maxLength: 1000),
                        reqCode = c.String(maxLength: 50),
                        data = c.String(maxLength: 1000),
                        receiveTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .Index(t => t.reqCode);
            
            CreateTable(
                "dbo.DST_DistributeTask",
                c => new
                    {
                        ID = c.Long(nullable: false, identity: true),
                        reqCode = c.String(nullable: false, maxLength: 50),
                        reqTime = c.DateTime(nullable: false),
                        clientCode = c.String(maxLength: 100),
                        tokenCode = c.String(maxLength: 100),
                        taskTyp = c.String(maxLength: 100),
                        userCallCode = c.String(maxLength: 100),
                        taskGroupCode = c.String(maxLength: 100),
                        startPosition = c.String(maxLength: 100),
                        endPosition = c.String(maxLength: 100),
                        podCode = c.String(maxLength: 100),
                        podDir = c.String(maxLength: 100),
                        priority = c.Int(nullable: false),
                        robotCode = c.String(maxLength: 100),
                        taskCode = c.String(maxLength: 100),
                        data = c.String(maxLength: 1000),
                        DistributeReqTypes = c.Int(nullable: false),
                        isResponse = c.Boolean(nullable: false),
                        arriveTime = c.DateTime(),
                        sendErrorCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .Index(t => t.reqCode);
            
            CreateTable(
                "dbo.FeedRecord",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FCCODE = c.String(maxLength: 64),
                        PLCODE = c.String(maxLength: 64),
                        STCODE = c.String(maxLength: 64),
                        GZZLIST = c.String(maxLength: 64),
                        MONUM = c.String(maxLength: 64),
                        PRDSEQ = c.String(maxLength: 64),
                        PRJCODE = c.String(maxLength: 64),
                        PRJSTEP = c.String(maxLength: 64),
                        PRJCFG = c.String(maxLength: 64),
                        PACKLINE = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.FeedZone",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        XGateIP = c.String(nullable: false, maxLength: 64),
                        Bus = c.Int(nullable: false),
                        Address = c.Int(nullable: false),
                        RFID = c.String(nullable: false, maxLength: 64),
                        GroundId = c.String(nullable: false, maxLength: 64),
                        AreaId = c.String(nullable: false, maxLength: 64),
                        MaterialId = c.String(maxLength: 64),
                        IsM3 = c.Boolean(nullable: false),
                        IsInteractive = c.Boolean(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MarketZone",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        XGateIP = c.String(nullable: false, maxLength: 64),
                        Bus = c.Int(nullable: false),
                        Address = c.Int(nullable: false),
                        GroundId = c.String(nullable: false),
                        AreaId = c.String(nullable: false),
                        Position = c.Int(nullable: false),
                        CFG_CartId = c.Int(),
                        Status = c.Int(nullable: false),
                        AreaStatus = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PickZone",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        XGateIP = c.String(nullable: false, maxLength: 64),
                        Bus = c.Int(nullable: false),
                        Address = c.Int(nullable: false),
                        CFG_ChannelId = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CFG_Channel", t => t.CFG_ChannelId)
                .Index(t => t.CFG_ChannelId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PickZone", "CFG_ChannelId", "dbo.CFG_Channel");
            DropForeignKey("dbo.AST_PalletPickResultMessage_PDA", "AST_PalletPickResultId", "dbo.AST_PalletPickResult_PDA");
            DropForeignKey("dbo.AST_PalletArrived_PDA", "CFG_PalletId", "dbo.CFG_Pallet");
            DropForeignKey("dbo.AST_PalletArrived_PDA", "CFG_ChannelId", "dbo.CFG_Channel");
            DropForeignKey("dbo.AST_PalletArrivedMessage_PDA", "AST_PalletArrivedId", "dbo.AST_PalletArrived_PDA");
            DropForeignKey("dbo.AST_LesTask_PDA", "CFG_WorkStationId", "dbo.CFG_WorkStation");
            DropForeignKey("dbo.AST_LesTask_PDA", "CFG_PalletId", "dbo.CFG_Pallet");
            DropForeignKey("dbo.AST_LesTask_PDA", "CFG_ChannelId", "dbo.CFG_Channel");
            DropForeignKey("dbo.AST_LesTaskMessage_PDA", "AST_LesTaskId", "dbo.AST_LesTask_PDA");
            DropForeignKey("dbo.AST_PalletTaskItem_PDA", "CFG_WorkStationId", "dbo.CFG_WorkStation");
            DropForeignKey("dbo.AST_PalletTaskItem_PDA", "CFG_EmployeeId", "dbo.CFG_Employee");
            DropForeignKey("dbo.AST_PalletTask_PDA", "CFG_PalletId", "dbo.CFG_Pallet");
            DropForeignKey("dbo.AST_PalletTask_PDA", "CFG_ChannelId", "dbo.CFG_Channel");
            DropForeignKey("dbo.AST_PalletTaskItem_PDA", "AST_PalletTask_PDA_Id", "dbo.AST_PalletTask_PDA");
            DropForeignKey("dbo.AST_LesTaskItem_PDA", "AST_PalletTaskItem_PDA_Id", "dbo.AST_PalletTaskItem_PDA");
            DropForeignKey("dbo.AST_LesTaskItem_PDA", "AST_LesTask_PDA_Id", "dbo.AST_LesTask_PDA");
            DropForeignKey("dbo.ASM_AssembleResultMessage", "ASM_AssembleResultId", "dbo.ASM_AssembleResult");
            DropForeignKey("dbo.CFG_CartPtlDevice", "CFG_CartId", "dbo.CFG_Cart");
            DropForeignKey("dbo.CFG_ChannelPtlDevice", "CFG_ChannelId", "dbo.CFG_Channel");
            DropForeignKey("dbo.CFG_ChannelCurrentCart", "CFG_ChannelId", "dbo.CFG_Channel");
            DropForeignKey("dbo.CFG_ChannelCurrentCart", "CFG_CartId", "dbo.CFG_Cart");
            DropForeignKey("dbo.AST_CartTask", "CFG_ChannelId", "dbo.CFG_Channel");
            DropForeignKey("dbo.AST_CartTask", "CFG_CartId", "dbo.CFG_Cart");
            DropForeignKey("dbo.AST_LesTaskItem", "AST_PalletTaskItemId", "dbo.AST_PalletTaskItem");
            DropForeignKey("dbo.CFG_ChannelCurrentPallet", "CFG_PalletId", "dbo.CFG_Pallet");
            DropForeignKey("dbo.CFG_ChannelCurrentPallet", "CFG_ChannelId", "dbo.CFG_Channel");
            DropForeignKey("dbo.AST_PalletTask", "CFG_PalletId", "dbo.CFG_Pallet");
            DropForeignKey("dbo.AST_PalletTask", "CFG_ChannelId", "dbo.CFG_Channel");
            DropForeignKey("dbo.AST_PalletTaskItem", "AST_PalletTaskId", "dbo.AST_PalletTask");
            DropForeignKey("dbo.AST_PalletResult", "CFG_PalletId", "dbo.CFG_Pallet");
            DropForeignKey("dbo.AST_PalletResult", "CFG_ChannelId", "dbo.CFG_Channel");
            DropForeignKey("dbo.AST_PalletResultMessage", "AST_PalletResultId", "dbo.AST_PalletResult");
            DropForeignKey("dbo.FND_DeliveryResultMessage", "FND_DeliveryResultId", "dbo.FND_DeliveryResult");
            DropForeignKey("dbo.FND_DeliveryResultItem", "FND_DeliveryResultId", "dbo.FND_DeliveryResult");
            DropForeignKey("dbo.FND_DeliveryResult", "CFG_WorkStationId", "dbo.CFG_WorkStation");
            DropForeignKey("dbo.FND_DeliveryResult", "FND_TaskId", "dbo.FND_Task");
            DropForeignKey("dbo.FND_Task", "CFG_WorkStationId", "dbo.CFG_WorkStation");
            DropForeignKey("dbo.FND_Task", "CFG_EmployeeId", "dbo.CFG_Employee");
            DropForeignKey("dbo.FND_Task", "CFG_CartId", "dbo.CFG_Cart");
            DropForeignKey("dbo.FND_DeliveryResult", "CFG_EmployeeId", "dbo.CFG_Employee");
            DropForeignKey("dbo.AST_PalletTaskItem", "CFG_EmployeeId", "dbo.CFG_Employee");
            DropForeignKey("dbo.AST_PalletResult", "CFG_EmployeeId", "dbo.CFG_Employee");
            DropForeignKey("dbo.AST_CartTaskItem", "CFG_EmployeeId", "dbo.CFG_Employee");
            DropForeignKey("dbo.AST_CartResult", "CFG_EmployeeId", "dbo.CFG_Employee");
            DropForeignKey("dbo.FND_DeliveryResult", "CFG_CartId", "dbo.CFG_Cart");
            DropForeignKey("dbo.CFG_WorkStationCurrentCart", "CFG_WorkStationId", "dbo.CFG_WorkStation");
            DropForeignKey("dbo.CFG_WorkStationCurrentCart", "CFG_CartId", "dbo.CFG_Cart");
            DropForeignKey("dbo.CFG_MarketWorkStationCurrentCart", "CFG_WorkStationId", "dbo.CFG_WorkStation");
            DropForeignKey("dbo.CFG_MarketWorkStationCurrentCart", "CFG_CartId", "dbo.CFG_Cart");
            DropForeignKey("dbo.CFG_CartCurrentMaterial", "CFG_WorkStationId", "dbo.CFG_WorkStation");
            DropForeignKey("dbo.CFG_CartCurrentMaterial", "CFG_PalletId", "dbo.CFG_Pallet");
            DropForeignKey("dbo.CFG_CartCurrentMaterial", "CFG_ChannelId", "dbo.CFG_Channel");
            DropForeignKey("dbo.CFG_CartCurrentMaterial", "CFG_CartId", "dbo.CFG_Cart");
            DropForeignKey("dbo.CFG_CartCurrentMaterial", "AST_CartTaskItemId", "dbo.AST_CartTaskItem");
            DropForeignKey("dbo.AST_PalletTaskItem", "CFG_WorkStationId", "dbo.CFG_WorkStation");
            DropForeignKey("dbo.AST_PalletResultItem", "CFG_WorkStationId", "dbo.CFG_WorkStation");
            DropForeignKey("dbo.AST_LesTask", "CFG_WorkStationId", "dbo.CFG_WorkStation");
            DropForeignKey("dbo.AST_CartTask", "CFG_WorkStationId", "dbo.CFG_WorkStation");
            DropForeignKey("dbo.AST_CartResult", "CFG_WorkStationId", "dbo.CFG_WorkStation");
            DropForeignKey("dbo.ASM_AssembleResult", "CFG_WorkStationId", "dbo.CFG_WorkStation");
            DropForeignKey("dbo.ASM_AssembleIndication", "CFG_WorkStationId", "dbo.CFG_WorkStation");
            DropForeignKey("dbo.AST_PalletResultItem", "CFG_CartId", "dbo.CFG_Cart");
            DropForeignKey("dbo.AST_PalletResultItem", "AST_PalletResultId", "dbo.AST_PalletResult");
            DropForeignKey("dbo.AST_PalletArrived", "CFG_PalletId", "dbo.CFG_Pallet");
            DropForeignKey("dbo.AST_PalletArrived", "CFG_ChannelId", "dbo.CFG_Channel");
            DropForeignKey("dbo.AST_PalletArrivedMessage", "AST_PalletArrivedId", "dbo.AST_PalletArrived");
            DropForeignKey("dbo.AST_LesTask", "CFG_PalletId", "dbo.CFG_Pallet");
            DropForeignKey("dbo.AST_LesTask", "CFG_ChannelId", "dbo.CFG_Channel");
            DropForeignKey("dbo.AST_LesTaskMessage", "AST_LesTaskId", "dbo.AST_LesTask");
            DropForeignKey("dbo.AST_LesTaskItem", "AST_LesTaskId", "dbo.AST_LesTask");
            DropForeignKey("dbo.AST_CartTaskItem", "AST_PalletTaskItemId", "dbo.AST_PalletTaskItem");
            DropForeignKey("dbo.AST_CartTaskItem", "AST_CartTaskId", "dbo.AST_CartTask");
            DropForeignKey("dbo.AST_CartResult", "CFG_ChannelId", "dbo.CFG_Channel");
            DropForeignKey("dbo.AST_CartResult", "CFG_CartId", "dbo.CFG_Cart");
            DropForeignKey("dbo.AST_CartResultMessage", "AST_CartResultId", "dbo.AST_CartResult");
            DropForeignKey("dbo.AST_CartResultItem", "AST_CartResultId", "dbo.AST_CartResult");
            DropForeignKey("dbo.ASM_TaskItem", "CFG_CartId", "dbo.CFG_Cart");
            DropForeignKey("dbo.ASM_TaskItem", "ASM_TaskId", "dbo.ASM_Task");
            DropForeignKey("dbo.ASM_Task", "ASM_AssembleIndicationId", "dbo.ASM_AssembleIndication");
            DropForeignKey("dbo.ASM_TaskItem", "ASM_AssembleIndicationItemId", "dbo.ASM_AssembleIndicationItem");
            DropForeignKey("dbo.ASM_AssembleResultItem", "CFG_CartId", "dbo.CFG_Cart");
            DropForeignKey("dbo.ASM_AssembleResultItem", "ASM_AssembleResultId", "dbo.ASM_AssembleResult");
            DropForeignKey("dbo.ASM_AssembleResult", "ASM_AssembleIndicationId", "dbo.ASM_AssembleIndication");
            DropForeignKey("dbo.ASM_AssembleIndicationMessage", "ASM_AssembleIndicationId", "dbo.ASM_AssembleIndication");
            DropForeignKey("dbo.ASM_AssembleIndicationItem", "ASM_AssembleIndicationId", "dbo.ASM_AssembleIndication");
            DropIndex("dbo.PickZone", new[] { "CFG_ChannelId" });
            DropIndex("dbo.DST_DistributeTask", new[] { "reqCode" });
            DropIndex("dbo.DST_DistributeTaskResult", new[] { "reqCode" });
            DropIndex("dbo.DST_DistributeArriveTask", new[] { "taskCode" });
            DropIndex("dbo.DST_DistributeArriveTask", new[] { "reqCode" });
            DropIndex("dbo.DST_DistributeArriveTaskResult", new[] { "reqCode" });
            DropIndex("dbo.DST_DistributeArriveResult", new[] { "reqCode" });
            DropIndex("dbo.AST_PalletPickResultMessage_PDA", new[] { "AST_PalletPickResultId" });
            DropIndex("dbo.AST_PalletPickResult_PDA", "UK_AST_PalletPickResult_PDA");
            DropIndex("dbo.AST_PalletArrivedMessage_PDA", new[] { "AST_PalletArrivedId" });
            DropIndex("dbo.AST_PalletArrived_PDA", new[] { "ArrivedTime" });
            DropIndex("dbo.AST_PalletArrived_PDA", new[] { "CFG_ChannelId" });
            DropIndex("dbo.AST_PalletArrived_PDA", "UK_AST_PalletArrived_PDA");
            DropIndex("dbo.AST_LesTaskMessage_PDA", new[] { "AST_LesTaskId" });
            DropIndex("dbo.AST_PalletTask_PDA", new[] { "CreateTime" });
            DropIndex("dbo.AST_PalletTask_PDA", new[] { "PickStatus" });
            DropIndex("dbo.AST_PalletTask_PDA", new[] { "CFG_ChannelId" });
            DropIndex("dbo.AST_PalletTask_PDA", new[] { "BatchCode" });
            DropIndex("dbo.AST_PalletTask_PDA", new[] { "CFG_PalletId" });
            DropIndex("dbo.AST_PalletTaskItem_PDA", new[] { "AST_PalletTask_PDA_Id" });
            DropIndex("dbo.AST_PalletTaskItem_PDA", new[] { "CFG_EmployeeId" });
            DropIndex("dbo.AST_PalletTaskItem_PDA", new[] { "PickStatus" });
            DropIndex("dbo.AST_PalletTaskItem_PDA", new[] { "CFG_WorkStationId" });
            DropIndex("dbo.AST_LesTaskItem_PDA", new[] { "AST_PalletTaskItem_PDA_Id" });
            DropIndex("dbo.AST_LesTaskItem_PDA", new[] { "AST_LesTask_PDA_Id" });
            DropIndex("dbo.AST_LesTask_PDA", new[] { "RequestTime" });
            DropIndex("dbo.AST_LesTask_PDA", new[] { "CFG_PalletId" });
            DropIndex("dbo.AST_LesTask_PDA", new[] { "CFG_ChannelId" });
            DropIndex("dbo.AST_LesTask_PDA", new[] { "BatchCode" });
            DropIndex("dbo.AST_LesTask_PDA", new[] { "CFG_WorkStationId" });
            DropIndex("dbo.AST_LesTask_PDA", new[] { "BillCode" });
            DropIndex("dbo.ASM_AssembleResultMessage", new[] { "ASM_AssembleResultId" });
            DropIndex("dbo.CFG_CartPtlDevice", "UK_CFG_CartPtlDevice");
            DropIndex("dbo.CFG_ChannelPtlDevice", "UK_CFG_ChannelPtlDevice");
            DropIndex("dbo.CFG_ChannelCurrentCart", new[] { "CFG_CartId" });
            DropIndex("dbo.CFG_ChannelCurrentCart", "UK_CFG_ChannelCurrentCart");
            DropIndex("dbo.CFG_ChannelCurrentPallet", new[] { "CFG_PalletId" });
            DropIndex("dbo.CFG_ChannelCurrentPallet", new[] { "CFG_ChannelId" });
            DropIndex("dbo.AST_PalletTask", new[] { "CreateTime" });
            DropIndex("dbo.AST_PalletTask", new[] { "PickStatus" });
            DropIndex("dbo.AST_PalletTask", new[] { "CFG_ChannelId" });
            DropIndex("dbo.AST_PalletTask", new[] { "BatchCode" });
            DropIndex("dbo.AST_PalletTask", new[] { "CFG_PalletId" });
            DropIndex("dbo.AST_PalletResultMessage", new[] { "AST_PalletResultId" });
            DropIndex("dbo.FND_DeliveryResultMessage", new[] { "FND_DeliveryResultId" });
            DropIndex("dbo.FND_DeliveryResultItem", new[] { "FND_DeliveryResultId" });
            DropIndex("dbo.FND_Task", new[] { "CFG_EmployeeId" });
            DropIndex("dbo.FND_Task", new[] { "FindingStatus" });
            DropIndex("dbo.FND_Task", new[] { "CFG_CartId" });
            DropIndex("dbo.FND_Task", new[] { "CFG_WorkStationId" });
            DropIndex("dbo.FND_Task", new[] { "RequestTime" });
            DropIndex("dbo.CFG_Employee", new[] { "Code" });
            DropIndex("dbo.FND_DeliveryResult", new[] { "CFG_EmployeeId" });
            DropIndex("dbo.FND_DeliveryResult", new[] { "CFG_CartId" });
            DropIndex("dbo.FND_DeliveryResult", new[] { "CFG_WorkStationId" });
            DropIndex("dbo.FND_DeliveryResult", new[] { "FND_TaskId" });
            DropIndex("dbo.CFG_WorkStationCurrentCart", new[] { "CFG_CartId" });
            DropIndex("dbo.CFG_WorkStationCurrentCart", "UK_CFG_WorkStationCurrentCart");
            DropIndex("dbo.CFG_MarketWorkStationCurrentCart", new[] { "CFG_CartId" });
            DropIndex("dbo.CFG_MarketWorkStationCurrentCart", new[] { "CFG_WorkStationId" });
            DropIndex("dbo.CFG_CartCurrentMaterial", new[] { "CFG_PalletId" });
            DropIndex("dbo.CFG_CartCurrentMaterial", new[] { "CFG_ChannelId" });
            DropIndex("dbo.CFG_CartCurrentMaterial", new[] { "CFG_WorkStationId" });
            DropIndex("dbo.CFG_CartCurrentMaterial", new[] { "AST_CartTaskItemId" });
            DropIndex("dbo.CFG_CartCurrentMaterial", "UK_CFG_CartCurrentMaterial");
            DropIndex("dbo.CFG_WorkStation", new[] { "Code" });
            DropIndex("dbo.AST_PalletResultItem", new[] { "CFG_CartId" });
            DropIndex("dbo.AST_PalletResultItem", new[] { "CFG_WorkStationId" });
            DropIndex("dbo.AST_PalletResultItem", new[] { "AST_PalletResultId" });
            DropIndex("dbo.AST_PalletResult", new[] { "CFG_EmployeeId" });
            DropIndex("dbo.AST_PalletResult", new[] { "EndPickTime" });
            DropIndex("dbo.AST_PalletResult", new[] { "CFG_PalletId" });
            DropIndex("dbo.AST_PalletResult", new[] { "CFG_ChannelId" });
            DropIndex("dbo.AST_PalletResult", new[] { "BatchCode" });
            DropIndex("dbo.AST_PalletArrivedMessage", new[] { "AST_PalletArrivedId" });
            DropIndex("dbo.AST_PalletArrived", new[] { "ArrivedTime" });
            DropIndex("dbo.AST_PalletArrived", new[] { "CFG_ChannelId" });
            DropIndex("dbo.AST_PalletArrived", "UK_AST_PalletArrived");
            DropIndex("dbo.CFG_Pallet", new[] { "Code" });
            DropIndex("dbo.AST_LesTaskMessage", new[] { "AST_LesTaskId" });
            DropIndex("dbo.AST_LesTask", new[] { "RequestTime" });
            DropIndex("dbo.AST_LesTask", new[] { "CFG_PalletId" });
            DropIndex("dbo.AST_LesTask", new[] { "CFG_ChannelId" });
            DropIndex("dbo.AST_LesTask", new[] { "BatchCode" });
            DropIndex("dbo.AST_LesTask", new[] { "CFG_WorkStationId" });
            DropIndex("dbo.AST_LesTask", new[] { "BillCode" });
            DropIndex("dbo.AST_LesTaskItem", new[] { "AST_PalletTaskItemId" });
            DropIndex("dbo.AST_LesTaskItem", new[] { "AST_LesTaskId" });
            DropIndex("dbo.AST_PalletTaskItem", new[] { "CFG_EmployeeId" });
            DropIndex("dbo.AST_PalletTaskItem", new[] { "PickStatus" });
            DropIndex("dbo.AST_PalletTaskItem", new[] { "CFG_WorkStationId" });
            DropIndex("dbo.AST_PalletTaskItem", new[] { "AST_PalletTaskId" });
            DropIndex("dbo.AST_CartTaskItem", new[] { "CFG_EmployeeId" });
            DropIndex("dbo.AST_CartTaskItem", new[] { "AssortingStatus" });
            DropIndex("dbo.AST_CartTaskItem", new[] { "AST_PalletTaskItemId" });
            DropIndex("dbo.AST_CartTaskItem", new[] { "AST_CartTaskId" });
            DropIndex("dbo.AST_CartTask", new[] { "CreateTime" });
            DropIndex("dbo.AST_CartTask", new[] { "AssortingStatus" });
            DropIndex("dbo.AST_CartTask", new[] { "CFG_ChannelId" });
            DropIndex("dbo.AST_CartTask", "UK_AST_CartTask");
            DropIndex("dbo.CFG_Channel", new[] { "Code" });
            DropIndex("dbo.AST_CartResultMessage", new[] { "AST_CartResultId" });
            DropIndex("dbo.AST_CartResultItem", new[] { "AST_CartResultId" });
            DropIndex("dbo.AST_CartResult", new[] { "CFG_EmployeeId" });
            DropIndex("dbo.AST_CartResult", new[] { "EndPickTime" });
            DropIndex("dbo.AST_CartResult", new[] { "CFG_CartId" });
            DropIndex("dbo.AST_CartResult", new[] { "CFG_ChannelId" });
            DropIndex("dbo.AST_CartResult", new[] { "CFG_WorkStationId" });
            DropIndex("dbo.ASM_Task", new[] { "AssembleStatus" });
            DropIndex("dbo.ASM_Task", new[] { "ASM_AssembleIndicationId" });
            DropIndex("dbo.ASM_TaskItem", new[] { "AssembleStatus" });
            DropIndex("dbo.ASM_TaskItem", new[] { "CFG_CartId" });
            DropIndex("dbo.ASM_TaskItem", new[] { "ASM_AssembleIndicationItemId" });
            DropIndex("dbo.ASM_TaskItem", new[] { "ASM_TaskId" });
            DropIndex("dbo.CFG_Cart", new[] { "Code" });
            DropIndex("dbo.ASM_AssembleResultItem", new[] { "CFG_CartId" });
            DropIndex("dbo.ASM_AssembleResultItem", new[] { "ASM_AssembleResultId" });
            DropIndex("dbo.ASM_AssembleResult", new[] { "CFG_WorkStationId" });
            DropIndex("dbo.ASM_AssembleResult", new[] { "ASM_AssembleIndicationId" });
            DropIndex("dbo.ASM_AssembleIndicationMessage", new[] { "ASM_AssembleIndicationId" });
            DropIndex("dbo.ASM_AssembleIndication", new[] { "AssembleStatus" });
            DropIndex("dbo.ASM_AssembleIndication", new[] { "CarArrivedTime" });
            DropIndex("dbo.ASM_AssembleIndication", "UK_ASM_AssembleIndication");
            DropIndex("dbo.ASM_AssembleIndicationItem", new[] { "ASM_AssembleIndicationId" });
            DropTable("dbo.PickZone");
            DropTable("dbo.MarketZone");
            DropTable("dbo.FeedZone");
            DropTable("dbo.FeedRecord");
            DropTable("dbo.DST_DistributeTask");
            DropTable("dbo.DST_DistributeTaskResult");
            DropTable("dbo.DST_DistributeArriveTask");
            DropTable("dbo.DST_DistributeArriveTaskResult");
            DropTable("dbo.DST_DistributeArriveResult");
            DropTable("dbo.DST_AgvSwitch");
            DropTable("dbo.AST_PalletPickResultMessage_PDA");
            DropTable("dbo.AST_PalletPickResult_PDA");
            DropTable("dbo.AST_PalletArrivedMessage_PDA");
            DropTable("dbo.AST_PalletArrived_PDA");
            DropTable("dbo.AST_LesTaskMessage_PDA");
            DropTable("dbo.AST_PalletTask_PDA");
            DropTable("dbo.AST_PalletTaskItem_PDA");
            DropTable("dbo.AST_LesTaskItem_PDA");
            DropTable("dbo.AST_LesTask_PDA");
            DropTable("dbo.ASM_AssembleResultMessage");
            DropTable("dbo.CFG_CartPtlDevice");
            DropTable("dbo.CFG_ChannelPtlDevice");
            DropTable("dbo.CFG_ChannelCurrentCart");
            DropTable("dbo.CFG_ChannelCurrentPallet");
            DropTable("dbo.AST_PalletTask");
            DropTable("dbo.AST_PalletResultMessage");
            DropTable("dbo.FND_DeliveryResultMessage");
            DropTable("dbo.FND_DeliveryResultItem");
            DropTable("dbo.FND_Task");
            DropTable("dbo.CFG_Employee");
            DropTable("dbo.FND_DeliveryResult");
            DropTable("dbo.CFG_WorkStationCurrentCart");
            DropTable("dbo.CFG_MarketWorkStationCurrentCart");
            DropTable("dbo.CFG_CartCurrentMaterial");
            DropTable("dbo.CFG_WorkStation");
            DropTable("dbo.AST_PalletResultItem");
            DropTable("dbo.AST_PalletResult");
            DropTable("dbo.AST_PalletArrivedMessage");
            DropTable("dbo.AST_PalletArrived");
            DropTable("dbo.CFG_Pallet");
            DropTable("dbo.AST_LesTaskMessage");
            DropTable("dbo.AST_LesTask");
            DropTable("dbo.AST_LesTaskItem");
            DropTable("dbo.AST_PalletTaskItem");
            DropTable("dbo.AST_CartTaskItem");
            DropTable("dbo.AST_CartTask");
            DropTable("dbo.CFG_Channel");
            DropTable("dbo.AST_CartResultMessage");
            DropTable("dbo.AST_CartResultItem");
            DropTable("dbo.AST_CartResult");
            DropTable("dbo.ASM_Task");
            DropTable("dbo.ASM_TaskItem");
            DropTable("dbo.CFG_Cart");
            DropTable("dbo.ASM_AssembleResultItem");
            DropTable("dbo.ASM_AssembleResult");
            DropTable("dbo.ASM_AssembleIndicationMessage");
            DropTable("dbo.ASM_AssembleIndication");
            DropTable("dbo.ASM_AssembleIndicationItem");
        }
    }
}
