USE [Cemco_DW]
GO
/****** Object:  StoredProcedure [Qlik].[Invoice]    Script Date: 20-07-2022 12:54:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jason Wu
-- Create date: 2021-12-7
-- Description:	Sproc for invoice for qlik
-- EXEC [Qlik].[Invoice] @DaysBack = -3
-- =============================================
ALTER PROCEDURE [Qlik].[Invoice]
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
		
		IF OBJECT_ID('tempdb..#ResultSet') IS NOT NULL
			DROP TABLE #ResultSet;

		--Invoice		
		SELECT		
			i.InvoiceID								AS 'InvoiceID',
			i.InvoiceItemID							AS 'InvoiceItemID',
			NULL									AS 'CreditMemoID',
			NULL									AS 'CreditMemoItemID',
			i.SalesOrderID							AS 'SalesOrderID',
			i.SalesOrderItemID						AS 'SalesOrderItemID',

			i.InvoiceStatus							AS 'InvoiceStatus',	
			CONVERT(DATE, i.InvoiceDate)			AS 'InvoiceDate',
			d.FiscalMonth							AS 'InvoiceFiscalMonth',
			d.FiscalYear							AS 'InvoiceFiscalYear',
			d.FiscalQuarter							AS 'InvoiceFiscalQuarter',
			d.FiscalWeekYearly						AS 'InvoiceWeek',

			i.InvoiceLocationID						AS 'InvoiceLocationID',
			i.InvoiceCustomerID						AS 'InvoiceCustomerID',
			i.InvoicePartDetailID					AS 'InvoicePartDetailID',
			i.SalesOrderContractID					AS 'InvoiceContractID',

			i.InvoiceWeight							AS 'InvoiceWeight',
			i.InvoicePriceVal						AS 'InvoicePrice',
			i.InvoiceExtendedPrice					AS 'InvoiceExtendedPrice',
			ISNULL(i.InvoiceChargePrice, 0)			AS 'InvoiceChargePrice',
			COALESCE(i.InvoiceTaxCollectable, 0)	AS 'InvoiceTaxCollectable',
			i.InvoiceExtendedPrice 
				+ ISNULL(i.InvoiceChargePrice, 0)
			+COALESCE(i.InvoiceTaxCollectable, 0)	AS 'InvoiceTotalPrice',
			i.InvoiceCost							AS 'InvoiceCost',
			i.InvoiceQuantity						AS 'InvoiceQuantity',
			i.InvoiceQuantityPartUnitDetailID		AS 'InvoicePartUnitDetailID',
			CONVERT(DATE, i.InvoiceModifiedDate)	AS 'InvoiceModifiedDate'
		
			INTO #ResultSet
			FROM Summary.Invoice AS i       

				--Fical dates
				JOIN Reporting.Date AS d  
					ON d.Date = i.InvoiceDate

			WHERE	i.InvoiceStatus IN (2,4,5)
				AND CONVERT(DATE, i.InvoiceModifiedDate) >= @_DaysBackDate
				AND CONVERT(DATE, i.InvoiceDate) >= '2019-12-28'

		UNION ALL

		SELECT
			cm.InvoiceID								AS 'InvoiceID',
			NULL										AS 'InvoiceItemID',
			cm.CreditMemoID								AS 'CreditMemoID',
			cm.CreditMemoItemID							AS 'CreditMemoItemID',
			NULL										AS 'SalesOrderID',
			NULL										AS 'SalesOrderItemID',
		 
			cm.CreditMemoStatus							AS 'InvoiceStatus',	
			CONVERT(DATE, cm.CreditMemoDate)			AS 'InvoiceDate',
			d.FiscalMonth								AS 'InvoiceFiscalMonth',
			d.FiscalYear								AS 'InvoiceFiscalYear',
			d.FiscalQuarter								AS 'InvoiceFiscalQuarter',
			d.FiscalWeekYearly							AS 'InvoiceWeek',
		 
			cm.CreditMemoLocationID						AS 'InvoiceLocationID',
			cm.CreditMemoCustomerID						AS 'InvoiceCustomerID',
			cm.CreditMemoPartDetailID					AS 'InvoicePartDetailID',
			NULL										AS 'InvoiceContractID',
		 
			-cm.CreditMemoWeight						AS 'InvoiceWeight',
			-cm.CreditMemoPrice							AS 'InvoicePrice',
			-cm.CreditMemoExtendedPrice					AS 'InvoiceExtendedPrice',
			NULL										AS 'InvoiceChargePrice',
			0											AS 'InvoiceTaxCollectable',
			-cm.CreditMemoExtendedPrice 				AS 'InvoiceTotalPrice', 
			NULL										AS 'InvoiceCost',
			-cm.CreditMemoBillQuantity					AS 'InvoiceQuantity',
			cm.CreditMemoBillPartUnitDetailID			AS 'InvoicePartUnitDetailID',
			CONVERT(DATE, cm.CreditMemoModifiedDate)	AS 'InvoiceModifiedDate'

			FROM Summary.CreditMemo AS cm

				JOIN Reporting.Date AS d
					ON d.Date = cm.CreditMemoDate

			WHERE 
					cm.CreditMemoStatus IN (2, 3)	
				AND cm.CreditMemoAdjustBill = 1
				AND CONVERT(DATE, cm.CreditMemoModifiedDate) >= @_DaysBackDate
				AND CONVERT(DATE, cm.CreditMemoDate) >= '2019-12-28';



		--Update SalesOrderID, SalesOrderItemID, and ContractID for CreditMemos
		UPDATE rs SET
			rs.SalesOrderID			= i.SalesOrderID,
			rs.SalesOrderItemID		= i.SalesOrderItemID,
			rs.InvoiceContractID	= i.SalesOrderContractID

			FROM #ResultSet AS rs

				JOIN Summary.Invoice AS i
					ON i.InvoiceID = rs.InvoiceID

			WHERE rs.CreditMemoItemID IS NOT NULL



		--Return Dataset
		SELECT

			rs.InvoiceID															AS 'InvoiceID',
			rs.InvoiceItemID														AS 'InvoiceItemID',
			rs.CreditMemoID															AS 'CreditMemoID',
			rs.CreditMemoItemID														AS 'CreditMemoItemID',
			rs.SalesOrderID															AS 'SalesOrderID',
			rs.SalesOrderItemID														AS 'SalesOrderItemID',
																					
			rs.InvoiceStatus														AS 'InvoiceStatus',	
			rs.InvoiceDate															AS 'InvoiceDate',
			rs.InvoiceFiscalMonth													AS 'InvoiceFiscalMonth',
			rs.InvoiceFiscalYear													AS 'InvoiceFiscalYear',
			rs.InvoiceFiscalQuarter													AS 'InvoiceFiscalQuarter',
			rs.InvoiceWeek															AS 'InvoiceWeek',
																					
			l.LocationCode															AS 'InvoiceLocation',
			ccom.CompanyCode														AS 'InvoiceCustomerCode',		
			REPLACE(COALESCE(csn.CustomerShortName,ccom.CompanyName), ',', ';')		AS 'InvoiceCustomerName',
			ccom.StateId															AS 'InvoiceCustomerStateId',
			pcom.CompanyCode														AS 'InvoiceMasterCustomerCode',	
			REPLACE(COALESCE(psn.CustomerShortName, pcom.CompanyName), ',', ';')	AS 'InvoiceMasterCustomerName',	
			COALESCE(srm.SalesRepName, t.TerritoryName)								AS 'InvoiceSalesRep',			
			r.RegionName				 											AS 'InvoiceRegion',
			pd.LengthFeet															AS 'InvoicePartLength',			
			p.PartNo																AS 'InvoicePartNumber',			
			REPLACE(REPLACE(p.Description, ',', ';'), '"', '')						AS 'InvoicePartDescription',	
			REPLACE(pt.PartTypeCode, ',', ';')										AS 'InvoicePartType',	
			REPLACE(REPLACE(pcat.PartCategoryCode, ',', ';'), '"', '')				AS 'InvoicePartCategory',	
			REPLACE(pclass.PartClassCode, ',', ';')									AS 'InvoicePartClass',
			job.ContractId															AS 'InvoiceJob',																		   
			REPLACE(job.JobName, ',', ';')											AS 'InvoiceJobDescription',
			CASE 
				WHEN job.ContractId IS NULL THEN 'Everyday'
				ELSE 'Job'
			END																		AS 'InvoiceJobEveryday',

			rs.InvoiceWeight														AS 'InvoiceWeight',
			rs.InvoicePrice															AS 'InvoicePrice',
			rs.InvoiceExtendedPrice													AS 'InvoiceExtendedPrice',
			rs.InvoiceChargePrice													AS 'InvoiceChargePrice',
			rs.InvoiceTaxCollectable												AS 'InvoiceTaxCollectable',
			rs.InvoiceTotalPrice													AS 'InvoiceTotalPrice', 
			rs.InvoiceCost															AS 'InvoiceCost',
			rs.InvoiceQuantity														AS 'InvoiceQuantity',
			pud.UnitName															AS 'InvoiceQuantityUnit',
			pud.UnitsPerBase														AS 'InvoiceQuantityUnitsPerBase',
			rs.InvoiceModifiedDate													AS 'InvoiceModifiedDate'

			FROM #ResultSet AS rs

				--Location
				JOIN dbo.Location AS l
					ON l.LocationId = rs.InvoiceLocationID

				--Child Customer
				JOIN dbo.Customer AS ccus
					ON ccus.CustomerId = rs.InvoiceCustomerID

				JOIN dbo.Company AS ccom
					ON ccom.CompanyID = ccus.CompanyId

				LEFT JOIN Reporting.CustomerShortNames AS csn
					ON csn.CustomerCode = ccom.CompanyCode

				--Sales Rep
				LEFT JOIN dbo.CustomerSalesRep AS sr
					ON sr.CustomerId = ccus.CustomerId
				
				LEFT JOIN dbo.Employee AS e
					ON e.EmployeeId = sr.EmployeeId

				LEFT JOIN dbo.Territory AS t
					ON t.TerritoryName = e.FirstName + ' ' + e.LastName

				--Region
				LEFT JOIN dbo.Region AS r
					ON r.RegionId = t.RegionId

				LEFT JOIN Reporting.SalesRepMapping AS srm
					ON srm.TerritoryId = t.TerritoryId

				--Master Customer
				LEFT JOIN dbo.customer AS pcus
					ON pcus.CustomerId = ccus.ParentCustomerId
		
				LEFT JOIN dbo.Company AS pcom
					ON pcom.CompanyID = pcus.CompanyId

				LEFT JOIN Reporting.CustomerShortNames AS psn
					ON psn.CustomerCode = pcom.CompanyCode

				--Part
				LEFT JOIN dbo.PartDetail AS pd	
					ON pd.PartDetailId = rs.InvoicePartDetailID

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
					ON job.ContractId = rs.InvoiceContractID

				--Quantity
				LEFT JOIN dbo.PartUnitDetail AS pud
					ON pud.PartUnitDetailId = rs.InvoicePartUnitDetailID;
		
	COMMIT

END
