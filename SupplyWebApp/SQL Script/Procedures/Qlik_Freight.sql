USE [Cemco_DW]
GO
/****** Object:  StoredProcedure [Qlik].[Freight]    Script Date: 20-07-2022 12:49:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Adelayde Rome
-- Create date: 2022-6-21
-- Description:	Sproc for Freight for qlik
-- EXEC [Qlik].[Freight] @DaysBack = -30
-- =============================================
ALTER PROCEDURE [Qlik].[Freight]
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


		--Shipment
		SELECT
			s.ShipmentID															AS 'ShipmentID',
			s.ShipmentItemID														AS 'ShipmentItemID',
			s.SalesOrderID															AS 'SalesOrderID',
			s.SalesOrderItemID														AS 'SalesOrderItemID',
			s.ShipmentStatus														AS 'ShipmentStatus',
			CONVERT(DATE, s.ShipmentDate)											AS 'ShipmentDate',
			d.FiscalMonth															AS 'ShipmentFiscalMonth',
			d.FiscalYear															AS 'ShipmentFiscalYear',
			d.FiscalQuarter															AS 'ShipmentFiscalQuarter',
			d.Week																	AS 'ShipmentWeek',
			CONVERT(DATE, s.ShipmentDeliveryDate)									AS 'ShipmentDeliveryDate',
			CONVERT(DATE, s.ShipmentPostDate)										AS 'ShipmentPostDate',
			CONVERT(DATE, s.ShipmentTransferReceivedDate)							AS 'ShipmentTransferReceivedDate',

			l.LocationCode															AS 'ShipmentLocation',
			ccom.CompanyCode														AS 'ShipmentCustomerCode',		
			REPLACE(COALESCE(csn.CustomerShortName,ccom.CompanyName), ',', ';')		AS 'ShipmentCustomerName',		
			pcom.CompanyCode														AS 'ShipmentMasterCustomerCode',	
			REPLACE(COALESCE(psn.CustomerShortName, pcom.CompanyName), ',', ';')	AS 'ShipmentMasterCustomerName',	
			COALESCE(srm.SalesRepName, t.TerritoryName)								AS 'ShipmentSalesRep',			
			r.RegionName															AS 'ShipmentRegion',			
			p.PartNo																AS 'ShipmentPartNumber',			
			REPLACE(REPLACE(p.Description, ',', ';'), '"', '')						AS 'ShipmentPartDescription',	
			REPLACE(pt.PartTypeDescription, ',', ';')								AS 'ShipmentPartType',	
			REPLACE(REPLACE(pcat.PartCategoryDescription, ',', ';'), '"', '')		AS 'ShipmentPartCategory',	
			REPLACE(pclass.PartClassDescription, ',', ';')							AS 'ShipmentPartClass',
			job.ContractId															AS 'ShipmentJob',
			REPLACE(job.JobName, ',', ';')											AS 'ShipmentJobDescription',
			CASE 
				WHEN job.ContractId IS NULL THEN 'Everyday'
				ELSE 'Job'
			END																		AS 'ShipmentJobEverday',
			REPLACE(REPLACE(car.Description, ',', ';'), '"', '')					AS 'ShipmentCarrier',

			i.InvoiceExtendedPrice 
				+ ISNULL(i.InvoiceChargePrice, 0)
				+COALESCE(i.InvoiceTaxCollectable, 0)								AS 'ShipmentTotalPrice',
			s.ShipmentWeight														AS 'ShipmentWeight',
			s.ShipmentTotalFreight													AS 'ShipmentTotalFreight',
			s.ShipmentQuantity														AS 'ShipmentQuantity',
			pud.UnitName															AS 'ShipmentQuantityUnitName',
			pud.UnitsPerBase														AS 'ShipmentQuantityUnitsPerBase',
			--shipment type
			CASE
				WHEN s.SalesOrderID IS NOT NULL 
					THEN 'Sales Order'			
				WHEN s.ShipmentTransferOrderItemID IS NOT NULL 
						OR s.ShipmentTransferOrderSteelID IS NOT NULL
					THEN 'Transfer' 
			END																		As 'ShipmentType',
			s.ShipmentTransferOrderItemID											AS 'TransferOrderItemID',
			s.ShipmentTransferOrderSteelID											AS 'TransferOrderSteelID',
			s.ShipmentPartID														AS 'ShipmentPartID',
			Convert(VARCHAR(10), NULL)												AS 'TransferFrom',
			Convert(VARCHAR(10), NULL)												AS 'TransferTo',
			Convert(VARCHAR(32), NULL)												AS 'TransferPartNumber',
			s.ShipmentStop															AS 'ShipmentStop',	
			CONVERT(DATE, s.ShipmentModifieddate)									AS 'ShipmentModifieddate',
			CASE 
				WHEN car.IsRailCar = 1 THEN 'Rail'
				ELSE 'Truck'
			END																		AS 'ShipmentIsRailCar'

			INTO #ResultSet
			FROM Summary.Shipment AS s 

				LEFT JOIN Summary.Invoice AS i
					ON i.ShipmentID = s.ShipmentID
						AND i.ShipmentItemID = s.ShipmentItemID --transfers do not have invoices 

				JOIN Reporting.Date AS d
					ON d.Date = s.ShipmentDate

				--Location
				JOIN dbo.Location AS l
					ON l.LocationId = s.ShipmentLocationID

				--Child Customer
				LEFT JOIN dbo.Customer AS ccus
					ON ccus.CustomerId = s.SalesOrderCustomerID

				LEFT JOIN dbo.Company AS ccom
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
				LEFT JOIN dbo.Part AS p
					ON p.PartId = s.ShipmentPartID

				LEFT JOIN dbo.PartType AS pt
					ON pt.PartTypeId = p.ProductLineId

				LEFT JOIN dbo.PartCategory AS pcat
					ON pcat.PartCategoryId = p.ProductCategoryId

				LEFT JOIN dbo.PartClass AS pclass
					ON pclass.PartClassId = p.ProductClassId

				--Job
				LEFT JOIN dbo.Contract AS job
					ON job.ContractId = s.SalesOrderContractID

				--Carrier
				LEFT JOIN dbo.Carrier AS car
					ON car.CarrierId = s.ShipmentCarrierID

				--Quantity
				LEFT JOIN dbo.PartUnitDetail AS pud
					ON pud.PartUnitDetailId = s.ShipmentQuantityPartUnitDetailID

			WHERE s.ShipmentStatus = 2
				AND ISNULL(i.InvoiceStatus, 2) IN (2,4,5)	--transfers null invoice 
				AND CONVERT(DATE, s.ShipmentModifieddate) >= @_DaysBackDate
				AND CONVERT(DATE, s.ShipmentDate) >= '2019-12-28';



			--Transfer Part Weight
			IF OBJECT_ID('tempdb..#TransferPartWeight') IS NOT NULL
				DROP TABLE #TransferPartWeight; 

			SELECT 
				rs.ShipmentID																			AS 'ShipmentID',
				rs.TransferOrderItemID																	AS 'TransferOrderItemID',
				SUM((rs.ShipmentQuantity * ISNULL(pd.LengthFeet, 1)) / pud.UnitsPerBase * p.ShipWeight) AS 'TransferPartWeight'

				INTO #TransferPartWeight
				FROM #ResultSet as rs

					JOIN Summary.TransferOrder AS t
						ON t.TransferOrderItemID = rs.TransferOrderItemID
				
					JOIN dbo.Part AS p
						ON p.PartId = rs.ShipmentPartID

					JOIN dbo.PartDetail AS pd
						ON pd.PartDetailId = t.TransferOrderPartDetailID
						
					JOIN dbo.PartUnitDetail AS pud
						ON pud.PartUnitDetailId = p.ShipWeightUnitId

					GROUP BY rs.ShipmentID, rs.TransferOrderItemID;



			--Update ShipmentWeight and transfer fields for transfer items
			UPDATE rs SET rs.ShipmentWeight = w.TransferPartWeight,		--t.TransferOrderWeight,
							rs.TransferFrom = lf.LocationCode,
							rs.TransferTo = lt.LocationCode,
							rs.TransferPartNumber = t.TransferOrderPartNumber

				FROM #ResultSet as rs

					JOIN Summary.TransferOrder AS t
						ON t.TransferOrderItemID = rs.TransferOrderItemID

					JOIN #TransferPartWeight AS w
						ON w.ShipmentID = rs.ShipmentID
							AND w.TransferOrderItemID = t.TransferOrderItemID
						
					JOIN dbo.Location AS lf
						ON lf.LocationId = t.TransferOrderFromLocationID
						
					JOIN dbo.Location AS lt
						ON lt.LocationId = t.TransferOrderToLocationID;



			--Update ShipmentWeight and transfer fields for transfer steel
			UPDATE rs SET rs.ShipmentWeight = ts.ActualWeight,
							rs.TransferFrom = lf.LocationCode,
							rs.TransferTo = lt.LocationCode

				FROM #ResultSet AS rs
					
					JOIN Summary.TransferOrderSteel AS ts
						ON ts.TransferOrderSteelId = rs.TransferOrderSteelID
					
					JOIN Summary.TransferOrder AS t
						ON t.TransferOrderID = ts.TransferOrderId

					JOIN dbo.Location AS lf
						ON lf.LocationId = t.TransferOrderFromLocationID
						
					JOIN dbo.Location AS lt
						ON lt.LocationId = t.TransferOrderToLocationID;



			--REMOVE SHIPMENTS WITH NEGATIVE WEIGHT
			IF OBJECT_ID('tempdb..#ExcludeShipment') IS NOT NULL
				DROP TABLE #ExcludeShipment;

			SELECT rs.ShipmentID 
				INTO #ExcludeShipment
				FROM #ResultSet AS rs 
				GROUP BY rs.ShipmentID 
				HAVING SUM(rs.ShipmentWeight) < 0 OR (SUM(rs.ShipmentWeight) = 0 and SUM(rs.ShipmentQuantity) < 0);



			SELECT 
				rs.ShipmentID,
				rs.ShipmentItemID,
				rs.SalesOrderID,
				rs.SalesOrderItemID,
				rs.ShipmentStatus,
				rs.ShipmentDate,
				rs.ShipmentFiscalMonth,
				rs.ShipmentFiscalYear,
				rs.ShipmentFiscalQuarter,
				rs.ShipmentWeek,
				rs.ShipmentDeliveryDate,
				rs.ShipmentPostDate,
				rs.ShipmentTransferReceivedDate,
				rs.ShipmentLocation,
				rs.ShipmentCustomerCode,		
				rs.ShipmentCustomerName,		
				rs.ShipmentMasterCustomerCode,
				rs.ShipmentMasterCustomerName,
				rs.ShipmentSalesRep,			
				rs.ShipmentRegion,			
				rs.ShipmentPartNumber,		
				rs.ShipmentPartDescription,	
				rs.ShipmentPartType,	
				rs.ShipmentPartCategory,	
				rs.ShipmentPartClass,
				rs.ShipmentJob,
				rs.ShipmentJobDescription,
				rs.ShipmentJobEverday,
				rs.ShipmentCarrier,
				rs.ShipmentTotalPrice,
				rs.ShipmentWeight,
				rs.ShipmentTotalFreight,
				rs.ShipmentQuantity,
				rs.ShipmentQuantityUnitName,
				rs.ShipmentQuantityUnitsPerBase,
				rs.ShipmentType,
				rs.TransferOrderItemID,
				rs.TransferOrderSteelID,
				rs.TransferFrom,
				rs.TransferTo,
				rs.TransferPartNumber,
				rs.ShipmentStop,	
				rs.ShipmentModifieddate,
				rs.ShipmentIsRailCar

				FROM #ResultSet rs
				
				LEFT JOIN #ExcludeShipment es
					ON es.ShipmentID = rs.ShipmentID

				WHERE es.ShipmentID IS NULL;



	COMMIT

END
