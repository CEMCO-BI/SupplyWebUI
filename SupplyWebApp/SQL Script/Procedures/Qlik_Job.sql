USE [Cemco_DW]
GO
/****** Object:  StoredProcedure [Qlik].[Job]    Script Date: 20-07-2022 19:35:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jason Wu
-- Create date: 2021-12-13
-- Description:	Sproc for job for qlik
-- EXEC [Qlik].[Job] @DaysBack = -30
-- =============================================
ALTER PROCEDURE [Qlik].[Job]
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

		SELECT
			c.ContractId							AS 'ContractId',
			COALESCE(c.Type, NULL)					AS 'ContractType',
			COALESCE(c.Status, NULL)				AS 'ContractStatus',
			COALESCE(c.Active, NULL)				AS 'ContractActive',
			CONVERT(DATE, c.StartDate)				AS 'ContractStartDate',
			CONVERT(DATE, c.EndDate)				AS 'ContractEndDate',
			CONVERT(DATE, c.FollowUp)				AS 'ContractFollowUpDate',
			CONVERT(DATE, c.PriceDate)				AS 'ContractPriceDate',
			CONVERT(DATE, c.CreatedOn)				AS 'ContractCreatedDate',
			CONVERT(DATE, c.SetAsPriceListDateOn)	AS 'ContractSetAsPriceListDateOn',
			CONVERT(DATE, c.SetAsCommittedDateOn)	AS 'ContractSetAsCommittedDateOn',
			CONVERT(DATE, c.SetAsInactiveDateOn)	AS 'ContractSetAsInactiveDateOn',
			CONVERT(DATE, c.SetAsCompleteDateOn)	AS 'ContractSetAsCompleteDateOn',
			CONVERT(DATE, c.SetAsAwardedDateOn)		AS 'ContractSetAsAwardedDateOn',
			REPLACE(c.JobName, ',', ';')			AS 'ContractJobName',
			REPLACE(c.ContractorName, ',', ';')		AS 'ContractContractorName',
			c.IsPriceDateOverride					AS 'ContractIsPriceDateOverride',
			c.CustomerId							AS 'CustomerId',
			c.PriceListLocationId					AS 'LocationId',
			COALESCE(cpm.ItemLineNo, NULL)			AS 'ContractItemLineNo',
			cpm.PartId								AS 'PartId',
			cpm.PartDetailId						AS 'PartDetailId',	
			cpm.QtyUnitDetailId						AS 'QtyUnitDetailId',
			cpm.QtyUnitId							AS 'QtyUnitId',
			cpm.DiscountUnitDetailId				AS 'DiscountUnitDetailId',
			cpm.DiscountUnitId						AS 'DiscountUnitId',
			cpm.Qty									AS 'ContractQuantity',
			cpm.DiscountType						AS 'ContractDiscountType',
			cpm.Discount							AS 'ContractDiscount',
			CONVERT(DATE, NULL)						AS 'ContractEscalationDate',
			CONVERT(FLOAT, NULL)					AS 'ContractEscalationPercentage',
			CONVERT(BIT, 1)							AS 'ContractIsContract',
			CONVERT(FLOAT, NULL)					AS 'ContractInvoiceWeight', 
			CONVERT(FLOAT, NULL)					AS 'ContractInvoiceChargePrice',
			CONVERT(FLOAT, NULL)					AS 'ContractInvoiceExtendedPrice',
			--**
			CONVERT(FLOAT, NULL)					AS 'ContractOrderWeight',
			CONVERT(FLOAT, NULL)					AS 'ContractOrderTotalAmount',
			--**
			COALESCE(e.FirstName, '') + ' ' + 
				COALESCE(e.LastName, '')			AS 'ContractCreatedBy',
			CONVERT(DATE, c.ModifiedOn)				AS 'ContractModifiedOn'
			
			INTO #ResultSet
			FROM dbo.Contract AS c

				--ContractPartMaster
				JOIN dbo.ContractPartMaster AS cpm
					ON cpm.ContractId = c.ContractId

				JOIN dbo.Employee AS e
					ON e.EmployeeId = c.CreatedBy

			WHERE CONVERT(DATE, c.ModifiedOn) >= @_DaysBackDate
				AND CONVERT(DATE, c.StartDate) >= '2019-12-28'
				AND c.Active = 1
				AND c.Type = 1;

			

		--Update ContractEscalation
		UPDATE rs SET
			rs.ContractEscalationDate			= e.EscalationDate,
			rs.ContractEscalationPercentage		= e.EscalationPercentage

			FROM #ResultSet AS rs
				
				JOIN dbo.ContractEscalation AS e
					ON e.ContractId = rs.ContractId;


		--Get distinct contracts for invoice and sales order table
		IF OBJECT_ID('tempdb..#DistinctContract') IS NOT NULL
			DROP TABLE #DistinctContract;

		SELECT DISTINCT ContractId

			INTO #DistinctContract
			FROM #ResultSet AS rs;


		
		IF OBJECT_ID('tempdb..#Shipped') IS NOT NULL
			DROP TABLE #Shipped;

		SELECT
			i.SalesOrderContractID		AS 'ContractID',
			i.InvoiceCustomerID			AS 'CustomerID',
			pd.PartId					AS 'PartId',
			SUM(i.InvoiceWeight)		AS 'InvoiceWeight',
			SUM(i.InvoiceChargePrice)	AS 'InvoiceChargePrice',
			SUM(i.InvoiceExtendedPrice)	AS 'InvoiceExtendedPrice',
			MAX(i.InvoiceModifiedDate)	AS 'InvoiceModifiedDate'
			
			INTO #Shipped
			FROM Summary.Invoice AS i

				JOIN #DistinctContract AS dc
					ON dc.ContractId = i.SalesOrderContractID

				JOIN dbo.PartDetail AS pd
					ON pd.PartDetailId = i.InvoicePartDetailID

			WHERE i.InvoiceStatus <> 3 --Not void

			GROUP BY 
				i.SalesOrderContractID,
				i.InvoiceCustomerID,
				pd.PartId;



		--Update shipped/invoiced weight from result set
		--There can be duplicate parts within a contract. Make sure to apply the shipped/invoiced weight to only one of the parts
		UPDATE rs SET
			rs.ContractInvoiceWeight = s.InvoiceWeight,
			rs.ContractInvoiceChargePrice = s.InvoiceChargePrice,
			rs.ContractInvoiceExtendedPrice = s.InvoiceExtendedPrice

			FROM #ResultSet AS rs
				
				JOIN (
					SELECT
						rs.ContractId				AS 'ContractId',
						MIN(rs.ContractItemLineNo)	AS 'ContractItemLineNo',
						rs.PartId					AS 'PartId',
						AVG(s.InvoiceWeight)		AS 'InvoiceWeight',
						AVG(s.InvoiceChargePrice)	AS 'InvoiceChargePrice',
						AVG(s.InvoiceExtendedPrice)	AS 'InvoiceExtendedPrice'

						FROM #ResultSet AS rs
				
							JOIN #Shipped AS s
								ON s.ContractID = rs.ContractId
									AND s.PartId = rs.PartId

						GROUP BY 
							rs.ContractId,
							rs.PartId)
					AS s
						ON s.ContractId = rs.ContractId
							AND s.ContractItemLineNo = rs.ContractItemLineNo
							AND s.PartId = rs.PartId;



		--Identify non contract part master for background discounts
		IF OBJECT_ID('tempdb..#BackgroundDiscount') IS NOT NULL
			DROP TABLE #BackgroundDiscount;

		SELECT
			s.*,
			CONVERT(INT, NULL)		AS 'ContractDiscountType',
			CONVERT(FLOAT, NULL)	AS 'ContractDiscount',
			CONVERT(INT, NULL)		AS 'DiscountUnitDetailId',
			CONVERT(INT, NULL)		AS 'DiscountUnitId',
			CONVERT(BIT, 0)			AS 'ContractIsContract'

			INTO #BackgroundDiscount
			FROM #Shipped AS s
				
					LEFT JOIN #ResultSet AS rs
						ON s.ContractID = rs.ContractId
							AND s.PartId = rs.PartId

				WHERE rs.PartId IS NULL;



		--Ones with part class
		UPDATE bd SET
			bd.ContractDiscountType = cpc.DiscountType,
			bd.ContractDiscount		= cpc.Discount,
			bd.DiscountUnitDetailId = cpc.DiscountUnitDetailId,
			bd.DiscountUnitId		= cpc.DiscountUnitId,
			bd.ContractIsContract	= 1

			FROM #BackgroundDiscount AS bd
				
				JOIN dbo.Part AS p
					ON p.PartId = bd.PartId

				JOIN dbo.ContractProductClass AS cpc
					ON cpc.ContractId = bd.ContractID
						AND cpc.ProductLineId		= p.ProductLineId
						AND cpc.ProductCategoryId	= p.ProductCategoryId
						AND cpc.ProductClassId		= p.ProductClassId;



		--Ones without part class
		UPDATE bd SET
			bd.ContractDiscountType = cpc.DiscountType,
			bd.ContractDiscount		= cpc.Discount,
			bd.DiscountUnitDetailId = cpc.DiscountUnitDetailId,
			bd.DiscountUnitId		= cpc.DiscountUnitId,
			bd.ContractIsContract	= 1

			FROM #BackgroundDiscount AS bd
				
				JOIN dbo.Part AS p
					ON p.PartId = bd.PartId

				JOIN dbo.ContractProductClass AS cpc
					ON cpc.ContractId = bd.ContractID
						AND cpc.ProductLineId		= p.ProductLineId
						AND cpc.ProductCategoryId	= p.ProductCategoryId
						AND cpc.ProductClassId		IS NULL
			
			WHERE bd.ContractIsContract = 0;



		--Contract Price List
		UPDATE bd SET
			bd.ContractDiscountType = cpl.DiscountType,
			bd.ContractDiscount		= cpl.Discount,
			bd.DiscountUnitDetailId = cpl.DiscountUnitDetailId,
			bd.DiscountUnitId		= cpl.DiscountUnitId,
			bd.ContractIsContract	= 1

			FROM #BackgroundDiscount AS bd

				JOIN dbo.Part AS p
					ON p.PartId = bd.PartId

				JOIN dbo.ContractPriceList AS cpl
					ON cpl.ContractId = bd.ContractID
						AND cpl.PriceListId = p.PriceListId

			WHERE bd.ContractIsContract = 0;


		
		--Add parts in contract that were not quoted
		INSERT INTO #ResultSet (
			ContractId,
			CustomerId,
			LocationId,
			PartId,
			ContractInvoiceWeight,
			ContractInvoiceChargePrice,
			ContractInvoiceExtendedPrice,
			ContractDiscountType,
			ContractDiscount,
			DiscountUnitDetailId,
			DiscountUnitId,
			ContractIsContract,
			ContractModifiedOn,
			ContractJobName,
			ContractContractorName,
			ContractIsPriceDateOverride,
			ContractType,
			ContractStatus,
			ContractActive,
			ContractStartDate,
			ContractEndDate,
			ContractFollowUpDate,
			ContractPriceDate,
			ContractCreatedDate,
			ContractSetAsPriceListDateOn,
			ContractSetAsCommittedDateOn,
			ContractSetAsInactiveDateOn,
			ContractSetAsCompleteDateOn,
			ContractSetAsAwardedDateOn)
			SELECT
				bd.ContractID							AS 'ContractID',
				bd.CustomerID							AS 'CustomerID',
				c.PriceListLocationId					AS 'LocationId',
				bd.PartId								AS 'PartID',
				bd.InvoiceWeight						AS 'ContractInvoiceWeight',
				bd.InvoiceChargePrice					AS 'InvoiceChargePrice',
				bd.InvoiceExtendedPrice					AS 'InvoiceExtendedPrice',
				bd.ContractDiscountType					AS 'ContractDiscountType',
				bd.ContractDiscount						AS 'ContractDiscount',
				bd.DiscountUnitDetailId					AS 'DiscountUnitDetailId',
				bd.DiscountUnitId						AS 'DiscountUnitId',
				bd.ContractIsContract					AS 'ContractIsContract',
				bd.InvoiceModifiedDate					AS 'ContractModifiedOn',
				REPLACE(c.JobName, ',', ';')			AS 'ContractJobName',
				REPLACE(c.ContractorName, ',', ';')		AS 'ContractContractorName',
				c.IsPriceDateOverride					AS 'ContractIsPriceDateOverride',
				COALESCE(c.Type, NULL)					AS 'ContractType',
				COALESCE(c.Status, NULL)				AS 'ContractStatus',
				COALESCE(c.Active, NULL)				AS 'ContractActive',
				CONVERT(DATE, c.StartDate)				AS 'ContractStartDate',
				CONVERT(DATE, c.EndDate)				AS 'ContractEndDate',
				CONVERT(DATE, c.FollowUp)				AS 'ContractFollowUpDate',
				CONVERT(DATE, c.PriceDate)				AS 'ContractPriceDate',
				CONVERT(DATE, c.CreatedOn)				AS 'ContractCreatedDate',
				CONVERT(DATE, c.SetAsPriceListDateOn)	AS 'ContractSetAsPriceListDateOn',
				CONVERT(DATE, c.SetAsCommittedDateOn)	AS 'ContractSetAsCommittedDateOn',
				CONVERT(DATE, c.SetAsInactiveDateOn)	AS 'ContractSetAsInactiveDateOn',
				CONVERT(DATE, c.SetAsCompleteDateOn)	AS 'ContractSetAsCompleteDateOn',
				CONVERT(DATE, c.SetAsAwardedDateOn)		AS 'ContractSetAsAwardedDateOn'
				

				FROM #BackgroundDiscount AS bd
					
					JOIN dbo.Contract AS c
						ON c.ContractId = bd.ContractID;		

		
		--**
		--Ordered 
		IF OBJECT_ID('tempdb..#Ordered') IS NOT NULL
			DROP TABLE #Ordered;

		SELECT
			s.SalesOrderContractID						AS 'ContractID',
			s.SalesOrderCustomerID						AS 'CustomerID',
			pd.PartId									AS 'PartId',
			SUM(s.SalesOrderWeight)						AS 'SalesOrderWeight',
			SUM(s.SalesOrderExtendedPrice 
				+ ISNULL(s.SalesOrderChargePrice, 0))	AS 'SalesOrderTotalAmount',
			MAX(s.SalesOrderModifiedDate)				AS 'SalesOrderModifiedDate'
			
			INTO #Ordered
			FROM Summary.SalesOrder AS s

				JOIN #DistinctContract AS dc
					ON dc.ContractId = s.SalesOrderContractID

				JOIN dbo.PartDetail AS pd
					ON pd.PartDetailId = s.SalesOrderPartDetailID

			WHERE s.SalesOrderStatus NOT IN (6, 7, 8, 12) --Not rejected, cancelled
					AND s.SalesOrderActive = 1

			GROUP BY 
				s.SalesOrderContractID,
				s.SalesOrderCustomerID,
				pd.PartId;



		--Update ordered weight and amount from result set
		--There can be duplicate parts within a contract. Make sure to apply the ordered weight to only one of the parts
		UPDATE rs SET
			rs.ContractOrderWeight = s.SalesOrderWeight,
			rs.ContractOrderTotalAmount = s.SalesOrderTotalAmount

			FROM #ResultSet AS rs
				
				JOIN (
					SELECT
						rs.ContractId					AS 'ContractId',
						MIN(rs.ContractItemLineNo)		AS 'ContractItemLineNo',
						rs.PartId						AS 'PartId',
						AVG(o.SalesOrderWeight)			AS 'SalesOrderWeight',
						AVG(o.SalesOrderTotalAmount)	AS 'SalesOrderTotalAmount'

						FROM #ResultSet AS rs
				
							JOIN #Ordered AS o
								ON o.ContractID = rs.ContractId
									AND o.PartId = rs.PartId

						GROUP BY 
							rs.ContractId,
							rs.PartId)
					AS s
						ON s.ContractId = rs.ContractId
							AND (s.ContractItemLineNo = rs.ContractItemLineNo OR rs.ContractItemLineNo IS NULL) --**
							AND s.PartId = rs.PartId;


		--Identify non contract part master for background discounts (Sales Orders)
		IF OBJECT_ID('tempdb..#BackgroundDiscountOrder') IS NOT NULL
			DROP TABLE #BackgroundDiscountOrder;

		SELECT
			o.*,
			CONVERT(INT, NULL)		AS 'ContractDiscountType',
			CONVERT(FLOAT, NULL)	AS 'ContractDiscount',
			CONVERT(INT, NULL)		AS 'DiscountUnitDetailId',
			CONVERT(INT, NULL)		AS 'DiscountUnitId',
			CONVERT(BIT, 0)			AS 'ContractIsContract'

			INTO #BackgroundDiscountOrder
			FROM #Ordered AS o
				
					LEFT JOIN #ResultSet AS rs
						ON o.ContractID = rs.ContractId
							AND o.PartId = rs.PartId

				WHERE rs.PartId IS NULL;



		--Ones with part class
		UPDATE bd SET
			bd.ContractDiscountType = cpc.DiscountType,
			bd.ContractDiscount		= cpc.Discount,
			bd.DiscountUnitDetailId = cpc.DiscountUnitDetailId,
			bd.DiscountUnitId		= cpc.DiscountUnitId,
			bd.ContractIsContract	= 1

			FROM #BackgroundDiscountOrder AS bd
				
				JOIN dbo.Part AS p
					ON p.PartId = bd.PartId

				JOIN dbo.ContractProductClass AS cpc
					ON cpc.ContractId = bd.ContractID
						AND cpc.ProductLineId		= p.ProductLineId
						AND cpc.ProductCategoryId	= p.ProductCategoryId
						AND cpc.ProductClassId		= p.ProductClassId;



		--Ones without part class
		UPDATE bd SET
			bd.ContractDiscountType = cpc.DiscountType,
			bd.ContractDiscount		= cpc.Discount,
			bd.DiscountUnitDetailId = cpc.DiscountUnitDetailId,
			bd.DiscountUnitId		= cpc.DiscountUnitId,
			bd.ContractIsContract	= 1

			FROM #BackgroundDiscountOrder AS bd
				
				JOIN dbo.Part AS p
					ON p.PartId = bd.PartId

				JOIN dbo.ContractProductClass AS cpc
					ON cpc.ContractId = bd.ContractID
						AND cpc.ProductLineId		= p.ProductLineId
						AND cpc.ProductCategoryId	= p.ProductCategoryId
						AND cpc.ProductClassId		IS NULL
			
			WHERE bd.ContractIsContract = 0;



		--Contract Price List
		UPDATE bd SET
			bd.ContractDiscountType = cpl.DiscountType,
			bd.ContractDiscount		= cpl.Discount,
			bd.DiscountUnitDetailId = cpl.DiscountUnitDetailId,
			bd.DiscountUnitId		= cpl.DiscountUnitId,
			bd.ContractIsContract	= 1

			FROM #BackgroundDiscountOrder AS bd

				JOIN dbo.Part AS p
					ON p.PartId = bd.PartId

				JOIN dbo.ContractPriceList AS cpl
					ON cpl.ContractId = bd.ContractID
						AND cpl.PriceListId = p.PriceListId

			WHERE bd.ContractIsContract = 0;


		
		--Add ordered parts in contract that were not quoted
		INSERT INTO #ResultSet (
			ContractId,
			CustomerId,
			LocationId,
			PartId,
			ContractOrderWeight,
			ContractOrderTotalAmount,
			ContractDiscountType,
			ContractDiscount,
			DiscountUnitDetailId,
			DiscountUnitId,
			ContractIsContract,
			ContractModifiedOn,
			ContractJobName,
			ContractContractorName,
			ContractIsPriceDateOverride,
			ContractType,
			ContractStatus,
			ContractActive,
			ContractStartDate,
			ContractEndDate,
			ContractFollowUpDate,
			ContractPriceDate,
			ContractCreatedDate,
			ContractSetAsPriceListDateOn,
			ContractSetAsCommittedDateOn,
			ContractSetAsInactiveDateOn,
			ContractSetAsCompleteDateOn,
			ContractSetAsAwardedDateOn)
			SELECT
				bd.ContractID							AS 'ContractID',
				bd.CustomerID							AS 'CustomerID',
				c.PriceListLocationId					AS 'LocationId',
				bd.PartId								AS 'PartID',
				bd.SalesOrderWeight						AS 'ContractOrderWeight',
				bd.SalesOrderTotalAmount				AS 'ContractOrderTotalAmount',
				bd.ContractDiscountType					AS 'ContractDiscountType',
				bd.ContractDiscount						AS 'ContractDiscount',
				bd.DiscountUnitDetailId					AS 'DiscountUnitDetailId',
				bd.DiscountUnitId						AS 'DiscountUnitId',
				bd.ContractIsContract					AS 'ContractIsContract',
				bd.SalesOrderModifiedDate				AS 'ContractModifiedOn',
				REPLACE(c.JobName, ',', ';')			AS 'ContractJobName',
				REPLACE(c.ContractorName, ',', ';')		AS 'ContractContractorName',
				c.IsPriceDateOverride					AS 'ContractIsPriceDateOverride',
				COALESCE(c.Type, NULL)					AS 'ContractType',
				COALESCE(c.Status, NULL)				AS 'ContractStatus',
				COALESCE(c.Active, NULL)				AS 'ContractActive',
				CONVERT(DATE, c.StartDate)				AS 'ContractStartDate',
				CONVERT(DATE, c.EndDate)				AS 'ContractEndDate',
				CONVERT(DATE, c.FollowUp)				AS 'ContractFollowUpDate',
				CONVERT(DATE, c.PriceDate)				AS 'ContractPriceDate',
				CONVERT(DATE, c.CreatedOn)				AS 'ContractCreatedDate',
				CONVERT(DATE, c.SetAsPriceListDateOn)	AS 'ContractSetAsPriceListDateOn',
				CONVERT(DATE, c.SetAsCommittedDateOn)	AS 'ContractSetAsCommittedDateOn',
				CONVERT(DATE, c.SetAsInactiveDateOn)	AS 'ContractSetAsInactiveDateOn',
				CONVERT(DATE, c.SetAsCompleteDateOn)	AS 'ContractSetAsCompleteDateOn',
				CONVERT(DATE, c.SetAsAwardedDateOn)		AS 'ContractSetAsAwardedDateOn'
				

				FROM #BackgroundDiscountOrder AS bd
					
					JOIN dbo.Contract AS c
						ON c.ContractId = bd.ContractID;
		--**

		--Fill in NULL Created By
		IF OBJECT_ID('tempdb..#CreatedBy') IS NOT NULL
			DROP TABLE #CreatedBy;

		SELECT DISTINCT
			rs.ContractId			AS 'ContractId',
			rs.ContractCreatedBy	AS 'ContractCreatedBy'

			INTO #CreatedBy
			FROM #ResultSet AS rs
			WHERE rs.ContractCreatedBy IS NOT NULL;



		UPDATE rs SET
			rs.ContractCreatedBy = c.ContractCreatedBy

			FROM #ResultSet AS rs

				JOIN #CreatedBy AS c
					ON c.ContractId = rs.ContractId

			WHERE rs.ContractCreatedBy IS NULL;



		--Round to the cents for below part type and category only
		DECLARE @_PartType_ML INT= (SELECT PartTypeId FROM dbo.PartType WHERE PartTypeCode = 'ML');
		DECLARE @_PartCat_CLIP INT = (SELECT PartCategoryId FROM dbo.PartCategory WHERE PartCategoryCode = 'CLIP');



		SELECT
			rs.ContractId															AS 'ContractId',
			ccom.CompanyCode														AS 'ContractCustomerCode',		
			REPLACE(COALESCE(csn.CustomerShortName,ccom.CompanyName), ',', ';')		AS 'ContractCustomerName',		
			pcom.CompanyCode														AS 'ContractMasterCustomerCode',	
			REPLACE(COALESCE(psn.CustomerShortName, pcom.CompanyName), ',', ';')	AS 'ContractMasterCustomerName',	
			COALESCE(srm.SalesRepName, t.TerritoryName)								AS 'ContractSalesRep',			
			r.RegionName															AS 'ContractRegion',
			l.LocationCode															AS 'ContractLocation',

			rs.ContractType															AS 'ContractType',
			rs.ContractStatus														AS 'ContractStatus',
			rs.ContractActive														AS 'ContractActive',
			rs.ContractStartDate													AS 'ContractStartDate',
			sd.FiscalYear															AS 'ContractStartFiscalYear',
			sd.FiscalMonth															AS 'ContractStartFiscalMonth',
			rs.ContractEndDate														AS 'ContractEndDate',
			ed.FiscalYear															AS 'ContractEndFiscalYear',
			ed.FiscalMonth															AS 'ContractEndFiscalMonth',
			rs.ContractFollowUpDate													AS 'ContractFollowUpDate',
			rs.ContractPriceDate													AS 'ContractPriceDate',
			rs.ContractCreatedDate													AS 'ContractCreatedDate',
			cd.FiscalYear															AS 'ContractCreatedFiscalYear',
			cd.FiscalMonth															AS 'ContractCreatedFiscalMonth',	
			rs.ContractSetAsPriceListDateOn											AS 'ContractSetAsPriceListDateOn',
			pld.FiscalYear															AS 'ContractSetAsPriceListFiscalYear',
			pld.FiscalMonth															AS 'ContractSetAsPriceListFiscalMonth',
			rs.ContractSetAsCommittedDateOn											AS 'ContractSetAsCommittedDateOn',
			rs.ContractSetAsInactiveDateOn											AS 'ContractSetAsInactiveDateOn',
			rs.ContractSetAsCompleteDateOn											AS 'ContractSetAsCompleteDateOn',
			rs.ContractSetAsAwardedDateOn											AS 'ContractSetAsAwardedDateOn',
			rs.ContractJobName														AS 'ContractJobName',
			rs.ContractContractorName												AS 'ContractContractorName',
			rs.ContractIsPriceDateOverride											AS 'ContractIsPriceDateOverride',

			rs.ContractItemLineNo													AS 'ContractItemLineNo',
			REPLACE(p.PartNo, ',', ';')												AS 'ContractPartNumber',			
			REPLACE(REPLACE(p.Description, ',', ';'), '"', '')						AS 'ContractPartDescription',	
			REPLACE(pt.PartTypeDescription, ',', ';')								AS 'ContractPartType',	
			REPLACE(REPLACE(pcat.PartCategoryDescription, ',', ';'), '"', '')		AS 'ContractPartCategory',	
			REPLACE(pclass.PartClassDescription, ',', ';')							AS 'ContractPartClass',
			p.ShipWeight															AS 'ContractPartWeight',
			wpud.UnitName															AS 'ContractPartWeightUnit',
			wpud.UnitsPerBase														AS 'ContractPartWeightUnitPerBase',

			COALESCE(pp.Price, 0)													AS 'ContractDefaultPrice',
			dp.UnitName																AS 'ContractDefaultPriceUnit',
			COALESCE(dp.UnitsPerBase, 0)											AS 'ContractDefaultPriceUnitsPerBase',

			rs.ContractQuantity														AS 'ContractQuantity',
			ppud.UnitName															AS 'ContractQuantityUnit',
			ppud.UnitsPerBase														AS 'ContractQuantityUnitsPerBase',
			puom.UOMName															AS 'ContractQuantityUnitOfMeasure',
			puom.UnitPerBase														AS 'ContractQuantityUnitOfMeasureUnitPerBase',
			rs.ContractDiscountType													AS 'ContractDiscountType',
			rs.ContractDiscount														AS 'ContractDiscount',
			dpud.UnitName															AS 'ContractDiscountUnit',
			dpud.UnitsPerBase														AS 'ContractDiscountUnitsPerBase',
			duom.UOMName															AS 'ContractDiscountUnitOfMeasure',
			duom.UnitPerBase														AS 'ContractDiscountUnitOfMeasureUnitPerBase',
			rs.ContractEscalationDate												AS 'ContractEscalationDate',
			rs.ContractEscalationPercentage											AS 'ContractEscalationPercentage',
			COALESCE(pd.LengthFeet, 1)												AS 'ContractLengthFeet',

			CASE
				WHEN rs.PartDetailId IS NULL
					THEN 'Part Number'
				ELSE 'Part Master'
			END																		AS 'ContractItemType',	
			--CASE
			--	WHEN p.ProductLineId =  @_PartType_ML OR p.ProductCategoryId = @_PartCat_CLIP THEN 0
			--	ELSE 1
			--END																		AS 'ContractPriceRound',
			p.PriceRound															AS 'ContractPriceRound',

			rs.ContractIsContract													AS 'ContractIsContract',
			rs.ContractInvoiceWeight												AS 'ContractInvoiceWeight',
			rs.ContractInvoiceChargePrice											AS 'ContractInvoiceChargePrice',
			rs.ContractInvoiceExtendedPrice											AS 'ContractInvoiceExtendedPrice',
			--**
			rs.ContractOrderWeight													AS 'ContractOrderWeight',
			rs.ContractOrderTotalAmount												AS 'ContractOrderTotalAmount',
			--**
			rs.ContractCreatedBy													AS 'ContractCreatedBy',
			rs.ContractModifiedOn													AS 'ContractModifiedOn'

			FROM #ResultSet AS rs
				
				--Dates
				JOIN Reporting.Date AS sd
					ON sd.Date = CONVERT(DATE, rs.ContractStartDate)

				LEFT JOIN Reporting.Date AS ed
					ON ed.Date = CONVERT(DATE, rs.ContractEndDate)

				JOIN Reporting.Date AS cd
					ON cd.Date = CONVERT(DATE, rs.ContractCreatedDate)

				LEFT JOIN Reporting.Date AS pld
					ON pld.Date = CONVERT(DATE, rs.ContractSetAsPriceListDateOn)
					
				--Child Customer
				JOIN dbo.Customer AS ccus
					ON ccus.CustomerId = rs.CustomerId

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

				--Location
				LEFT JOIN dbo.Location AS l
					ON l.LocationId = rs.LocationId

				LEFT JOIN dbo.PartDetail AS pd
					ON pd.PartDetailId = rs.PartDetailId

				--Part
				LEFT JOIN dbo.Part AS p
					ON p.PartId = rs.PartId

				LEFT JOIN dbo.PartType AS pt
					ON pt.PartTypeId = p.ProductLineId

				LEFT JOIN dbo.PartCategory AS pcat
					ON pcat.PartCategoryId = p.ProductCategoryId

				LEFT JOIN dbo.PartClass AS pclass
					ON pclass.PartClassId = p.ProductClassId

				--Part PartUnitDetail
				LEFT JOIN dbo.PartUnitDetail AS ppud
					ON ppud.PartUnitDetailId = rs.QtyUnitDetailId

				--Part UnitOfMeasure
				LEFT JOIN dbo.UnitOfMeasure AS puom
					ON puom.UnitOfMeasureId = rs.QtyUnitId

				--Weight PartUnitDetail
				LEFT JOIN dbo.PartUnitDetail AS wpud
					ON wpud.PartUnitDetailId = p.ShipWeightUnitId

				--Discount PartUnitDetail
				LEFT JOIN dbo.PartUnitDetail AS dpud
					ON dpud.PartUnitDetailId = rs.DiscountUnitDetailId

				--Discount UnitOfMeasure
				LEFT JOIN dbo.UnitOfMeasure AS duom
					ON duom.UnitOfMeasureId = rs.DiscountUnitId

				--Default Part Price
				LEFT JOIN dbo.PartPrice AS pp
					ON pp.PartPriceId = (SELECT TOP 1 PartPriceId 
											FROM dbo.PartPrice AS pp1
											WHERE	pp1.PartId = rs.PartId
												AND CONVERT(DATE, pp1.EffectiveDate) <= CASE
																							WHEN rs.ContractIsPriceDateOverride = 1 THEN rs.ContractPriceDate
																							ELSE rs.ContractStartDate
																						END
											ORDER BY pp1.EffectiveDate DESC)

				--Price PartUnitDetail
				LEFT JOIN dbo.PartUnitDetail AS dp
					ON dp.PartUnitDetailId = pp.PriceUnitId;
				
				
	COMMIT

END
