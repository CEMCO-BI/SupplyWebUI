USE [Cemco_DW]
GO
/****** Object:  StoredProcedure [Qlik].[SalesOrder]    Script Date: 21-07-2022 12:12:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jason Wu
-- Create date: 2021-12-6
-- Description:	Sproc for sales order for qlik
-- EXEC [Qlik].[SalesOrder] @DaysBack = -30
-- =============================================
ALTER PROCEDURE [Qlik].[SalesOrder]
	@DaysBack		INT					/* Required */

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	SET ARITHABORT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;


	-- No parameter sniffing
	DECLARE @_DaysBack			INT			= @DaysBack;


	-- Generate "schema.procedure" name
	DECLARE @_StoredProcedure NVARCHAR(128) = OBJECT_SCHEMA_NAME(@@PROCID) + CONVERT(NCHAR(1), '.') + OBJECT_NAME(@@PROCID);

	-- Generate XML from current parameter list
	DECLARE @_Parameter XML = (
		SELECT
			@_DaysBack			AS DaysBack
			FOR XML PATH('root'), TYPE
	);

	-- Create log entry
	EXEC Administration.StoredProcedure_CreateExecutionLog
		@StoredProcedure	= @_StoredProcedure,
		@Parameter			= @_Parameter;



	BEGIN TRAN

		DECLARE @_DaysBackDate DATE = DATEADD(DAY, @_DaysBack, GETDATE());
		
		--Sales Orders
		IF OBJECT_ID('tempdb..#ResultSet') IS NOT NULL
			DROP TABLE #ResultSet;

		SELECT		
			so.SalesOrderID															AS 'SalesOrderID',    			
			so.SalesOrderItemID														AS 'SalesOrderItemID',
			so.SalesOrderRowNum														AS 'SalesOrderLineNumber',
			CONVERT(DATE, so.SalesOrderOrderDate)									AS 'SalesOrderOrderDate',
			d.FiscalMonth															AS 'SalesOrderFiscalMonth',
			d.FiscalYear															AS 'SalesOrderFiscalYear',
			d.FiscalQuarter															AS 'SalesOrderFiscalQuarter',
			d.Week																	AS 'SalesOrderWeek',
			COALESCE(so.SalesOrderLineItemShipmentDate, so.SalesOrderShipmentDate)	AS 'SalesOrderShipmentDate',
			sd.FiscalMonth															AS 'SalesOrderShipmentFiscalMonth',
			sd.FiscalYear															AS 'SalesOrderShipmentFiscalYear',
			sd.FiscalQuarter														AS 'SalesOrderShipmentFiscalQuarter',
			sd.FiscalWeek															AS 'SalesOrderShipmentFiscalWeek',
			l.LocationCode															AS 'SalesOrderLocation',
			l.Zip																	AS 'SalesOrderLocationZip',
			ccom.CompanyCode														AS 'SalesOrderCustomerCode',				
			REPLACE(COALESCE(csn.CustomerShortName,ccom.CompanyName), ',', ';')		AS 'SalesOrderCustomerName',				
			pcom.CompanyCode														AS 'SalesOrderMasterCustomerCode',		
			REPLACE(COALESCE(psn.CustomerShortName, pcom.CompanyName), ',', ';')	AS 'SalesOrderMasterCustomerName',		
			COALESCE(srm.SalesRepName, t.TerritoryName)								AS 'SalesOrderSalesRep',					
			r.RegionName															AS 'SalesOrderRegion',					
			pd.LengthFeet															AS 'SalesOrderPartLength',				
			p.PartNo																AS 'SalesOrderPartNumber',																		     
			REPLACE(REPLACE(p.Description, ',', ';'), '"', '')						AS 'SalesOrderPartDescription',																	       
			REPLACE(pt.PartTypeDescription, ',', ';')								AS 'SalesOrderPartType',																			    
			REPLACE(REPLACE(pcat.PartCategoryDescription, ',', ';'), '"', '')		AS 'SalesOrderPartCategory',																		       
			REPLACE(pclass.PartClassDescription, ',', ';')							AS 'SalesOrderPartClass',																			       
			so.SalesOrderQuantityVal												AS 'SalesOrderQuantity',
			qpud.UnitName															AS 'SalesOrderQuantityUnit',
			qpud.UnitsPerBase														AS 'SalesOrderQuantityUnitsPerBase',
			so.SalesOrderWeight														AS 'SalesOrderWeight',    
			so.SalesOrderPrice														AS 'SalesOrderPrice',
			so.SalesOrderExtendedPrice												AS 'SalesOrderExtendedPrice',
			so.SalesOrderChargePrice												AS 'SalesOrderChargePrice',	
			COALESCE(so.SalesOrderTaxableAmount, 0)									AS 'SalesOrderTaxCollectable',
			so.SalesOrderExtendedPrice 
				+ ISNULL(so.SalesOrderChargePrice, 0)
				+ COALESCE(so.SalesOrderTaxableAmount, 0)								AS 'SalesOrderTotalAmount',
			IIF(so.SalesOrderWeight>0, 
				(so.SalesOrderExtendedPrice
					+ ISNULL(so.SalesOrderChargePrice, 0))
				/ so.SalesOrderWeight
				* 100, 0)															AS 'SalesOrderSPCWT',
			so.SalesOrderBillQuantity												AS 'SalesOrderLength',
			bpud.UnitName															AS 'SalesOrderLengthUnit',
			bpud.UnitsPerBase														AS 'SalesOrderLengthUnitsPerBase',
			so.SalesOrderStatus														AS 'SalesOrderStatus',													
			REPLACE(so.SalesOrderPurchaseOrder, ',', ';')							AS 'SalesOrderPurchaseOrder',
			job.ContractId															AS 'SalesOrderJob',																				   
			REPLACE(job.JobName, ',', ';')											AS 'SalesOrderJobDescription',
			CASE 
				WHEN job.ContractId IS NULL THEN 'Everyday'
				ELSE 'Job'
			END																		AS 'SalesOrderJobEverday',
			csr.FirstName + ' ' + csr.LastName										AS 'SalesOrderCSR',
			car.Description															AS 'SalesOrderCarrier',
			REPLACE(so.SalesOrderShipAddress1, ',', ';')							AS 'SalesOrderShipAddress1',
			REPLACE(so.SalesOrderShipAddress2, ',', ';')							AS 'SalesOrderShipAddress2',
			REPLACE(so.SalesOrderShipCity, ',', ';')								AS 'SalesOrderShipCity',
			so.SalesOrderShipZip													AS 'SalesOrderShipZip',	
			s.StateName																AS 'SalesOrderShipState',
			c.CountryName															AS 'SalesOrderShipCountry',
			cc.Code																	AS 'SalesOrderClassCode',
			cc.Description															AS 'SalesOrderClassCodeDescription',
			ft.Description															AS 'SalesOrderFreightType',
			sa.StorageAreaName														AS 'SalesOrderStorageArea',
			w.WarehouseName															AS 'SalesOrderWarehouse',
			CONVERT(FLOAT, 0)														AS 'SalesOrderInvoiceAmount',
			CONVERT(FLOAT, 0)														AS 'SalesOrderPostedWeight',
			CONVERT(DATE, NULL)														AS 'SalesOrderPostedDate',
			CONVERT(INT, NULL)														AS 'SalesOrderPostedFiscalMonth',
			CONVERT(INT, NULL)														AS 'SalesOrderPostedFiscalYear',
			CONVERT(INT, NULL)														AS 'SalesOrderPostedFiscalQuarter',
			CONVERT(INT, NULL)														AS 'SalesOrderPostedFiscalWeek',
			CONVERT(FLOAT, 0)														AS 'SalesOrderUnpostedWeight',
			CONVERT(DATE, NULL)														AS 'SalesOrderUnpostedDate',
			CONVERT(INT, NULL)														AS 'SalesOrderUnpostedFiscalMonth',
			CONVERT(INT, NULL)														AS 'SalesOrderUnpostedFiscalYear',
			CONVERT(INT, NULL)														AS 'SalesOrderUnpostedFiscalQuarter',
			CONVERT(INT, NULL)														AS 'SalesOrderUnpostedFiscalWeek',
			CONVERT(DATE, so.SalesOrderModifiedDate)								AS 'SalesOrderModifiedDate',
			so.SalesOrderCreatedDate												AS 'SalesOrderCreatedDate'
		

			INTO #ResultSet
			FROM Summary.SalesOrder AS so       

				-- sales Order Date
				JOIN Reporting.Date AS d  
					ON d.Date = so.SalesOrderOrderDate

				--Shipment Dates
				LEFT JOIN Reporting.Date AS sd
					ON sd.Date = COALESCE(so.SalesOrderLineItemShipmentDate, so.SalesOrderShipmentDate)
			
				--Location
				JOIN dbo.Location AS l
					ON l.LocationId = so.SalesOrderOrderLocationID

				--Lenght Unit
				LEFT JOIN dbo.PartUnitDetail AS bpud
					ON bpud.PartUnitDetailID = so.SalesOrderBillPartUnitDetailID

				--Quantity Unit
				LEFT JOIN dbo.PartUnitDetail AS qpud
					ON qpud.PartUnitDetailID = so.SalesOrderQuantityPartUnitDetailID

				--Child Customer
				JOIN dbo.Customer AS ccus
					ON ccus.CustomerId = so.SalesOrderCustomerID

				JOIN dbo.Company AS ccom
					ON ccom.CompanyID = ccus.CompanyId

				LEFT JOIN Reporting.CustomerShortNames AS csn
						ON csn.CustomerCode = ccom.CompanyCode

				--Sales Rep
				LEFT JOIN dbo.CustomerSalesRep AS sr
					ON sr.CustomerId = so.SalesOrderCustomerID
				
				LEFT JOIN dbo.Employee AS e
					ON e.EmployeeId = sr.EmployeeId

				LEFT JOIN dbo.Territory AS t
					ON t.TerritoryName = e.FirstName + ' ' + e.LastName

				LEFT JOIN Reporting.SalesRepMapping AS srm
					ON srm.TerritoryId = t.TerritoryId

				--Region
				LEFT JOIN dbo.Region AS r
					ON r.RegionId = t.RegionId

				--Master Customer
				LEFT JOIN dbo.customer AS pcus
					ON pcus.CustomerId = ccus.ParentCustomerId
		
				LEFT JOIN dbo.Company AS pcom
					ON pcom.CompanyID = pcus.CompanyId

				LEFT JOIN Reporting.CustomerShortNames AS psn
						ON psn.CustomerCode = pcom.CompanyCode

				--Part
				LEFT JOIN dbo.PartDetail AS pd	
					ON pd.PartDetailId = so.SalesOrderPartDetailID

				LEFT JOIN dbo.Part AS p
					ON p.PartId = pd.PartId

				LEFT JOIN dbo.PartType AS pt
					ON pt.PartTypeId = p.ProductLineId

				LEFT JOIN dbo.PartCategory AS pcat
					ON pcat.PartCategoryId = p.ProductCategoryId

				LEFT JOIN dbo.PartClass AS pclass
					ON pclass.PartClassId = p.ProductClassId

				--Job
				LEFT JOIN dbo.Contract AS job
					ON job.ContractId = so.SalesOrderContractID

				--CSR
				LEFT JOIN dbo.Employee AS csr
					ON csr.EmployeeId = so.SalesOrderCreatedByEmployeeID

				--Carrier
				LEFT JOIN dbo.Carrier AS car
					ON car.CarrierId = so.SalesOrderCarrierID

				--State
				LEFT JOIN dbo.State AS s
					ON s.StateId = so.SalesOrderShipStateID

				--Country
				LEFT JOIN dbo.Country AS c
					ON c.CountryId = so.SalesOrderShipCountryID

				--ClassCode
				LEFT JOIN dbo.ClassCode AS cc
					ON cc.ClassCodeId = so.SalesOrderClassCodeID

				--FreightType
				LEFT JOIN dbo.FreightType AS ft
					ON ft.FreightTypeId = so.SalesOrderFreightTypeID

				--StorageArea
				LEFT JOIN dbo.StorageArea AS sa
					ON sa.StorageAreaId = so.SalesOrderStorageAreaID

				--Warehouse
				LEFT JOIN dbo.Warehouse AS w
					ON w.WarehouseId = so.SalesOrderWarehouseID

			WHERE	so.SalesOrderStatus <> 7
				AND so.SalesOrderActive = 1
				AND CONVERT(DATE, so.SalesOrderModifiedDate) >= @_DaysBackDate
				AND CONVERT(DATE, so.SalesOrderOrderDate) >= '2019-12-28';




		--Get Invoice amounts for sales orders
		IF OBJECT_ID('tempdb..#Invoice') IS NOT NULL
			DROP TABLE #Invoice;

		SELECT
			rs.SalesOrderItemID																AS 'SalesOrderItemID',
			SUM(COALESCE(i.InvoiceExtendedPrice, 0) + COALESCE(i.InvoiceChargePrice, 0)
			+COALESCE(i.InvoiceTaxCollectable, 0))											AS 'InvoiceAmount'

			INTO #Invoice
			FROM #ResultSet AS rs
				
				JOIN Summary.Invoice AS i
					ON i.SalesOrderItemID = rs.SalesOrderItemID

			WHERE i.InvoiceStatus IN (2,4,5)
			GROUP BY rs.SalesOrderItemID;



		UPDATE rs SET
			rs.SalesOrderInvoiceAmount = i.InvoiceAmount

			FROM #ResultSet AS rs
				
				JOIN #Invoice AS i
					ON i.SalesOrderItemID = rs.SalesOrderItemID;



		--Get Shipped
		IF OBJECT_ID('tempdb..#Shipped') IS NOT NULL
			DROP TABLE #Shipped;

		SELECT
			s.SalesOrderItemID		AS 'SalesOrderItemID',
			s.ShipmentStatus		AS 'ShipmentStatus',
			MAX(s.ShipmentDate)		AS 'ShipmentDate',
			MAX(d.FiscalYear)		AS 'ShipmentFiscalYear',
			MAX(d.FiscalQuarter)	AS 'ShipmentFiscalQuarter',
			MAX(d.FiscalMonth)		AS 'ShipmentFiscalMonth',
			MAX(d.FiscalWeek)		AS 'ShipmentFiscalWeek',
			SUM(s.ShipmentWeight)	AS 'ShipmentWeight'

			INTO #Shipped
			FROM Summary.Shipment AS s 
		
				JOIN #ResultSet AS rs
					ON rs.SalesOrderItemID = s.SalesOrderItemID

				JOIN Reporting.Date AS d
					ON d.Date = s.ShipmentDate

			WHERE s.ShipmentStatus IN (1, 2) --Unposted and Posted
		
			GROUP BY 
				s.SalesOrderItemID,
				s.ShipmentStatus;


		
		--Unposted
		UPDATE rs SET
			rs.SalesOrderUnpostedWeight			= s.ShipmentWeight,
			rs.SalesOrderUnpostedDate			= s.ShipmentDate,
			rs.SalesOrderUnpostedFiscalYear		= s.ShipmentFiscalYear,
			rs.SalesOrderUnpostedFiscalQuarter	= s.ShipmentFiscalQuarter,
			rs.SalesOrderUnpostedFiscalMonth	= s.ShipmentFiscalMonth,
			rs.SalesOrderUnpostedFiscalWeek		= s.ShipmentFiscalWeek

			FROM #ResultSet AS rs
				
				JOIN #Shipped AS s
					ON s.SalesOrderItemID = rs.SalesOrderItemID

			WHERE s.ShipmentStatus = 1;



		--Posted
		UPDATE rs SET
			rs.SalesOrderUnpostedWeight			= s.ShipmentWeight,
			rs.SalesOrderUnpostedDate			= s.ShipmentDate,
			rs.SalesOrderUnpostedFiscalYear		= s.ShipmentFiscalYear,
			rs.SalesOrderUnpostedFiscalQuarter	= s.ShipmentFiscalQuarter,
			rs.SalesOrderUnpostedFiscalMonth	= s.ShipmentFiscalMonth,
			rs.SalesOrderUnpostedFiscalWeek		= s.ShipmentFiscalWeek

			FROM #ResultSet AS rs
				
				JOIN #Shipped AS s
					ON s.SalesOrderItemID = rs.SalesOrderItemID

			WHERE s.ShipmentStatus = 2;



		SELECT 
			rs.SalesOrderID									AS 'SalesOrderID',					
			rs.SalesOrderItemID								AS 'SalesOrderItemID',
			rs.SalesOrderLineNumber							AS 'SalesOrderLineNumber',
			rs.SalesOrderOrderDate							AS 'SalesOrderOrderDate',				
			rs.SalesOrderFiscalMonth						AS 'SalesOrderFiscalMonth',			
			rs.SalesOrderFiscalYear							AS 'SalesOrderFiscalYear',			
			rs.SalesOrderFiscalQuarter						AS 'SalesOrderFiscalQuarter',			
			rs.SalesOrderWeek								AS 'SalesOrderWeek',					
			rs.SalesOrderShipmentDate						AS 'SalesOrderShipmentDate',			
			rs.SalesOrderShipmentFiscalMonth				AS 'SalesOrderShipmentFiscalMonth',	
			rs.SalesOrderShipmentFiscalYear					AS 'SalesOrderShipmentFiscalYear',	
			rs.SalesOrderShipmentFiscalQuarter				AS 'SalesOrderShipmentFiscalQuarter',
			rs.SalesOrderShipmentFiscalWeek					AS 'SalesOrderShipmentFiscalWeek',	
			rs.SalesOrderLocation							AS 'SalesOrderLocation',				
			rs.SalesOrderLocationZip						AS 'SalesOrderLocationZip',			
			rs.SalesOrderCustomerCode						AS 'SalesOrderCustomerCode',			
			rs.SalesOrderCustomerName						AS 'SalesOrderCustomerName',			
			rs.SalesOrderMasterCustomerCode					AS 'SalesOrderMasterCustomerCode',	
			rs.SalesOrderMasterCustomerName					AS 'SalesOrderMasterCustomerName',
			rs.SalesOrderSalesRep							AS 'SalesOrderSalesRep',
			rs.SalesOrderRegion								AS 'SalesOrderRegion',
			rs.SalesOrderPartLength							AS 'SalesOrderPartLength',
			rs.SalesOrderPartNumber							AS 'SalesOrderPartNumber',
			rs.SalesOrderPartDescription					AS 'SalesOrderPartDescription',
			rs.SalesOrderPartType							AS 'SalesOrderPartType',
			rs.SalesOrderPartCategory						AS 'SalesOrderPartCategory',
			rs.SalesOrderPartClass							AS 'SalesOrderPartClass',
			rs.SalesOrderQuantity							AS 'SalesOrderQuantity',
			rs.SalesOrderQuantityUnit						AS 'SalesOrderQuantityUnit',
			rs.SalesOrderQuantityUnitsPerBase				AS 'SalesOrderQuantityUnitsPerBase',
			rs.SalesOrderWeight								AS 'SalesOrderWeight',
			rs.SalesOrderPrice								AS 'SalesOrderPrice',
			rs.SalesOrderExtendedPrice						AS 'SalesOrderExtendedPrice',
			rs.SalesOrderChargePrice						AS 'SalesOrderChargePrice',
			rs.SalesOrderTaxCollectable						AS 'SalesOrderTaxCollectable',
			rs.SalesOrderTotalAmount						AS 'SalesOrderTotalAmount',
			rs.SalesOrderSPCWT								AS 'SalesOrderSPCWT',			
			rs.SalesOrderLength								AS 'SalesOrderLength',		
			rs.SalesOrderLengthUnit							AS 'SalesOrderLengthUnit',
			rs.SalesOrderLengthUnitsPerBase					AS 'SalesOrderLengthUnitsPerBase',
			rs.SalesOrderStatus								AS 'SalesOrderStatus',		
			rs.SalesOrderPurchaseOrder						AS 'SalesOrderPurchaseOrder',
			rs.SalesOrderJob								AS 'SalesOrderJob',	
			rs.SalesOrderJobDescription						AS 'SalesOrderJobDescription',
			rs.SalesOrderJobEverday							AS 'SalesOrderJobEverday',
			rs.SalesOrderCSR								AS 'SalesOrderCSR',			
			rs.SalesOrderCarrier							AS 'SalesOrderCarrier',		
			rs.SalesOrderShipAddress1						AS 'SalesOrderShipAddress1',
			rs.SalesOrderShipAddress2						AS 'SalesOrderShipAddress2',
			rs.SalesOrderShipCity							AS 'SalesOrderShipCity',
			rs.SalesOrderShipZip							AS 'SalesOrderShipZip',
			rs.SalesOrderShipState							AS 'SalesOrderShipState',
			rs.SalesOrderShipCountry						AS 'SalesOrderShipCountry',
			rs.SalesOrderClassCode							AS 'SalesOrderClassCode',
			rs.SalesOrderClassCodeDescription				AS 'SalesOrderClassCodeDescription',
			rs.SalesOrderFreightType						AS 'SalesOrderFreightType',
			rs.SalesOrderStorageArea						AS 'SalesOrderStorageArea',
			rs.SalesOrderWarehouse							AS 'SalesOrderWarehouse',
			rs.SalesOrderInvoiceAmount						AS 'SalesOrderInvoiceAmount',
			rs.SalesOrderPostedWeight						AS 'SalesOrderPostedWeight',
			rs.SalesOrderPostedDate							AS 'SalesOrderPostedDate',
			rs.SalesOrderPostedFiscalMonth					AS 'SalesOrderPostedFiscalMonth',
			rs.SalesOrderPostedFiscalYear					AS 'SalesOrderPostedFiscalYear',
			rs.SalesOrderPostedFiscalQuarter				AS 'SalesOrderPostedFiscalQuarter',
			rs.SalesOrderPostedFiscalWeek					AS 'SalesOrderPostedFiscalWeek',
			rs.SalesOrderUnpostedWeight						AS 'SalesOrderUnpostedWeight',
			rs.SalesOrderUnpostedDate						AS 'SalesOrderUnpostedDate',
			rs.SalesOrderUnpostedFiscalMonth				AS 'SalesOrderUnpostedFiscalMonth',
			rs.SalesOrderUnpostedFiscalYear					AS 'SalesOrderUnpostedFiscalYear',
			rs.SalesOrderUnpostedFiscalQuarter				AS 'SalesOrderUnpostedFiscalQuarter',
			rs.SalesOrderUnpostedFiscalWeek					AS 'SalesOrderUnpostedFiscalWeek',
			COALESCE(rs.SalesOrderWeight, 0) - 
				COALESCE(rs.SalesOrderPostedWeight, 0) - 
				COALESCE(rs.SalesOrderUnpostedWeight, 0)	AS 'SalesOrderUnallocatedWeight',
			rs.SalesOrderModifiedDate						AS 'SalesOrderModifiedDate',
			rs.SalesOrderCreatedDate						AS 'SalesOrderCreatedDate'

			FROM #ResultSet AS rs;
		
	COMMIT

END
